using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [CompatibleEvent("LMT: The evaluation value of event is TRUE(FALSE if `REVERSED` has been set to TRUE) if the `EXPRESSION` value is within the range. In Any Other Case, the value is FALSE(TRUE if `REVERSED` has been set to TRUE).")]
    public class LMT : Event
    {
        public override string Tag => "LMT";
        public Expression.Expression? EXPRESSION { get; init; }
        public double? LOWER { get; init; }
        public double? UPPER { get; init; }
        public bool? REVERSED { get; init; }

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("EXPRESSION", true, "0", "The string must be a valid Expression string."),
            ("LOWER", false, String.Empty, "Optional parameter of type Double.\nSpecify the lower limit of the closed interval. The default value is 0.0. "),
            ("UPPER", false, String.Empty, "Optional parameter of type Double.\nSpecify the upper limit of the closed interval. The default value is 0.0."),
            ("REVERSED", false, String.Empty, "Optional parameter of type Boolean.\nTo adopt the method of reverse logic, set `REVERSED` to TRUE(other than 0.0).")
        ]);
        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("EXPRESSION", EXPRESSION.ToString()),
                    ("LOWER", LOWER.HasValue?LOWER.Value.ToString():String.Empty),
                    ("UPPER", UPPER.HasValue?UPPER.Value.ToString():String.Empty),
                    ("REVERSED", Event.BooleanParameterValue(REVERSED))
                };
                return parameters;
            }
        }

        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;

        public override IEnumerable<uint> UserVariablesUsage => EXPRESSION.UserVariablesUsage;

        public override IEnumerable<ObjectReference> ObjectReferences => EXPRESSION.ObjectReferences;

        public override JsonNode ToJson()
        {
            JsonNode node = new JsonObject();
            node["TYPE"] = Tag;
            node["EXPRESSION"] = EXPRESSION.ToString();
            if (LOWER.HasValue)
                node["LOWER"] = LOWER;
            if (UPPER.HasValue)
                node["UPPER"] = UPPER;
            if (REVERSED.HasValue)
                node["REVERSED"] = REVERSED;
            return node;
        }

        public LMT(JsonNode node)
        {
            try
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Type name '{(string)node["TYPE"]}' is not supported by {Tag} event object.");
                EXPRESSION = new Expression.Expression((string)node["EXPRESSION"], null);

                if (node.AsObject().TryGetPropertyValue("REVERSED", out var opt))
                    REVERSED = opt.AsValue().GetValue<bool>();
                else
                    REVERSED = null;

                if (node.AsObject().TryGetPropertyValue("LOWER", out opt))
                    LOWER = opt.AsValue().GetValue<double>();
                else
                    LOWER = null;

                if (node.AsObject().TryGetPropertyValue("UPPER", out opt))
                    UPPER = opt.AsValue().GetValue<double>();
                else
                    UPPER = null;
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid LMT event object.", ex);//ok
            }
        }

        public LMT(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "LMT(EXPRESSION, [LOWER], [UPPER], [REVERSED])");
            foreach (var param in parameters)
            {
                switch(param.pname)
                {
                    case "EXPRESSION":
                        EXPRESSION = param.pvalue; break;
                    case "REVERSED":
                        if (param.pvalue.IsImmediateOperand)
                            REVERSED = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'REVERSED' should be an immediate operand.");
                        break;
                    case "LOWER":
                        if (param.pvalue.IsImmediateOperand)
                            LOWER = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'LOWER' should be an immediate operand."); 
                        break;
                    case "UPPER":
                        if (param.pvalue.IsImmediateOperand)
                            UPPER = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'UPPER' should be an immediate operand.");
                        break;
                }
            }
            if (EXPRESSION == null)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "LMT(EXPRESSION, [LOWER], [UPPER], [REVERSED])");
        }

        public LMT(Expression.Expression a)
        {
            EXPRESSION = a;
        }
    }
}
