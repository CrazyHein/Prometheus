using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock.Process;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class SimpleStep_S : ProcessStepSource
    {
        public override IEnumerable<ProcessShader> Shaders
        {
            get
            {
                if (_step.TryGetPropertyValue("SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(ProcessStepSource.DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<ProcessShader> PostShaders
        {
            get
            {
                if (_step["END_POINTS"][0].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(ProcessStepSource.DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<KeyValuePair<uint, (string name, Event evt)>> LocalEvents
        {
            get
            {
                if (_step.TryGetPropertyValue("EVENTS", out var locals))
                {
                    return locals.AsArray().Select(x => KeyValuePair.Create(x["ID"].GetValue<uint>(), (ProcessStepSource.DEFAULT_NAME(x["NAME"]), Event.MAKE(x["EVENT"]))));
                }
                else
                    return Enumerable.Empty<KeyValuePair<uint, (string name, Event evt)>>();

            }
        }

        public string TerminationCondition
        {
            get
            {
                return string.Join('\n', _step["END_POINTS"][0]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
            }
        }

        public SimpleStep_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, JsonArray? completionCondition, ProcessShaders? postShaders = null) : base(name)
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

            if (shaders != null)
                _step["SHADERS"] = shaders.ToJson();

            JsonObject branch = new JsonObject();
            if (completionCondition != null && completionCondition.Count > 0)
            {
                branch["TRIGGER"] = completionCondition.DeepClone();
                if (postShaders != null)
                    branch["POST_SHADERS"] = postShaders.ToJson();
            }
            else
            {
                branch["TRIGGER"] = new JsonArray() { CST.ON_STRING };
                if (postShaders != null)
                    branch["POST_SHADERS"] = postShaders.ToJson();
            }
            _step["END_POINTS"] = new JsonArray() { branch };

            StepFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        private SimpleStep_S(JsonObject node): base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            AddGlobalEventRefernce(_step["END_POINTS"][0]["TRIGGER"].AsArray());
            if (_step.ContainsKey("SHADERS"))
                AddShaderUserVariablesUsage(_step["SHADERS"].AsArray());
            if (_step["END_POINTS"][0].AsObject().ContainsKey("POST_SHADERS"))
                AddShaderUserVariablesUsage(_step["END_POINTS"][0]["POST_SHADERS"].AsArray());
            StepFootprint = 1;
        }

        public override ProcessStepObject ResolveTarget(uint next, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed;
            chewed = _step.DeepClone().AsObject();
            chewed["ID"] = stepLinkMapping.Span[0];
            chewed["END_POINTS"][0]["TARGET"] = next;
            stepNameMapping[stepLinkMapping.Span[0]] = Name;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            List<ProcessShader> postShaderObjectDirectAssignments = PostShaderObjectDirectAssignments.ToList();
            List<ProcessShader> shaderObjectDirectAssignments = ShaderObjectDirectAssignments.ToList();
            int pos = container.IndexOf(this);

            var postShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var shaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Reverse();

            if (postShaderObjectDirectAssignments.Count() != 0)
            {
                var tempShaders = PostShaderObjectDirectAssignments.ToList();
                foreach (var assign in postShaderObjectDirectAssignments)
                {
                    if (assign.Shader.Expr.IsImmediateOperand && assign.Shader.Operand is ObjectReference)
                    {
                        var ret = postShaderSearchRange.FirstOrDefault(x => (x.Shader.Operand).Equals(assign.Shader.Operand));
                        if (ret != null && ret.Shader.Expr.Equals(assign.Shader.Expr))
                            tempShaders.Remove(assign);
                    }
                }
                if (tempShaders.Count() != 0)
                    chewed["END_POINTS"][0]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                else
                    chewed["END_POINTS"][0].AsObject().Remove("POST_SHADERS");
            }
            else
                chewed["END_POINTS"][0].AsObject().Remove("POST_SHADERS");

            if (shaderObjectDirectAssignments.Count() != 0)
            {
                var tempShaders = new List<ProcessShader>(shaderObjectDirectAssignments);
                foreach(var assign in shaderObjectDirectAssignments)
                {
                    if (assign.Shader.Expr.IsImmediateOperand && assign.Shader.Operand is ObjectReference)
                    {
                        var ret = shaderSearchRange.FirstOrDefault(x => (x.Shader.Operand).Equals(assign.Shader.Operand));
                        if (ret != null && ret.Shader.Expr.Equals(assign.Shader.Expr))
                            tempShaders.Remove(assign);
                    }
                }
                if (tempShaders.Count() != 0)
                    chewed["SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                else
                    chewed.Remove("SHADERS");
            }
            else
                chewed.Remove("SHADERS");
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new SimpleStep_O(Name, chewed, StepFootprint, UserVariableFootprint);
        }

        public override JsonObject SaveAsJson(Sequential_S container)
        {
            JsonObject node  = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            node["STEP"] = _step.DeepClone();

            return node;
        }

        public static ProcessStepSource MAKE(JsonObject node, Sequential_S container)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(SimpleStep_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(SimpleStep_S).FullName}.");

                return new SimpleStep_S(node);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore SimpleStep_S object from node:\n{node.ToString()}", ex);
            }
        }

        public override string ToString(Sequential_S? container)
        {
            StringBuilder sb = new StringBuilder(typeof(SimpleStep_S).FullName);
            sb.Append($"\nName:\n\t{Name}");
            if (_step.TryGetPropertyValue("EVENTS", out var localEvents))
            {
                sb.Append("\nLocal Events:");
                foreach (var evt in localEvents.AsArray())
                    sb.Append($"\n\t{evt["ID"].GetValue<uint>():D10} {ProcessStepSource.DEFAULT_NAME(evt["NAME"])}: {evt["EVENT"].ToJsonString()}");
            }
            if (_step.TryGetPropertyValue("SHADERS", out var shaders))
            {
                sb.Append("\nActions:");
                foreach (var s in shaders.AsArray())
                    sb.Append($"\n\t{ProcessStepSource.DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
            }
            sb.Append("\nTermination conditions:");
            int i = 0;
            foreach (var end in _step["END_POINTS"].AsArray())
            {
                sb.Append($"\n\tPriority {i}:\n\t\tIf:");
                foreach (var line in end["TRIGGER"].AsArray())
                    sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                if (end.AsObject().TryGetPropertyValue("POST_SHADERS", out var post))
                {
                    sb.Append($"\n\t\tThen:");
                    foreach (var s in post.AsArray())
                        sb.Append($"\n\t\t\t{ProcessStepSource.DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
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

    public class SimpleStep_O : ProcessStepObject
    { 
        public SimpleStep_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint): base(name, step)
        {
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
                    $"Can not build SimpleStep with the following JSON node:\n{_step.ToString()}", e);
            }
        }
    }
}
