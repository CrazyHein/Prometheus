using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Mod", "MOD(x, y): Returns the floating-point remainder of x/y (rounded towards zero).")]
    public class MOD: ArithmeticUnit
    {
        public MOD(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, 
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "MOD";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Mod", "ABS(x): Returns the absolute value of x: |x|.")]
    public class ABS : ArithmeticUnit
    {
        public ABS(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "ABS";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
