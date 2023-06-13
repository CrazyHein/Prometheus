﻿using System;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public enum LOMBARDIA_ERROR_CODE_T : uint
    {
        NO_ERROR = 0x00000000,

        UNSUPPORTED_FILE_FORMAT_VERSION = 0x00000002,
        ELEMENT_MISSING = 0x00000003,
        FILE_DATA_EXCEPTION = 0x00000004,

        ILLEGAL_DATA_TYPE_DEFINITION = 0x00000005,
        DUPLICATED_DATA_TYPE_NAME = 0x00000006,
        UNDEFINED_DATA_TYPE_NAME = 0x00000007,

        INVALID_VARIABLE_DATA_TYPE = 0x00000010,
        DUPLICATED_MODEL_ID = 0x00000011,

        VARIABLE_UNFOUND = 0x00000020,
        DUPLICATED_VARIABLE = 0x00000021,
        VARIABLE_NOT_BE_SUBSCRIBED = 0x00000022,
        VARIABLE_BE_SUBSCRIBED = 0x00000023,
        INVALID_VARIABLE_NAME = 0x00000024,
        INVALID_VARIABLE_UNIT = 0x00000025,

        INVALID_CONTROLLER_EXTENSION_MODEL = 0x00000030,
        DUPLICATED_CONTROLLER_MODULE_REFERENCE = 0x00000031,
        INVALID_MODULE_LOCAL_ADDRESS = 0x0000032,
        INVALID_MODULE_IPV4_ADDRESS = 0x0000033,
        CONTROLLER_MODULE_NOT_BE_SUBSCRIBED = 0x00000034,
        CONTROLLER_MODULE_BE_SUBSCRIBED = 0x00000035,
        CONTROLLER_MODULE_UNFOUND = 0x00000036,
        INVALID_CONTROLLER_MODULE_REFERENCE = 0x00000037,
        CONTROLLER_MODULE_LOCAL_ADDRESS_OVERLAPPING = 0x00000038,

        INVALID_OBJECT_VARIABLE_NAME = 0x00000040,
        INVALID_OBJECT_BINDING_INFO = 0x00000041,
        INVALID_OBJECT_BINDING_MODULE_NAME = 0x00000042,
        INVALID_OBJECT_BINDING_CHANNEL_NAME = 0x00000043,
        INVALID_OBJECT_BINDING_CHANNEL_INDEX = 0x00000044,
        INVALID_OBJECT_RANGE_INFO = 0x00000045,
        INVALID_OBJECT_CONVERTER_INFO = 0x00000046,
        DUPLICATED_OBJECT_INDEX = 0x00000047,
        OBJECT_UNFOUND = 0x00000048,
        OBJECT_NOT_BE_SUBSCRIBED = 0x00000049,
        OBJECT_BE_SUBSCRIBED = 0x0000004A,

        INVALID_OBJECT_REFERENCE_IN_DATA_IMAGE = 0x00000050,
        INVALID_OBJECT_DATA_TYPE_IN_DATA_IMAGE = 0x00000051,
        PROCESS_DATA_IMAGE_SIZE_OUT_OF_RANGE = 0x00000052,
        DUPLICATED_OBJECT_REFERENCE_IN_DATA_IMAGE = 0x00000053,
        PROCESS_DATA_NOT_BE_SUBSCRIBED = 0x00000054,
        PROCESS_DATA_BE_SUBSCRIBED = 0x00000055,
        PROCESS_DATA_UNFOUND = 0x00000056,
        INVALID_BINDING_CHANNEL_IN_DATA_IMAGE = 0x00000057,
        PROCESS_DATA_IMAGE_ADDRESS_OVERLAPPING = 0x00000058,

        INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE = 0x00000060,
        INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION = 0x00000061,
        INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT = 0x00000062,
        INVALID_PROCESS_DATA_REFERENCE_IN_INTERLOCK = 0x00000063,

        INVALID_MC_SERVER_IPV4_ADDRESS = 0x00000070,
        INVALID_BASIC_INFO_ELEMENT_NODE = 0x00000071,
        INVALID_BASIC_INFO_VARIABLE_DICTIONARY_NAME = 0x00000072,

        VARIABLE_DICTIONARY_IO_LIST_SAME_NAME = 0x00000100,

        DUPLICATED_CUSTOM_FIELD_NAME_IN_LOCAL_MODULE = 0x00000200,
        DUPLICATED_CUSTOM_FIELD_NAME_IN_REMOTE_MODULE = 0x00000201,
        INVALID_HOST_CPU_ADDRESS = 0x00000202,

        POSIX_PRIORITY_OUT_OF_RANGE = 0x00000300,
        INVALID_TIMER_CYCLE_SETTING = 0x00000301,
        INVALID_TIME_OUT_SETTING = 0x00000302,

        DUPLICATED_WORKSHEET_NAME = 0x00000400,

        RUNTIME_ERROR = 0x80000000
    }

    public class LombardiaException : Exception
    {
        public LOMBARDIA_ERROR_CODE_T ExceptionCode { get; private set; }
        public Exception? RuntimeException { get; private set; } = null;

        public LombardiaException(LOMBARDIA_ERROR_CODE_T code)
        {
            ExceptionCode = code;
        }

        public LombardiaException(Exception exp)
        {
            ExceptionCode = LOMBARDIA_ERROR_CODE_T.RUNTIME_ERROR;
            RuntimeException = exp;
        }

        public override string ToString()
        {
            if (ExceptionCode != LOMBARDIA_ERROR_CODE_T.RUNTIME_ERROR)
                return ExceptionCode.ToString();
            else if (RuntimeException != null)
                return RuntimeException.ToString();
            else
                return "UNDEFINED_RUNTIME_ERROR";
        }

        public override string Message
        {
            get
            {
                if (ExceptionCode != LOMBARDIA_ERROR_CODE_T.RUNTIME_ERROR)
                    return ExceptionCode.ToString();
                else if (RuntimeException != null)
                    return RuntimeException.Message;
                else
                    return "UNDEFINED_RUNTIME_ERROR";
            }
        }

    }
}
