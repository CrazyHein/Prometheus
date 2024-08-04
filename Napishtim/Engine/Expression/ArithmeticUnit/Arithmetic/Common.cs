using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Common", "INRANGE(min, max, v0, v1, ...): Returns 1 when all the entered variables(v0, v1, ...) are in the specified range([min, max]), and 0 in other cases.")]
    public class INRANGE : ArithmeticUnit
    {
        public INRANGE(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count < 3)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, 
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "INRANGE";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Common", "DIFF(v0, ...): Returns the calculation result of MAX(v0, ...) - MIN(v0, ...).")]
    public class DIFF : ArithmeticUnit
    {
        public DIFF(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "DIFF";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

}
