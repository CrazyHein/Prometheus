using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.BranchMechansim
{
    public class Branch
    {
        public uint Target { get; private set; }
        public Trigger Trigger { get; private init; }
        public IReadOnlyList<Shader> PostShaders { get; private init; }
        private List<Shader> __post_shaders = new List<Shader>();
        //public IEnumerable<ObjectReference> ObjectReferences;
        //public IEnumerable<EnvVariableReference> EnvVariableReferences;

        public Branch(JsonNode node, IReadOnlyDictionary<uint, Event> globalEvts, Dictionary<uint, Event> localEvts, ref uint inlineEventIndex)
        {
            try
            {
                Target = node["TARGET"].GetValue<uint>();
                Trigger = new Trigger(node["TRIGGER"], globalEvts, localEvts, ref inlineEventIndex);
                if (node.AsObject().TryGetPropertyValue("POST_SHADERS", out var postShaders))
                {
                    foreach (var s in postShaders.AsArray())
                        __post_shaders.Add(new Shader(s));
                }
                PostShaders = __post_shaders;
                //ObjectReferences = __post_shaders.Where(x => x.Operand is ObjectReference).Select(x => (ObjectReference)x.Operand).Distinct();
                //EnvVariableReferences = __post_shaders.Where(x => x.Operand is EnvVariableReference).Select(x => (EnvVariableReference)x.Operand).Distinct();
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_BRANCH_PARSE_ERROR, $"{node.ToString()}\nis not a valid BRANCH object.", ex);//ok
            }
        }

        public JsonNode ToJson()
        {
            JsonObject o = new JsonObject();
            o["TARGET"] = Target;
            o["TRIGGER"] = Trigger.ToJson();
            if (PostShaders.Count != 0)
            {
                JsonArray postShaders = new JsonArray();
                foreach (var s in PostShaders)
                    postShaders.Add(s.ToJson());
                o["POST_SHADERS"] = postShaders;
            }
            return o;
        }

        //public IEnumerable<ObjectReference> ObjectReferences => PostShaders.SelectMany(x => x.ObjectReferences).Distinct();
    }
}
