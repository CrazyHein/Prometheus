using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Protocol
{
    public class Common
    {
        private static readonly uint __FLAG;
        public static readonly int USER_DATA_SIZE_IN_BYTE = (DataPackager.PER_PACKAGE_SIZE - Marshal.SizeOf<DATA_PACKAGE_HEAD_T>()) * DataPackager.MAX_PACKAGE_QUANTITY;
        public static readonly int MAX_DOWNLOAD_DATA_SIZE_IN_BYTE = USER_DATA_SIZE_IN_BYTE - Marshal.SizeOf<RECIPE_DOWNLOAD_REQUEST_FRAME_T>();
        public static readonly int MAX_ECHO_DATA_SIZE_BYTE = USER_DATA_SIZE_IN_BYTE - Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>();
        public static readonly int MAX_DOWNLOAD_STR_SIZE_IN_BYTE = USER_DATA_SIZE_IN_BYTE - Marshal.SizeOf<RECIPE_DOWNLOAD_REQUEST_FRAME_T>() - 1;
        public static readonly int MAX_ECHO_STR_SIZE_BYTE = USER_DATA_SIZE_IN_BYTE - Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>() - 1;

        static Common()
        {
            __FLAG = 0;
            foreach (var b in Encoding.ASCII.GetBytes("ARK").Reverse())
            {
                __FLAG = __FLAG << 8;
                __FLAG += b;
            }
            __FLAG = __FLAG << 8;
        }
        public static uint MakeProtocolHead(byte index) => ((uint)index | __FLAG);
    }
    enum FUNC_CODE_T : Byte
    {
        DOWNLOAD = 0x01,
        ECHO = 0x02,
        CLEAR = 0x03,
        INFO = 0x10,

        RESPONSE_FLAG = 0x80
    }

    public enum RECIPE_SEGMENT_T : Byte
    {
        GLOBAL_EVENT = 0x01,
        SCROLL = 0x03,
        STEP = 0x04,
        EXCEPTION = 0x05,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PROTOCOL_HEAD_T
    {
        public uint trans_head;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FUNCTION_HEAD_T
    {
        public FUNC_CODE_T func_code;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_DOWNLOAD_REQUEST_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint content_size_in_byte;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_DOWNLOAD_RESPONSE_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint content_size_in_byte;
        public int exception;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_ECHO_REQUEST_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint start_index;
        public uint end_index;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_ECHO_RESPONSE_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint start_index;
        public uint end_index;
        public int exception;
        public uint content_size_in_byte;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_CLEAR_REQUEST_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint start_index;
        public uint end_index;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_CLEAR_RESPONSE_HEAD_T
    {
        public RECIPE_SEGMENT_T segment;
        public uint start_index;
        public uint end_index;
        public int exception;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ENGINE_INFO_RESPONSE_HEAD_T
    {
        public int exception;
        public uint version;
        public uint step_capacity;
        public uint throughput;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_DOWNLOAD_REQUEST_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_DOWNLOAD_REQUEST_HEAD_T request;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_DOWNLOAD_RESPONSE_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_DOWNLOAD_RESPONSE_HEAD_T response;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_ECHO_REQUEST_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_ECHO_REQUEST_HEAD_T request;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_ECHO_RESPONSE_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_ECHO_RESPONSE_HEAD_T response;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_CLEAR_REQUEST_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_CLEAR_REQUEST_HEAD_T request;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECIPE_CLEAR_RESPONSE_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public RECIPE_CLEAR_RESPONSE_HEAD_T response;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ENGINE_INFO_REQUEST_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ENGINE_INFO_RESPONSE_FRAME_T
    {
        public PROTOCOL_HEAD_T protocol_head;
        public FUNCTION_HEAD_T function_head;
        public ENGINE_INFO_RESPONSE_HEAD_T response;
    }

    
}
