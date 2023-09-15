using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol
{
    public class Master
    {
        public DatagramMaster Port { set; get; }
        public Master(SocketInterface port, int bufferSize) { Port = new DatagramMaster(port, bufferSize); }

        public void AcquisiteServerInfo(ushort trans, ref DAQ_SERVER_INFO_T info)
        {
            DAQ_DATAGRAM_HEADER_T request = new DAQ_DATAGRAM_HEADER_T(trans, DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.INQUIRE, 0);

            Port.Send(ref request);

            var body = Port.Recv(out DAQ_DATAGRAM_HEADER_T header);

            if ((header.func & DAQ_FUNC_CODE_T.EXCEPTION) != 0)
            {
                if (body.Length == 1)
                    throw new DAQException(DAQExceptionCode.EXCEPTION_DATAGRAM, ((DAQ_EXCEPTION_CODE_T)body.Span[0]).ToString());
                else
                    throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);
            }

            if (header.transaction != trans || header.func != (DAQ_FUNC_CODE_T.ACK | DAQ_FUNC_CODE_T.INQUIRE) || header.protocol_identifier_d != 0x44 || header.protocol_identifier_a != 0x41 || header.protocol_identifier_q != 0x51)
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);

            info = MemoryMarshal.Read<DAQ_SERVER_INFO_T>(body.Span);
        }

        public async Task <DAQ_SERVER_INFO_T> AcquisiteServerInfoAsync(ushort trans)
        {
            DAQ_SERVER_INFO_T info = new DAQ_SERVER_INFO_T();
            await Task.Run(() => AcquisiteServerInfo(trans, ref info));
            return info;
        }

        public void AcquisiteData(bool txAcquisition, ushort trans, 
            int expected, out int received, out int residual, out int corrupted, out ReadOnlyMemory<byte> data)
        {
            DAQ_DATAGRAM_POLL_RSQ_T request = new DAQ_DATAGRAM_POLL_RSQ_T(trans,
                txAcquisition ? DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.POLL_TX : DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.POLL_RX, expected);

            Port.Send(ref request);

            var body = Port.Recv(out DAQ_DATAGRAM_HEADER_T header);

            if ((header.func & DAQ_FUNC_CODE_T.EXCEPTION) != 0)
            {
                if (body.Length == 1)
                {
                    received = 0;
                    residual = 0;
                    corrupted = 0;
                    data = null;
                    throw new DAQException(DAQExceptionCode.EXCEPTION_DATAGRAM, ((DAQ_EXCEPTION_CODE_T)body.Span[0]).ToString());
                }
                else
                    throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);
            }

            if (body.Length < Marshal.SizeOf <DAQ_DATAGRAM_POLL_RSP_BDY_T>())
                throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);

            if (header.func != (DAQ_FUNC_CODE_T.ACK | (txAcquisition? DAQ_FUNC_CODE_T.POLL_TX : DAQ_FUNC_CODE_T.POLL_RX)))
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);

            if (header.transaction != trans || header.protocol_identifier_d != 0x44 || header.protocol_identifier_a != 0x41 || header.protocol_identifier_q != 0x51)
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);

            DAQ_DATAGRAM_POLL_RSP_BDY_T info = MemoryMarshal.Read<DAQ_DATAGRAM_POLL_RSP_BDY_T>(body.Span);
            received = info.received_frame_count;
            residual = info.residual_frame_count;
            corrupted = info.corrupted_frame_count;

            int size = info.received_frame_count * 2 * (info.timestamp_size_in_word + info.ad_size_in_word + info.bit_size_in_word + info.blk_size_in_word);
            if(body.Length != Marshal.SizeOf<DAQ_DATAGRAM_POLL_RSP_BDY_T>() + size)
                throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);

            data = body.Slice(Marshal.SizeOf<DAQ_DATAGRAM_POLL_RSP_BDY_T>(), size);
        }

        public async Task<ValueTuple<int, int, int, ReadOnlyMemory<byte>>> AcquisiteDataAsync(bool txAcquisition, ushort trans, int expected)
        {
            int received = 0, residual = 0, corrupted = 0;
            ReadOnlyMemory<byte> data = null;
            await Task.Run(() => AcquisiteData(txAcquisition, trans, expected, out received, out residual, out corrupted, out data));
            return (received, residual, corrupted, data);
        }

        public void DiscardAcquisitionData(bool txAcquisition, ushort trans)
        {
            DAQ_DATAGRAM_HEADER_T request = new DAQ_DATAGRAM_HEADER_T(trans, 
                txAcquisition ? DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.DISCARD_TX : DAQ_FUNC_CODE_T.CMD | DAQ_FUNC_CODE_T.DISCARD_RX, 0);

            Port.Send(ref request);

            var body = Port.Recv(out DAQ_DATAGRAM_HEADER_T header);

            if ((header.func & DAQ_FUNC_CODE_T.EXCEPTION) != 0)
            {
                if (body.Length == 1)
                    throw new DAQException(DAQExceptionCode.EXCEPTION_DATAGRAM, ((DAQ_EXCEPTION_CODE_T)body.Span[0]).ToString());
                else
                    throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);
            }

            if (body.Length != 0)
                throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);

            if (header.func != (DAQ_FUNC_CODE_T.ACK | (txAcquisition ? DAQ_FUNC_CODE_T.DISCARD_TX : DAQ_FUNC_CODE_T.DISCARD_RX)))
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);

            if (header.transaction != trans || header.protocol_identifier_d != 0x44 || header.protocol_identifier_a != 0x41 || header.protocol_identifier_q != 0x51)
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);
        }

        public async Task DiscardAcquisitionDataAsync(bool txAcquisition, ushort trans)
        {
            await Task.Run(() => DiscardAcquisitionData(txAcquisition, trans));
        }

        public void ResetAcquisitionData(ushort trans, int bias)
        {
            DAQ_DATAGRAM_RESET_RSQ_T request = new DAQ_DATAGRAM_RESET_RSQ_T(trans, bias);

            Port.Send(ref request);

            var body = Port.Recv(out DAQ_DATAGRAM_HEADER_T header);

            if ((header.func & DAQ_FUNC_CODE_T.EXCEPTION) != 0)
            {
                if (body.Length == 1)
                    throw new DAQException(DAQExceptionCode.EXCEPTION_DATAGRAM, ((DAQ_EXCEPTION_CODE_T)body.Span[0]).ToString());
                else
                    throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);
            }

            if (body.Length != 0)
                throw new DAQException(DAQExceptionCode.INVALID_DATAGRAM);

            if (header.func != (DAQ_FUNC_CODE_T.ACK | DAQ_FUNC_CODE_T.RESET ))
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);

            if (header.transaction != trans || header.protocol_identifier_d != 0x44 || header.protocol_identifier_a != 0x41 || header.protocol_identifier_q != 0x51)
                throw new DAQException(DAQExceptionCode.TRANSACTION_MISMATCH);
        }

        public async Task ResetAcquisitionDataAsync(ushort trans, int bias)
        {
            await Task.Run(() => ResetAcquisitionData(trans, bias));
        }
    }
}
