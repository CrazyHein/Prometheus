using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim
{
    public class NaposhtimException : System.Exception
    {
        public NaposhtimExceptionCode? Code { get; private set; }
        public NaposhtimException(NaposhtimExceptionCode code, string msg = "N/A"): base($"{code}:\n{msg}")
        {
            Code = code;
        }

        public NaposhtimException(NaposhtimExceptionCode code, string msg, Exception exp): base($"{code}:\n{msg}", exp)
        {
            Code = code;
        }

        public override string ToString()
        {
            if(InnerException == null)
                return $"{Message}";
            else
                return $"{Message}\nThe following information may be helpful:\n{InnerException.ToString()}" ;

        }
    }

    public class NaposhtimIOException : NaposhtimException 
    {
        public NaposhtimIOException(NaposhtimExceptionCode code, string msg = "N/A") : base(code, msg)
        {

        }

        public NaposhtimIOException(NaposhtimExceptionCode code, string msg, Exception exp) : base(code, msg, exp)
        {

        }
    }

    public class NaposhtimDataPackageException : NaposhtimException
    {
        public NaposhtimDataPackageException(NaposhtimExceptionCode code, string msg = "N/A") : base(code, msg)
        {

        }


        public NaposhtimDataPackageException(NaposhtimExceptionCode code, string msg, NaposhtimException exp) : base(code, msg, exp)
        {

        }
    }

    public class NaposhtimProtocolException : NaposhtimException
    {
        public NaposhtimProtocolException(NaposhtimExceptionCode code, string msg = "N/A") : base(code, msg)
        {

        }

        public NaposhtimProtocolException(NaposhtimExceptionCode code, string msg, Exception exp) : base(code, msg, exp)
        {

        }
    }

    public class NaposhtimScriptException : NaposhtimException
    {
        public NaposhtimScriptException(NaposhtimExceptionCode code, string msg = "N/A") : base(code, msg)
        {

        }

        public NaposhtimScriptException(NaposhtimExceptionCode code, string msg, Exception exp) : base(code, msg, exp)
        {

        }
    }
    public class NaposhtimDocumentException : NaposhtimException
    {
        public NaposhtimDocumentException(NaposhtimExceptionCode code, string msg = "N/A") : base(code, msg)
        {

        }

        public NaposhtimDocumentException(NaposhtimExceptionCode code, string msg, Exception exp) : base(code, msg, exp)
        {

        }
    }

    public enum NaposhtimExceptionCode : ushort
    {
        
        REMOTE_SERVER_DISCONNECTED                                  = 0x1001,

        INVALID_PACKAGE_HEAD                                        = 0x2001,
        PACKAGE_DATA_LENGTH_OUT_OF_RANGE                            = 0x2002,
        PACKAGE_QUANTITY_OUT_OF_RANGE                               = 0x2003,
        PACKAGE_INDEX_OUT_OF_RANGE                                  = 0x2004,
        PACKAGE_INDEX_CORRUPT                                       = 0x2005,
        PACKAGE_INDEX_DUPLICATED                                    = 0x2006,
        PACKAGE_HEAD_INCONSISTENCY                                  = 0x2007,
        PACKAGE_DATA_CORRUPT                                        = 0x2008,
        PACKAGE_RECV_BUFFER_INSUFFICIENT                            = 0x2009,

        PROTOCOL_DATA_LENGTH_OUT_RANGE                              = 0x3001,
        PROTOCOL_RECV_UNKNOWN_RESPONSE                              = 0x3002,
        PROTOCOL_TRANSID_MISMATCH                                   = 0x3003,
        PROTOCOL_FUNC_CODE_MISMATCH                                 = 0x3004,
        PROTOCOL_SEGMENT_MISMATCH                                   = 0x3005,
        PROTOCOL_SERVER_EXCEPTION                                   = 0x3006,
        PROTOCOL_DATA_CORRUPT                                       = 0x3007,
        PROTOCOL_RECV_BUFFER_INSUFFICIENT                           = 0x3008,

        SCRIPT_EXPRESSION_PARSE_ERROR                               = 0x4001,
        SCRIPT_EXPRESSION_ZERO_DIVISION                             = 0x4002,
        SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR                       = 0x4003,
        SCRIPT_EVENT_PARSE_ERROR                                    = 0x4004,
        SCRIPT_TRIGGER_LOGIC_TREE_DEPTH_OUT_OF_RANGE                = 0x4005,
        SCRIPT_SPRITE_NESTING_DEPTH_OUT_OF_RANGE                    = 0x4006,
        SCRIPT_TRIGGER_PARSE_ERROR                                  = 0x4007,
        SCRIPT_SHADER_PARSE_ERROR                                   = 0x4008,
        SCRIPT_BRANCH_PARSE_ERROR                                   = 0x4009,
        SCRIPT_STEP_PARSE_ERROR                                     = 0x400A,
        SCRIPT_OBJECT_REF_NOT_FOUND                                 = 0x4010,
        SCRIPT_EVENT_REF_NOT_FOUND                                  = 0x4011,
        SCRIPT_SHADER_ARGUMENTS_ERROR                               = 0x4012,
        SCRIPT_EVENT_ARGUMENTS_ERROR                                = 0x4013,
        SCRIPT_ENV_VAR_ARGUMENTS_ERROR                              = 0x4014,
        SCRIPT_EXCEPTION_RESPONSE_PARSE_ERROR                       = 0x4015,
        SCRIPT_INITIALIZATION_PARSE_ERROR                           = 0x4016,

        DOCUMENT_INVALID_OPERATION                                  = 0x5001,
        DOCUMENT_INVALID_ARGUMENTS                                  = 0x5002,
        DOCUMENT_STEP_BUILD_ERROR                                   = 0x5003,
        DOCUMENT_CONTROL_BLK_BUILD_ERROR                            = 0x5004,
        DOCUMENT_EXCEPTION_RSP_BUILD_ERROR                          = 0x5005,
        DOCUMENT_FILE_VERSION_UNSUPPORTED                           = 0x5006,
        DOCUMENT_FILE_ASSEMBLY_MISMATCH                             = 0x5007,

        PROCESS_COMPONENT_ARGUMENTS_ERROR                           = 0x5011,
        PROCESS_COMPONENT_INVALID_OPERATION                         = 0x5012,

        CONTROL_BLOCK_ARGUMENTS_ERROR                               = 0x5023,
        CONTROL_BLOCK_INVALID_OPERATION                             = 0x5024,

        EXCEPTION_HANDLING_ARGUMENTS_ERROR                          = 0x5031,
        EXCEPTION_HANDLING_INVALID_OPERATION                        = 0x5032,

        INITIALIZATION_BLOCK_ARGUMENTS_ERROR                     = 0x5041,
        INITIALIZATION_BLOCK_INVALID_OPERATION                   = 0x5042,

        RUNTIME_EXCEPTION                                           = 0x8000
    }
}
