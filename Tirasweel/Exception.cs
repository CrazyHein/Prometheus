using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ
{
    public class DAQException : System.Exception
    {
        public string What { get; private set; }
        public DAQExceptionCode Code { get; private set; }
        public System.Exception RuntimeException { get; private set; }
        public DAQException(DAQExceptionCode code, string msg = "N/A")
        {
            What = msg;
            Code = code;
            RuntimeException = null;
        }

        public DAQException(System.Exception exp)
        {
            Code = DAQExceptionCode.RUNTIME_ERROR;
            What = exp.Message;
            RuntimeException = exp;
        }

        public override string ToString()
        {
            return $"{Code}: {What}";
        }

        public override string Message
        {
            get
            {
                return ToString() ;
            }
        }
    }

    public enum DAQExceptionCode : short
    {
        INVALID_DATAGRAM = 0x1000,
        TRANSACTION_MISMATCH = 0x1001,
        REMOTE_SERVER_DISCONNECTED = 0x1002,
        EXCEPTION_DATAGRAM = 0x1003,
        RUNTIME_ERROR = 0x0050,
    }
}
