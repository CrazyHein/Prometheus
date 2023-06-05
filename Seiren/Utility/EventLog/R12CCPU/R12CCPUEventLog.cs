using Spire.Pdf.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility.R12CCPU
{
    class EventLog
    {
        public FileHeader Header { get; private set; }
        public ManagementInformation Info { get; private set; }
        private List<Log> __records = new List<Log>();
        public IReadOnlyList<Log> Records { get; private set; }
       
        public EventLog(System.IO.BinaryReader sm)
        {
            Records = __records;

            Header = new FileHeader { bytes = sm.ReadBytes(Marshal.SizeOf<FileHeader>()) };

            byte[] info = sm.ReadBytes(Marshal.SizeOf<ManagementInformation>());
            Info = MemoryMarshal.Read<ManagementInformation>(info);

            byte[] content = sm.ReadBytes(Info.size_in_kbyte * 1024 - Marshal.SizeOf<FileHeader>() - Marshal.SizeOf<ManagementInformation>());

            int start = Info.record_end_pos_in_byte - Info.record_size_in_byte >= 0 ?
                Info.record_end_pos_in_byte - Info.record_size_in_byte : content.Length + (Info.record_end_pos_in_byte - Info.record_size_in_byte);
            __parse(content, start, Info.record_size_in_byte, content.Length);
        }

        private byte[] __read_record(byte[] content, int start, int capacity)
        {
            if(capacity < 4)
                return null;

            byte stx_value0 = content[start % capacity];
            byte stx_value1 = content[(start + 1) % capacity];

            int end = (start + 2) % capacity;
            while (true)
            {
                byte etx_value0 = content[end % capacity];
                byte etx_value1 = content[(end + 1) % capacity];
                if(etx_value0 == stx_value0 && etx_value1 == stx_value1)
                    break;

                end = (end + 1) % capacity;
            }

            if(end != start && (end + 1) % capacity != start)
            {
                int length = end > start ? end - start + 2 : capacity - (start - end) + 2;
                byte[] one = new byte[length];
                end = (end + 1) % capacity;
                if (end > start)
                    Buffer.BlockCopy(content, start, one, 0, end - start + 1);
                else
                {
                    Buffer.BlockCopy(content, start, one, 0, capacity - start);
                    Buffer.BlockCopy(content, 0, one, capacity - start, one.Length - (capacity - start));
                }
                return one;
            }
            else
                return null; 
        }

        private void __parse(byte[] content, int start, int size, int capacity)
        {
            int read = 0;
            RecordHeader rheater;
            ReadOnlySpan<byte> buffer = content;
            while(read < size)
            {
                byte[] r = __read_record(content, (start + read) % capacity, capacity);
                if (r.Length + read <= size)
                {
                    rheater = MemoryMarshal.Read<RecordHeader>(buffer.Slice((start + read) % capacity));
                    string data =
                        $"{rheater.data_time_0:X02}/{rheater.data_time_1:X02}/{rheater.data_time_2:X02} {rheater.data_time_3:X02}:{rheater.data_time_4:X02}:{rheater.data_time_5:X02}.{rheater.data_time_8:X}{rheater.data_time_9:X02}";
                    __records.Add(new Log(data, rheater.event_type, rheater.event_code, rheater.source_code, rheater.start_io, r));
                    read += r.Length;
                }
                else
                    break;
            }
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.U1)]
        public byte[] bytes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ManagementInformation
    {
        public ushort unknown_word;
        public ushort size_in_kbyte;
        public int record_end_pos_in_byte;
        public int record_size_in_byte;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RecordHeader
    {
        public ushort stx;
        public ushort unknown_word;
        public ushort event_type;
        public ushort event_code;
        public uint unknown_dword;
        public byte data_time_0;
        public byte data_time_1;
        public byte data_time_2;
        public byte data_time_3;
        public byte data_time_4;
        public byte data_time_5;
        public byte data_time_6;
        public byte data_time_7;
        public byte data_time_8;
        public byte data_time_9;
        public uint source_code;
        public ushort start_io;
    }

    public record Log(string Data, ushort EventType, ushort EventCode, uint Source, ushort StartIO, byte[] Raw)
    {

    }
}
