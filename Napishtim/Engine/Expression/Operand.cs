using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{

    public abstract class Operand: Element, IEquatable<Operand>
    {
        public abstract double Value();
        public abstract void Assign(double value);
        public abstract void Assign(Operand value);

        public bool Equals(Operand? other)
        {
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


}
