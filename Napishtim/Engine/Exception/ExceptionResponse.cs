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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim
{
    public class ExceptionResponse
    {
        public IReadOnlyList<Branch> Branches { get; private init; }
        private List<Branch> __branches = new List<Branch>();
        public IReadOnlyDictionary<uint, Event> LocalEventStorage { get; private init; }
        private Dictionary<uint, Event> __local_event_stroage = new Dictionary<uint, Event>();

        public IReadOnlySet<uint> GlobalEventReferences;
        public IReadOnlySet<uint> UserVariablesUsage;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            if (LocalEventStorage.Count != 0)
            {
                sb.Append("\n");
                sb.Append($"\t\"EVENTS\":\n\t[");
                foreach (var e in LocalEventStorage)
                    sb.Append("\n\t\t{\"ID\":").Append($"{e.Key},\"EVENT\":{e.Value.ToString()}").Append("},");
                sb.Remove(sb.Length - 1, 1).Append("\n\t]");
                sb.Append(",\n");
            }
            else
                sb.Append("\n");
            sb.Append($"\t\"END_POINTS\":\n\t[");
            foreach (var b in Branches)
            {
                sb.Append("\n\t\t{\n\t\t\t\"TARGET\":").Append(b.Target);
                sb.Append(",\n\t\t\t\"TRIGGER\":\n\t\t\t[");
                foreach (var line in b.Trigger.ToJson().AsArray())
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

        public string ToString(IReadOnlyDictionary<uint, Event> globalEvts)
        {
            StringBuilder sb = new StringBuilder("{");
            if (LocalEventStorage.Count != 0)
            {
                sb.Append("\n");
                sb.Append($"\t\"EVENTS\":\n\t[");
                foreach (var e in LocalEventStorage)
                    sb.Append("\n\t\t{\"ID\":").Append($"{e.Key},\"EVENT\":{e.Value.ToString()}").Append("},");
                sb.Remove(sb.Length - 1, 1).Append("\n\t]");
                sb.Append(",\n");
            }
            else
                sb.Append("\n");
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

        public ExceptionResponse(JsonNode node, IReadOnlyDictionary<uint, Event> globalEvts, ref uint inlineEventIndex)
        {
            try
            {
                if (node.AsObject().TryGetPropertyValue("EVENTS", out var localEventsNode))
                {
                    foreach (var e in localEventsNode.AsArray())
                    {
                        uint eventidx = e["ID"].GetValue<uint>();
                        if (__local_event_stroage.ContainsKey(eventidx))
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXCEPTION_RESPONSE_PARSE_ERROR, $"Event with the same ID({eventidx}) already exists.");
                        else
                            __local_event_stroage[eventidx] = Event.MAKE(e["EVENT"]);
                    }
                }
                LocalEventStorage = __local_event_stroage;

                JsonArray endPointsNode = node["END_POINTS"].AsArray();
                foreach (var e in endPointsNode)
                    __branches.Add(new Branch(e, globalEvts, __local_event_stroage, ref inlineEventIndex));
                Branches = __branches;

                GlobalEventReferences = new SortedSet<uint>(__branches.SelectMany(e => e.Trigger.ReferencedGlobalEvents));
                UserVariablesUsage = new SortedSet<uint>(__local_event_stroage.Values.SelectMany(x => x.UserVariablesUsage).Concat(__branches.SelectMany(x => x.PostShaders.SelectMany(x => x.UserVariablesUsage))));
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXCEPTION_RESPONSE_PARSE_ERROR, $"{node.ToString()}\nis not a valid ExceptionResponse object.", ex);
            }
        }

        public JsonNode ToJson()
        {
            JsonObject o = new JsonObject();
            if (LocalEventStorage.Count != 0)
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

            JsonArray endPoints = new JsonArray();
            foreach (var e in Branches)
                endPoints.Add(e.ToJson());
            o["END_POINTS"] = endPoints;

            return o;
        }

        public IEnumerable<ObjectReference> ObjectReferences(IReadOnlyDictionary<uint, Event> globalEvts)
        {
            IEnumerable<ObjectReference> locals = LocalEventStorage.Values.SelectMany(x => x.ObjectReferences);
            IEnumerable<ObjectReference> postShaders = Branches.SelectMany(x => x.PostShaders.SelectMany(y => y.ObjectReferences));
            IEnumerable<ObjectReference> globals = GlobalEventReferences.SelectMany(x => globalEvts[x].ObjectReferences);
            return locals.Concat(postShaders).Concat(globals).Distinct();
        }
    }
}
