using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statement = AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim.__Statement;
using TriggerOperator = AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim.TRIGGER_OPERATOR_T;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.NeoTriggerMechansim
{
    internal class Element {
        public required int Priority { get; init; }
    }

    internal class Operator : Element
    {
        public required TriggerOperator Op { get; init; }
        public int OperandCounts { get; set; } = 0;

        public override string ToString()
        {
            return $"<{Op}({Priority})>";
        }
    }

    internal class Operand : Element
    {

    }

    internal class EventOperand : Operand
    {
        public required uint Idx { get; init; }
        public required bool Region { get; init; }

        public override string ToString()
        {
            return $"[{(Region ? "GEVENT" : "EVENT")}{Idx}]";
        }
    }

    internal class IntermediateResult : Operand
    {
        public int Step { get; private set; }
        public string Description { get; private set; }

        public IntermediateResult(int step, Operator op, IEnumerable<Operand> operands)
        {
            Step = step;

            StringBuilder description = new StringBuilder(op.ToString());
            description.Append('(');
            foreach ( Operand operand in operands ) 
                description.Append(operand);
            description.Append(')');
            Description = description.ToString();
        }

        public override string ToString()
        {
            return $"[STEP{Step:D02}()]";
        }
    }


    internal class LogicExpression
    {
        private StringBuilder __expression_string = new StringBuilder();

        //private record OperatorRecord(NeoTriggerMechansim.Operator Op, int Subs)
        //{
        //    public int Subs { get; set; } = Subs;
        //}

        public LogicExpression(IReadOnlyList<Statement> statements)
        {
            //if (statements.Count == 1)
            //{
            //    if(statements[0].Operator == null)
            //        __original_order_vector.Add(new Operand() { Idx = statements[0].Idx, Region = statements[0].Region });
            //    else
            //        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "A certain operator is missing an operand.");
            //}
            //else
            {
                Stack<Operator> operatorStack = new();
                operatorStack.Push(new Operator { Op = TriggerOperator.ROOT, Priority = -1, OperandCounts = 0 });

                Stack<Operand> operandStack = new Stack<Operand>();

                int step = 0;

                foreach (var st in statements.Append(new Statement() { Idx = 0, Region = false, Operator = TriggerOperator.ROOT, Tabs = -1}))
                {
                    while (operatorStack.Count != 0 && operatorStack.Peek().Priority >= st.Tabs)
                    {
                        var opRecord = operatorStack.Pop();

                        if (opRecord.OperandCounts == 0)
                            throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands for '{opRecord.Op}' is incorrect.");

                        operandStack.Push(new IntermediateResult(step, opRecord, Enumerable.Range(0, opRecord.OperandCounts).Select(i => operandStack.Pop())) { Priority = opRecord.Priority});
                        step++;

                        Debug.WriteLine($"Step {(operandStack.Peek() as IntermediateResult).Step:D02}: {(operandStack.Peek() as IntermediateResult).Description}");

                        if (opRecord.Op == TriggerOperator.NOT || opRecord.Op == TriggerOperator.NAND || opRecord.Op == TriggerOperator.NOR)
                        {
                            //if (opRecord.Op == TriggerOperator.NOT && opRecord.OperandCounts != 1)
                            //    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands for '{TriggerOperator.NOT}' is incorrect.");
                            //else if(opRecord.Op != TriggerOperator.NOT && opRecord.OperandCounts == 0)
                            //    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands for '{opRecord.Op}' is incorrect.");

                            var notOperator = new Operator() { Op = TriggerOperator.NOT, Priority = opRecord.Priority };
                            __expression_string.Append(notOperator);
                        }
                    }

                    if(st.Tabs < 0)
                        break;

                    //if (operatorStack.Count == 0)
                    //throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "One of the operands is missing an operator.");
                    var contextOperator = operatorStack.Peek();
                    if (contextOperator.Priority != st.Tabs - 1)
                        throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "One of the operands is missing an operator.");
                    if (contextOperator.Op == TriggerOperator.NOT && contextOperator.OperandCounts > 0)
                        throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands for '{TriggerOperator.NOT}' is incorrect.");
                    if (contextOperator.Op == TriggerOperator.ROOT && contextOperator.OperandCounts > 0)
                        throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands for '{TriggerOperator.ROOT}' is incorrect.");

                    if (contextOperator.OperandCounts > 0)
                    {
                        switch (contextOperator.Op)
                        {
                            case TriggerOperator.NAND:
                                __expression_string.Append(new Operator() { Op = TriggerOperator.AND, Priority = contextOperator.Priority }); break;
                            case TriggerOperator.NOR:
                                __expression_string.Append(new Operator() { Op = TriggerOperator.OR, Priority = contextOperator.Priority }); break;
                            default:
                                __expression_string.Append(new Operator() { Op = contextOperator.Op, Priority = contextOperator.Priority }); break;
                        }
                    }
                    if (st.Operator == null)
                    {
                        operandStack.Push(new EventOperand() { Idx = st.Idx, Region = st.Region, Priority = st.Tabs });
                        __expression_string.Append(operandStack.Peek());
                    }
                    else
                        operatorStack.Push(new Operator() { Op = (TriggerOperator)st.Operator, Priority = st.Tabs, OperandCounts = 0 });

                    contextOperator.OperandCounts++;
                }


                if (__expression_string.Length == 0)
                    throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "A certain operator is missing operands.");
            }
        }
    }
}
