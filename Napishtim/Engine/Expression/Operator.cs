using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public enum OPERATOR_T
    {
        ADD = 0,
        SUBTRACT = 1,
        MULTIPLY = 2,
        DIVIDE = 3,
        LEFT_PARENTHESIS = 4,
        RIGHT_PARENTHESIS = 5,
        TERMINAL = 6
    }
    public class Operator: Element, IEquatable<Operator>
    {
        private OPERATOR_T __operator;
        static Operator[] __operators;
        static Operator()
        {
            __operators = new Operator[]
            {
                new Operator('+'),
                new Operator('-'),
                new Operator('*'),
                new Operator('/'),
                new Operator('('),
                new Operator(')'),
                new Operator('\0')
            };
        }

        public static Operator MAKE(char ch)
        {
            switch (ch)
            {
                case '+':
                    return __operators[(int)OPERATOR_T.ADD];
                case '-':
                    return __operators[(int)OPERATOR_T.SUBTRACT];
                case '*':
                    return __operators[(int)OPERATOR_T.MULTIPLY];
                case '/':
                    return __operators[(int)OPERATOR_T.DIVIDE];
                case '\0':
                    return __operators[(int)OPERATOR_T.TERMINAL];
                case '(':
                    return __operators[(int)OPERATOR_T.LEFT_PARENTHESIS];
                case ')':
                    return __operators[(int)OPERATOR_T.RIGHT_PARENTHESIS];
                default:
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, $"Unknown operator: {ch}");
            }
        }

        protected Operator(char ch): base()
        {
            switch (ch)
            {
                case '+':
                    __operator = OPERATOR_T.ADD;
                    break;
                case '-':
                    __operator = OPERATOR_T.SUBTRACT;
                    break;
                case '*':
                    __operator = OPERATOR_T.MULTIPLY;
                    break;
                case '/':
                    __operator = OPERATOR_T.DIVIDE;
                    break;
                case '\0':
                    __operator = OPERATOR_T.TERMINAL;
                    break;
                case '(':
                    __operator = OPERATOR_T.LEFT_PARENTHESIS;
                    break;
                case ')':
                    __operator = OPERATOR_T.RIGHT_PARENTHESIS;
                    break;
                default:
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, $"Unknown operator: {ch}");
            }
        }
        public override string ToString()
        {
            switch (__operator)
            {
                case OPERATOR_T.ADD:
                    return "+";
                case OPERATOR_T.SUBTRACT:
                    return "-";
                case OPERATOR_T.MULTIPLY:
                    return "*";
                case OPERATOR_T.DIVIDE:
                    return "/";
                case OPERATOR_T.LEFT_PARENTHESIS:
                    return "(";
                case OPERATOR_T.RIGHT_PARENTHESIS:
                    return ")";
                case OPERATOR_T.TERMINAL:
                    return "\0";
                default:
                    return "\0";
            }
        }

        public static bool IS_VALID_OPERATOR(char ch)
        {
           return "+-*/()\0".Contains(ch);
        }

        public OPERATOR_T Name { get { return __operator; } }

        public override ELEMENT_TYPE_T Type => ELEMENT_TYPE_T.OPERATOR;

        public bool Equals(Operator? other)
        {
            if (other == null)
                return false;
            else
                return __operator == other.Name;
        }

        public override int GetHashCode()
        {
            return (int)__operator;
        }
    }
}
