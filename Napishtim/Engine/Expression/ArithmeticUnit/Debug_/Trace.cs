using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Debug", "TRACE(p0, p1, ...): Output the expression text and its value to the system log.")]
    public class TRACE : ArithmeticUnit
    {
        public TRACE(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "TRACE";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Debug", "CTRACE(c, p0, p1, ...): Output the expression text and its value to the system log if c.value() != 0.0.")]
    public class CTRACE : ArithmeticUnit
    {
        public CTRACE(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count <= 1)
                throw new NapishtimScriptException(NapishtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "CTRACE";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
