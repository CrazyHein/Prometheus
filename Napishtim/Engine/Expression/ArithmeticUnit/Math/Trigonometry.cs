using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Trigonometry", "SIN(x): Returns the sine of an angle of x radians.")]
    public class SIN : ArithmeticUnit
    {
        public SIN(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "SIN";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Trigonometry", "COS(x): Returns the cosine of an angle of x radians.")]
    public class COS : ArithmeticUnit
    {
        public COS(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, 
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "COS";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Trigonometry", "TAN(x): Returns the tangent of an angle of x radians.")]
    public class TAN : ArithmeticUnit
    {
        public TAN(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, 
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "TAN";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
