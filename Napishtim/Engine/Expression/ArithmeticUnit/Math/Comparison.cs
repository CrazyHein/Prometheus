using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("Comparison", "EQU(a,b,p_tol,n_tol,t_expr,f_expr): Returns the value of t_expr if p_tol >= (a - b) >= n_tol, and the value of f_expr otherwise.")]
    public class EQU : ArithmeticUnit
    {
        public EQU(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 6)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "EQU";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Comparison", "GTE(a,b,n_tol,t_expr,f_expr): Returns the value of t_expr if (a - b) >= n_tol, and the value of f_expr otherwise.")]
    public class GTE : ArithmeticUnit
    {
        public GTE(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 5)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "GTE";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Comparison", "GRT(a,b,p_tol,t_expr,f_expr): Returns the value of t_expr if (a - b) > p_tol, and the value of f_expr otherwise.")]
    public class GRT : ArithmeticUnit
    {
        public GRT(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 5)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "GRT";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Comparison", "LTE(a,b,p_tol,t_expr,f_expr): Returns the value of t_expr if (a - b) <= p_tol, and the value of f_expr otherwise.")]
    public class LTE : ArithmeticUnit
    {
        public LTE(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 5)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "LTE";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("Comparison", "LES(a,b,n_tol,t_expr,f_expr): Returns the value of t_expr if (a - b) < n_tol, and the value of f_expr otherwise.")]
    public class LES : ArithmeticUnit
    {
        public LES(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 5)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "LES";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
