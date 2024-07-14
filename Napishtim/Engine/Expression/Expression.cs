using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public class Expression: IEquatable<Expression>
    {
        private static readonly char[,] __PRIORITY = new char[7,7]
		{ 
			//       '+'  '-'  '*'  '/'  '('  ')'  '\0'    
			/*'+'*/{ '>', '>', '<', '<', '<', '>', '>' },
			/*'-'*/{ '>', '>', '<', '<', '<', '>', '>' },
			/*'*'*/{ '>', '>', '>', '>', '<', '>', '>' },
			/*'/'*/{ '>', '>', '>', '>', '<', '>', '>' },
			/*'('*/{ '<', '<', '<', '<', '<', '=', 'X' },
			/*')'*/{ '>', '>', '>', '>', 'X', '>', '>' },
		   /*'\0'*/{ '<', '<', '<', '<', '<', 'X', '=' },
		};
        private static Expression __zero = new Expression(new ImmediateOperand(0));
        private static Expression __one = new Expression(new ImmediateOperand(1));

        public static Expression ZERO { get; } = __zero;
        public static Expression ONE { get; } = __one;
        public static Expression NUMBER_EXPR(double value)
        {
            return new Expression(new ImmediateOperand(value));
        }

        public static Expression USER_VARIABLE_EXPR(uint index)
        {
            return new Expression(new EnvVariableReference($"&USER{index}"));
        }

        public static Expression OBJECT_REFERENCE_EXPR(uint index)
        {
            return new Expression(new ObjectReference($"@0x{index:X8}"));
        }

        private static Regex __OBJECT_REFERENCE_SEARCH_PATTERN = new Regex("@0[xX][0-9a-fA-F]{1,8}", RegexOptions.Compiled);
        private static Regex __ENV_VARIABLE_REFERENCE_SEARCH_PATTERN = new Regex("&(DEBUG|TICK|STEP|RETURN|STBEGIN|STDURA|USER([0-9]{1,3}))", RegexOptions.Compiled);
        private static Regex __IMMEDIATE_OPERAND_SEARCH_PATTERN = new Regex("(-?(([1-9][0-9]{0,})|0))(\\.[0-9]{1,})?", RegexOptions.Compiled);
        private static Regex __PATH_SEARCH_PATTERN = new Regex("[a-zA-z_]+[0-9a-zA-z_]*\\(", RegexOptions.Compiled);
        private List<Element> __post_order_vector = new List<Element>();
        private List<Element> __original_order_vector = new List<Element>();
        private List<double> __post_order_buffer = new List<double>();
        private readonly string __original_expression;
        private HashSet<ObjectReference> __object_reference_set = new HashSet<ObjectReference>();
        private HashSet<EnvVariableReference> __env_variable_set = new HashSet<EnvVariableReference>();

        public IEnumerable<Element> PostOrderElements => __post_order_vector;
        public IEnumerable<Element> OriginalOrderElements => __original_order_vector;
        public IEnumerable<ObjectReference> ObjectReferences => __object_reference_set;
        public IEnumerable<EnvVariableReference> EnvVariableReferences => __env_variable_set;

        public IEnumerable<uint> UserVariablesUsage => EnvVariableReferences.Where(x => x.EnvType == ENV_VARIABLE_TYPE_T.USER).Select(x => x.UserVariableIndex);

        public Expression(ImmediateOperand op)
        {
            __original_expression = op.ToString();
            __original_order_vector.Add(op);
            __original_order_vector.Add(Operator.MAKE('\0'));
            __post_order_vector.Add(op);
        }

        public Expression(EnvVariableReference env)
        {
            __original_expression = env.ToString();
            __original_order_vector.Add(env);
            __original_order_vector.Add(Operator.MAKE('\0'));
            __post_order_vector.Add(env);
            __env_variable_set.Add(env);
        }

        public Expression(ObjectReference obj)
        {
            __original_expression = obj.ToString();
            __original_order_vector.Add(obj);
            __original_order_vector.Add(Operator.MAKE('\0'));
            __post_order_vector.Add(obj);
            __object_reference_set.Add(obj);
        }

        public Expression(string expr, Sprite? upper)
        {
            __original_expression = expr;
            Stack<Operator> operatorStack = new Stack<Operator>();
           
            ELEMENT_TYPE_T type = ELEMENT_TYPE_T.NONE;
            int start = 0, next = 0;

            bool res = true;
            while(res)
            {
                res = __FIND_NEXT_ELEMENT(expr, start, out next, out type);
                if (res)
                {
                    switch(type)
                    {
                        case ELEMENT_TYPE_T.OPERATOR:
                            __original_order_vector.Add(Operator.MAKE(expr[start]));
                            break;
                        case ELEMENT_TYPE_T.OBJECT_REFERENCE:
                            var obj = new ObjectReference(expr.Substring(start, next - start));
                            __original_order_vector.Add(obj);
                            __object_reference_set.Add(obj);
                            break;
                        case ELEMENT_TYPE_T.ENV_REFERENCE:
                            var env = new EnvVariableReference(expr.Substring(start, next - start));
                            __original_order_vector.Add(env);
                            __env_variable_set.Add(env);
                            break;
                        case ELEMENT_TYPE_T.IMMEDIATE_OPERAND:
                            __original_order_vector.Add(new ImmediateOperand(expr.Substring(start, next - start)));
                            break;
                        case ELEMENT_TYPE_T.SPRITE:
                            __original_order_vector.Add(new Sprite(expr.Substring(start, next - start), upper));
                            break;
                        default:
                            break;
                    }
                    start = next;
                }
            }
            if (start < expr.Length)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, expr);

            __original_order_vector.Add(Operator.MAKE('\0'));
            operatorStack.Push(Operator.MAKE('\0'));
            foreach (var e in __original_order_vector)
            {
                if(e.Type == ELEMENT_TYPE_T.OPERATOR)
                {
                    while(operatorStack.Count != 0)
                    {
                        char priority = __PRIORITY[(int)operatorStack.Peek().Name, (int)(e as Operator).Name];

                        if (priority == '<')
                        {
                            operatorStack.Push(e as Operator);
                            break;
                        }
                        else if (priority == '=')
                        {
                            operatorStack.Pop();
                            break;
                        }
                        else if (priority == 'X')
                        {
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, expr);
                        }
                        else if (priority == '>')
                        {
                            __post_order_vector.Add(operatorStack.Peek());
                            operatorStack.Pop();
                        }
                    }
                }
                else
                {
                    switch(e.Type)
                    {
                        case ELEMENT_TYPE_T.OBJECT_REFERENCE:
                        case ELEMENT_TYPE_T.IMMEDIATE_OPERAND:
                        case ELEMENT_TYPE_T.ENV_REFERENCE:
                        case ELEMENT_TYPE_T.SPRITE:
                            __post_order_vector.Add(e);
                            break;
                        default:break;
                    }
                }
                if (operatorStack.Count == 0)
                    break;
            }
            if (operatorStack.Count() != 0 || __post_order_vector.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, expr);

            __post_order_walk_through();
        }

        public override string ToString()
        {
            return String.Join("", __original_order_vector.Where(e => e.Type != ELEMENT_TYPE_T.OPERATOR || (e as Operator).Name != OPERATOR_T.TERMINAL).Select(e => e.ToString()));
        }

        private static bool __FIND_NEXT_ELEMENT(string expr, int start, out int next, out ELEMENT_TYPE_T type)
        {
            type = ELEMENT_TYPE_T.NONE;
            next = 0;
            int counter = 1;

            if (start >= expr.Length)
                return false;
            var match = __PATH_SEARCH_PATTERN.Match(expr, start);
            if(match.Success && match.Index == start)
            {
                int i;
                for(i = match.Index + match.Length; i < expr.Length && counter != 0; ++i)
                {
                    if (expr[i] == ')') --counter;
                    else if (expr[i] == '(') ++counter;
                }

                if (counter == 0)
                {
                    type = ELEMENT_TYPE_T.SPRITE;
                    next = i;
                    return true;
                }
                else
                    return false;
            }

            match = __OBJECT_REFERENCE_SEARCH_PATTERN.Match(expr, start);
            if (match.Success && match.Index == start)
            {
                type = ELEMENT_TYPE_T.OBJECT_REFERENCE;
                next = match.Index + match.Length;
                return true;
            }

            match = __ENV_VARIABLE_REFERENCE_SEARCH_PATTERN.Match(expr, start);
            if (match.Success && match.Index == start)
            {
                type = ELEMENT_TYPE_T.ENV_REFERENCE;
                next = match.Index + match.Length;
                return true;
            }


            if (__IS_VALID_OPERATOR(expr, start))
            {
                type = ELEMENT_TYPE_T.OPERATOR;
                next = start + 1;
                return true;
            }

            match = __IMMEDIATE_OPERAND_SEARCH_PATTERN.Match(expr, start);
            if (match.Success && match.Index == start)
            {
                type = ELEMENT_TYPE_T.IMMEDIATE_OPERAND;
                next = match.Index + match.Length;
                return true;
            }

            return false;
        }

        private static bool __IS_VALID_OPERATOR(string str, int pos)
        {
            if (Operator.IS_VALID_OPERATOR(str[pos]) == false)
                return false;
            else
            {
                if (str[pos] == '-' && Operator.IS_VALID_OPERATOR(pos < str.Length - 1? str[pos + 1]:'\0') == false)
                {
                    if (pos == 0)
                        return false;
                    else if (pos > 0 && Operator.IS_VALID_OPERATOR(str[pos - 1]) == true && str[pos - 1] != ')')
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
        }

        public double Value(bool dividbyzeroException, double defaultValue)
        {
            __post_order_buffer.Clear();
            foreach(var e in __post_order_vector)
            {
                if (e.Type != ELEMENT_TYPE_T.OPERATOR)
                    __post_order_buffer.Add((e as Operand).Value());
                else
                {
                    if(__post_order_buffer.Count < 2)
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);

                    switch((e as Operator).Name)
                    {
                        case OPERATOR_T.ADD:
                            __post_order_buffer[__post_order_buffer.Count - 2] += __post_order_buffer[__post_order_buffer.Count - 1];
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.SUBTRACT:
                            __post_order_buffer[__post_order_buffer.Count - 2] -= __post_order_buffer[__post_order_buffer.Count - 1];
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.MULTIPLY:
                            __post_order_buffer[__post_order_buffer.Count - 2] *= __post_order_buffer[__post_order_buffer.Count - 1];
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.DIVIDE:
                            if (dividbyzeroException && __post_order_buffer[__post_order_buffer.Count - 1] == 0)
                                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_ZERO_DIVISION, __original_expression);
                            __post_order_buffer[__post_order_buffer.Count - 2] /= __post_order_buffer[__post_order_buffer.Count - 1];
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        default:
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);
                    }
                }
            }

            if (__post_order_buffer.Count == 1)
                return __post_order_buffer[0];
            else if (__post_order_buffer.Count == 0)
                return defaultValue;
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);
        }

        private void __post_order_walk_through()
        {
            __post_order_buffer.Clear();
            foreach (var e in __post_order_vector)
            {
                if (e.Type != ELEMENT_TYPE_T.OPERATOR)
                    __post_order_buffer.Add(1.0);
                else
                {
                    if (__post_order_buffer.Count < 2)
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);

                    switch ((e as Operator).Name)
                    {
                        case OPERATOR_T.ADD:
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.SUBTRACT:
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.MULTIPLY:
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        case OPERATOR_T.DIVIDE:
                            __post_order_buffer.RemoveAt(__post_order_buffer.Count - 1);
                            break;
                        default:
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);
                    }
                }
            }

            if (__post_order_buffer.Count == 1)
                return;
            else if (__post_order_buffer.Count == 0)
                return;
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, __original_expression);
        }

        public bool Equals(Expression? other)
        {
            if(other == null)
                return false;
            else
                return __original_order_vector.Count == other.__original_order_vector.Count &&
                    __original_order_vector.Zip(other.__original_order_vector).All(x => x.First.Equals(x.Second));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool IsImmediateOperand {
            get { return __original_order_vector.All(x => x is ImmediateOperand || x is Operator); }
        }
    }
}
