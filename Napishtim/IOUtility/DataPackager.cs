using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility
{
    public class DataPackager
    {
        public const int PER_PACKAGE_SIZE = 1460;
        public const int MAX_PACKAGE_QUANTITY = 4096;
        public static ushort MakeDataPackageIndex(ushort i) => (ushort)(i & 0x7FFF);
        public static bool IsLastDataPackageIndex(ushort i) => (ushort)(i & 0x8000) != 0;
        public static ushort MakeLastDataPackageIndex(ushort i) => (ushort)(i | 0x8000);

        protected SocketInterface _port;
        private byte[] __internal_package_buffer;
        private object __lock_me;
        private static readonly uint __COOKIES;
        private readonly ushort __user_data_length_per_package;

        static DataPackager()
        {
            __COOKIES = 0;
            foreach (var b in Encoding.ASCII.GetBytes("AMEC").Reverse())
            {
                __COOKIES = __COOKIES << 8;
                __COOKIES += b;
            }
        }
        public DataPackager(SocketInterface port, object? mutex) 
        {
            __user_data_length_per_package = (ushort)(PER_PACKAGE_SIZE - Marshal.SizeOf<DATA_PACKAGE_HEAD_T>());
            _port = port;
            __internal_package_buffer = new byte[PER_PACKAGE_SIZE];
            __lock_me = mutex ?? new object();
        }
        public uint DataRecv(Memory<byte> buffer)
        {
            uint totalLength = 0xFFFFFFFF;
            ushort totalPackage = 0;
            uint finalPackageLenth = 0;
            ushort lastPackageIndex = 0;
            uint recv = 0;
            lock(__lock_me)
            {
                DATA_PACKAGE_HEAD_T head;
                while(true)
                {
                    var packageData = __recv_package(out head);
                    if(totalLength == 0xFFFFFFFF)
                    {
                        if(MakeDataPackageIndex(head.index) != 0)
                            throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_CORRUPT, $"Expect to receive package with index 0 but receive package with index {head.index} instead.");
                        else if(head.total_length > (uint)buffer.Length)
                            throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_RECV_BUFFER_INSUFFICIENT, $"Receive buffer length is {buffer.Length}, the whole data length is {head.total_length}.");
                        totalLength = head.total_length;
                        totalPackage = (ushort)(totalLength / __user_data_length_per_package + (totalLength % __user_data_length_per_package != 0 ? 1 : 0));
                        finalPackageLenth = totalLength % __user_data_length_per_package == 0 ? __user_data_length_per_package : totalLength % __user_data_length_per_package;
                        if (totalPackage > MAX_PACKAGE_QUANTITY)
                            throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_QUANTITY_OUT_OF_RANGE, $"The data received requires {totalPackage} packets. The maximum number of packages is {MAX_PACKAGE_QUANTITY}.");
                    }
                    else if(lastPackageIndex != MakeDataPackageIndex(head.index) - 1)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_CORRUPT, $"Expect to receive package with index {lastPackageIndex + 1} but receive package with index {MakeDataPackageIndex(head.index)} instead.");

                    lastPackageIndex = MakeDataPackageIndex(head.index);

                    if (MakeDataPackageIndex(head.index) >= totalPackage)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_INDEX_OUT_OF_RANGE, $"The index value {head.index} of the received package is out of range(0 - {totalPackage-1}).");
                    else if(MakeDataPackageIndex(head.index) == totalPackage - 1 && IsLastDataPackageIndex(head.index) == false)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_INDEX_CORRUPT, $"The index value {head.index} of the received package should be decorated with 0x8000.");
                    else if (MakeDataPackageIndex(head.index) != totalPackage - 1 && IsLastDataPackageIndex(head.index) == true)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_INDEX_CORRUPT, $"The index value {head.index} of the received package should not be decorated with 0x8000.");
                    else if (totalLength != head.total_length)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_HEAD_INCONSISTENCY, $"The total length should be {totalLength} bytes, but the subsequent package showed the total length is {head.total_length} bytes.");
                    else if(!IsLastDataPackageIndex(head.index) && packageData.Length != __user_data_length_per_package)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_CORRUPT, $"The desired package length is {__user_data_length_per_package} bytes, but the received package length is {packageData.Length} bytes.");
                    else if (IsLastDataPackageIndex(head.index) && packageData.Length != finalPackageLenth)
                        throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_CORRUPT, $"The desired package length is {finalPackageLenth} bytes, but the received package length is {packageData.Length} bytes.");

                    packageData.CopyTo(buffer.Slice((int)recv));

                    recv += (uint)packageData.Length;
                    if (recv == totalLength)
                        break;
                }

                return totalLength;
            }
        }
        public void DataSend(ReadOnlyMemory<byte> data)
        {
            lock (__lock_me)
            {
                if (data.Length / __user_data_length_per_package + (data.Length % __user_data_length_per_package != 0 ? 1:0) > MAX_PACKAGE_QUANTITY)
                    throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_QUANTITY_OUT_OF_RANGE, $"The length({data.Length} bytes) of data to be sent is out of range.");

                ushort index = 0;
                uint send = 0;
                ushort slice = 0;
                while(send < data.Length)
                {
                    if (data.Length - send <= __user_data_length_per_package)
                    {
                        index = MakeLastDataPackageIndex(index);
                        slice = (ushort)(data.Length - send);
                    }
                    else
                    {
                        slice = __user_data_length_per_package;
                    }
                    __send_package(index++, (uint)data.Length, data.Slice((int)send, slice));
                    send += slice;
                }
            }
        }

        private ReadOnlyMemory<byte> __recv_package(out DATA_PACKAGE_HEAD_T head)
        {
            Array.Clear(__internal_package_buffer);
            _port.Receive(__internal_package_buffer, 0, Marshal.SizeOf<DATA_PACKAGE_HEAD_T>());
            head = MemoryMarshal.Read<DATA_PACKAGE_HEAD_T>(__internal_package_buffer);
            if (head.cookies != __COOKIES)
                throw new NaposhtimDataPackageException(NaposhtimExceptionCode.INVALID_PACKAGE_HEAD, $"The value of Cookies is not 'AMEC'.");
            else if(head.length == 0 || head.length > __user_data_length_per_package)
                throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_LENGTH_OUT_OF_RANGE, $"The length({head.length} bytes) of received package is out of range.");
            _port.Receive(__internal_package_buffer, Marshal.SizeOf<DATA_PACKAGE_HEAD_T>(), head.length);
            return __internal_package_buffer.AsMemory().Slice(Marshal.SizeOf<DATA_PACKAGE_HEAD_T>(), head.length);
        }

        private void __send_package(ushort index, uint totalLength, ReadOnlyMemory<byte> data)
        {
            if (data.Length > __user_data_length_per_package)
                throw new NaposhtimDataPackageException(NaposhtimExceptionCode.PACKAGE_DATA_LENGTH_OUT_OF_RANGE, $"The length({data.Length} bytes) of package to be sent is out of range.");
            DATA_PACKAGE_HEAD_T head;
            head.cookies = __COOKIES;
            head.index = index;
            head.total_length = totalLength;
            head.length = (ushort)data.Length;
            MemoryMarshal.Write<DATA_PACKAGE_HEAD_T>(__internal_package_buffer, in head);
            data.CopyTo(__internal_package_buffer.AsMemory().Slice(Marshal.SizeOf<DATA_PACKAGE_HEAD_T>()));
            _port.Send(__internal_package_buffer.AsSpan().Slice(0, Marshal.SizeOf<DATA_PACKAGE_HEAD_T>() + data.Length));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DATA_PACKAGE_HEAD_T
    {
        public uint cookies;
        public uint total_length;
        public ushort index;
        public ushort length;
    }
}
