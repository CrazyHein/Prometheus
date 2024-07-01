using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [CompatibleEvent("CST:The event is triggered according to the value of parameter \"EVAL\".")]
    public class CST : Event
    {
        public override string Tag => "CST";
        public bool EVAL { get; init; } = false;

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("EVAL", true, "0", "The evaluation value of event. 0 -> false / Others -> true"),
        ]);


        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;

        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("EVAL", EVAL == true?1.0.ToString():0.0.ToString()),
                };
                return parameters;
            }
        }

        public CST(JsonNode node)
        {
            try
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Type name '{(string)node["TYPE"]}' is not supported by {Tag} event object.");
                EVAL = (bool)node["EVAL"];
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid {Tag} event object.", ex);
            }
        }

        public CST(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag || parameters.Length != 1 || parameters[0].pvalue.IsImmediateOperand == false || parameters[0].pname != "EVAL")
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "CST(EVAL)");
            EVAL = parameters[0].pvalue.Value(true, 0.0) == 0.0 ? false : true;
        }

        public CST(bool eval)
        {
            EVAL = eval;
        }
        public override JsonNode ToJson()
        {
            JsonNode node = new JsonObject();
            node["TYPE"] = Tag;
            node["EVAL"] = EVAL;
            return node;
        }

        public static CST ON { get; } = new CST(true);
        public static CST OFF { get; } = new CST(false);
        public static string ON_STRING { get; } = ON.ToJson().ToJsonString();
        public static string OFF_STRING { get; } = OFF.ToJson().ToJsonString();

        public override IEnumerable<ObjectReference> ObjectReferences => Enumerable.Empty<ObjectReference>();

        public override IEnumerable<uint> UserVariablesUsage => Enumerable.Empty<uint>();
    }
}
