using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU.ArithmeticUnit;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CompatibleEventAttribute : Attribute
    {
        private List<KeyValuePair<string, string>> __compatible_eventType_collection;
        public IEnumerable<KeyValuePair<string, string>> CompatibleEventTypeCollection  => __compatible_eventType_collection;

        public CompatibleEventAttribute(params string[] compatibleEventTypeCollection)
        {
            __compatible_eventType_collection =
                new List<KeyValuePair<string, string>>();
            foreach (var x in compatibleEventTypeCollection)
            {
                var evtInfo = x.Split(':', 2).ToList();
                __compatible_eventType_collection.Add(KeyValuePair.Create(evtInfo[0], evtInfo[1]));
            }
        }

        public bool Contains(string evt)
        {
            return CompatibleEventTypeCollection.Any(x => x.Key == evt);
        }
    }
    public abstract class Event
    {
        public abstract JsonNode ToJson();
        public abstract string Tag { get; }
        private static List<KeyValuePair<string, string>> __COMPATIBLE_EVENT_TYPES = new List<KeyValuePair<string, string>>();
        public static IEnumerable<KeyValuePair<string, string>> CompatibleEventTypes { get; }
        private static Dictionary<string, ReadOnlyCollection<(string pname, bool required, string pdefault, string comment )>> __EVENT_DEFAULT_PARAMETERS 
            = new Dictionary<string, ReadOnlyCollection<(string pname, bool required, string pdefault, string comment)>>();
        public static IReadOnlyDictionary<string, ReadOnlyCollection<(string pname, bool required, string pdefault, string comment)>> EventDefaultParameters;
        static Event()
        {
            var assem = typeof(Event).Assembly;
            foreach (var t in assem.GetTypes())
            {
                var bt = t.BaseType;
                if (bt != null && bt.FullName == typeof(Event).FullName)
                {
                    var ta = t.GetCustomAttributes(typeof(CompatibleEventAttribute), false);
                    if (ta.Length != 0)
                    {
                        foreach (var evt in (ta[0] as CompatibleEventAttribute).CompatibleEventTypeCollection)
                        {
                            __BUILD_EVENT_WITH_JSON[evt.Key] = (n) => Activator.CreateInstance(t, n);
                            __BUILD_EVENT_WITH_ARGUMENTS[evt.Key] = (name, p) => Activator.CreateInstance(t, name, p);
                            __COMPATIBLE_EVENT_TYPES.Add(evt);
                            var parameterdescriptions = t.GetProperty("_ParameterDescriptions", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                            __EVENT_DEFAULT_PARAMETERS[evt.Key] = parameterdescriptions as ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>;
                        }
                    }
                }
            }
            BUILD_EVENT_WITH_JSON = __BUILD_EVENT_WITH_JSON;
            BUILD_EVENT_WITH_ARGUMENTS = __BUILD_EVENT_WITH_ARGUMENTS;
            CompatibleEventTypes = __COMPATIBLE_EVENT_TYPES;
            EventDefaultParameters = __EVENT_DEFAULT_PARAMETERS;
        }

        private static Dictionary<string, MakeEventWithJSON> __BUILD_EVENT_WITH_JSON = new Dictionary<string, MakeEventWithJSON>();
        private static Dictionary<string, MakeEventWithArgements> __BUILD_EVENT_WITH_ARGUMENTS = new Dictionary<string, MakeEventWithArgements>();
        public static IReadOnlyDictionary<string, MakeEventWithJSON> BUILD_EVENT_WITH_JSON;
        public static IReadOnlyDictionary<string, MakeEventWithArgements> BUILD_EVENT_WITH_ARGUMENTS;
        public delegate Object? MakeEventWithJSON(JsonNode node);
        public delegate Object? MakeEventWithArgements(string name, params (string pname, Expression.Expression pvalue)[] parameters);

        public abstract IEnumerable<(string pname, string pvalue)> ParameterSettings { get; }
        public abstract IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions { get; }
        public abstract IEnumerable<uint> UserVariablesUsage { get; }

        public static Event MAKE(JsonNode node)
        {
            try
            {
                if (node.GetValueKind() != System.Text.Json.JsonValueKind.Object)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid JSON object.");
                if (node.AsObject().TryGetPropertyValue("TYPE", out var name) && name.GetValueKind() == System.Text.Json.JsonValueKind.String)
                {
                    if (BUILD_EVENT_WITH_JSON.ContainsKey((string)name))
                    {
                        var ev = BUILD_EVENT_WITH_JSON[(string)name](node) as Event;
                        if (ev != null)
                            return ev;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Unknown event type: {(string)name}.");
                    }
                    else
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Unknown event type: {(string)name}.");
                }
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid EVENT object.");
            }
            catch (NaposhtimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        public static Event MAKE(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            try
            {
                if (BUILD_EVENT_WITH_ARGUMENTS.ContainsKey(name))
                {
                    var ev = BUILD_EVENT_WITH_ARGUMENTS[name](name, parameters) as Event;
                    if (ev != null)
                        return ev;
                    else
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"Unknown event type: {(string)name}.");
                }
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"Unknown event type: {(string)name}.");
            }
            catch(NaposhtimException)
            {
                throw;
            }
            catch(Exception ex)
            {
                if(ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        public override string ToString()
        {
            return ToJson().ToJsonString();
        }

        public string ToString(int tabs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\x20', tabs).Append(ToJson().ToJsonString());
            return sb.ToString();
        }

        public abstract IEnumerable<ObjectReference> ObjectReferences { get; }

        public static string BooleanParameterValue(bool? value)
        {
            if(value == null)
                return string.Empty;
            else if(value == false)
                return 0.0.ToString();
            else
                return 1.0.ToString();
        }
    }
}
