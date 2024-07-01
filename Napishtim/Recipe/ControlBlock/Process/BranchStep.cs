using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock.Process
{
    public class BranchStep_S : ProcessStepSource
    {
        public int NumOfBranches { get; private init; } = 1;

        public override IEnumerable<ProcessShader> Shaders => throw new NotImplementedException();

        public override IEnumerable<ProcessShader> PostShaders => throw new NotImplementedException();

        public override IEnumerable<KeyValuePair<uint, (string name, Event evt)>> LocalEvents => throw new NotImplementedException();

        public BranchStep_S(string name, IEnumerable<(string name, string condition, Expression a, Expression b, ProcessShaders? postShader, uint next)> branches) : base(name)
        {
            _step["END_POINTS"] = new JsonArray();
            JsonObject branch;
            foreach (var b in branches)
            {
                branch = new JsonObject();
                branch["NAME"] = b.name;
                branch["TARGET"] = b.next;
                branch["TRIGGER"] = new JsonArray()
                    {
                        Event.MAKE(b.condition,("COMPARAND_A", b.a), ("COMPARAND_B", b.b)).ToString()
                    };
                if (b.postShader != null)
                    branch["POST_SHADERS"] = b.postShader.ToJson();
                _step["END_POINTS"].AsArray().Add(branch);
                NumOfBranches++;
            }

            branch = new JsonObject();
            branch["NAME"] = "#Default#";
            branch["TRIGGER"] = new JsonArray() { CST.ON_STRING };
            _step["END_POINTS"].AsArray().Add(branch);

            StepFootprint = 1;
            UserVariableFootprint = 0;
        }

        public BranchStep_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, IEnumerable<(string name, JsonArray condition, ProcessShaders? postShader, uint next)> branches):base(name)
        {
            if (locals != null && locals.Count > 0)
            {
                _step["EVENTS"] = new JsonArray();
                foreach (var local in locals)
                {
                    JsonObject localEvt = new JsonObject();
                    localEvt["ID"] = local.Key;
                    localEvt["NAME"] = local.Value.name;
                    localEvt["EVENT"] = local.Value.evt.ToJson();
                    _step["EVENTS"].AsArray().Add(localEvt);
                }
            }

            _step["END_POINTS"] = new JsonArray();
            JsonObject branch;
            foreach (var b in branches)
            {
                branch = new JsonObject();
                branch["NAME"] = b.name;
                branch["TARGET"] = b.next;
                branch["TRIGGER"] = b.condition.DeepClone();
                if (b.postShader != null)
                    branch["POST_SHADERS"] = b.postShader.ToJson();
                _step["END_POINTS"].AsArray().Add(branch);
                NumOfBranches++;
                AddGlobalEventRefernce(b.condition);
            }

            branch = new JsonObject();
            branch["NAME"] = "#Default#";
            branch["TRIGGER"] = new JsonArray() { CST.ON_STRING };
            _step["END_POINTS"].AsArray().Add(branch);

            StepFootprint = 1;
            UserVariableFootprint = 0;
        }

        private BranchStep_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            foreach(var end in _step["END_POINTS"].AsArray())
            {
                AddGlobalEventRefernce(end["TRIGGER"].AsArray());
                NumOfBranches++;
            }
            StepFootprint = 1;
            UserVariableFootprint = 0;
        }

        public override ProcessStepObject ResolveTarget(uint next, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed;
            chewed = _step.DeepClone().AsObject();
            chewed["ID"] = stepLinkMapping.Span[0];
            chewed["END_POINTS"].AsArray()[^1]["TARGET"] = next;
            stepNameMapping[stepLinkMapping.Span[0]] = Name;

            return new BranchStep_O(Name, chewed, StepFootprint, UserVariableFootprint, NumOfBranches);
        }

        public override JsonObject SaveAsJson(Sequential_S container)
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            node["STEP"] = _step.DeepClone();
            //node["GLOBAL_REF"] = new JsonArray();
            //foreach (var r in GlobalEventReference)
                //node["GLOBAL_REF"].AsArray().Add(r);
            //node["BRANCH"] = NumOfBranches;

            return node;
        }

        public static ProcessStepSource MAKE(JsonObject node, Sequential_S container)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(BranchStep_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(BranchStep_S).FullName}.");

                return new BranchStep_S(node);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore BranchStep_S object from node:\n{node.ToString()}", ex);
            }
        }

        public override string ToString(Sequential_S? container)
        {
            StringBuilder sb = new StringBuilder(typeof(BranchStep_S).FullName);
            sb.Append($"\nName:\n\t{Name}");
            sb.Append("\nSwitch:");
            int i = 0;
            foreach (var end in _step["END_POINTS"].AsArray())
            {
                sb.Append($"\n\tPriority {i}:\n\t\tIf {end["NAME"].GetValue<string>()}:");
                foreach (var line in end["TRIGGER"].AsArray())
                    sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                if (end.AsObject().TryGetPropertyValue("POST_SHADERS", out var post))
                {
                    sb.Append($"\n\t\tThen:");
                    foreach (var s in post.AsArray())
                        sb.Append($"\n\t\t\t{s["OBJECT"].GetValue<string>()} <- {s["VALUE"].GetValue<string>()}");
                }
                i++;
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(null);
        }
    }

    public class BranchStep_O : ProcessStepObject
    {
        public int NumOfBranches { get; private init; } = 1;
        public BranchStep_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint, int numOfBranches) : base(name, step)
        {
            _step = step;
            StepFootprint = stepFootprint;
            UserVariableFootprint = userVariableFootprint;
        }

        public override Step Build(Context context, IReadOnlyDictionary<uint, Event> globals, Sequential_O container)
        {
            try
            {
                uint inlineEvent = 10000;
                var s = new Step(_step, globals, ref inlineEvent);

                return s;
            }
            catch (NaposhtimException e)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_STEP_BUILD_ERROR,
                    $"Can not build BranchStep with the following JSON node:\n{_step.ToString()}", e);
            }
        }
    }
}
