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
        public IEnumerable<Log> Records { get; private set; }

        public EventLog(System.IO.BinaryReader sm)
        {
            Header = new FileHeader { bytes = sm.ReadBytes(Marshal.SizeOf<FileHeader>()) };

            byte[] info = sm.ReadBytes(Marshal.SizeOf<ManagementInformation>());
            Info = MemoryMarshal.Read<ManagementInformation>(info);

            byte[] content = sm.ReadBytes(Info.size_in_kbyte * 1024 - Marshal.SizeOf<FileHeader>() - Marshal.SizeOf<ManagementInformation>());

            if (Info.record_size_in_byte > 0)
            {
                int end = __modulus(Info.record_end_pos_in_byte - 1, content.Length);
                __parse_backwards(content, end, Info.record_size_in_byte, content.Length);
                Records = __records.Reverse<Log>();
            }
        }

        private int __modulus(int v, int capacity)
        {
            if(v >= 0)
                return v % capacity;
            else
                return capacity + v % capacity;
        }

        private byte[] __read_record_backwards(byte[] content, int end, int leftbytes, int capacity)
        {
            if (leftbytes < Marshal.SizeOf<RecordHeader>() || capacity < Marshal.SizeOf<RecordHeader>())
                return null;

            byte etx_value0 = content[__modulus(end - 0, capacity)];
            byte etx_value1 = content[__modulus(end - 1, capacity)];
            byte etx_value2 = content[__modulus(end - 2, capacity)];
            byte etx_value3 = content[__modulus(end - 3, capacity)];

            int start = (end - Marshal.SizeOf<RecordHeader>()) % capacity;
            int read = Marshal.SizeOf<RecordHeader>();
            bool match = false;
            while (read + 4 <= leftbytes)
            {
                byte stx_value0 = content[__modulus(start - 0, capacity)];
                byte stx_value1 = content[__modulus(start - 1, capacity)];
                byte stx_value2 = content[__modulus(start - 2, capacity)];
                byte stx_value3 = content[__modulus(start - 3, capacity)];
                if (etx_value2 == stx_value0 && etx_value3 == stx_value1 && etx_value0 == stx_value2 && etx_value1 == stx_value3)
                {
                    match = true;
                    break;
                }

                start = __modulus(start - 4, capacity);
                read += 4;
            }
            if (match)
            {
                start = __modulus(start - 3, capacity);
                end = __modulus(end - 3, capacity);

                end = (end + 2) % capacity;
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

        private byte[] __read_record(byte[] content, int start, int leftbytes, int capacity)
        {
            if(leftbytes < Marshal.SizeOf<RecordHeader>() || capacity < Marshal.SizeOf<RecordHeader>())
                return null;

            byte stx_value0 = content[start % capacity];
            byte stx_value1 = content[(start + 1) % capacity];
            byte stx_value2 = content[(start + 2) % capacity];
            byte stx_value3 = content[(start + 3) % capacity];

            int end = (start + Marshal.SizeOf<RecordHeader>()) % capacity;
            int read = Marshal.SizeOf<RecordHeader>();
            bool match = false;
            while (read + 4 <= leftbytes)
            {
                byte etx_value0 = content[(end + 0) % capacity];
                byte etx_value1 = content[(end + 1) % capacity];
                byte etx_value2 = content[(end + 2) % capacity];
                byte etx_value3 = content[(end + 3) % capacity];
                if (etx_value2 == stx_value0 && etx_value3 == stx_value1 && etx_value0 == stx_value2 && etx_value1 == stx_value3)
                {
                    match = true;
                    break;
                }

                end = (end + 4) % capacity;
                read += 4;
            }
            if(match)
            {
                end = (end + 2) % capacity;
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

        private void __parse_backwards(byte[] content, int end, int size, int capacity)
        {
            int read = 0;
            RecordHeader rheader;
            ReadOnlySpan<byte> buffer = content;
            while (read < size)
            {
                byte[] r = __read_record_backwards(content, __modulus(end - read, capacity), size - read, capacity);
                if (r != null)
                {
                    rheader = MemoryMarshal.Read<RecordHeader>(r);
                    string data =
                        $"{rheader.data_time_0:X02}/{rheader.data_time_1:X02}/{rheader.data_time_2:X02} {rheader.data_time_3:X02}:{rheader.data_time_4:X02}:{rheader.data_time_5:X02}.{rheader.data_time_8:X}{rheader.data_time_9:X02}";
                    __records.Add(new Log(data, rheader.event_type, rheader.event_code, rheader.source_code, rheader.start_io, r));
                    read += r.Length;
                }
                else
                    break;
            }
        }

        private void __parse(byte[] content, int start, int size, int capacity)
        {
            int read = 0;
            RecordHeader rheader;
            ReadOnlySpan<byte> buffer = content;
            while(read < size)
            {
                if(content[(start + read) % capacity] == 0)
                {
                    read++;
                    continue;
                }
                byte[] r = __read_record(content, (start + read) % capacity, size - read, capacity);
                if (r != null)
                {
                    rheader = MemoryMarshal.Read<RecordHeader>(r);
                    string data =
                        $"{rheader.data_time_0:X02}/{rheader.data_time_1:X02}/{rheader.data_time_2:X02} {rheader.data_time_3:X02}:{rheader.data_time_4:X02}:{rheader.data_time_5:X02}.{rheader.data_time_8:X}{rheader.data_time_9:X02}";
                    __records.Add(new Log(data, rheader.event_type, rheader.event_code, rheader.source_code, rheader.start_io, r));
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
