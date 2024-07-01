using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock.Process;
using System.ComponentModel;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class SimpleStepWithTimeout_S : ProcessStepSource
    {
        public SimpleStepWithTimeout_S? EmployPreceding { get; set; }
        //public Expression? TimeToTimeout { get; set; }

        private int __timeout;
        public int Timeout 
        {
            get { return __timeout; }
            set
            {
                if (value < 0)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"The timeout period({value}) must be a positive integer.");
                __timeout = value;
            } 
        }

        public override IEnumerable<ProcessShader> Shaders { 
            get 
            { 
                if(_step.TryGetPropertyValue("SHADERS", out var shaders))
                {
                    return _step["SHADERS"].AsArray().Select(x => new ProcessShader(ProcessStepSource.DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<ProcessShader> PostShaders
        {
            get
            {
                if (_step["END_POINTS"][1].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
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
                return string.Join('\n', _step["END_POINTS"][1]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
            }
        }

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, int timeout, JsonArray completionCondition, ProcessShaders? postShaders = null): base(name)
        {
            Timeout = timeout;
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

            JsonObject timeoutBranch = new JsonObject();
            //timeoutBranch["TRIGGER"] = new JsonArray() { (new TIM(timeout)).ToJson().ToJsonString() };

            JsonObject defaultBranch = new JsonObject();
            defaultBranch["TRIGGER"] = completionCondition.DeepClone();
            if (postShaders != null)
                defaultBranch["POST_SHADERS"] = postShaders.ToJson();

            _step["END_POINTS"] = new JsonArray() { timeoutBranch, defaultBranch };

            StepFootprint = 1;
            UserVariableFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, SimpleStepWithTimeout_S timeout, JsonArray completionCondition, ProcessShaders? postShaders = null) : base(name)
        {
            EmployPreceding = timeout;
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

            JsonObject timeoutBranch = new JsonObject();
            //timeoutBranch["TRIGGER"] = new JsonArray();

            JsonObject defaultBranch = new JsonObject();
            defaultBranch["TRIGGER"] = completionCondition.DeepClone();
            if (postShaders != null)
                defaultBranch["POST_SHADERS"] = postShaders.ToJson();
            
            _step["END_POINTS"] = new JsonArray() { timeoutBranch, defaultBranch };

            StepFootprint = 1;
            UserVariableFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        private SimpleStepWithTimeout_S(JsonObject node, Sequential_S container) : base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            AddGlobalEventRefernce(_step["END_POINTS"][1]["TRIGGER"].AsArray());
            if (_step.ContainsKey("SHADERS"))
                AddShaderUserVariablesUsage(_step["SHADERS"].AsArray());
            if (_step["END_POINTS"][1].AsObject().ContainsKey("POST_SHADERS"))
                AddShaderUserVariablesUsage(_step["END_POINTS"][1]["POST_SHADERS"].AsArray());
            StepFootprint = 1;
            UserVariableFootprint = 1;
            if (node.TryGetPropertyValue("TIMEOUT", out var timeout))
                Timeout = timeout.GetValue<int>();
            else if (node.TryGetPropertyValue("TIMEOUT_REF", out var preceding))
            {
                int index = preceding.GetValue<int>();
                ProcessStepSource? step = container.ProcessStepAt(index);
                if (step == null || !(step is SimpleStepWithTimeout_S))
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, "Can not find the referenced SimpleStepWithTimeout_S in the 'Sequential' Control Block.");
                EmployPreceding = step as SimpleStepWithTimeout_S;
            }
            else
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore SimpleStepWithTimeout_S object from node:\n{node.ToString()}");
        }

        public override ProcessStepObject ResolveTarget(uint next, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed;
            chewed = _step.DeepClone().AsObject();

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
                    chewed["END_POINTS"][1]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                else
                    chewed["END_POINTS"][1].AsObject().Remove("POST_SHADERS");
            }
            else
                chewed["END_POINTS"][1].AsObject().Remove("POST_SHADERS");

            if (shaderObjectDirectAssignments.Count() != 0)
            {
                var tempShaders = new List<ProcessShader>(shaderObjectDirectAssignments);
                foreach (var assign in shaderObjectDirectAssignments)
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

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////



            JsonArray postShaders = new JsonArray();
            JsonObject postShader = new JsonObject();
            postShader["OBJECT"] = $"&USER{userVariableMapping.Span[0]}";
            //postShader["VALUE"] = $"{Timeout}-&STDURA";
            if(container.TimeToTimeout.ContainsKey(this) == false)
                container.TimeToTimeout[this] = new Expression(postShader["OBJECT"].GetValue<string>(), null);
            if (chewed["END_POINTS"][1].AsObject().TryGetPropertyValue("POST_SHADERS", out _))
                chewed["END_POINTS"][1]["POST_SHADERS"].AsArray().Add(postShader);
            else
            {
                postShaders.Add(postShader);
                chewed["END_POINTS"][1]["POST_SHADERS"] = postShaders;
            }

            chewed["ID"] = stepLinkMapping.Span[0];
            chewed["END_POINTS"][0]["TARGET"] = next;
            chewed["END_POINTS"][1]["TARGET"] = next;
            stepNameMapping[stepLinkMapping.Span[0]] = Name;

            return new SimpleStepWithTimeout_O(Name, chewed, StepFootprint, UserVariableFootprint, EmployPreceding, Timeout);
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
            if(EmployPreceding == null)
                node["TIMEOUT"] = Timeout;
            else
                node["TIMEOUT_REF"] = container.IndexOf(EmployPreceding);
            return node;
        }

        public static ProcessStepSource MAKE(JsonObject node, Sequential_S container)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(SimpleStepWithTimeout_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(SimpleStepWithTimeout_S).FullName}.");

                return new SimpleStepWithTimeout_S(node, container);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore SimpleStepWithTimeout_S object from node:\n{node.ToString()}", ex);
            }
        }

        private string __time_to_timeout_string(Sequential_S? container = null)
        {
            if (EmployPreceding == null)
            {
                if (container == null)
                    return $"({Timeout} - STEP_DURATION)";
                else
                    return $"({Timeout} - STEP_DURATION[{container.IndexOf(this)}])";
            }
            else
            {
                if (container == null)
                    return $"({EmployPreceding.__time_to_timeout_string(container)} - STEP_DURATION)";
                else
                    return $"({EmployPreceding.__time_to_timeout_string(container)} - STEP_DURATION[{container.IndexOf(this)}])";
            }
        }

        public override string ToString(Sequential_S? container)
        {
            StringBuilder sb = new StringBuilder(typeof(SimpleStepWithTimeout_S).FullName);
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

            sb.Append($"\n\tPriority 0:\n\t\tIf:");
            if (EmployPreceding == null)
                sb.Append($"\n\t\t\t{Timeout} millisecond(s) passed");
            else
                sb.Append($"\n\t\t\t[{EmployPreceding.__time_to_timeout_string(container)}] millisecond(s) passed"); ;
            JsonObject defaultBranch = _step["END_POINTS"][1].AsObject();
            sb.Append($"\n\tPriority 1:\n\t\tIf:");
            foreach (var line in defaultBranch["TRIGGER"].AsArray())
                sb.Append($"\n\t\t\t{line.GetValue<string>()}");
            if (defaultBranch.TryGetPropertyValue("POST_SHADERS", out var post))
            {
                sb.Append($"\n\t\tThen:");
                foreach (var s in post.AsArray())
                    sb.Append($"\n\t\t\t{ProcessStepSource.DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString( null );
        }
    }

    public class SimpleStepWithTimeout_O : ProcessStepObject
    {
        public SimpleStepWithTimeout_S? EmployPreceding { get; private init; }
        public int Timeout { get; private init; }

        public SimpleStepWithTimeout_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint, SimpleStepWithTimeout_S? employPreceding, int timeout) : base(name, step)
        {
            StepFootprint = stepFootprint;
            UserVariableFootprint = userVariableFootprint;
            EmployPreceding = employPreceding;
            Timeout = timeout;
        }

        public override Step Build(Context context, IReadOnlyDictionary<uint, Event> globals, Sequential_O container)
        {
            JsonObject chewed = _step.DeepClone().AsObject(); 
            if (EmployPreceding != null)
            {
                if (container.TimeToTimeout.ContainsKey(EmployPreceding) == false)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Can not get the remaining time EXPRESSION of the specific 'SimpleStepWithTimeout'.");
                else
                {
                    var node = new TIM("TIM", ("TIMEOUT", container.TimeToTimeout[EmployPreceding]));
                    chewed["END_POINTS"][0]["TRIGGER"] = new JsonArray() { node.ToJson().ToJsonString() };
                }
                chewed["END_POINTS"][1]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{container.TimeToTimeout[EmployPreceding]}-&STDURA";
            }
            else
            {
                chewed["END_POINTS"][0]["TRIGGER"] = new JsonArray() { (new TIM(Timeout)).ToJson().ToJsonString() };
                chewed["END_POINTS"][1]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{Timeout}-&STDURA";
            }

            try
            {
                uint inlineEvent = 10000;
                var s = new Step(chewed, globals, ref inlineEvent);
                return s;
            }
            catch (NaposhtimException e)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_STEP_BUILD_ERROR, 
                    $"Can not build SimpleStepWithTimeout with the following JSON node:\n{chewed.ToString()}", e);
            }
        }
    }
}
