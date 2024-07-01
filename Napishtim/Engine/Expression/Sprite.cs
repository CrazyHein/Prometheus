using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public class Sprite : Operand, IEquatable<Sprite>
    {
        private List<Expression> __parameters = new List<Expression>();
        private ArithmeticUnit __au;
        public IEnumerable<Expression> Parameters => __parameters;
        public string ArithmeticUnitName => __au.Name;
        public int Layer { get; private init; }
        public const int MaxNesting = 3;

        public override ELEMENT_TYPE_T Type => ELEMENT_TYPE_T.SPRITE;

        public Sprite(string value, Sprite? upper): base()
        {
            Layer = upper == null ? 0 : upper.Layer + 1;
            if (Layer == MaxNesting)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_SPRITE_NESTING_DEPTH_OUT_OF_RANGE, $"The maximum nesting depth of sprite invoking is {MaxNesting}.");
            int counter = 1;
            int i = 0, j = 0;
            while(i < value.Length)
            {
                if (value[i] == '(')
                    break;
                i++;
            }
            string name = value.Substring(0, i);

            i++;
            j = i;
            while(i < value.Length)
            {
                if (value[i] == '(')
					counter++;
                else if (value[i] == ')')
					counter--;
                if ((value[i] == ',' && counter == 1) || (value[i] == ')' && counter == 0))
                {
                    __parameters.Add(new Expression(value.Substring(j, i - j), this));
                    j = i + 1;
                }
                i++;
            }

            __au = ArithmeticUnit.MAKE(name, __parameters);
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
            return __au.Value();
        }

        public override string ToString()
        {
            return __au.ToString();
        }

        public bool Equals(Sprite? other)
        {
            if (other == null)
                return false;
            else
                return ArithmeticUnitName == other.ArithmeticUnitName && 
                    __parameters.Count == other.__parameters.Count && 
                    __parameters.Zip(other.__parameters).All(x => x.First.Equals(x.Second));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
