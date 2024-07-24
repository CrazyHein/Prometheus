using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    public enum LevelAlertPattern
    {
        Above,
        Below,
    }

    [CompatibleEvent("LVL:The evaluation value of event turns to TRUE when the `EXPRESSION` value enters the alert range.The evaluation value of event turns to FALSE when the `EXPRESSION` value leaves the alert range.")]
    public class LVL : Event
    {
        public override string Tag => "LVL";
        public Expression.Expression? EXPRESSION { get; init; }
        public LevelAlertPattern? PATTERN { get; init; }
        public double? LOWER { get; init; }
        public double? UPPER { get; init; }
        public bool? INITIAL_VALUE { get; init; }

        private static string PatternParameterValue(LevelAlertPattern? pattern)
        {
            if(pattern == null)
                return string.Empty;
            else if(pattern == LevelAlertPattern.Above)
                return 1.0.ToString();
            else
                return 0.0.ToString();
        }

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("EXPRESSION", true, "0", "The string must be a valid Expression string."),
            ("PATTERN", false, String.Empty,
            """
            Optional parameter of type String.
            Specify the alert pattern. The default pattern is "Below"(0).
            If you want to use "Above" pattern, you should use a string "Above"(other than 0) to explicitly specify, otherwise, the default pattern will be used.
            "Above"(other than 0) pattern:
               The evaluation value of event turns to TRUE when the "EXPRESSION" value is equal to or greater than "UPPER"; 
               The evaluation value of event turns to FALSE when the "EXPRESSION" value is equal to or less than "LOWER";
            "Below"(0) pattern:
               The evaluation value of event turns to TRUE when the "EXPRESSION" value is equal to or less than "LOWER";
               The evaluation value of event turns to FALSE when the "EXPRESSION" value is equal to or greater than "UPPER";
            """),

            ("LOWER", false, String.Empty, "Optional parameter of type Double.\nSpecify the lower limit of the closed interval. The default value is 0.0. "),
            ("UPPER", false, String.Empty, "Optional parameter of type Double.\nSpecify the upper limit of the closed interval. The default value is 0.0."),
            ("INITIAL_VALUE", false, String.Empty, "Optional parameter of type Boolean.\nSpecify the initial evaluation value of event. The default value is FALSE. ")
        ]);

        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("EXPRESSION", EXPRESSION.ToString()),
                    ("PATTERN", LVL.PatternParameterValue(PATTERN)),
                    ("LOWER", LOWER.HasValue?LOWER.Value.ToString():String.Empty),
                    ("UPPER", UPPER.HasValue?UPPER.Value.ToString():String.Empty),
                    ("INITIAL_VALUE", Event.BooleanParameterValue(INITIAL_VALUE))
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
            if(PATTERN.HasValue)
                node["PATTERN"] = PATTERN.Value.ToString();
            if (LOWER.HasValue)
                node["LOWER"] = LOWER;
            if (UPPER.HasValue)
                node["UPPER"] = UPPER;
            if (INITIAL_VALUE.HasValue)
                node["INITIAL_VALUE"] = INITIAL_VALUE;
            return node;
        }

        public LVL(JsonNode node)
        {
            try
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Type name '{(string)node["TYPE"]}' is not supported by {Tag} event object.");
                EXPRESSION = new Expression.Expression((string)node["EXPRESSION"], null);

                if (node.AsObject().TryGetPropertyValue("PATTERN", out var opt))
                    PATTERN = Enum.Parse<LevelAlertPattern>(opt.AsValue().GetValue<string>());
                else
                    PATTERN = null;

                if (node.AsObject().TryGetPropertyValue("LOWER", out opt))
                    LOWER = opt.AsValue().GetValue<double>();
                else
                    LOWER = null;

                if (node.AsObject().TryGetPropertyValue("UPPER", out opt))
                    UPPER = opt.AsValue().GetValue<double>();
                else
                    UPPER = null;

                if (node.AsObject().TryGetPropertyValue("INITIAL_VALUE", out opt))
                    INITIAL_VALUE = opt.AsValue().GetValue<bool>();
                else
                    INITIAL_VALUE = null;
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid LVL event object.", ex);//ok
            }
        }

        public LVL(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "LVL(EXPRESSION, [PATTERN], [LOWER], [UPPER], [INITIAL_VALUE])");
            foreach (var param in parameters)
            {
                switch (param.pname)
                {
                    case "EXPRESSION":
                        EXPRESSION = param.pvalue; break;
                    case "PATTERN":
                        if (param.pvalue.IsImmediateOperand)
                            PATTERN = param.pvalue.Value(true, 0.0) == 0 ? LevelAlertPattern.Below : LevelAlertPattern.Above;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'PATTERN' should be an immediate operand.");
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
                    case "INITIAL_VALUE":
                        if (param.pvalue.IsImmediateOperand)
                            INITIAL_VALUE = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'INITIAL_VALUE' should be an immediate operand.");
                        break;
                }
            }
            if (EXPRESSION == null)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "LVL(EXPRESSION, [PATTERN], [LOWER], [UPPER], [INITIAL_VALUE])");
        }

        public LVL(Expression.Expression a)
        {
            EXPRESSION = a;
        }
    }
}
