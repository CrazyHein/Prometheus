using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public enum ELEMENT_TYPE_T
    {
        NONE,
        OBJECT_REFERENCE,
        IMMEDIATE_OPERAND,
        ENV_REFERENCE,
        OPERATOR,
        SPRITE,
    }
    public abstract class Element: IEquatable<Element>
    {
        public abstract ELEMENT_TYPE_T Type { get; }

        public bool Equals(Element? other)
        {
            switch(Type)
            {
                case ELEMENT_TYPE_T.IMMEDIATE_OPERAND:
                    return (this as ImmediateOperand).Equals(other as ImmediateOperand);
                case ELEMENT_TYPE_T.OBJECT_REFERENCE:
                    return (this as ObjectReference).Equals(other as ObjectReference);
                case ELEMENT_TYPE_T.ENV_REFERENCE:
                    return (this as EnvVariableReference).Equals(other as EnvVariableReference);
                case ELEMENT_TYPE_T.OPERATOR:
                    return (this as Operator).Equals(other as Operator);
                case ELEMENT_TYPE_T.SPRITE:
                    return (this as Sprite).Equals(other as Sprite);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            switch (Type)
            {
                case ELEMENT_TYPE_T.IMMEDIATE_OPERAND:
                    return (this as ImmediateOperand).GetHashCode();
                case ELEMENT_TYPE_T.OBJECT_REFERENCE:
                    return (this as ObjectReference).GetHashCode();
                case ELEMENT_TYPE_T.ENV_REFERENCE:
                    return (this as EnvVariableReference).GetHashCode();
                case ELEMENT_TYPE_T.OPERATOR:
                    return (this as Operator).GetHashCode();
                case ELEMENT_TYPE_T.SPRITE:
                    return (this as Sprite).GetHashCode();
                default:
                    return base.GetHashCode();
            }
        }
    }
}
