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
    [CompatibleEvent("ERR:The event is triggered according to whether the following error exceeds the set tolerance.")]
    public class ERR : Event
    {
        public override string Tag => "ERR";
        public Expression.Expression? SETPOINT { get; init; }
        public Expression.Expression? FEEDBACK { get; init; }
        public Expression.Expression? DISABLED { get; init; }
        public double? POSITIVE_TOLERANCE { get; init; }
        public double? NEGATIVE_TOLERANCE { get; init; }
        public double? STABLE_POSITIVE_TOLERANCE { get; init; }
        public double? STABLE_NEGATIVE_TOLERANCE { get; init; }
        public bool? INITIAL_VALUE { get; init; }
        public bool? INITIAL_STATE { get; init; }
        public TIM? ON_DELAY { get; init; }
        public TIM? OFF_DELAY { get; init; }
        public TIM? STABLE_DELAY { get; init; }
        public bool? DELAY_TIME_PRIORITY { get; init; }
        public bool? IN_PROPORTION { get; init; }

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("DISABLED", false, string.Empty, "Optional parameter of type Expression.\nThe value of the expression is used to indicate whether this event is temporarily disabled.\nIf the value is not zero, the event will be temporarily disabled. The default value is 0.0."),
            ("SETPOINT", true, "0", "The string must be a valid Expression string."),
            ("FEEDBACK", true, "0", "The string must be a valid Expression string."),  
            ("POSITIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation(FB - SP). The default value is 0.0."),
            ("NEGATIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the negative tolerance of deviation(FB - SP). The default value is 0.0."),
            ("STABLE_POSITIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation. The default value is 0.0."),
            ("STABLE_NEGATIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation. The default value is 0.0."),
            ("INITIAL_VALUE", false, string.Empty, "Optional parameter of type Boolean. 0.0 -> false / Others -> true\nSpecify the initial evaluation value of event. The default value is false."),
            ("INITIAL_STATE", false, string.Empty, "Optional parameter of type Boolean. 0.0 -> not in stable state / Others -> in stable state\nSpecify the initial state value of event. The default value is \"not in stable state\"."),
            ("ON_DELAY", false, string.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
            ("OFF_DELAY", false, string.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
            ("STABLE_DELAY", false, string.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
            ("DELAY_TIME_PRIORITY", false, string.Empty, "Optional parameter of type Boolean. 0.0 -> false / Others -> true\nIf this property is true, it will wait for a period of time (STABLE_DELAY) before entering the stable state and then starting the error judgment phase.\nIf this property is false, it will wait for a period of time (STABLE_DELAY) or FEEDBACK to enter the stable state interval before entering the stable state and then starting the error judgment phase.\nThe default value is false."),
            ("IN_PROPORTION", false, string.Empty, "Optional parameter of type Boolean. 0.0 -> false / Others -> true\nIf the value is true, the actual tolerance value used is the result of SETPOINT*(XXX_TOLERANCE). The default value is 0.0.")

        ]);

        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;

        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("DISABLED", DISABLED == null? string.Empty :DISABLED.ToString()),
                    ("SETPOINT", SETPOINT.ToString()),
                    ("FEEDBACK", FEEDBACK.ToString()),
                    ("POSITIVE_TOLERANCE", POSITIVE_TOLERANCE.HasValue?POSITIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("NEGATIVE_TOLERANCE", NEGATIVE_TOLERANCE.HasValue?NEGATIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("STABLE_POSITIVE_TOLERANCE", STABLE_POSITIVE_TOLERANCE.HasValue?STABLE_POSITIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("STABLE_NEGATIVE_TOLERANCE", STABLE_NEGATIVE_TOLERANCE.HasValue?STABLE_NEGATIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("INITIAL_VALUE", Event.BooleanParameterValue(INITIAL_VALUE)),
                    ("INITIAL_STATE", Event.BooleanParameterValue(INITIAL_STATE)),
                    ("ON_DELAY", ON_DELAY!=null?ON_DELAY._TIMEOUT_STRING:string.Empty),
                    ("OFF_DELAY", OFF_DELAY!=null?OFF_DELAY._TIMEOUT_STRING:string.Empty),
                    ("STABLE_DELAY", STABLE_DELAY!=null?STABLE_DELAY._TIMEOUT_STRING:string.Empty),
                    ("DELAY_TIME_PRIORITY", Event.BooleanParameterValue(DELAY_TIME_PRIORITY)),
                    ("IN_PROPORTION", Event.BooleanParameterValue(IN_PROPORTION))
                };
                return parameters;
            }
        }

        public override IEnumerable<ObjectReference> ObjectReferences
        {
            get
            {
                var temp = SETPOINT.ObjectReferences.Concat(FEEDBACK.ObjectReferences);
                if (DISABLED != null)
                    temp = temp.Concat(DISABLED.ObjectReferences);
                if(ON_DELAY != null)
                    temp = temp.Concat(ON_DELAY.ObjectReferences);
                if(OFF_DELAY != null)
                    temp = temp.Concat(OFF_DELAY.ObjectReferences);
                if (STABLE_DELAY != null)
                    temp = temp.Concat(STABLE_DELAY.ObjectReferences);
                return temp.Distinct();
            }
        }

        public override IEnumerable<uint> UserVariablesUsage
        {
            get
            {
                var ret = SETPOINT.UserVariablesUsage.Concat(FEEDBACK.UserVariablesUsage);
                if (DISABLED != null)
                    ret = ret.Concat(DISABLED.UserVariablesUsage);
                if (ON_DELAY != null)
                    ret.Concat(ON_DELAY.UserVariablesUsage);
                if (OFF_DELAY != null)
                    ret.Concat(OFF_DELAY.UserVariablesUsage);
                if (STABLE_DELAY != null)
                    ret.Concat(STABLE_DELAY.UserVariablesUsage);
                return ret.Distinct();
            }
        }

        public ERR(JsonNode node)
        {
            try
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Type name '{(string)node["TYPE"]}' is not supported by {Tag} event object.");

                if (node.AsObject().TryGetPropertyValue("DISABLED", out var opt))
                    DISABLED = opt.AsValue().GetValue<string>();
                else
                    DISABLED = null;

                SETPOINT = new Expression.Expression((string)node["SETPOINT"], null);
                FEEDBACK = new Expression.Expression((string)node["FEEDBACK"], null);

                if (node.AsObject().TryGetPropertyValue("INITIAL_VALUE", out opt))
                    INITIAL_VALUE = opt.AsValue().GetValue<bool>();
                else
                    INITIAL_VALUE = null;

                if (node.AsObject().TryGetPropertyValue("INITIAL_STATE", out opt))
                    INITIAL_STATE = opt.AsValue().GetValue<bool>();
                else
                    INITIAL_STATE = null;

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

                if (node.AsObject().TryGetPropertyValue("STABLE_DELAY", out opt))
                {
                    if (opt.GetValueKind() == System.Text.Json.JsonValueKind.Object)
                        STABLE_DELAY = new TIM(opt);
                    else
                        STABLE_DELAY = new TIM(opt.AsValue().GetValue<int>());
                }
                else
                    STABLE_DELAY = null;

                if (node.AsObject().TryGetPropertyValue("STABLE_POSITIVE_TOLERANCE", out opt))
                    STABLE_POSITIVE_TOLERANCE = opt.AsValue().GetValue<double>();
                else
                    STABLE_POSITIVE_TOLERANCE = null;

                if (node.AsObject().TryGetPropertyValue("STABLE_NEGATIVE_TOLERANCE", out opt))
                    STABLE_NEGATIVE_TOLERANCE = opt.AsValue().GetValue<double>();
                else
                    STABLE_NEGATIVE_TOLERANCE = null;

                if (node.AsObject().TryGetPropertyValue("DELAY_TIME_PRIORITY", out opt))
                    DELAY_TIME_PRIORITY = opt.AsValue().GetValue<bool>();
                else
                    DELAY_TIME_PRIORITY = null;

                if (node.AsObject().TryGetPropertyValue("IN_PROPORTION", out opt))
                    IN_PROPORTION = opt.AsValue().GetValue<bool>();
                else
                    IN_PROPORTION = null;
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid {Tag} event object.", ex);
            }
        }

        public ERR(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "ERR([DISABLED], SETPOINT, FEEDBACK, [INITIAL_VALUE], [INITIAL_STATE], [POSITIVE_TOLERANCE], [NEGATIVE_TOLERANCE], [STABLE_POSITIVE_TOLERANCE], [STABLE_NEGATIVE_TOLERANCE], [ON_DELAY], [OFF_DELAY], [STABLE_DELAY], [DELAY_TIME_PRIORITY], [IN_PROPORTION])");
            bool sp = false, fb = false;
            foreach (var param in parameters)
            {
                switch (param.pname)
                {
                    case "DISABLED":
                        DISABLED = param.pvalue; break;
                    case "SETPOINT":
                        SETPOINT = param.pvalue; sp = true; break;
                    case "FEEDBACK":
                        FEEDBACK = param.pvalue; fb = true; break;
                    case "INITIAL_VALUE":
                        if (param.pvalue.IsImmediateOperand)
                            INITIAL_VALUE = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'INITIAL_VALUE' should be an immediate operand.");
                        break;
                    case "INITIAL_STATE":
                        if (param.pvalue.IsImmediateOperand)
                            INITIAL_STATE = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'INITIAL_STATE' should be an immediate operand.");
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
                    case "STABLE_DELAY":
                        if (param.pvalue.IsImmediateOperand)
                            STABLE_DELAY = new TIM((int)param.pvalue.Value(true, 0.0));
                        else
                            STABLE_DELAY = new TIM("TIM", ("TIMEOUT", param.pvalue));
                        break;
                    case "STABLE_POSITIVE_TOLERANCE":
                        if (param.pvalue.IsImmediateOperand)
                            STABLE_POSITIVE_TOLERANCE = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'STABLE_POSITIVE_TOLERANCE' should be an immediate operand.");
                        break;
                    case "STABLE_NEGATIVE_TOLERANCE":
                        if (param.pvalue.IsImmediateOperand)
                            STABLE_NEGATIVE_TOLERANCE = param.pvalue.Value(true, 0.0);
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'STABLE_NEGATIVE_TOLERANCE' should be an immediate operand.");
                        break;
                    case "DELAY_TIME_PRIORITY":
                        if (param.pvalue.IsImmediateOperand)
                            DELAY_TIME_PRIORITY = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'DELAY_TIME_PRIORITY' should be an immediate operand.");
                        break;
                    case "IN_PROPORTION":
                        if (param.pvalue.IsImmediateOperand)
                            IN_PROPORTION = param.pvalue.Value(true, 0.0) == 0.0 ? false : true;
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "The value of property 'IN_PROPORTION' should be an immediate operand.");
                        break;
                }
            }
            if(sp == false || fb == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "ERR([DISABLED], SETPOINT, FEEDBACK, [INITIAL_VALUE], [INITIAL_STATE], [POSITIVE_TOLERANCE], [NEGATIVE_TOLERANCE], [STABLE_POSITIVE_TOLERANCE], [STABLE_NEGATIVE_TOLERANCE], [ON_DELAY], [OFF_DELAY], [STABLE_DELAY], [DELAY_TIME_PRIORITY], [IN_PROPORTION])");
        }

        public override JsonNode ToJson()
        {
            JsonNode node = new JsonObject();
            node["TYPE"] = Tag;
            if (DISABLED != null)
                node["DISABLED"] = DISABLED.ToString();

            node["SETPOINT"] = SETPOINT.ToString();
            node["FEEDBACK"] = FEEDBACK.ToString();

            if(INITIAL_VALUE.HasValue)
                node["INITIAL_VALUE"] = INITIAL_VALUE;
            if (INITIAL_STATE.HasValue)
                node["INITIAL_STATE"] = INITIAL_STATE;
            if ( POSITIVE_TOLERANCE.HasValue)
                node["POSITIVE_TOLERANCE"] = POSITIVE_TOLERANCE;
            if(NEGATIVE_TOLERANCE.HasValue)
                node["NEGATIVE_TOLERANCE"] = NEGATIVE_TOLERANCE;
            if (STABLE_POSITIVE_TOLERANCE.HasValue)
                node["STABLE_POSITIVE_TOLERANCE"] = STABLE_POSITIVE_TOLERANCE;
            if (STABLE_NEGATIVE_TOLERANCE.HasValue)
                node["STABLE_NEGATIVE_TOLERANCE"] = STABLE_NEGATIVE_TOLERANCE;
            if (ON_DELAY != null)
                node["ON_DELAY"] = ON_DELAY.ToJson();
            if(OFF_DELAY != null)
                node["OFF_DELAY"] = OFF_DELAY.ToJson();
            if(STABLE_DELAY != null)
                node["STABLE_DELAY"] = STABLE_DELAY.ToJson();
            if (DELAY_TIME_PRIORITY.HasValue)
                node["DELAY_TIME_PRIORITY"] = DELAY_TIME_PRIORITY;
            if (IN_PROPORTION.HasValue)
                node["IN_PROPORTION"] = IN_PROPORTION;
            return node;
        }
    }
}
