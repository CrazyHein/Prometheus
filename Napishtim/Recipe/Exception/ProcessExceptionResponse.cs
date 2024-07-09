using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process.ProcessStepSource;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling
{
    public abstract class ProcessExceptionResponse
    {
        public string Name { get; }
        protected ProcessExceptionResponse(string name)
        {
            name = name.Trim();
            if (name.Length == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.EXCEPTION_HANDLING_ARGUMENTS_ERROR, $"Invaild name for ExceptionResponse(name string length should be greater than zero).");
            Name = name;
        }
    }

    public abstract class ExceptionResponseSource : ProcessExceptionResponse
    {
        protected ExceptionResponseSource(string name) : base(name)
        {
        }
        protected JsonObject _exception_response { get; set; } = new JsonObject();
        public abstract ExceptionResponseObject ResolveTarget(uint next, Context context, IReadOnlyDictionary<uint, Event> globals);

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
            AddGlobalEventRefernce(ProcessStep.SearchGlobalEventIndex(conditions));
        }

        public abstract JsonObject SaveAsJson();

        public delegate ExceptionResponseSource MakeExceptionResponseSource(JsonObject node);
        private static Dictionary<string, MakeExceptionResponseSource> __BUIILD_EXCEPTION_RESPONSE_SOURCE = new Dictionary<string, MakeExceptionResponseSource>();
        public static ExceptionResponseSource MAKE_RESPONSE(JsonObject node)
        {
            string name = node["ASSEMBLY"].GetValue<string>();
            if (__BUIILD_EXCEPTION_RESPONSE_SOURCE.ContainsKey(name) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.EXCEPTION_HANDLING_ARGUMENTS_ERROR, $"Unknown ExceptionResponseSource: {name}.");
            return __BUIILD_EXCEPTION_RESPONSE_SOURCE[name](node); 
        }

        static ExceptionResponseSource()
        {
            __BUIILD_EXCEPTION_RESPONSE_SOURCE[typeof(SimpleExceptionResponse_S).FullName] = SimpleExceptionResponse_S.MAKE;
        }

        public override string ToString()
        {
            return _exception_response.ToString();
        }

        public static string DEFAULT_NAME(JsonNode? node)
        {
            return node == null ? "unnamed" : node.GetValue<string>();
        }

        public abstract IEnumerable<KeyValuePair<uint, (string name, Event evt)>> LocalEvents { get; }
        public abstract IEnumerable<(string name, JsonArray condition, ProcessShaders postShaders)> Branches { get; }
    }

    public abstract class ExceptionResponseObject : ProcessExceptionResponse
    {
        public abstract ExceptionResponse Build(Context context, IReadOnlyDictionary<uint, Event> globals);
        protected JsonObject _response;

        public ExceptionResponseObject(string name, JsonObject response) : base(name)
        {
            _response = response;
        }

        public override string ToString()
        {
            return _response.ToString();
        }
    }
}
