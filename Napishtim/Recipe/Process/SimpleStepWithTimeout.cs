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
                if (_step.ContainsKey("BREAK_POINT") && _step["BREAK_POINT"].AsObject().TryGetPropertyValue("POST_SHADERS", out var shaders))
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
                return string.Join('\n', _step["END_POINTS"][1]["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
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

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, int timeout, 
            JsonArray completionCondition, ProcessShaders? postShaders = null, byte priorityCompletion = 0,
            JsonArray? abortCondition = null, ProcessShaders? abortShaders = null, byte priorityAbort = 0,
            JsonArray? breakCondition = null, ProcessShaders? breakShaders = null, byte priorityBreak = 0,
            JsonArray? continueCondition = null, ProcessShaders? continueShaders = null, byte priorityContinue = 0) : base(name)
        {
            CompletionPriority = priorityCompletion;
            AbortPriority = priorityAbort;
            BreakPriority = priorityBreak;
            ContinuePriority = priorityContinue;

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
            if (completionCondition == null || completionCondition.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"You must define <Completion Condition>.");
            JsonObject defaultBranch = new JsonObject();
            defaultBranch["TRIGGER"] = completionCondition.DeepClone();
            if (postShaders != null)
                defaultBranch["POST_SHADERS"] = postShaders.ToJson();

            defaultBranch["PRIORITY"] = priorityCompletion;
            _step["END_POINTS"] = new JsonArray() { timeoutBranch, defaultBranch };
            __branch_priorities.Add((priorityCompletion, "COMPLETION_POINT"));

            if (abortCondition != null && abortCondition.Count > 0)
            {
                JsonObject abortBranch = new JsonObject();
                abortBranch["TRIGGER"] = abortCondition.DeepClone();
                if (abortShaders != null)
                    abortBranch["POST_SHADERS"] = abortShaders.ToJson();
                abortBranch["PRIORITY"] = priorityAbort;
                _step["ABORT_POINT"] = abortBranch;
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
            UserVariableFootprint = 1;
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

        public SimpleStepWithTimeout_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, ProcessShaders? shaders, SimpleStepWithTimeout_S timeout, 
            JsonArray completionCondition, ProcessShaders? postShaders = null, byte priorityCompletion = 0,
            JsonArray? abortCondition = null, ProcessShaders? abortShaders = null, byte priorityAbort = 0,
            JsonArray? breakCondition = null, ProcessShaders? breakShaders = null, byte priorityBreak = 0,
            JsonArray? continueCondition = null, ProcessShaders? continueShaders = null, byte priorityContinue = 0) : base(name)
        {
            CompletionPriority = priorityCompletion;
            AbortPriority = priorityAbort;
            BreakPriority = priorityBreak;
            ContinuePriority = priorityContinue;

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
            if (completionCondition == null || completionCondition.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"You must define <Completion Condition>.");
            JsonObject defaultBranch = new JsonObject();
            defaultBranch["TRIGGER"] = completionCondition.DeepClone();
            if (postShaders != null)
                defaultBranch["POST_SHADERS"] = postShaders.ToJson();

            defaultBranch["PRIORITY"] = priorityCompletion;
            _step["END_POINTS"] = new JsonArray() { timeoutBranch, defaultBranch };
            __branch_priorities.Add((priorityCompletion, "COMPLETION_POINT"));

            if (abortCondition != null && abortCondition.Count > 0)
            {
                JsonObject abortBranch = new JsonObject();
                abortBranch["TRIGGER"] = abortCondition.DeepClone();
                if (abortShaders != null)
                    abortBranch["POST_SHADERS"] = abortShaders.ToJson();
                abortBranch["PRIORITY"] = priorityAbort;
                _step["ABORT_POINT"] = abortBranch;
                __branch_priorities.Add((priorityAbort, "ABORT_POINT"));
            }
            else if(abortShaders?.Shaders.Count() > 0)
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
            UserVariableFootprint = 1;
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

        private SimpleStepWithTimeout_S(JsonObject node, Sequential_S container) : base(node["NAME"].GetValue<string>())
        {
            _step = node["STEP"].DeepClone().AsObject();
            AddGlobalEventRefernce(_step["END_POINTS"][1]["TRIGGER"].AsArray());

            if (_step.ContainsKey("SHADERS"))
                AddShaderUserVariablesUsage(_step["SHADERS"].AsArray());
            if (_step["END_POINTS"][1].AsObject().ContainsKey("POST_SHADERS"))
                AddShaderUserVariablesUsage(_step["END_POINTS"][1]["POST_SHADERS"].AsArray());
            if (_step["END_POINTS"][1].AsObject().ContainsKey("PRIORITY"))
            {
                __branch_priorities.Add((_step["END_POINTS"][1]["PRIORITY"].GetValue<byte>(), "COMPLETION_POINT"));
                CompletionPriority = _step["END_POINTS"][1]["PRIORITY"].GetValue<byte>();
            }
            else
            {
                __branch_priorities.Add((0, "COMPLETION_POINT"));
                CompletionPriority = 0;
            }

            if (_step["END_POINTS"].AsArray().Count == 3)
            {
                _step["ABORT_POINT"] = _step["END_POINTS"][2].DeepClone();
                _step["END_POINTS"].AsArray().RemoveAt(2);
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
            if (_step.ContainsKey("CONTINUE_POINT"))
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

        public override ProcessStepObject ResolveTarget(uint next, uint abort, uint? breakp, uint? continuep, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping)
        {
            JsonObject chewed = new JsonObject();
            //chewed = _step.DeepClone().AsObject();
            if (_step.ContainsKey("EVENTS"))
                chewed["EVENTS"] = _step["EVENTS"].DeepClone();
            if (_step.ContainsKey("SHADERS"))
                chewed["SHADERS"] = _step["SHADERS"].DeepClone();
            chewed["ID"] = stepLinkMapping.Span[0];

            int completionIdx = -1, abortIdx = -1, breakIdx = -1, continueIdx = -1;
            JsonNode branch;
            chewed["END_POINTS"] = new JsonArray();
            for (int p = 0; p < __branch_priorities.Count; ++p)
            {
                switch (__branch_priorities[p].Item2)
                {
                    case "COMPLETION_POINT":
                        if (container.TimeToTimeout.ContainsKey(this) == false)
                            container.TimeToTimeout[this] = new Expression($"&USER{userVariableMapping.Span[0]}", null);

                        branch = new JsonObject();
                        branch["POST_SHADERS"] = new JsonArray() 
                        { 
                            new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" },{ "VALUE", $"0" } } 
                        };
                        branch["TARGET"] = next;
                        chewed["END_POINTS"].AsArray().Add(branch);

                        branch = _step["END_POINTS"][1].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = next;
                        if (branch.AsObject().ContainsKey("POST_SHADERS"))
                            branch["POST_SHADERS"].AsArray().Add(new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" } , { "VALUE", $"0" } });
                        else
                            branch["POST_SHADERS"] = new JsonArray()
                            {
                                new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" },{ "VALUE", $"0" } }
                            };
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
                        if (branch.AsObject().ContainsKey("POST_SHADERS"))
                            branch["POST_SHADERS"].AsArray().Add(new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" } , { "VALUE", $"0" }});
                        else
                            branch["POST_SHADERS"] = new JsonArray(){new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" }, { "VALUE", $"0" }}};
                        chewed["END_POINTS"].AsArray().Add(branch);
                        breakIdx = chewed["END_POINTS"].AsArray().Count - 1;
                        break;
                    case "CONTINUE_POINT":
                        if (continuep == null)
                            throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Must provide continue branch target to resolve ProcessStepSource with CONTINUE_POINT.");
                        branch = _step["CONTINUE_POINT"].DeepClone();
                        branch.AsObject().Remove("PRIORITY");
                        branch["TARGET"] = continuep;
                        if (branch.AsObject().ContainsKey("POST_SHADERS"))
                            branch["POST_SHADERS"].AsArray().Add(new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}"}, { "VALUE", $"0" }});
                        else
                            branch["POST_SHADERS"] = new JsonArray(){new JsonObject { { "OBJECT", $"&USER{userVariableMapping.Span[0]}" } ,{"VALUE",$"0"}}};
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

            if (BreakShaders.Count() != 0)
            {
                tempShaders = BreakShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (BreakShaders.TakeLast(BreakShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(breaskShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));
                chewed["END_POINTS"][breakIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
            }

            if (ContinueShaders.Count() != 0)
            {
                tempShaders = ContinueShaders.Where(
                    (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                    (ContinueShaders.TakeLast(ContinueShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                    !(continueShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));
                chewed["END_POINTS"][continueIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
            }

            if (PostShaders.Count() != 0)
            {
                tempShaders = PostShaders.Where(
                (x, p) => !(x.Shader.Operand is ObjectReference) || x.Shader.Expr.IsImmediateOperand == false ||
                (PostShaders.TakeLast(PostShaders.Count() - p - 1).FirstOrDefault(y => x.Shader.Operand.Equals(y.Shader.Operand)) == null &&
                !(postShaderSearchRange.FirstOrDefault(z => x.Shader.Operand.Equals(z.Shader.Operand))?.Shader.Expr.Equals(x.Shader.Expr) == true)));

                chewed["END_POINTS"][completionIdx]["POST_SHADERS"] = new JsonArray(tempShaders.Select(x => x.ToJson()).ToArray());
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

            return new SimpleStepWithTimeout_O(Name, chewed, StepFootprint, UserVariableFootprint, EmployPreceding, Timeout,  completionIdx, breakIdx, continueIdx);
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

            foreach (var p in __branch_priorities)
            {
                switch (p.Item2)
                {
                    case "COMPLETION_POINT":
                        sb.Append("\nCompletion conditions:");
                        sb.Append($"\n\tPriority {p.Item1}.0:\n\t\tIf:");
                        if (EmployPreceding == null)
                            sb.Append($"\n\t\t\t{Timeout} millisecond(s) passed");
                        else
                            sb.Append($"\n\t\t\t[{EmployPreceding.__time_to_timeout_string(container)}] millisecond(s) passed"); ;
                        JsonObject defaultBranch = _step["END_POINTS"][1].AsObject();
                        sb.Append($"\n\tPriority {p.Item1}.1:\n\t\tIf:");
                        foreach (var line in defaultBranch["TRIGGER"].AsArray())
                            sb.Append($"\n\t\t\t{line.GetValue<string>()}");
                        if (defaultBranch.TryGetPropertyValue("POST_SHADERS", out var post))
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
                        if (abortBranch.TryGetPropertyValue("POST_SHADERS", out var abort))
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
                        sb.Append($"\n\tPriority  {p.Item1}.0:\n\t\tIf:");
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

    public class SimpleStepWithTimeout_O : ProcessStepObject
    {
        public SimpleStepWithTimeout_S? EmployPreceding { get; private init; }
        public int Timeout { get; private init; }
        public int CompletionIdx { get; private init; }
        public int BreakIdx { get; private init; }
        public int ContinueIdx { get; private init; }

        internal SimpleStepWithTimeout_O(string name, JsonObject step, int stepFootprint, int userVariableFootprint, SimpleStepWithTimeout_S? employPreceding, int timeout, int completionIdx, int breakIdx, int continueIdx) : base(name, step)
        {
            StepFootprint = stepFootprint;
            UserVariableFootprint = userVariableFootprint;
            EmployPreceding = employPreceding;
            Timeout = timeout;
            CompletionIdx = completionIdx;
            BreakIdx = breakIdx;
            ContinueIdx = continueIdx;
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
                    chewed["END_POINTS"][CompletionIdx - 1]["TRIGGER"] = new JsonArray() { node.ToJson().ToJsonString() };
                }
                chewed["END_POINTS"][CompletionIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{container.TimeToTimeout[EmployPreceding]}-&STDURA";
                if (BreakIdx != -1)
                    chewed["END_POINTS"][BreakIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{container.TimeToTimeout[EmployPreceding]}-&STDURA";
                if (ContinueIdx != -1)
                    chewed["END_POINTS"][ContinueIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{container.TimeToTimeout[EmployPreceding]}-&STDURA";
            }
            else
            {
                chewed["END_POINTS"][CompletionIdx - 1]["TRIGGER"] = new JsonArray() { new TIM(Timeout).ToJson().ToJsonString() };
                chewed["END_POINTS"][CompletionIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{Timeout}-&STDURA";
                if (BreakIdx != -1)
                    chewed["END_POINTS"][BreakIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{Timeout}-&STDURA";
                if (ContinueIdx != -1)
                    chewed["END_POINTS"][ContinueIdx]["POST_SHADERS"].AsArray()[^1]["VALUE"] = $"{Timeout}-&STDURA";
            }

            uint inlineEvent = 10000;
            var s = new Step(chewed, globals, ref inlineEvent);
            return s;
        }
    }
}
