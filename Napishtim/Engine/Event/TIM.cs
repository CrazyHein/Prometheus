using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim
{
    [CompatibleEvent("TIM:Timer event. The event is triggered after the specified time(ms) has elapsed.")]
    public class TIM : Event
    {
        public override string Tag => "TIM";

        public Expression.Expression? DISABLED { get; init; }

        private int? __timeout = null;
        public int? TIMEOUT 
        { 
            get { return __timeout; }
            init {
                if (value.HasValue && value.Value < 0)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, $"The timeout period({value}) must be a positive integer.");
                __timeout = value;
            }
        }
        public Expression.Expression? TIMEOUT_EXPR { get; init; } = null;

        public static ReadOnlyCollection<(string name, bool required, string defaultv, string comment)> _ParameterDescriptions { get; } = new ReadOnlyCollection<(string name, bool required, string defaultv, string comment)>
        ([
            ("DISABLED", false, string.Empty, "Optional parameter of type Expression.\nThe value of the expression is used to indicate whether this event is temporarily disabled.\nIf the value is not zero, the event will be temporarily disabled. The default value is 0.0."),
            ("TIMEOUT", true, "0",
            """
            If the parameter is of type Integer32 , the parameter should be a positive integer or zero. In another case, the parameter must be valid Expression string.
            The time-out interval, in milliseconds. If a nonzero value is specified, the evaluation value of event will not be true until the specified interval elapses. If this value is zero, the evaluation value of event is always true.
            """),
        ]);

        public override IEnumerable<(string pname, bool required, string defaultv, string comment)> ParameterDescriptions => _ParameterDescriptions;

        public override IEnumerable<(string pname, string pvalue)> ParameterSettings
        {
            get
            {
                (string pname, string pvalue)[] parameters = new (string pname, string pvalue)[]
                {
                    ("DISABLED", DISABLED == null? string.Empty :DISABLED.ToString()),
                    ("TIMEOUT", _TIMEOUT_STRING),
                };
                return parameters;
            }
        }

        public string _TIMEOUT_STRING
        {
            get
            {
                if (TIMEOUT != null) return TIMEOUT.Value.ToString();
                else return ToJson().ToJsonString();
            }
        }

        public override IEnumerable<ObjectReference> ObjectReferences
        {
            get 
            {
                IEnumerable<ObjectReference> temp = Enumerable.Empty<ObjectReference>();
                if (DISABLED != null)
                    temp = temp.Concat(DISABLED.ObjectReferences);

                if (TIMEOUT != null)
                    temp = temp.Concat(Enumerable.Empty<ObjectReference>());
                else if(TIMEOUT_EXPR != null)
                    temp = temp.Concat(TIMEOUT_EXPR.ObjectReferences);

                return temp.Distinct();
            }
        }

        public override IEnumerable<uint> UserVariablesUsage
        {
            get
            {
                IEnumerable<uint> ret = Enumerable.Empty<uint>();
                if (DISABLED != null)
                    ret = ret.Concat(DISABLED.UserVariablesUsage);

                if (TIMEOUT == null)
                    ret = ret.Concat(TIMEOUT_EXPR.UserVariablesUsage);
                else
                    ret = ret.Concat(Enumerable.Empty<uint>());

                return ret.Distinct();
            }
        }

        public TIM(string name, params (string pname, Expression.Expression pvalue)[] parameters)
        {
            if (name != Tag)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "TIM([DISABLED], TIMEOUT)");

            bool timeout = false;
            foreach (var param in parameters)
            {
                switch (param.pname)
                {
                    case "DISABLED":
                        DISABLED = param.pvalue; break;
                    case "TIMEOUT":
                        if (param.pvalue.IsImmediateOperand)
                            TIMEOUT = (int)parameters[0].pvalue.Value(true, 0.0);
                        else
                            TIMEOUT_EXPR = param.pvalue;
                        timeout = true; 
                        break;
                }
            }

            if(timeout == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_ARGUMENTS_ERROR, "TIM([DISABLED], TIMEOUT)");
        }

        public TIM(int timeout, Expression.Expression? disabled = null)
        {
            TIMEOUT = timeout;
            DISABLED = disabled;
        }

        public TIM(JsonNode node)
        {
            TIMEOUT = 0;
            try 
            {
                if ((string)node["TYPE"] != Tag)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Event type {(string)node["TYPE"]}' is not supported by {Tag} event object.");
                
                if (node.AsObject().TryGetPropertyValue("DISABLED", out var opt))
                    DISABLED = opt.AsValue().GetValue<string>();
                else
                    DISABLED = null;

                var propertyNode = node["TIMEOUT"];
                if(propertyNode == null)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Property with name 'TIMEOUT' must be provided.");
                var t = propertyNode.GetValueKind();
                if (t == System.Text.Json.JsonValueKind.Number && (int)propertyNode >= 0)
                {
                    TIMEOUT = (int)propertyNode;
                    TIMEOUT_EXPR = null;
                }
                else if (t == System.Text.Json.JsonValueKind.String)
                {
                    TIMEOUT_EXPR = new Expression.Expression((string)propertyNode, null);
                    TIMEOUT = null;
                }
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"Property with name 'TIMEOUT' must be of type int or string.");
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_PARSE_ERROR, $"{node.ToString()}\nis not a valid {Tag} event object.", ex);
            }
        }

        public override JsonNode ToJson()
        {
            if (TIMEOUT == null)
            {
                JsonNode node = new JsonObject();
                node["TYPE"] = Tag;
                if (DISABLED != null)
                    node["DISABLED"] = DISABLED.ToString();
                if (TIMEOUT_EXPR != null)
                    node["TIMEOUT"] = TIMEOUT_EXPR.ToString();
                else
                    node["TIMEOUT"] = 0;
                return node;
            }
            else
            {
                JsonNode node = new JsonObject();
                node["TYPE"] = Tag;
                if (DISABLED != null)
                    node["DISABLED"] = DISABLED.ToString();
                node["TIMEOUT"] = JsonValue.Create(TIMEOUT.Value);
                return node;
            }
        }
    }
}
