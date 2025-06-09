using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Protocol
{
    public class Master
    {
        private object __lock_me;
        private Random __trans_index;
        private byte[] __user_data;
        DataPackager __data_packager;
        public Master(DataPackager packager, object? mutex)
        {
            __data_packager = packager;
            __lock_me = mutex??new object();
            __trans_index = new Random();
            __user_data = new byte[Common.USER_DATA_SIZE_IN_BYTE];
        }

        public void Download(RECIPE_SEGMENT_T segment, ReadOnlyMemory<byte> pJSON)
        {
            if (pJSON.Length > Common.MAX_DOWNLOAD_DATA_SIZE_IN_BYTE)
                throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_DATA_LENGTH_OUT_RANGE, $"The length({pJSON.Length} bytes) of recipe segment to be downloaded is out of range.");
            lock (__lock_me)
            {
                RECIPE_DOWNLOAD_REQUEST_FRAME_T request;
                request.protocol_head.trans_head = Common.MakeProtocolHead((byte)__trans_index.Next(256));
                request.function_head.func_code = FUNC_CODE_T.DOWNLOAD;
                request.request.segment = segment;
                request.request.content_size_in_byte = (uint)pJSON.Length;
                MemoryMarshal.Write<RECIPE_DOWNLOAD_REQUEST_FRAME_T>(__user_data, in request);
                pJSON.CopyTo(__user_data.AsMemory().Slice(Marshal.SizeOf<RECIPE_DOWNLOAD_REQUEST_FRAME_T>()));
                __data_packager.DataSend(__user_data.AsMemory().Slice(0, Marshal.SizeOf<RECIPE_DOWNLOAD_REQUEST_FRAME_T>() + pJSON.Length));

                var recvLength = __data_packager.DataRecv(__user_data);
                if(recvLength != Marshal.SizeOf<RECIPE_DOWNLOAD_RESPONSE_FRAME_T>())
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_UNKNOWN_RESPONSE, $"The length of download recipe segment response <RECIPE_DOWNLOAD_RESPONSE_FRAME_T> should be {Marshal.SizeOf<RECIPE_DOWNLOAD_RESPONSE_FRAME_T>()} bytes, the received data length is {recvLength} bytes.");
                var response = MemoryMarshal.Read<RECIPE_DOWNLOAD_RESPONSE_FRAME_T>(__user_data);
                if(response.protocol_head.trans_head != request.protocol_head.trans_head)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_TRANSID_MISMATCH, $"TRANS_ID: {request.protocol_head.trans_head:X8} vs {response.protocol_head.trans_head:X8}");
                else if((response.function_head.func_code ^ FUNC_CODE_T.DOWNLOAD) != FUNC_CODE_T.RESPONSE_FLAG)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_FUNC_CODE_MISMATCH, $"FUNC_CODE: {(byte)FUNC_CODE_T.DOWNLOAD:X2} vs {(byte)response.function_head.func_code:X2}");
                else if(response.response.segment != request.request.segment)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"SEGMENT_ID: {(byte)request.request.segment:X2} vs {(byte)response.response.segment:X2}");
                else if (response.response.content_size_in_byte != request.request.content_size_in_byte)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"CONTENT_LENGTH: {request.request.content_size_in_byte} vs {response.response.content_size_in_byte}");
                else if(response.response.exception != 0)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SERVER_EXCEPTION, $"The server returned an exception code {response.response.exception:X8}.");
            }
        }

        public void Clear(RECIPE_SEGMENT_T segment, uint startIndex, uint endIndex)
        {
            lock(__lock_me)
            {
                RECIPE_CLEAR_REQUEST_FRAME_T request;
                request.protocol_head.trans_head = Common.MakeProtocolHead((byte)__trans_index.Next(256));
                request.function_head.func_code = FUNC_CODE_T.CLEAR;
                request.request.segment = segment;
                request.request.start_index = startIndex;
                request.request.end_index = endIndex;
                MemoryMarshal.Write<RECIPE_CLEAR_REQUEST_FRAME_T>(__user_data, in request);
                __data_packager.DataSend(__user_data.AsMemory().Slice(0, Marshal.SizeOf<RECIPE_CLEAR_REQUEST_FRAME_T>()));

                var recvLength = __data_packager.DataRecv(__user_data);
                if (recvLength != Marshal.SizeOf<RECIPE_CLEAR_RESPONSE_FRAME_T>())
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_UNKNOWN_RESPONSE, $"The length of clear recipe segment response <RECIPE_CLEAR_RESPONSE_FRAME_T> should be {Marshal.SizeOf<RECIPE_CLEAR_RESPONSE_FRAME_T>()} bytes, the received data length is {recvLength} bytes.");
                var response = MemoryMarshal.Read<RECIPE_CLEAR_RESPONSE_FRAME_T>(__user_data);
                if (response.protocol_head.trans_head != request.protocol_head.trans_head)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_TRANSID_MISMATCH, $"TRANS_ID: {request.protocol_head.trans_head:X8} vs {response.protocol_head.trans_head:X8}");
                else if ((response.function_head.func_code ^ FUNC_CODE_T.CLEAR) != FUNC_CODE_T.RESPONSE_FLAG)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_FUNC_CODE_MISMATCH, $"FUNC_CODE: {(byte)FUNC_CODE_T.CLEAR:X2} vs {(byte)response.function_head.func_code:X2}");
                else if (response.response.segment != request.request.segment)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"SEGMENT_ID: {(byte)request.request.segment:X2} vs {(byte)response.response.segment:X2}");
                else if (response.response.start_index != request.request.start_index || response.response.end_index != request.request.end_index)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"RANGE: [{request.request.start_index - request.request.end_index}] vs [{response.response.start_index - response.response.end_index}]");
                else if (response.response.exception != 0)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SERVER_EXCEPTION, $"The server returned an exception code {response.response.exception:X8}.");
            }
        }

        public uint Echo(RECIPE_SEGMENT_T segment, uint startIndex, uint endIndex, Memory<byte> buffer)
        {
            lock (__lock_me)
            {
                RECIPE_ECHO_REQUEST_FRAME_T request;
                request.protocol_head.trans_head = Common.MakeProtocolHead((byte)__trans_index.Next(256));
                request.function_head.func_code = FUNC_CODE_T.ECHO;
                request.request.segment = segment;
                request.request.start_index = startIndex;
                request.request.end_index = endIndex;
                MemoryMarshal.Write<RECIPE_ECHO_REQUEST_FRAME_T>(__user_data, in request);
                __data_packager.DataSend(__user_data.AsMemory().Slice(0, Marshal.SizeOf<RECIPE_ECHO_REQUEST_FRAME_T>()));

                var recvLength = __data_packager.DataRecv(__user_data);
                if (recvLength < Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>())
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_UNKNOWN_RESPONSE, $"The length of echo recipe segment response <RECIPE_ECHO_RESPONSE_FRAME_T> should be at least {Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>()} bytes, the received data length is {recvLength} bytes.");
                var response = MemoryMarshal.Read<RECIPE_ECHO_RESPONSE_FRAME_T>(__user_data);
                var contentSize = response.response.content_size_in_byte;
                if(recvLength != Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>() + contentSize)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_DATA_CORRUPT, $"The length of echo recipe segment response <RECIPE_ECHO_RESPONSE_FRAME_T> should be {Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>() + contentSize} bytes, the received data length is {recvLength} bytes.");
                else if(buffer.Length < contentSize)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_BUFFER_INSUFFICIENT, $"Receive buffer length is {buffer.Length}, the echo recipe segment length is {contentSize}.");
                else if (response.protocol_head.trans_head != request.protocol_head.trans_head)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_TRANSID_MISMATCH, $"TRANS_ID: {request.protocol_head.trans_head:X8} vs {response.protocol_head.trans_head:X8}");
                else if ((response.function_head.func_code ^ FUNC_CODE_T.ECHO) != FUNC_CODE_T.RESPONSE_FLAG)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_FUNC_CODE_MISMATCH, $"FUNC_CODE: {(byte)FUNC_CODE_T.CLEAR:X2} vs {(byte)response.function_head.func_code:X2}");
                else if (response.response.segment != request.request.segment)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"SEGMENT_ID: {(byte)request.request.segment:X2} vs {(byte)response.response.segment:X2}");
                else if (response.response.start_index != request.request.start_index || response.response.end_index != request.request.end_index)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"RANGE: [{request.request.start_index - request.request.end_index}] vs [{response.response.start_index - response.response.end_index}]");
                else if (response.response.exception != 0)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SERVER_EXCEPTION, $"The server returned an exception code {response.response.exception:X8}.");

                __user_data.AsMemory<byte>(Marshal.SizeOf<RECIPE_ECHO_RESPONSE_FRAME_T>(), (int)contentSize).CopyTo(buffer);
                return contentSize;
            }
        }

        public void Info(out uint version, out uint stepCapacity, out uint throughput)
        {
            lock (__lock_me)
            {
                try
                {
                    ENGINE_INFO_REQUEST_FRAME_T request;
                    request.protocol_head.trans_head = Common.MakeProtocolHead((byte)__trans_index.Next(256));
                    request.function_head.func_code = FUNC_CODE_T.INFO;
                    MemoryMarshal.Write<ENGINE_INFO_REQUEST_FRAME_T>(__user_data, in request);
                    __data_packager.DataSend(__user_data.AsMemory().Slice(0, Marshal.SizeOf<ENGINE_INFO_REQUEST_FRAME_T>()));

                    var recvLength = __data_packager.DataRecv(__user_data);
                    if (recvLength != Marshal.SizeOf<ENGINE_INFO_RESPONSE_FRAME_T>())
                        throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_UNKNOWN_RESPONSE, $"The length of engine info response <ENGINE_INFO_RESPONSE_FRAME_T> should be {Marshal.SizeOf<ENGINE_INFO_RESPONSE_FRAME_T>()} bytes, the received data length is {recvLength} bytes.");
                    var response = MemoryMarshal.Read<ENGINE_INFO_RESPONSE_FRAME_T>(__user_data);
                    if (response.protocol_head.trans_head != request.protocol_head.trans_head)
                        throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_TRANSID_MISMATCH, $"TRANS_ID: {request.protocol_head.trans_head:X8} vs {response.protocol_head.trans_head:X8}");
                    else if ((response.function_head.func_code ^ FUNC_CODE_T.INFO) != FUNC_CODE_T.RESPONSE_FLAG)
                        throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_FUNC_CODE_MISMATCH, $"FUNC_CODE: {(byte)FUNC_CODE_T.DOWNLOAD:X2} vs {(byte)response.function_head.func_code:X2}");
                    else if (response.response.exception != 0)
                        throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SERVER_EXCEPTION, $"The server returned an exception code {response.response.exception:X8}.");

                    version = response.response.version;
                    stepCapacity = response.response.step_capacity;
                    throughput = response.response.throughput;
                }
                catch
                {
                    version = 0;
                    stepCapacity = 0;
                    throughput = 0;
                    throw;
                }
            }
        }

        public void DownloadGlobalEvent(uint idx, Event evt)
        {
            string content = $"{{\"ID\":{idx},\"EVENT\":{evt.ToString()}}}\0";
            Download(RECIPE_SEGMENT_T.GLOBAL_EVENT, Encoding.ASCII.GetBytes(content));
        }

        public void Initialize(ReadOnlyMemory<byte> pJSON)
        {
            if (pJSON.Length > Common.MAX_DOWNLOAD_DATA_SIZE_IN_BYTE)
                throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_DATA_LENGTH_OUT_RANGE, $"The length({pJSON.Length} bytes) of recipe initialization list to be downloaded is out of range.");
            lock (__lock_me)
            {
                RECIPE_INITIALIZE_REQUEST_FRAME_T request;
                request.protocol_head.trans_head = Common.MakeProtocolHead((byte)__trans_index.Next(256));
                request.function_head.func_code = FUNC_CODE_T.INITIALIZE;
                request.request.content_size_in_byte = (uint)pJSON.Length;
                MemoryMarshal.Write<RECIPE_INITIALIZE_REQUEST_FRAME_T>(__user_data, in request);
                pJSON.CopyTo(__user_data.AsMemory().Slice(Marshal.SizeOf<RECIPE_INITIALIZE_REQUEST_FRAME_T>()));
                __data_packager.DataSend(__user_data.AsMemory().Slice(0, Marshal.SizeOf<RECIPE_INITIALIZE_REQUEST_FRAME_T>() + pJSON.Length));

                var recvLength = __data_packager.DataRecv(__user_data);
                if (recvLength != Marshal.SizeOf<RECIPE_INITIALIZE_RESPONSE_FRAME_T>())
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_RECV_UNKNOWN_RESPONSE, $"The length of initialize recipe response <RECIPE_INITIALIZE_RESPONSE_FRAME_T> should be {Marshal.SizeOf<RECIPE_INITIALIZE_RESPONSE_FRAME_T>()} bytes, the received data length is {recvLength} bytes.");
                var response = MemoryMarshal.Read<RECIPE_INITIALIZE_RESPONSE_FRAME_T>(__user_data);
                if (response.protocol_head.trans_head != request.protocol_head.trans_head)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_TRANSID_MISMATCH, $"TRANS_ID: {request.protocol_head.trans_head:X8} vs {response.protocol_head.trans_head:X8}");
                else if ((response.function_head.func_code ^ FUNC_CODE_T.INITIALIZE) != FUNC_CODE_T.RESPONSE_FLAG)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_FUNC_CODE_MISMATCH, $"FUNC_CODE: {(byte)FUNC_CODE_T.INITIALIZE:X2} vs {(byte)response.function_head.func_code:X2}");
                else if (response.response.content_size_in_byte != request.request.content_size_in_byte)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SEGMENT_MISMATCH, $"CONTENT_LENGTH: {request.request.content_size_in_byte} vs {response.response.content_size_in_byte}");
                else if (response.response.exception != 0)
                    throw new NaposhtimProtocolException(NaposhtimExceptionCode.PROTOCOL_SERVER_EXCEPTION, $"The server returned an exception code {response.response.exception:X8}.");
            }
        }
    }


}
