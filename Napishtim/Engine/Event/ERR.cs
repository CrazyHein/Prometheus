using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [CompatibleEvent("ERR:The event is triggered according to whether the following error exceeds the set tolerance.")]
    public class ERR : Event
    {
        public override string Tag => "ERR";
        public Expression.Expression? SETPOINT { get; init; }
        public Expression.Expression? FEEDBACK { get; init; }
        public double? POSITIVE_TOLERANCE { get; init; }
        public double? NEGATIVE_TOLERANCE { get; init; }
        public bool? INITIAL_VALUE { get; init; }
        public TIM? ON_DELAY { get; init; }
        public TIM? OFF_DELAY { get; init; }

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("SETPOINT", true, "0", "The string must be a valid Expression string."),
            ("FEEDBACK", true, "0", "The string must be a valid Expression string."),
            ("POSITIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation. The default value is 0.0."),
            ("NEGATIVE_TOLERANCE", false, string.Empty, "Optional parameter of type Double.\nSpecify the positive tolerance of deviation. The default value is 0.0."),
            ("INITIAL_VALUE", false, string.Empty, "Optional parameter of type Boolean. 0.0 -> false / Others -> true\nSpecify the initial evaluation value of event. The default value is false."),
            ("ON_DELAY", false, string.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
            ("OFF_DELAY", false, string.Empty, "Optional parameter of type Integer32 or JSON object.\nIf the parameter value is a Integer32 , it must be a non-negative integer number;\nIf the parameter value is a JSON object, it must be a valid Expression object;"),
        ]);

        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;

        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("SETPOINT", SETPOINT.ToString()),
                    ("FEEDBACK", FEEDBACK.ToString()),
                    ("POSITIVE_TOLERANCE", POSITIVE_TOLERANCE.HasValue?POSITIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("NEGATIVE_TOLERANCE", NEGATIVE_TOLERANCE.HasValue?NEGATIVE_TOLERANCE.Value.ToString():string.Empty),
                    ("INITIAL_VALUE", (INITIAL_VALUE.HasValue&&INITIAL_VALUE == true)?1.0.ToString():string.Empty),
                    ("ON_DELAY", ON_DELAY!=null?ON_DELAY._TIMEOUT_STRING:string.Empty),
                    ("OFF_DELAY", OFF_DELAY!=null?OFF_DELAY._TIMEOUT_STRING:string.Empty),
                };
                return parameters;
            }
        }

        public override IEnumerable<ObjectReference> ObjectReferences
        {
            get
            {
                var temp = SETPOINT.ObjectReferences.Concat(FEEDBACK.ObjectReferences);
                if(ON_DELAY != null)
                    temp = temp.Concat(ON_DELAY.ObjectReferences);
                if(OFF_DELAY != null)
                    temp = temp.Concat(OFF_DELAY.ObjectReferences);
                return temp.Distinct();
            }
        }

        public override IEnumerable<uint> UserVariablesUsage
        {
            get
            {
                var ret = SETPOINT.UserVariablesUsage.Concat(FEEDBACK.UserVariablesUsage);
                if(ON_DELAY != null)
                    ret.Concat(ON_DELAY.UserVariablesUsage);
                if (OFF_DELAY != null)
                    ret.Concat(OFF_DELAY.UserVariablesUsage);
                return ret.Distinct();
            }
        }

        public ERR(JsonNode node)
        {
            try
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Type name '{(string)node["TYPE"]}' is not supported by {Tag} event object.");
                SETPOINT = new Expression.Expression((string)node["SETPOINT"], null);
                FEEDBACK = new Expression.Expression((string)node["FEEDBACK"], null);

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
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid {Tag} event object.", ex);
            }
        }

        public ERR(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "ERR(SETPOINT, FEEDBACK, [INITIAL_VALUE], [POSITIVE_TOLERANCE], [NEGATIVE_TOLERANCE], [ON_DELAY], [OFF_DELAY])");
            bool sp = false, fb = false;
            foreach (var param in parameters)
            {
                switch (param.pname)
                {
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
            if(sp == false || fb == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "ERR(SETPOINT, FEEDBACK, [INITIAL_VALUE], [POSITIVE_TOLERANCE], [NEGATIVE_TOLERANCE], [ON_DELAY], [OFF_DELAY])");
        }

        public override JsonNode ToJson()
        {
            JsonNode node = new JsonObject();
            node["TYPE"] = Tag;
            node["SETPOINT"] = SETPOINT.ToString();
            node["FEEDBACK"] = FEEDBACK.ToString();
            if(INITIAL_VALUE.HasValue)
                node["INITIAL_VALUE"] = INITIAL_VALUE;
            if( POSITIVE_TOLERANCE.HasValue)
                node["POSITIVE_TOLERANCE"] = POSITIVE_TOLERANCE;
            if(NEGATIVE_TOLERANCE.HasValue)
                node["NEGATIVE_TOLERANCE"] = NEGATIVE_TOLERANCE;
            if(ON_DELAY != null)
                node["ON_DELAY"] = ON_DELAY.ToJson();
            if( OFF_DELAY != null)
                node["OFF_DELAY"] = OFF_DELAY.ToJson();
            return node;
        }
    }
}
