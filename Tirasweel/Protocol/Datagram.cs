using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol
{ 
    public class DatagramMaster
    {
        private SocketInterface __port;
        private byte[] __datagram_header_data_array = new byte[Marshal.SizeOf<DAQ_DATAGRAM_HEADER_T>()];
        private byte[] __datagram_body_data;

        public DatagramMaster(SocketInterface tcp, int bodyLength) { __port = tcp; __datagram_body_data = new byte[bodyLength]; }

        public ReadOnlyMemory<byte> Recv(out DAQ_DATAGRAM_HEADER_T header)
        {
            __port.Receive(__datagram_header_data_array, 0, Marshal.SizeOf<DAQ_DATAGRAM_HEADER_T>());
            header = MemoryMarshal.Read<DAQ_DATAGRAM_HEADER_T>(__datagram_header_data_array);

            if(__datagram_body_data.Length < header.datagram_bytes)
                __datagram_body_data = new byte[header.datagram_bytes];
            __port.Receive(__datagram_body_data, 0, header.datagram_bytes);

            return __datagram_body_data.AsMemory().Slice(0, header.datagram_bytes);
        }

        public void Send<T>(ref T rsq) where T : struct
        {
            var sp = MemoryMarshal.CreateSpan(ref rsq, 1);
            var byte_sp = MemoryMarshal.AsBytes(sp);
            __port.Send(byte_sp);
        }
    }
}
