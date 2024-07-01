using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression
{
    public class ObjectReference: Operand, IEquatable<ObjectReference>
    {
        public static readonly Regex PATTERN = new Regex("^@0[xX][0-9a-fA-F]{1,8}$", RegexOptions.Compiled);
        private uint __object_index;

        public override ELEMENT_TYPE_T Type => ELEMENT_TYPE_T.OBJECT_REFERENCE;
        public uint Index => __object_index;

        public ObjectReference(string value) : base()
        {
            if (PATTERN.IsMatch(value))
                __object_index = Convert.ToUInt32(value.Substring(1), 16);
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_PARSE_ERROR, $"{value} -> uint");

            //if (?.RegisterProcessDataReference(__object_index) == false)
                //throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_OBJECT_REF_NOT_FOUND, $"Can not find object with index {value} in process data images.");
            //else
                //__context = context;
        }

        public override string ToString()
        {
            return $"@0x{__object_index.ToString("X8")}";
        }

        public override void Assign(double value)
        {
            throw new NotImplementedException();
        }

        public override void Assign(Operand value)
        {
            throw new NotImplementedException();
        }

        public override double Value()
        {
            throw new NotImplementedException();
        }

        public bool Equals(ObjectReference? other)
        {
            if (other == null)
                return false;
            else
                return Index == other.Index;
        }

        public override int GetHashCode()
        {
            return (int)Index;
        }
    }
}
