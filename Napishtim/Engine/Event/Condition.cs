using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [CompatibleEvent(("EQU:The event is triggered depends on the arithmetic comparison of the two operands: Whether the operand A is equal to the operand B ?"),
        ("GTE:The event is triggered depends on the arithmetic comparison of the two operands: Whether the operand A is greater than or equal to the operand B ?"),
        ("GRT:The event is triggered depends on the arithmetic comparison of the two operands: Whether the operand A is greater than the operand B ?"),
        ("LTE:The event is triggered depends on the arithmetic comparison of the two operands: Whether the operand A is less than or equal to the operand B ?"),
        ("LES:The event is triggered depends on the arithmetic comparison of the two operands: Whether the operand A is less than the operand B ?"))]
    public class Condition : Event
    {
        private readonly string __condition = string.Empty;
        public override string Tag => __condition;


        public Expression.Expression? COMPARAND_A { get; init; }
        public Expression.Expression? COMPARAND_B { get; init; }
        public double? POSITIVE_TOLERANCE { get; init; }
        public double? NEGATIVE_TOLERANCE { get; init; }
        public bool? INITIAL_VALUE { get; init; }
        public TIM? ON_DELAY { get; init; }
        public TIM? OFF_DELAY { get; init; }

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("COMPARAND_A", true, "0", "The string must be a valid Expression string."),
            ("COMPARAND_B", true, "0", "The string must be a valid Expression string."),
            ("POSITIVE_TOLERANCE", false, String.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation. The default value is 0.0."),
            ("NEGATIVE_TOLERANCE", false, String.Empty, "Optional parameter of type Double.\nSpecify the negative tolerance of deviation. The default value is 0.0."),
            ("INITIAL_VALUE", false, String.Empty, "Optional parameter of type Boolean. 0.0 -> false / Others -> true\nSpecify the initial evaluation value of event. The default value is false."),
            ("ON_DELAY", false, String.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
            ("OFF_DELAY", false, String.Empty,"Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
        ]);


        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;
        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("COMPARAND_A", COMPARAND_A.ToString()),
                    ("COMPARAND_B", COMPARAND_B.ToString()),
                    ("POSITIVE_TOLERANCE", POSITIVE_TOLERANCE.HasValue?POSITIVE_TOLERANCE.Value.ToString():String.Empty),
                    ("NEGATIVE_TOLERANCE", NEGATIVE_TOLERANCE.HasValue?NEGATIVE_TOLERANCE.Value.ToString():String.Empty),
                    ("INITIAL_VALUE", Event.BooleanParameterValue(INITIAL_VALUE)),
                    ("ON_DELAY", ON_DELAY!=null?ON_DELAY._TIMEOUT_STRING:String.Empty),
                    ("OFF_DELAY", OFF_DELAY!=null?OFF_DELAY._TIMEOUT_STRING:String.Empty)
                };
                return parameters;
            }
        }

        public Condition(JsonNode node)
        {
            try
            {
                string name = (string)node["TYPE"];
                CompatibleEventAttribute meta = this.GetType().GetCustomAttribute<CompatibleEventAttribute>();
                if(meta.Contains(name))
                    __condition = name;
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Comparison operator '{name}'is not supported by CONDITION event object.");//ok
                COMPARAND_A = new Expression.Expression((string)node["COMPARAND_A"], null);
                COMPARAND_B = new Expression.Expression((string)node["COMPARAND_B"], null);

                if (node.AsObject().TryGetPropertyValue("INITIAL_VALUE", out var opt))
                    INITIAL_VALUE = opt.AsValue().GetValue<bool>();
                else
                    INITIAL_VALUE = null;

                if (node.AsObject().TryGetPropertyValue("POSITIVE_TOLERANCE", out opt))
                    POSITIVE_TOLERANCE = opt.AsValue().GetValue<double>();
                else
                    POSITIVE_TOLERANCE = null;

                if (node.AsObject().TryGetPropertyValue("NEGATIVE_TOLERANCE", out opt))
                    NEGATIVE_TOLERANCE = opt.AsValue().GetValue<double>();
                else
                    NEGATIVE_TOLERANCE = null;

                if (node.AsObject().TryGetPropertyValue("ON_DELAY", out opt))
                {
                    if (opt.GetValueKind() == System.Text.Json.JsonValueKind.Object)
                        ON_DELAY = new TIM(opt);
                    else
                        ON_DELAY = new TIM(opt.AsValue().GetValue<int>());
                }
                else
                    ON_DELAY = null;

                if (node.AsObject().TryGetPropertyValue("OFF_DELAY", out opt))
                {
                    if (opt.GetValueKind() == System.Text.Json.JsonValueKind.Object)
                        OFF_DELAY = new TIM(opt);
                    else
                        OFF_DELAY = new TIM(opt.AsValue().GetValue<int>());
                }
                else
                    OFF_DELAY = null;

            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid CONDITION event object.", ex);//ok
            }
        }

        public Condition(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            CompatibleEventAttribute meta = this.GetType().GetCustomAttribute<CompatibleEventAttribute>();
            if (meta.Contains(name))
                __condition = name;
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"'{name}' is not a valid comparison operator.");

            bool a = false, b = false;
            foreach (var param in parameters)
            {
                switch (param.pname)
                {
                    case "COMPARAND_A":
                        COMPARAND_A = param.pvalue; a = true; break;
                    case "COMPARAND_B":
                        COMPARAND_B = param.pvalue; b = true; break;
                    case "INITIAL_VALUE":
                        if (param.pvalue.IsImmediateOperand)
                            INITIAL_VALUE = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'INITIAL_VALUE' should be an immediate operand.");
                        break;
                    case "POSITIVE_TOLERANCE":
                        if (param.pvalue.IsImmediateOperand)
                            POSITIVE_TOLERANCE = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'POSITIVE_TOLERANCE' should be an immediate operand.");
                        break;
                    case "NEGATIVE_TOLERANCE":
                        if (param.pvalue.IsImmediateOperand)
                            NEGATIVE_TOLERANCE = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'NEGATIVE_TOLERANCE' should be an immediate operand.");
                        break;
                    case "ON_DELAY":
                        if (param.pvalue.IsImmediateOperand)
                            ON_DELAY = new TIM((int)param.pvalue.Value(true, 0.0));
                        else
                            ON_DELAY = new TIM("TIM", ("TIMEOUT", param.pvalue));
                        break;
                    case "OFF_DELAY":
                        if (param.pvalue.IsImmediateOperand)
                            OFF_DELAY = new TIM((int)param.pvalue.Value(true, 0.0));
                        else
                            OFF_DELAY = new TIM("TIM", ("TIMEOUT", param.pvalue));
                        break;
                }
            }
            if (a == false || b == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"{name}(COMPARAND_A, COMPARAND_B, [INITIAL_VALUE], [POSITIVE_TOLERANCE], [NEGATIVE_TOLERANCE], [ON_DELAY, [OFF_DELAY])");
        }

        public Condition(string condition, Expression.Expression a, Expression.Expression b)
        {
            CompatibleEventAttribute meta = this.GetType().GetCustomAttribute<CompatibleEventAttribute>();
            if (meta.Contains(condition))
                __condition = condition;
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"'{condition}' is not a valid comparison operator.");
            COMPARAND_A = a;
            COMPARAND_B = b;
        }

        public override IEnumerable<ObjectReference> ObjectReferences
        {
            get
            {
                var temp = COMPARAND_A.ObjectReferences.Concat(COMPARAND_B.ObjectReferences);
                if (ON_DELAY != null)
                    temp = temp.Concat(ON_DELAY.ObjectReferences);
                if (OFF_DELAY != null)
                    temp = temp.Concat(OFF_DELAY.ObjectReferences);
                return temp.Distinct();
            }
        }

        public override IEnumerable<uint> UserVariablesUsage
        {
            get
            {
                var ret = COMPARAND_A.UserVariablesUsage.Concat(COMPARAND_B.UserVariablesUsage);
                if (ON_DELAY != null)
                    ret.Concat(ON_DELAY.UserVariablesUsage);
                if (OFF_DELAY != null)
                    ret.Concat(OFF_DELAY.UserVariablesUsage);
                return ret.Distinct();
            }
        }

        public override JsonNode ToJson()
        {
            JsonNode node = new JsonObject();
            node["TYPE"] = Tag;
            node["COMPARAND_A"] = COMPARAND_A.ToString();
            node["COMPARAND_B"] = COMPARAND_B.ToString();
            if (INITIAL_VALUE.HasValue)
                node["INITIAL_VALUE"] = INITIAL_VALUE;
            if (POSITIVE_TOLERANCE.HasValue)
                node["POSITIVE_TOLERANCE"] = POSITIVE_TOLERANCE;
            if (NEGATIVE_TOLERANCE.HasValue)
                node["NEGATIVE_TOLERANCE"] = NEGATIVE_TOLERANCE;
            if (ON_DELAY != null)
                node["ON_DELAY"] = ON_DELAY.ToJson();
            if (OFF_DELAY != null)
                node["OFF_DELAY"] = OFF_DELAY.ToJson();
            return node;
        }
    }
}
