using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Polynomial", "POLY(a0, a1, x1, a2, x2, ...): Returns the calculation result of the expression 'a0 + a1*x1 + a2*x2*x2, ...'.")]
    public class POLY : ArithmeticUnit
    {
        public POLY(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count % 2 != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, 
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "POLY";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
