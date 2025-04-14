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
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System.Net;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process
{
    public class SimpleStep_S : ProcessStepSource
    {
        public override IEnumerable<ProcessShader> Shaders
        {
            get
            {
                if (_step.TryGetPropertyValue("SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
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
                if (_step.ContainsKey("ABORT_POINT") && _step["ABORT_POINT"].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<ProcessShader> BreakShaders
        {
            get
            {
                if(_step.ContainsKey("BREAK_POINT") && _step["BREAK_POINT"].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
                {
                    return shaders.AsArray().Select(x => new ProcessShader(DEFAULT_NAME(x["NAME"]), new Shader(x["OBJECT"].GetValue<string>(), x["VALUE"].GetValue<string>())));
                }
                else
                    return Enumerable.Empty<ProcessShader>();
            }
        }

        public override IEnumerable<ProcessShader> ContinueShaders
        {
            get
            {
                if (_step.ContainsKey("CONTINUE_POINT") && _step["CONTINUE_POINT"].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
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

        public string CompletionCondition
        {
            get
            {
                return string.Join('\n', _step["END_POINTS"][0]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
            }
        }

        public string AbortCondition
        {
            get
            {
                if (_step.ContainsKey("ABORT_POINT"))
                    return string.Join('\n', _step["ABORT_POINT"]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
                else
                    return string.Empty;
            }
        }

        public string BreakCondition
        {
            get
            {
                if (_step.ContainsKey("BREAK_POINT"))
                    return string.Join('\n', _step["BREAK_POINT"]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
                else
                    return string.Empty;
            }
        }

        public string ContinueCondition
        {
            get
            {
                if (_step.ContainsKey("CONTINUE_POINT"))
                    return string.Join('\n', _step["CONTINUE_POINT"]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
                else
                    return string.Empty;
            }
        }

        private List<(byte, string)> __branch_priorities = new List<(byte, string)>();
        public byte CompletionPriority { get; init; }
        public byte AbortPriority { get; init; }
        public byte BreakPriority { get; init; }
        public byte ContinuePriority { get; init; }

        public SimpleStep_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, 
            JsonArray? completionCondition, ProcessShaders? postShaders = null, byte priorityCompletion = 0,
            JsonArray? abortCondition = null, ProcessShaders? abortShaders = null, byte priorityAbort = 0,
            JsonArray? breakCondition = null, ProcessShaders? breakShaders = null, byte priorityBreak = 0,
            JsonArray? continueCondition = null, ProcessShaders? continueShaders = null, byte priorityContinue = 0) : base(name)
        {
            CompletionPriority = priorityCompletion;
            AbortPriority = priorityAbort;
            BreakPriority = priorityBreak;
            ContinuePriority = priorityContinue;

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
            branch["PRIORITY"] = priorityCompletion;

            _step["END_POINTS"] = new JsonArray() { branch };
            __branch_priorities.Add((priorityCompletion, "COMPLETION_POINT"));

            if (abortCondition != null && abortCondition.Count > 0)
            {
                JsonObject abort = new JsonObject();
                abort["TRIGGER"] = abortCondition.DeepClone();
                if (abortShaders != null)
                    abort["POST_SHADERS"] = abortShaders.ToJson();
                abort["PRIORITY"] = priorityAbort;
                _step["ABORT_POINT"] = abort;
                __branch_priorities.Add((priorityAbort, "ABORT_POINT"));
            }
            else if (abortShaders?.Shaders.Count() > 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"You must define <Abort Condition> to enable <Abort Shaders>.");

            if (breakCondition != null && breakCondition.Count > 0)
            {
                JsonObject breakPoint = new JsonObject();
                breakPoint["TRIGGER"] = breakCondition.DeepClone();
                if (breakShaders != null)
                    breakPoint["POST_SHADERS"] = breakShaders.ToJson();
                breakPoint["PRIORITY"] = priorityBreak;
                _step["BREAK_POINT"] = breakPoint;
                __branch_priorities.Add((priorityBreak, "BREAK_POINT"));
            }
            else if (breakShaders?.Shaders.Count() > 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"You must define <Break Condition> to enable <Break Shaders>.");

            if (continueCondition != null && continueCondition.Count > 0)
            {
                JsonObject continuePoint = new JsonObject();
                continuePoint["TRIGGER"] = continueCondition.DeepClone();
                if (continueShaders != null)
                    continuePoint["POST_SHADERS"] = continueShaders.ToJson();
                continuePoint["PRIORITY"] = priorityContinue;
                _step["CONTINUE_POINT"] = continuePoint;
                __branch_priorities.Add((priorityContinue, "CONTINUE_POINT"));
            }
            else if (continueShaders?.Shaders.Count() > 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"You must define <Continue Condition> to enable <Continue Shaders>.");
 
            __branch_priorities.Sort(delegate (ValueTuple<byte, string> x, ValueTuple<byte, string> y)
            {
                return x.Item1.CompareTo(y.Item1);
            });

            StepFootprint = 1;
            AddGlobalEventRefernce(completionCondition);
            AddGlobalEventRefernce(abortCondition);
            AddGlobalEventRefernce(breakCondition);
            AddGlobalEventRefernce(continueCondition);      
            if (shaders != null)
                AddShaderUserVariablesUsage(shaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (postShaders != null)
                AddShaderUserVariablesUsage(postShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (abortShaders != null)
                AddShaderUserVariablesUsage(abortShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (breakShaders != null)
                AddShaderUserVariablesUsage(breakShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
            if (continueShaders != null)
                AddShaderUserVariablesUsage(continueShaders.Shaders.SelectMany(x => x.Shader.UserVariablesUsage));
        }

        private SimpleStep_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            AddGlobalEventRefernce(_step["END_POINTS"][0]["TRIGGER"].AsArray());

            if (_step.ContainsKey("SHADERS"))
                AddShaderUserVariablesUsage(_step["SHADERS"].AsArray());
            if (_step["END_POINTS"][0].AsObject().ContainsKey("POST_SHADERS"))
                AddShaderUserVariablesUsage(_step["END_POINTS"][0]["POST_SHADERS"].AsArray());
            if (_step["END_POINTS"][0].AsObject().ContainsKey("PRIORITY"))
            {
                __branch_priorities.Add((_step["END_POINTS"][0]["PRIORITY"].GetValue<byte>(), "COMPLETION_POINT"));
                CompletionPriority = _step["END_POINTS"][0]["PRIORITY"].GetValue<byte>();
            }
            else
            {
                __branch_priorities.Add((0, "COMPLETION_POINT"));
                CompletionPriority = 0;
            }

            if (_step["END_POINTS"].AsArray().Count == 2)
            {
                _step["ABORT_POINT"] = _step["END_POINTS"][1].DeepClone();
                _step["END_POINTS"].AsArray().RemoveAt(1);
            }

            if (_step.ContainsKey("ABORT_POINT"))
            {
                AddGlobalEventRefernce(_step["ABORT_POINT"]["TRIGGER"].AsArray());
                if (_step["ABORT_POINT"].AsObject().ContainsKey("POST_SHADERS"))
                    AddShaderUserVariablesUsage(_step["ABORT_POINT"]["POST_SHADERS"].AsArray());
                if (_step["ABORT_POINT"].AsObject().ContainsKey("PRIORITY"))
                {
                    __branch_priorities.Add((_step["ABORT_POINT"]["PRIORITY"].GetValue<byte>(), "ABORT_POINT"));
                    AbortPriority = _step["ABORT_POINT"]["PRIORITY"].GetValue<byte>();
                }
                else
                {
                    __branch_priorities.Add((0, "ABORT_POINT"));
                    AbortPriority = 0;
                }
            }

            if (_step.ContainsKey("BREAK_POINT"))
            {
                AddGlobalEventRefernce(_step["BREAK_POINT"]["TRIGGER"].AsArray());
                if (_step["BREAK_POINT"].AsObject().ContainsKey("POST_SHADERS"))
                    AddShaderUserVariablesUsage(_step["BREAK_POINT"]["POST_SHADERS"].AsArray());
                if (_step["BREAK_POINT"].AsObject().ContainsKey("PRIORITY"))
                {
                    __branch_priorities.Add((_step["BREAK_POINT"]["PRIORITY"].GetValue<byte>(), "BREAK_POINT"));
                    BreakPriority = _step["BREAK_POINT"]["PRIORITY"].GetValue<byte>();
                }
                else
                {
                    __branch_priorities.Add((0, "BREAK_POINT"));
                    BreakPriority = 0;
                }
            }
            if(_step.ContainsKey("CONTINUE_POINT"))
            {
                AddGlobalEventRefernce(_step["CONTINUE_POINT"]["TRIGGER"].AsArray());
                if (_step["CONTINUE_POINT"].AsObject().ContainsKey("POST_SHADERS"))
                    AddShaderUserVariablesUsage(_step["CONTINUE_POINT"]["POST_SHADERS"].AsArray());
                if (_step["CONTINUE_POINT"].AsObject().ContainsKey("PRIORITY"))
                {
                    __branch_priorities.Add((_step["CONTINUE_POINT"]["PRIORITY"].GetValue<byte>(), "CONTINUE_POINT"));
                    ContinuePriority = _step["CONTINUE_POINT"]["PRIORITY"].GetValue<byte>();
                }
                else
                {
                    __branch_priorities.Add((0, "CONTINUE_POINT"));
                    ContinuePriority = 0;
                }
            }

            __branch_priorities.Sort(delegate (ValueTuple<byte, string> x, ValueTuple<byte, string> y)
            {
                return x.Item1.CompareTo(y.Item1);
            });

            StepFootprint = 1;
        }

        public override ProcessStepObject ResolveTarget(uint next, uint abort, uint? breakp, uint? continuep, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed = new JsonObject();
            //chewed = _step.DeepClone().AsObject();
            if(_step.ContainsKey("EVENTS"))
                chewed["EVENTS"] = _step["EVENTS"].DeepClone();
            if (_step.ContainsKey("SHADERS"))
                chewed["SHADERS"] = _step["SHADERS"].DeepClone();
            chewed["ID"] = stepLinkMapping.Span[0];

            int completionIdx = -1, abortIdx = -1, breakIdx = -1, continueIdx = -1;
            JsonNode branch;
            chewed["END_POINTS"] = new JsonArray();
            foreach(var p in __branch_priorities)
            {
                switch (p.Item2)
                {
                    case "COMPLETION_POINT":
                        branch = _step["END_POINTS"][0].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = next;
                        chewed["END_POINTS"].AsArray().Add(branch);
                        completionIdx = chewed["END_POINTS"].AsArray().Count - 1;
                        break;
                    case "ABORT_POINT":
                        branch = _step["ABORT_POINT"].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = abort;
                        chewed["END_POINTS"].AsArray().Add(branch);
                        abortIdx = chewed["END_POINTS"].AsArray().Count - 1; 
                        break;
                    case "BREAK_POINT":
                        if (breakp == null)
                            throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Must provide break branch target to resolve ProcessStepSource with BREAK_POINT.");
                        branch = _step["BREAK_POINT"].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = breakp;
                        chewed["END_POINTS"].AsArray().Add(branch);
                        breakIdx = chewed["END_POINTS"].AsArray().Count - 1;
                        break;
                    case "CONTINUE_POINT":
                        if (continuep == null)
                            throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Must provide continue branch target to resolve ProcessStepSource with CONTINUE_POINT.");
                        branch = _step["CONTINUE_POINT"].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = continuep;
                        chewed["END_POINTS"].AsArray().Add(branch);
                        continueIdx = chewed["END_POINTS"].AsArray().Count - 1;
                        break;
                }

            }

            stepNameMapping[stepLinkMapping.Span[0]] = String.Join('/', container.FullName, Name);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            int pos = container.IndexOf(this);

            var abortShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var breaskShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var continueShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var postShaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Concat(ShaderObjectDirectAssignments).Reverse();
            var shaderSearchRange = container.OriginalProcessSteps.Take(pos).SelectMany(x => x.ShaderObjectDirectAssignments.Concat(x.PostShaderObjectDirectAssignments)).Reverse();

            IEnumerable<ProcessShader>? tempShaders;
            if (AbortShaders.Count() != 0)
            {
                tempShaders = AbortShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (AbortShaders.TakeLast(AbortShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(abortShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));


                chewed["END_POINTS"][abortIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][abortIdx]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][abortIdx].AsObject().Remove("POST_SHADERS");
            }

            if(BreakShaders.Count() != 0)
            {
                tempShaders = BreakShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (BreakShaders.TakeLast(BreakShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(breaskShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));
                chewed["END_POINTS"][breakIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][breakIdx]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][breakIdx].AsObject().Remove("POST_SHADERS");
            }

            if(ContinueShaders.Count() != 0 )
            {
                tempShaders = ContinueShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (ContinueShaders.TakeLast(ContinueShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(continueShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));
                chewed["END_POINTS"][continueIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][continueIdx]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][continueIdx].AsObject().Remove("POST_SHADERS");
            }

            if (PostShaders.Count() != 0)
            {
                tempShaders = PostShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (PostShaders.TakeLast(PostShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(postShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));

                chewed["END_POINTS"][completionIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
                if ((chewed["END_POINTS"][completionIdx]["POST_SHADERS"] as JsonArray).Count == 0)
                    chewed["END_POINTS"][completionIdx].AsObject().Remove("POST_SHADERS");
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

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new SimpleStep_O(Name, chewed, StepFootprint, UserVariableFootprint);
        }

        public override JsonObject SaveAsJson(Sequential_S container)
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = GetType().FullName;
            node["NAME"] = Name;
            node["STEP"] = _step.DeepClone();

            return node;
        }

        public static ProcessStepSource MAKE(JsonObject node, Sequential_S container)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(SimpleStep_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(SimpleStep_S).FullName}.");

                return new SimpleStep_S(node);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Can not restore SimpleStep_S object from node:\n{node.ToString()}", ex);
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
                    sb.Append($"\n\t{evt["ID"].GetValue<uint>():D10} {DEFAULT_NAME(evt["NAME"])}: {evt["EVENT"].ToJsonString()}");
            }
            if (_step.TryGetPropertyValue("SHADERS", out var shaders))
            {
                sb.Append("\nActions:");
                foreach (var s in shaders.AsArray())
                    sb.Append($"\n\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
            }

            foreach(var p in __branch_priorities)
            {
                switch(p.Item2)
                {
                    case "COMPLETION_POINT":
                        sb.Append("\nCompletion conditions:");
                        JsonObject defaultBranch = _step["END_POINTS"][0].AsObject();
                        sb.Append($"\n\tPriority {p.Item1}.0:\n\t\tIf:");
                        foreach (var line in defaultBranch["TRIGGER"].AsArray())
                            sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                        if (defaultBranch.AsObject().TryGetPropertyValue("POST_SHADERS", out var post))
                        {
                            sb.Append($"\n\t\tThen:");
                            foreach (var s in post.AsArray())
                                sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
                        }
                        break;
                    case "ABORT_POINT":
                        sb.Append("\nAbort conditions:");
                        JsonObject abortBranch = _step["ABORT_POINT"].AsObject();
                        sb.Append($"\n\tPriority {p.Item1}.0:\n\t\tIf:");
                        foreach (var line in abortBranch["TRIGGER"].AsArray())
                            sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                        if (abortBranch.AsObject().TryGetPropertyValue("POST_SHADERS", out var abort))
                        {
                            sb.Append($"\n\t\tThen:");
                            foreach (var s in abort.AsArray())
                                sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
                        }
                        break;
                    case "BREAK_POINT":
                        sb.Append("\nBreak conditions:");
                        JsonObject breakBranch = _step["BREAK_POINT"].AsObject();
                        sb.Append($"\n\tPriority {p.Item1}.0:\n\t\tIf:");
                        foreach (var line in breakBranch["TRIGGER"].AsArray())
                            sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                        if (breakBranch.AsObject().TryGetPropertyValue("POST_SHADERS", out var breakp))
                        {
                            sb.Append($"\n\t\tThen:");
                            foreach (var s in breakp.AsArray())
                                sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
                        }
                        break;
                    case "CONTINUE_POINT":
                        sb.Append("\nContinue conditions:");
                        JsonObject continueBranch = _step["CONTINUE_POINT"].AsObject();
                        sb.Append($"\n\tPriority {p.Item1}.0:\n\t\tIf:");
                        foreach (var line in continueBranch["TRIGGER"].AsArray())
                            sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                        if (continueBranch.AsObject().TryGetPropertyValue("POST_SHADERS", out var continuep))
                        {
                            sb.Append($"\n\t\tThen:");
                            foreach (var s in continuep.AsArray())
                                sb.Append($"\n\t\t\t{DEFAULT_NAME(s["NAME"])}: {s["OBJECT"].GetValue<string>()} := {s["VALUE"].GetValue<string>()}");
                        }
                        break;
                }
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
        internal SimpleStep_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint) : base(name, step)
        {
            StepFootprint = stepFootprint;
            UserVariableFootprint = userVariableFootprint;
        }

        public override Step Build(Context context, IReadOnlyDictionary<uint, Event> globals, Sequential_O container)
        {
            uint inlineEvent = 10000;
            var s = new Step(_step, globals, ref inlineEvent);
            return s;
        }
    }
}
