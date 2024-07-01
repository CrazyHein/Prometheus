using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.BranchMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim
{
    public class Step
    {
        public uint ID { get; private init; }
        public IReadOnlyList<Shader> Shaders { get; private init; }
        private List<Shader> __shaders = new List<Shader>();
        public IReadOnlyList<Branch> Branches { get; private init; }
        private List<Branch> __branches = new List<Branch>();

        public IReadOnlyDictionary<uint, Event> LocalEventStorage { get; private init; }
        private Dictionary<uint, Event> __local_event_stroage = new Dictionary<uint, Event>();

        //public IEnumerable<ObjectReference> ObjectReferences;
        //public IEnumerable<EnvVariableReference> EnvVariableReferences;
        public IReadOnlySet<uint> GlobalEventReferences;
        public IReadOnlySet<uint> UserVariablesUsage;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{\n\t");
            sb.Append($"\"ID\": {ID}");
            if (LocalEventStorage.Count != 0)
            {
                sb.Append(",\n");
                sb.Append($"\t\"EVENTS\":\n\t[");
                foreach (var e in LocalEventStorage)
                    sb.Append("\n\t\t{\"ID\":").Append($"{e.Key},\"EVENT\":{e.Value.ToString()}").Append("},");
                sb.Remove(sb.Length - 1, 1).Append("\n\t]");
            }
            if(Shaders.Count != 0)
            {
                sb.Append(",\n");
                sb.Append($"\t\"SHADERS\":\n\t[");
                foreach (var s in Shaders)
                    sb.Append($"\n\t\t{s.ToString()},");
                sb.Remove(sb.Length - 1, 1).Append("\n\t]");
            }
            sb.Append(",\n");
            sb.Append($"\t\"END_POINTS\":\n\t[");
            foreach (var b in Branches)
            {
                sb.Append("\n\t\t{\n\t\t\t\"TARGET\":").Append(b.Target);
                sb.Append(",\n\t\t\t\"TRIGGER\":\n\t\t\t[");
                foreach(var line in b.Trigger.ToJson().AsArray())
                    sb.Append($"\n\t\t\t\t\"{line.GetValue<string>()}\",");
                sb.Remove(sb.Length - 1, 1).Append("\n\t\t\t]");
                if(b.PostShaders.Count != 0)
                {
                    sb.Append(",\n\t\t\t\"POST_SHADERS\":\n\t\t\t[");
                    foreach(var ps in b.PostShaders)
                        sb.Append($"\n\t\t\t\t{ps.ToString()},");
                    sb.Remove(sb.Length - 1, 1).Append("\n\t\t\t]");
                }
                sb.Append("\n\t\t},");
            }
            sb.Remove(sb.Length - 1, 1).Append("\n\t]");
            sb.Append("\n}");
            return sb.ToString();
        }

        public string ToString(IReadOnlyDictionary<uint, Event> globalEvts)
        {
            StringBuilder sb = new StringBuilder("{\n\t");
            sb.Append($"\"ID\": {ID}");
            if (Shaders.Count != 0)
            {
                sb.Append(",\n");
                sb.Append($"\t\"SHADERS\":\n\t[");
                foreach (var s in Shaders)
                    sb.Append($"\n\t\t{s.ToString()},");
                sb.Remove(sb.Length - 1, 1).Append("\n\t]");
            }
            sb.Append(",\n");
            sb.Append($"\t\"END_POINTS\":\n\t[");
            foreach (var b in Branches)
            {
                sb.Append("\n\t\t{\n\t\t\t\"TARGET\":").Append(b.Target);
                sb.Append(",\n\t\t\t\"TRIGGER\":\n\t\t\t[");
                foreach (var line in b.Trigger.ToJson(globalEvts, LocalEventStorage).AsArray())
                    sb.Append($"\n\t\t\t\t\"{line.GetValue<string>()}\",");
                sb.Remove(sb.Length - 1, 1).Append("\n\t\t\t]");
                if (b.PostShaders.Count != 0)
                {
                    sb.Append(",\n\t\t\t\"POST_SHADERS\":\n\t\t\t[");
                    foreach (var ps in b.PostShaders)
                        sb.Append($"\n\t\t\t\t{ps.ToString()},");
                    sb.Remove(sb.Length - 1, 1).Append("\n\t\t\t]");
                }
                sb.Append("\n\t\t},");
            }
            sb.Remove(sb.Length - 1, 1).Append("\n\t]");
            sb.Append("\n}");
            return sb.ToString();
        }

        public Step(JsonNode node, IReadOnlyDictionary<uint, Event> globalEvts, ref uint inlineEventIndex)
        {
            try
            {
                ID = node["ID"].GetValue<uint>();
                if (node.AsObject().TryGetPropertyValue("EVENTS", out var localEventsNode))
                {
                    foreach (var e in localEventsNode.AsArray())
                    {
                        uint eventidx = e["ID"].GetValue<uint>();
                        if (__local_event_stroage.ContainsKey(eventidx))
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_STEP_PARSE_ERROR, $"Event with the same ID({eventidx}) already exists.");
                        else
                            __local_event_stroage[eventidx] = Event.MAKE(e["EVENT"]);
                    }
                }
                LocalEventStorage = __local_event_stroage;

                if (node.AsObject().TryGetPropertyValue("SHADERS", out var shadersNode))
                {
                    foreach (var e in shadersNode.AsArray())
                        __shaders.Add(new Shader(e));
                }
                Shaders = __shaders;

                JsonArray endPointsNode = node["END_POINTS"].AsArray();
                foreach (var e in endPointsNode)
                    __branches.Add(new Branch(e, globalEvts, __local_event_stroage, ref inlineEventIndex));
                Branches = __branches;

                //ObjectReferences = __shaders.Where(x => x.Operand is ObjectReference).Select(x => (ObjectReference)x.Operand).Concat(__branchs.SelectMany(e => e.ObjectReferences)).Distinct();
                //EnvVariableReferences = __shaders.Where(x => x.Operand is EnvVariableReference).Select(x => (EnvVariableReference)x.Operand).Concat(__branchs.SelectMany(e => e.EnvVariableReferences)).Distinct();
                GlobalEventReferences = new SortedSet<uint>(__branches.SelectMany(e => e.Trigger.ReferencedGlobalEvents));
                UserVariablesUsage = new SortedSet<uint>(__local_event_stroage.Values.SelectMany(x => x.UserVariablesUsage).Concat(__shaders.SelectMany(x => x.UserVariablesUsage)).Concat(__branches.SelectMany(x => x.PostShaders.SelectMany(x => x.UserVariablesUsage))));
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_STEP_PARSE_ERROR, $"{node.ToString()}\nis not a valid STEP object.", ex);
            }
        }

        public JsonNode ToJson()
        {
            JsonObject o = new JsonObject();
            o["ID"] = ID;
            if(LocalEventStorage.Count != 0)
            {
                JsonArray localEvents = new JsonArray();
                foreach (var e in LocalEventStorage)
                {
                    JsonObject localEvt = new JsonObject();
                    localEvt["ID"] = e.Key;
                    localEvt["EVENT"] = e.Value.ToJson(); 
                    localEvents.Add(localEvt);
                }
                o["EVENTS"] = localEvents;
            }
            if(Shaders.Count != 0)
            {
                JsonArray shaders = new JsonArray();
                foreach (var e in Shaders)
                    shaders.Add(e.ToJson());
                o["SHADERS"] = shaders;
            }

            JsonArray endPoints = new JsonArray();
            foreach (var e in Branches)
                endPoints.Add(e.ToJson());
            o["END_POINTS"] = endPoints;

            return o;
        }

        public IEnumerable<ObjectReference> ObjectReferences(IReadOnlyDictionary<uint, Event> globalEvts)
        {
            IEnumerable<ObjectReference> locals = LocalEventStorage.Values.SelectMany(x => x.ObjectReferences);
            IEnumerable<ObjectReference> shaders = Shaders.SelectMany(x => x.ObjectReferences);
            IEnumerable<ObjectReference> postShaders = Branches.SelectMany(x => x.PostShaders.SelectMany(y => y.ObjectReferences));
            IEnumerable<ObjectReference> globals = GlobalEventReferences.SelectMany(x => globalEvts[x].ObjectReferences);
            return locals.Concat(shaders).Concat(postShaders).Concat(globals).Distinct();
        }
            
    }
}
