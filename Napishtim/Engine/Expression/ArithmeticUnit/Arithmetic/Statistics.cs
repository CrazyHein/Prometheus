using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("MAX(v0, ...): Returns the maximum value of all input variables.")]
    public class MAX : ArithmeticUnit
    {
        public MAX(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }


        public override string Name => "MAX";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("MIN(v0, ...): Returns the minimum value of all input variables.")]
    public class MIN : ArithmeticUnit
    {
        public MIN(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "MIN";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("SUM(v0, ...): Returns the sum of all input variables.")]
    public class SUM : ArithmeticUnit
    {
        public SUM(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "SUM";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("MEAN(v0, ...): Returns the mean value of all input variables.")]
    public class MEAN : ArithmeticUnit
    {
        public MEAN(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "MEAN";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("DMEAN(v0, ...): Returns the mean value of all input variables.")]
    public class DMEAN : ArithmeticUnit
    {
        public DMEAN(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "DMEAN";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("STDEV(v0, v1, ...): Returns the standard deviation of all input variables.")]
    public class STDEV : ArithmeticUnit
    {
        public STDEV(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count < 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "STDEV";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("DSTDEV(v0, v1, ...): Returns the standard deviation of all input variables.")]
    public class DSTDEV : ArithmeticUnit
    {
        public DSTDEV(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count < 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }

        public override string Name => "DSTDEV";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
