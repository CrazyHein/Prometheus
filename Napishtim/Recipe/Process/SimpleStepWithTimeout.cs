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
using System.ComponentModel;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process
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
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"The timeout period({value}) must be a positive integer.");
                __timeout = value;
            }
        }

        public override IEnumerable<ProcessShader> Shaders
        {
            get
            {
                if (_step.TryGetPropertyValue("SHADERS", out var shaders))
                {
                    return _step["SHADERS"].AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
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
                    return shaders.AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<ProcessShader> AbortShaders
        {
            get
            {
                if (_step["END_POINTS"].AsArray().Count == 3 && _step["END_POINTS"][2].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
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
                    return locals.AsArray().Select(x => KeyValuePair.Create(x["ID"].GetValue<uint>(), (DEFAULT_NAME(x["NAME"]), Event.MAKE(x["EVENT"]))));
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

        public string AbortCondition
        {
            get
            {
                if (_step["END_POINTS"].AsArray().Count == 3)
                    return string.Join('\n', _step["END_POINTS"][2]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
                else
                    return string.Empty;
            }
        }

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, int timeout, JsonArray completionCondition, ProcessShaders? postShaders = null, JsonArray? abortCondition = null, ProcessShaders? abortShaders = null) : base(name)
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

            if (abortCondition != null && abortCondition.Count > 0)
            {
                JsonObject abortBranch = new JsonObject();
                abortBranch["TRIGGER"] = abortCondition.DeepClone();
                if (abortShaders != null)
                    abortBranch["POST_SHADERS"] = abortShaders.ToJson();
                _step["END_POINTS"].AsArray().Add(abortBranch);
            }

            StepFootprint = 1;
            UserVariableFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            AddGlobalEventRefernce(abortCondition);
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (abortShaders != null)
                AddShaderUserVariablesUsage(abortShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, SimpleStepWithTimeout_S timeout, JsonArray completionCondition, ProcessShaders? postShaders = null, JsonArray? abortCondition = null, ProcessShaders? abortShaders = null) : base(name)
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

            if (abortCondition != null && abortCondition.Count > 0)
            {
                JsonObject abortBranch = new JsonObject();
                abortBranch["TRIGGER"] = abortCondition.DeepClone();
                if (abortShaders != null)
                    abortBranch["POST_SHADERS"] = abortShaders.ToJson();
                _step["END_POINTS"].AsArray().Add(abortBranch);
            }

            StepFootprint = 1;
            UserVariableFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            AddGlobalEventRefernce(abortCondition);
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (abortShaders != null)
                AddShaderUserVariablesUsage(abortShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        private SimpleStepWithTimeout_S(JsonObject node, Sequential_S container) : base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            AddGlobalEventRefernce(_step["END_POINTS"][1]["TRIGGER"].AsArray());
            if (_step.ContainsKey("SHADERS"))
                AddShaderUserVariablesUsage(_step["SHADERS"].AsArray());
            if (_step["END_POINTS"][1].AsObject().ContainsKey("POST_SHADERS"))
                AddShaderUserVariablesUsage(_step["END_POINTS"][1]["POST_SHADERS"].AsArray());

            if (_step["END_POINTS"].AsArray().Count == 3)
            {
                AddGlobalEventRefernce(_step["END_POINTS"][2]["TRIGGER"].AsArray());
                if (_step["END_POINTS"][2].AsObject().ContainsKey("POST_SHADERS"))
                    AddShaderUserVariablesUsage(_step["END_POINTS"][2]["POST_SHADERS"].AsArray());
            }

            StepFootprint = 1;
            UserVariableFootprint = 1;
            if (node.TryGetPropertyValue("TIMEOUT", out var timeout))
                Timeout = timeout.GetValue<int>();
            else if (node.TryGetPropertyValue("TIMEOUT_REF", out var preceding))
            {
                int index = preceding.GetValue<int>();
                ProcessStepSource? step = container.ProcessStepAt(index);
                if (step == null || !(step is SimpleStepWithTimeout_S))
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, "Can not find the referenced SimpleStepWithTimeout_S in the 'Sequential' Control Block.");
                EmployPreceding = step as SimpleStepWithTimeout_S;
            }
            else
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Can not restore SimpleStepWithTimeout_S object from node:\n{node.ToString()}");
        }

        public override ProcessStepObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed;
            chewed = _step.DeepClone().AsObject();

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            int pos = container.IndexOf(this);

            var abortShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var postShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var shaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Reverse();

            IEnumerable<ProcessShader>? tempShaders;

            if (AbortShaders.Count() != 0)
            {
                tempShaders = AbortShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (AbortShaders.TakeLast(AbortShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(abortShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));

                chewed["END_POINTS"][2]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][2]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][2].AsObject().Remove("POST_SHADERS");
            }

            if (PostShaders.Count() != 0)
            {
                tempShaders = PostShaders.Where(
                (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                (PostShaders.TakeLast(PostShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                !(postShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));

                chewed["END_POINTS"][1]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][1]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][1].AsObject().Remove("POST_SHADERS");
            }

            if (Shaders.Count() != 0)
            {
                tempShaders = Shaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (Shaders.TakeLast(Shaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(shaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));

                chewed["SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["SHADERS"] as JsonArray).Count == 0)
                    chewed.Remove("SHADERS");
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////



            JsonArray postShaders = new JsonArray();
            JsonObject postShader = new JsonObject();
            postShader["OBJECT"] = $"&USER{userVariableMapping.Span[0]}";
            //postShader["VALUE"] = $"{Timeout}-&STDURA";
            if (container.TimeToTimeout.ContainsKey(this) == false)
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
            if (chewed["END_POINTS"].AsArray().Count == 3)
                chewed["END_POINTS"][2]["TARGET"] = abort;
            stepNameMapping[stepLinkMapping.Span[0]] = String.Join('/', container.FullName, Name);

            return new SimpleStepWithTimeout_O(Name, chewed, StepFootprint, UserVariableFootprint, EmployPreceding, Timeout);
        }

        public override JsonObject SaveAsJson(Sequential_S container)
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = GetType().FullName;
            node["NAME"] = Name;
            node["STEP"] = _step.DeepClone();
            //node["GLOBAL_REF"] = new JsonArray();
            //foreach (var r in GlobalEventReference)
            //node["GLOBAL_REF"].AsArray().Add(r);
            if (EmployPreceding == null)
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
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(SimpleStepWithTimeout_S).FullName}.");

                return new SimpleStepWithTimeout_S(node, container);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Can not restore SimpleStepWithTimeout_S object from node:\n{node.ToString()}", ex);
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
                    sb.Append($"\n\t{evt["ID"].GetValue<uint>():D10} {DEFAULT_NAME(evt["NAME"])}: {evt["EVENT"].ToJsonString()}");
            }
            if (_step.TryGetPropertyValue("SHADERS", out var shaders))
            {
                sb.Append("\nActions:");
                foreach (var s in shaders.AsArray())
                    sb.Append($"\n\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
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
                    sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
            }

            if (_step["END_POINTS"].AsArray().Count == 3)
            {
                sb.Append("\nAbort conditions:");
                JsonObject abortBranch = _step["END_POINTS"][2].AsObject();
                sb.Append($"\n\tPriority 0:\n\t\tIf:");

                foreach (var line in abortBranch["TRIGGER"].AsArray())
                    sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                if (abortBranch.TryGetPropertyValue("POST_SHADERS", out var abort))
                {
                    sb.Append($"\n\t\tThen:");
                    foreach (var s in abort.AsArray())
                        sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
                }
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(null);
        }
    }

    public class SimpleStepWithTimeout_O : ProcessStepObject
    {
        public SimpleStepWithTimeout_S? EmployPreceding { get; private init; }
        public int Timeout { get; private init; }

        internal SimpleStepWithTimeout_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint, SimpleStepWithTimeout_S? employPreceding, int timeout) : base(name, step)
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
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_INVALID_OPERATION, "Can not get the remaining time EXPRESSION of the specific 'SimpleStepWithTimeout'.");
                else
                {
                    var node = new TIM("TIM", ("TIMEOUT", container.TimeToTimeout[EmployPreceding]));
                    chewed["END_POINTS"][0]["TRIGGER"] = new JsonArray() { node.ToJson().ToJsonString() };
                }
                chewed["END_POINTS"][1]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{container.TimeToTimeout[EmployPreceding]}-&STDURA";
            }
            else
            {
                chewed["END_POINTS"][0]["TRIGGER"] = new JsonArray() { new TIM(Timeout).ToJson().ToJsonString() };
                chewed["END_POINTS"][1]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{Timeout}-&STDURA";
            }

            uint inlineEvent = 10000;
            var s = new Step(chewed, globals, ref inlineEvent);
            return s;
        }
    }
}
