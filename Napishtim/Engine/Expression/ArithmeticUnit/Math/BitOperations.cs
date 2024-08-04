using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [ArithmeticUnitUsage("BitOperation", "AND(a0,...): Converts all input parameters to unsigned 32-bit integers and returns the BIT-AND result of those integers.")]
    public class AND : ArithmeticUnit
    {
        public AND(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "AND";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "OR(a0,...): Converts all input parameters to unsigned 32-bit integers and returns the BIT-OR result of those integers.")]
    public class OR : ArithmeticUnit
    {
        public OR(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "OR";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "NOT(a): Converts input parameter to an unsigned 32-bit integer and returns the BIT-NOT result of the integer.")]
    public class NOT : ArithmeticUnit
    {
        public NOT(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 1)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "NOT";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "XOR(a,b): Converts all input parameters to unsigned 32-bit integers and returns the BIT-XOR result of those integers.")]
    public class XOR : ArithmeticUnit
    {
        public XOR(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "XOR";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "NAND(a0,...): Converts all input parameters to unsigned 32-bit integers and returns the BIT-NAND result of those integers.")]
    public class NAND : ArithmeticUnit
    {
        public NAND(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "NAND";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "NOR(a0,...): Converts all input parameters to unsigned 32-bit integers and returns the BIT-NOR result of those integers.")]
    public class NOR : ArithmeticUnit
    {
        public NOR(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "NOR";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "ISBITON(a,n): Converts 'a' to an unsigned 32-bit integer, returns 1.0 if the n-th bit of the integer is 1 and 0.0 otherwise.")]
    public class ISBITON : ArithmeticUnit
    {
        public ISBITON(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "ISBITON";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "ISBITOFF(a,n): Converts 'a' to an unsigned 32-bit integer, returns 1.0 if the n-th bit of the integer is 0 and 0.0 otherwise.")]
    public class ISBITOFF : ArithmeticUnit
    {
        public ISBITOFF(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "ISBITOFF";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "SETBIT(a,n): Converts 'a' to an unsigned 32-bit integer, sets the n-th bit of the integer to 1 and returns the result.")]
    public class SETBIT : ArithmeticUnit
    {
        public SETBIT(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "SETBIT";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }

    [ArithmeticUnitUsage("BitOperation", "CLRBIT(a,n): Converts 'a' to an unsigned 32-bit integer, clears the n-th bit of the integer to 0 and returns the result.")]
    public class CLRBIT : ArithmeticUnit
    {
        public CLRBIT(IReadOnlyList<Expression> parameters) : base(parameters)
        {
            if (parameters.Count != 2)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR,
                    this.GetType().GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
        }
        public override string Name => "CLRBIT";

        public override double Value()
        {
            throw new NotImplementedException();
        }
    }
}
