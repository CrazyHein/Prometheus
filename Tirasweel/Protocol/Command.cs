using System;
using System.Runtime.InteropServices;

namespace AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol
{
    public enum DAQ_FUNC_CODE_T : byte
    {
        DISCARD_TX              = 0x01,
        DISCARD_RX              = 0x02,
        POLL_TX                 = 0x04,
        POLL_RX                 = 0x08,
        INQUIRE                 = 0x10,
        INQUIRE_EX              = 0x1F,
        CMD                     = 0x20,
        ACK                     = 0x40,
        EXCEPTION               = 0x80,
        RESET                   = 0x03
    }

    public enum DAQ_EXCEPTION_CODE_T : byte
    {
        NO_EXCEPTION = 0x00,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_HEADER_T
    {
        public ushort transaction;
        public byte protocol_identifier_d;//'D''A''Q'
        public byte protocol_identifier_a;//'D''A''Q'
        public byte protocol_identifier_q;//'D''A''Q'
        public DAQ_FUNC_CODE_T func;
        public int datagram_bytes;
        public DAQ_DATAGRAM_HEADER_T(ushort trans, DAQ_FUNC_CODE_T function, int bytes)
        {
            transaction = trans;
            protocol_identifier_d = 0x44;
            protocol_identifier_a = 0x41;
            protocol_identifier_q = 0x51;
            func = function;
            datagram_bytes = bytes;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_EXCEPTION_RSP_T
    {
        public DAQ_EXCEPTION_CODE_T exception_code;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_SERVER_INFO_T
    {
        public int capacity;
        public int tx_timestamp_size_in_word;
        public int diag_size_in_word;
        public int tx_bit_size_in_word;
        public int tx_blk_size_in_word;
        public int rx_timestamp_size_in_word;
        public int ctrl_size_in_word;
        public int rx_bit_size_in_word;
        public int rx_blk_size_in_word;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Runtime.CompilerServices.InlineArray(16)]
    public struct DAQ_SERVER_IO_HASH_T
    {
        public byte md5_code;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_INQUIRE_RSP_T
    {
        public DAQ_DATAGRAM_HEADER_T header;
        public DAQ_SERVER_INFO_T info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_INQUIRE_EX_RSP_T
    {
        public DAQ_DATAGRAM_HEADER_T header;
        public DAQ_SERVER_IO_HASH_T var_hash;
        public DAQ_SERVER_IO_HASH_T io_hash;
        public DAQ_SERVER_INFO_T info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_POLL_RSQ_BDY_T
    {
        public int expect_frame_count;
        public DAQ_DATAGRAM_POLL_RSQ_BDY_T(int expect)
        {
            expect_frame_count = expect;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_POLL_RSQ_T
    {
        public DAQ_DATAGRAM_HEADER_T header;
        public DAQ_DATAGRAM_POLL_RSQ_BDY_T body;
        public DAQ_DATAGRAM_POLL_RSQ_T(ushort trans, DAQ_FUNC_CODE_T poll, int expect)
        {
            header = new DAQ_DATAGRAM_HEADER_T(trans, DAQ_FUNC_CODE_T.CMD | poll, Marshal.SizeOf<DAQ_DATAGRAM_POLL_RSQ_BDY_T>());
            body = new DAQ_DATAGRAM_POLL_RSQ_BDY_T(expect);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_POLL_RSP_BDY_T
    {
        public int received_frame_count;
        public int residual_frame_count;
        public int corrupted_frame_count;
        public int timestamp_size_in_word;
        public int ad_size_in_word;
        public int bit_size_in_word;
        public int blk_size_in_word;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_POLL_RSP_T
    {
        public DAQ_DATAGRAM_HEADER_T header;
        public DAQ_DATAGRAM_POLL_RSP_BDY_T body;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_RESET_RSQ_BDY_T
    {
        public int clock_bias;
        public DAQ_DATAGRAM_RESET_RSQ_BDY_T(int bias)
        {
            clock_bias = bias;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DAQ_DATAGRAM_RESET_RSQ_T
    {
        public DAQ_DATAGRAM_HEADER_T header;
        public DAQ_DATAGRAM_RESET_RSQ_BDY_T body;
        public DAQ_DATAGRAM_RESET_RSQ_T(ushort trans, int bias)
        {
            header = new DAQ_DATAGRAM_HEADER_T(trans, DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.RESET, Marshal.SizeOf<DAQ_DATAGRAM_RESET_RSQ_BDY_T>());
            body = new DAQ_DATAGRAM_RESET_RSQ_BDY_T(bias);
        }
    }
}
