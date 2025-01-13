using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public class ImmediateOperand: Operand, IEquatable<ImmediateOperand>
    {
        private double __value;
        public static readonly Regex PATTERN = new Regex("^(-?(([1-9][0-9]{0,})|0))(\\.[0-9]{1,})?$", RegexOptions.Compiled);

        public override ELEMENT_TYPE_T Type => ELEMENT_TYPE_T.IMMEDIATE_OPERAND;

        public ImmediateOperand(string value): base()
        {
            if (PATTERN.IsMatch(value) && double.TryParse(value, out var result))
                __value = result;
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, $"{value} -> double");
        }

        public ImmediateOperand(double value)
        {
            __value = value;
        }

        public override string ToString()
        {
            return __value.ToString("0.#################");
        }

        public override double Value()
        {

            return __value;
        }

        public override void Assign(double value)
        {
            throw new NotImplementedException();
        }

        public override void Assign(Operand value)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ImmediateOperand? other)
        {
            if (other == null)
                return false;
            else
                return __value == other.Value();
        }

        public override int GetHashCode()
        {
            return __value.GetHashCode();
        }
    }
}
