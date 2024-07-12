using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process
{
    public abstract class ProcessStep
    {
        public string Name { get; }
        public int StepFootprint { get; protected init; }
        public int UserVariableFootprint { get; protected init; }

        public static IEnumerable<uint> SearchGlobalEventIndex(JsonArray? conditions)
        {
            if (conditions != null)
            {
                foreach (var global in conditions.Select(x => x.GetValue<string>().Trim()).Where(x => Trigger.GLOBAL_EVENT_PATTERN.IsMatch(x)))
                    yield return uint.Parse(global.Substring("GEVENT".Length));
            }
        }

        protected ProcessStep(string name)
        {
            name = name.Trim();
            if (name.Length == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Invaild name for ProcessStep(name string length should be greater than zero).");
            Name = name;
        }
    }

    public abstract class ProcessStepSource : ProcessStep
    {
        protected JsonObject _step { get; set; } = new JsonObject();
        public abstract ProcessStepObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Sequential_S container, Dictionary<uint, string> stepNameMapping);
        public SortedSet<uint> __shader_user_variables_usage = new SortedSet<uint>();
        public IEnumerable<uint> ShaderUserVariablesUsage { get { return __shader_user_variables_usage; } }

        protected void AddShaderUserVariablesUsage(IEnumerable<uint> idxes)
        {
            foreach (var idx in idxes)
                __shader_user_variables_usage.Add(idx);
        }

        protected void AddShaderUserVariablesUsage(JsonArray shaderArray)
        {
            foreach (var sh in shaderArray)
            {
                Shader shader = new Shader(sh);
                AddShaderUserVariablesUsage(shader.UserVariablesUsage);
            }
        }

        private SortedSet<uint> __global_event_reference = new SortedSet<uint>();
        public IEnumerable<uint> GlobalEventReference { get { return __global_event_reference; } }

        protected void AddGlobalEventRefernce(IEnumerable<uint> idxes)
        {
            foreach (var idx in idxes)
                __global_event_reference.Add(idx);
        }

        protected void AddGlobalEventRefernce(JsonArray? conditions)
        {
            AddGlobalEventRefernce(SearchGlobalEventIndex(conditions));
        }


        public abstract JsonObject SaveAsJson(Sequential_S container);
        public abstract string ToString(Sequential_S? container);
        public delegate ProcessStepSource MakeProcessStepSource(JsonObject node, Sequential_S container);
        private static Dictionary<string, MakeProcessStepSource> __BUIILD_PROCESS_STEP_SOURCE = new Dictionary<string, MakeProcessStepSource>();
        public static ProcessStepSource MAKE_STEP(JsonObject node, Sequential_S container)
        {
            string name = node["ASSEMBLY"].GetValue<string>();
            if (__BUIILD_PROCESS_STEP_SOURCE.ContainsKey(name) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.PROCESS_COMPONENT_ARGUMENTS_ERROR, $"Unknown ProcessStepSource: {name}.");
            return __BUIILD_PROCESS_STEP_SOURCE[name](node, container);
        }

        public override string ToString()
        {
            return _step.ToString();
        }

        static ProcessStepSource()
        {
            __BUIILD_PROCESS_STEP_SOURCE[typeof(SimpleStep_S).FullName] = SimpleStep_S.MAKE;
            __BUIILD_PROCESS_STEP_SOURCE[typeof(SimpleStepWithTimeout_S).FullName] = SimpleStepWithTimeout_S.MAKE;
            __BUIILD_PROCESS_STEP_SOURCE[typeof(BranchStep_S).FullName] = BranchStep_S.MAKE;
        }

        protected ProcessStepSource(string name) : base(name)
        {
        }

        public static string DEFAULT_NAME(JsonNode? node)
        {
            return node == null ? "unnamed" : node.GetValue<string>();
        }

        public abstract IEnumerable<ProcessShader> Shaders { get; }
        public abstract IEnumerable<ProcessShader> PostShaders { get; }
        public abstract IEnumerable<ProcessShader> AbortShaders { get; }
        public IEnumerable<ProcessShader> ShaderObjectDirectAssignments => Shaders.Reverse().Where(x => x.Shader.Operand is ObjectReference && x.Shader.Expr.IsImmediateOperand).DistinctBy(x => x.Shader.Operand).Reverse();
        public IEnumerable<ProcessShader> PostShaderObjectDirectAssignments => PostShaders.Reverse().Where(x => x.Shader.Operand is ObjectReference && x.Shader.Expr.IsImmediateOperand).DistinctBy(x => x.Shader.Operand).Reverse();
        public IEnumerable<ProcessShader> AbortShaderObjectDirectAssignments => AbortShaders.Reverse().Where(x => x.Shader.Operand is ObjectReference && x.Shader.Expr.IsImmediateOperand).DistinctBy(x => x.Shader.Operand).Reverse();
        public abstract IEnumerable<KeyValuePair<uint, (string name, Event evt)>> LocalEvents { get; }
    }

    public abstract class ProcessStepObject : ProcessStep
    {
        public abstract Step Build(Context context, IReadOnlyDictionary<uint, Event> globals, Sequential_O container);
        protected JsonObject _step;

        internal ProcessStepObject(string name, JsonObject step) : base(name)
        {
            _step = step;
        }
        public uint? ID
        {
            get
            {
                if (_step.TryGetPropertyValue("ID", out var ret))
                    return ret.AsValue().GetValue<uint>();
                else return null;
            }
        }

        public override string ToString()
        {
            return _step.ToString();
        }
    }
}
