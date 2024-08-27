using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public enum ENV_VARIABLE_TYPE_T
    {
        TICK = 1,
        STEP = 2,
        RETURN = 3,
        DEBUG = 4,
        USER = 5,
        STBEGIN = 6,
        STDURA = 7,

        EXTICK = 8,
        EXSTEP = 9,
        EXSTBEGIN = 10,
        EXSTDURA = 11
    }

    public class EnvVariableReference : Operand, IEquatable<EnvVariableReference>
    {
        public static readonly Regex PATTERN = new Regex("^&(DEBUG|TICK|STEP|RETURN|STBEGIN|STDURA|EXTICK|EXSTEP|EXSTBEGIN|EXSTDURA|USER([0-9]{1,3}))$", RegexOptions.Compiled);
        private static Dictionary<ENV_VARIABLE_TYPE_T, string> __ENV_VARIABLE_INFO;
        public static IReadOnlyDictionary<ENV_VARIABLE_TYPE_T, string> ENV_VARIABLE_INFO { get { return __ENV_VARIABLE_INFO; } }
        private ENV_VARIABLE_TYPE_T __env_variable_type;
        private uint __env_user_index = 0;

        public override ELEMENT_TYPE_T Type => ELEMENT_TYPE_T.ENV_REFERENCE;
        public ENV_VARIABLE_TYPE_T EnvType => __env_variable_type;
        public uint UserVariableIndex => __env_user_index;

        static EnvVariableReference()
        {
            __ENV_VARIABLE_INFO = new Dictionary<ENV_VARIABLE_TYPE_T, string>();
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.TICK] = "Returns the value of engine internal clock in milliseconds.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.STEP] = "Returns the number of step that is being executed.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.RETURN] = "Variable that users can freely read and write, usually used to store the result of running the recipe.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.DEBUG] = "For debugging purposes, users should avoid using this variable.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.USER] = "Variable that users can freely read and write, the number of variables available is limited by the system and recipe configuration.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.STBEGIN] = "Returns the value of engine internal clock of the step began in milliseconds.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.STDURA] = "Returns the time(milliseconds) since the current step has been executed.";

            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.EXTICK] = "Returns the value of engine internal clock in milliseconds when an exception occurs.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.EXSTEP] = "Returns the number of step that is being executed when an exception occurs.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.EXSTBEGIN] = "Returns the value of engine internal clock of the step began in milliseconds when an exception occurs.";
            __ENV_VARIABLE_INFO[ENV_VARIABLE_TYPE_T.EXSTDURA] = "Returns the time(milliseconds) since the current step has been executed when an exception occurs.";
        }

        public EnvVariableReference(string value) : base()
        {
            if (PATTERN.IsMatch(value) == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, $"Unknown environment variable: {value}.");
            switch(value)
            {
                case "&DEBUG":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.DEBUG;
                    break;
                case "&TICK":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.TICK;
                    break;
                case "&STEP":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.STEP;
                    break;
                case "&RETURN":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.RETURN;
                    break;
                case "&STBEGIN":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.STBEGIN;
                    break;
                case "&STDURA":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.STDURA;
                    break;
                case "&EXTICK":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.EXTICK;
                    break;
                case "&EXSTEP":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.EXSTEP;
                    break;
                case "&EXSTBEGIN":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.EXSTBEGIN;
                    break;
                case "&EXSTDURA":
                    __env_variable_type = ENV_VARIABLE_TYPE_T.EXSTDURA;
                    break;
                default:
                    __env_variable_type = ENV_VARIABLE_TYPE_T.USER;
                    __env_user_index = uint.Parse(value.Substring(5));
                    break;
            }
        }

        public override string ToString()
        {
            switch(__env_variable_type)
            {
                case ENV_VARIABLE_TYPE_T.TICK:
                    return "&TICK";
                case ENV_VARIABLE_TYPE_T.STEP:
                    return "&STEP";
                case ENV_VARIABLE_TYPE_T.DEBUG:
                    return "&DEBUG";
                case ENV_VARIABLE_TYPE_T.RETURN:
                    return "&RETURN";
                case ENV_VARIABLE_TYPE_T.STBEGIN:
                    return "&STBEGIN";
                case ENV_VARIABLE_TYPE_T.STDURA:
                    return "&STDURA";
                case ENV_VARIABLE_TYPE_T.EXTICK:
                    return "&EXTICK";
                case ENV_VARIABLE_TYPE_T.EXSTEP:
                    return "&EXSTEP";
                case ENV_VARIABLE_TYPE_T.EXSTBEGIN:
                    return "&EXSTBEGIN";
                case ENV_VARIABLE_TYPE_T.EXSTDURA:
                    return "&EXSTDURA";
                case ENV_VARIABLE_TYPE_T.USER:
                    return $"&USER{__env_user_index}";
                default:
                    return "&DEBUG";
            }
        }

        public override void Assign(double value)
        {
            throw new NotImplementedException();
        }

        public override void Assign(Operand value)
        {
            throw new NotImplementedException();
        }

        public override double Value()
        {
            throw new NotImplementedException();
        }

        public bool Equals(EnvVariableReference? other)
        {
            if(other == null)
                return false;
            else
                return other.EnvType == EnvType && other.UserVariableIndex == UserVariableIndex;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
