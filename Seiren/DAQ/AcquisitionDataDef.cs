using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    public enum AcquisitionDataType : byte
    {
        BOOLEAN,
        BYTE,
        SBYTE,
        USHORT,
        SHORT,
        UINT,
        INT,
        DUINT,
        DINT,
        FLOAT,
        DOUBLE,
        FIXEDPOINT3201,
        FIXEDPOINT3202,
        FIXEDPOINT6401,
        FIXEDPOINT6402,
        FIXEDPOINT6404,
        FINGERPRINT,
        UNKNOWN
    }

    public enum AcquisitionDataDisplayFormat : int
    {
        DEC = 10,
        HEX = 16,
        BIN = 2,
        OCT = 8,
        BOOL = 1
    }

    public class AcquisitionDataIndex
    {
        public string FriendlyName { get; init; }
        public uint BitPos { get; init; }
        public uint BytePos { get { return BitPos / 8; } }
        public AcquisitionDataType DataType { get; private set; }

        public AcquisitionDataIndex(string datatype)
        {
            switch(datatype)
            {
                case "BIT":
                    DataType = AcquisitionDataType.BOOLEAN; break;
                case "BYTE":
                    DataType = AcquisitionDataType.BYTE; break;
                case "SBYTE":
                    DataType = AcquisitionDataType.SBYTE; break;
                case "USHORT":
                    DataType = AcquisitionDataType.USHORT; break;
                case "SHORT":
                    DataType = AcquisitionDataType.SHORT; break;
                case "UINT":
                    DataType = AcquisitionDataType.UINT; break;
                case "INT":
                    DataType = AcquisitionDataType.INT; break;
                case "UDINT":
                case "DUINT":
                    DataType = AcquisitionDataType.DUINT; break;
                case "DINT":
                    DataType = AcquisitionDataType.DINT; break;
                case "FLOAT":
                    DataType = AcquisitionDataType.FLOAT; break;
                case "DOUBLE":
                    DataType = AcquisitionDataType.DOUBLE; break;
                case "FIXEDPOINT3201":
                    DataType = AcquisitionDataType.FIXEDPOINT3201; break;
                case "FIXEDPOINT3202":
                    DataType = AcquisitionDataType.FIXEDPOINT3202; break;
                case "FIXEDPOINT6401":
                    DataType = AcquisitionDataType.FIXEDPOINT6401; break;
                case "FIXEDPOINT6402":
                    DataType = AcquisitionDataType.FIXEDPOINT6402; break;
                case "FIXEDPOINT6404":
                    DataType = AcquisitionDataType.FIXEDPOINT6404; break;
                case "FINGERPRINT":
                    DataType = AcquisitionDataType.FINGERPRINT; break;
                default:
                    DataType = AcquisitionDataType.UNKNOWN; break;
            }
        }

        public string DataStringValue(ReadOnlySpan<byte> data, AcquisitionDataDisplayFormat fmt  = AcquisitionDataDisplayFormat.DEC)
        {
            switch(DataType)
            {
                case AcquisitionDataType.BOOLEAN:
                    var v = data[(int)BytePos];
                    int bits = (int)(BitPos % 8);
                    if ((v & (1 <<bits)) == 0)
                        return "0";
                    else
                        return "1";
                case AcquisitionDataType.BYTE:
                    return Convert.ToString(MemoryMarshal.Read<byte>(data.Slice((int)BytePos, 1)), (int)fmt);
                case AcquisitionDataType.SBYTE:
                    return Convert.ToString(MemoryMarshal.Read<sbyte>(data.Slice((int)BytePos, 1)), (int)fmt);
                case AcquisitionDataType.USHORT:
                    return Convert.ToString(MemoryMarshal.Read<ushort>(data.Slice((int)BytePos, 2)), (int)fmt);
                case AcquisitionDataType.SHORT:
                    return Convert.ToString(MemoryMarshal.Read<short>(data.Slice((int)BytePos, 2)), (int)fmt);
                case AcquisitionDataType.UINT:
                    return Convert.ToString(MemoryMarshal.Read<uint>(data.Slice((int)BytePos, 4)), (int)fmt);
                case AcquisitionDataType.INT:
                    return Convert.ToString(MemoryMarshal.Read<int>(data.Slice((int)BytePos, 4)), (int)fmt);
                case AcquisitionDataType.DUINT:
                    if (fmt == AcquisitionDataDisplayFormat.HEX)
                        return MemoryMarshal.Read<ulong>(data.Slice((int)BytePos, 8)).ToString("X");
                    else
                        return MemoryMarshal.Read<ulong>(data.Slice((int)BytePos, 8)).ToString();
                case AcquisitionDataType.DINT:
                    return Convert.ToString(MemoryMarshal.Read<long>(data.Slice((int)BytePos, 8)), (int)fmt);
                case AcquisitionDataType.FLOAT:
                    return MemoryMarshal.Read<float>(data.Slice((int)BytePos, 4)).ToString("G9");
                case AcquisitionDataType.DOUBLE:
                    return MemoryMarshal.Read<double>(data.Slice((int)BytePos, 8)).ToString("G17");
                case AcquisitionDataType.FIXEDPOINT3201:
                    return (MemoryMarshal.Read<int>(data.Slice((int)BytePos, 4)) / 10.0).ToString("F1");
                case AcquisitionDataType.FIXEDPOINT3202:
                    return (MemoryMarshal.Read<int>(data.Slice((int)BytePos, 4)) / 100.0).ToString("F2");
                case AcquisitionDataType.FIXEDPOINT6401:
                    return (MemoryMarshal.Read<long>(data.Slice((int)BytePos, 8)) / 10.0).ToString("F1");
                case AcquisitionDataType.FIXEDPOINT6402:
                    return (MemoryMarshal.Read<long>(data.Slice((int)BytePos, 8)) / 100.0).ToString("F2");
                case AcquisitionDataType.FIXEDPOINT6404:
                    return (MemoryMarshal.Read<long>(data.Slice((int)BytePos, 8)) / 10000.0).ToString("F4");
                case AcquisitionDataType.FINGERPRINT:
                    StringBuilder sb = new StringBuilder(16 * 2);
                    foreach(var b in data.Slice((int)BytePos, 16))
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                default: return "Not yet supported";
            }
        }
    }
}
