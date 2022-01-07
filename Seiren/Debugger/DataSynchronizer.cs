using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.IOUtility;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger
{
    public enum DataSynchronizerState : byte
    {
        Exception = 0x01,
        Ready = 0x02,
        Connecting = 0x03,
        Connected = 0x04,
    }

    public enum DataSyncMode
    {
        Read,
        Write,
        Readback,
    }

    public class DataSynchronizer
    {
        private object __sync_property_access_lock = new object();
        private SemaphoreSlim __sync_operation_access_lock = new SemaphoreSlim(1);
        SocketInterface __io = null;
        private string __sync_exception_message = "";
        private int __polling_interval = 0;
        private List<(uint start, ushort size, DataSyncMode access, ushort[] data)> __process_data;
        private List<ushort[]> __internal_process_data;
        private ushort __process_data_end;
        private ushort __internal_process_data_end;
        private AutoResetEvent __stop_event = new AutoResetEvent(false);
        private Thread __data_sync_thread;
        private DataSynchronizerState __sync_state = DataSynchronizerState.Ready;
        private uint __heartbeat_counter = 0;

        public DataSynchronizer(IEnumerable<(uint start, ushort size, DataSyncMode access)> datas)
        {
            __process_data = new List<(uint start, ushort size, DataSyncMode access, ushort[] data)>(
                    datas.Select(i => (i.start, i.size, i.access, new ushort[i.size])));
            __internal_process_data = new List<ushort[]>(datas.Select(i => new ushort[i.size]));
            __process_data_end = 0;
            __internal_process_data_end = 0;
        }

        public async Task<DataSynchronizerState> Startup(SlmpTargetProperty target, IReadOnlyList<ushort[]> datas)
        {
            __sync_operation_access_lock.Wait();
            try
            {
                switch (State)
                {
                    case DataSynchronizerState.Connected:
                        return DataSynchronizerState.Connected;
                    case DataSynchronizerState.Exception:
                        if (__data_sync_thread != null)
                        {
                            __data_sync_thread.Join();
                            __data_sync_thread = null;
                        }
                        break;
                    case DataSynchronizerState.Ready:
                        break;
                }

                __stop_event.Reset();
                if (target.UDPTransportLayer)
                    __io = new UDP(new System.Net.IPEndPoint(target.SourceIPv4, target.SourcePort),
                                    new System.Net.IPEndPoint(target.DestinationIPv4, target.DestinationPort),
                                    target.ReceiveBufferSize, target.SendTimeoutValue, target.ReceiveTimeoutValue);
                else
                    __io = new TCP(new System.Net.IPEndPoint(target.SourceIPv4, 0),
                                    new System.Net.IPEndPoint(target.DestinationIPv4, target.DestinationPort),
                                    target.SendTimeoutValue, target.ReceiveTimeoutValue);
                DESTINATION_ADDRESS_T destination = new DESTINATION_ADDRESS_T(target.NetworkNumber, target.StationNumber, target.ModuleIONumber, target.MultidropNumber, target.ExtensionStationNumber);

                DeviceAccessMaster master = new DeviceAccessMaster(target.FrameType, target.DataCode, target.R_DedicatedMessageFormat, __io,
                                                                    ref destination, target.SendBufferSize, target.ReceiveBufferSize);
                DeviceAccessMode deviceReadMode, deviceWriteMode, deviceReadbackMode;
                if (target.DeviceReadMode == DeviceAccessMode.Auto)
                {
                    var reads = __process_data.Where(d => d.access == DataSyncMode.Read);
                    int totalDeive = reads.Sum(d => d.size);
                    int totalBlock = reads.Count();
                    if (totalDeive <= 960 && ((totalBlock <= 60 && target.R_DedicatedMessageFormat == true) || (totalBlock <= 120 && target.R_DedicatedMessageFormat == false)))
                        deviceReadMode = DeviceAccessMode.Inconsecutive;
                    else
                        deviceReadMode = DeviceAccessMode.Consecutive;

                    reads = __process_data.Where(d => d.access == DataSyncMode.Readback);
                    totalDeive = reads.Sum(d => d.size);
                    totalBlock = reads.Count();
                    if (totalDeive <= 960 && ((totalBlock <= 60 && target.R_DedicatedMessageFormat == true) || (totalBlock <= 120 && target.R_DedicatedMessageFormat == false)))
                        deviceReadbackMode = DeviceAccessMode.Inconsecutive;
                    else
                        deviceReadbackMode = DeviceAccessMode.Consecutive;
                }
                else
                {
                    deviceReadMode = target.DeviceReadMode;
                    deviceReadbackMode = target.DeviceReadMode;
                }
                if (target.DeviceWriteMode == DeviceAccessMode.Auto)
                {
                    var writes = __process_data.Where(d => d.access == DataSyncMode.Write);
                    int totalDeive = writes.Sum(d => d.size);
                    int totalBlock = writes.Count();
                    int extraDevice = totalBlock * (target.R_DedicatedMessageFormat == true ? 9 : 4);
                    if (totalDeive + extraDevice <= 960 && ((totalBlock <= 60 && target.R_DedicatedMessageFormat == true) || (totalBlock <= 120 && target.R_DedicatedMessageFormat == false)))
                        deviceWriteMode = DeviceAccessMode.Inconsecutive;
                    else
                        deviceWriteMode = DeviceAccessMode.Consecutive;
                }
                else
                    deviceWriteMode = target.DeviceWriteMode;

                if (__io is TCP)
                {
                    State = DataSynchronizerState.Connecting;
                    await Task.Run(() => (__io as TCP).Connect());
                    //await Task.Delay(2000);
                }

                State = DataSynchronizerState.Connected;
                ExceptionMessage = "N/A";

                await Task.Run(() => __init_rx_data(Tuple.Create(master, (ushort)(target.MonitoringTimer / 250), deviceWriteMode, datas)));

                if (State == DataSynchronizerState.Exception)
                    return State;

                __heartbeat_counter = 0;
                __data_sync_thread = new Thread(new ParameterizedThreadStart(__data_sync_routine));
                //__data_sync_thread = new Thread(new ParameterizedThreadStart(__data_sync_delay));
                __data_sync_thread.Start(Tuple.Create(master, (ushort)(target.MonitoringTimer / 250), target.PollingInterval, deviceReadMode, deviceWriteMode, deviceReadbackMode));
                return State;
            }
            catch (Exception ex)
            {
                State = DataSynchronizerState.Exception;
                ExceptionMessage = ex.Message;
                return DataSynchronizerState.Exception;
            }
            finally
            {
                __sync_operation_access_lock.Release();
            }
        }

        public async Task<DataSynchronizerState> Stop()
        {
            __sync_operation_access_lock.Wait();
            try
            {
                switch (State)
                {
                    case DataSynchronizerState.Ready:
                        return DataSynchronizerState.Ready;
                    case DataSynchronizerState.Connected:
                        __stop_event.Set();
                        break;
                    case DataSynchronizerState.Exception:
                        __stop_event.Reset();
                        break;
                }

                if (__data_sync_thread != null)
                {
                    await Task.Run(() => __data_sync_thread.Join());
                    __data_sync_thread = null;
                }
                if (__io != null)
                {
                    __io.Dispose();
                    __io = null;
                }
                State = DataSynchronizerState.Ready;
                return DataSynchronizerState.Ready;
            }
            finally
            {
                __sync_operation_access_lock.Release();
            }
        }

        public void Exchange(List<ushort[]> datas, out ushort end)
        {
            end = 0;
            lock (__sync_property_access_lock)
            {
                for (int i = 0; i < __process_data.Count; ++i)
                {
                    if (__process_data[i].access == DataSyncMode.Read || __process_data[i].access == DataSyncMode.Readback)
                        __internal_process_data[i].CopyTo(datas[i], 0);
                    else
                        datas[i].CopyTo(__internal_process_data[i], 0);
                }
                end = __internal_process_data_end;
            }
        }

        private void __init_rx_data(object param)
        {
            (DeviceAccessMaster master, ushort monitoring, DeviceAccessMode write, IReadOnlyList<ushort[]> datas) =
                (Tuple<DeviceAccessMaster, ushort, DeviceAccessMode, IReadOnlyList<ushort[]>>)param;

            IEnumerable<(string, uint, ushort)> readIndex = null;
            Memory<ushort>[] readArray = null;
            ushort end = 0;
            if (write == DeviceAccessMode.Inconsecutive)
            {
                readIndex = __process_data.Where(d => d.access == DataSyncMode.Write && d.size > 0).Select(d => new ValueTuple<string, uint, ushort>("D", d.start, d.size));
                readArray = (__process_data.Where(d => d.access == DataSyncMode.Write && d.size > 0).Select(d => d.data.AsMemory<ushort>())).ToArray();
                if (readArray.Length != 0)
                {
                    master.ReadLocalDeviceInWord(monitoring, readIndex, null, out end, readArray, null);
                    __process_data_end = end;
                    if (end != 0)
                    {
                        ExceptionMessage = $"End Code : {end:X04}";
                        Counter = 0;
                        State = DataSynchronizerState.Exception;
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < __process_data.Count; ++i)
                {
                    if (__process_data[i].access == DataSyncMode.Write && __process_data[i].size > 0)
                    {
                        master.ReadLocalDeviceInWord(monitoring, "D", __process_data[i].start, __process_data[i].size, out end, __process_data[i].data);
                        __process_data_end = end;
                        if (end != 0)
                        {
                            ExceptionMessage = $"End Code : {end:X04}";
                            Counter = 0;
                            State = DataSynchronizerState.Exception;
                            return;
                        }
                    }
                }
            }

            for(int i = 0; i < __process_data.Count; ++i)
            {
                if (__process_data[i].access == DataSyncMode.Write)
                {
                    __process_data[i].data.CopyTo(datas[i], 0);
                }
            }
        }

        private void __data_sync_routine(object param)
        {
            (DeviceAccessMaster master, ushort monitoring, int interval, DeviceAccessMode read, DeviceAccessMode write, DeviceAccessMode readback) =
                (Tuple<DeviceAccessMaster, ushort, int, DeviceAccessMode, DeviceAccessMode, DeviceAccessMode>)param;
            uint counter = 0;
            ushort end = 0;
            Stopwatch sw = new Stopwatch();

            IEnumerable<(string, uint, ushort)> readIndex = null;
            IEnumerable<(string, uint, ushort)> readbackIndex = null;
            IEnumerable<(string, uint, ushort, ReadOnlyMemory<ushort>)> writeIndex = null;
            Memory<ushort>[] readArray = null;
            Memory<ushort>[] readbackArray = null;
            if (read == DeviceAccessMode.Inconsecutive)
            {
                readIndex = __process_data.Where(d => d.access == DataSyncMode.Read && d.size > 0).Select(d => new ValueTuple<string, uint, ushort>("D", d.start, d.size));
                readArray = (__process_data.Where(d => d.access == DataSyncMode.Read && d.size > 0).Select(d => d.data.AsMemory<ushort>())).ToArray();
            }
            if (readback == DeviceAccessMode.Inconsecutive)
            {
                readbackIndex = __process_data.Where(d => d.access == DataSyncMode.Readback && d.size > 0).Select(d => new ValueTuple<string, uint, ushort>("D", d.start, d.size));
                readbackArray = (__process_data.Where(d => d.access == DataSyncMode.Readback && d.size > 0).Select(d => d.data.AsMemory<ushort>())).ToArray();
            }
            if (write == DeviceAccessMode.Inconsecutive)
            {
                writeIndex = __process_data.Where(d => d.access == DataSyncMode.Write && d.size > 0).Select(d => new ValueTuple<string, uint, ushort, ReadOnlyMemory<ushort>>("D", d.start, d.size, d.data.AsMemory<ushort>()));
            }

            sw.Start();
            while (true)
            {
                if (__stop_event.WaitOne(0))
                {
                    break;
                }
                try
                {
                    if (read == DeviceAccessMode.Consecutive)
                    {
                        for (int i = 0; i < __process_data.Count; ++i)
                        {
                            if (__process_data[i].access == DataSyncMode.Read && __process_data[i].size > 0)
                            {
                                master.ReadLocalDeviceInWord(monitoring, "D", __process_data[i].start, __process_data[i].size, out end, __process_data[i].data);
                                __process_data_end = end;
                                if (end != 0)
                                {
                                    ExceptionMessage = $"End Code : {end:X04}";
                                    Counter = 0;
                                    State = DataSynchronizerState.Exception;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (readArray.Length != 0)
                        {
                            master.ReadLocalDeviceInWord(monitoring, readIndex, null, out end, readArray, null);
                            __process_data_end = end;
                            if (end != 0)
                            {
                                ExceptionMessage = $"End Code : {end:X04}";
                                Counter = 0;
                                State = DataSynchronizerState.Exception;
                                return;
                            }
                        }
                    }

                    if (readback == DeviceAccessMode.Consecutive)
                    {
                        for (int i = 0; i < __process_data.Count; ++i)
                        {
                            if (__process_data[i].access == DataSyncMode.Readback && __process_data[i].size > 0)
                            {
                                master.ReadLocalDeviceInWord(monitoring, "D", __process_data[i].start, __process_data[i].size, out end, __process_data[i].data);
                                __process_data_end = end;
                                if (end != 0)
                                {
                                    ExceptionMessage = $"End Code : {end:X04}";
                                    Counter = 0;
                                    State = DataSynchronizerState.Exception;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (readbackArray.Length != 0)
                        {
                            master.ReadLocalDeviceInWord(monitoring, readbackIndex, null, out end, readbackArray, null);
                            __process_data_end = end;
                            if (end != 0)
                            {
                                ExceptionMessage = $"End Code : {end:X04}";
                                Counter = 0;
                                State = DataSynchronizerState.Exception;
                                return;
                            }
                        }
                    }

                    if (write == DeviceAccessMode.Consecutive)
                    {
                        for (int i = 0; i < __process_data.Count; ++i)
                        {
                            if (__process_data[i].access == DataSyncMode.Write && __process_data[i].size > 0)
                            {
                                master.WriteLocalDeviceInWord(monitoring, "D", __process_data[i].start, __process_data[i].size, out end, __process_data[i].data);
                                __process_data_end = end;
                                if (end != 0)
                                {
                                    ExceptionMessage = $"End Code : {end:X04}";
                                    Counter = 0;
                                    State = DataSynchronizerState.Exception;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (writeIndex.Any() == true)
                        {
                            master.WriteLocalDeviceInWord(monitoring, writeIndex, null, out end);
                            __process_data_end = end;
                            if (end != 0)
                            {
                                ExceptionMessage = $"End Code : {end:X04}";
                                Counter = 0;
                                State = DataSynchronizerState.Exception;
                                return;
                            }
                        }
                    }
                    Counter = counter++;
                }
                catch (Exception ex)
                {
                    ExceptionMessage = ex.Message;
                    Counter = 0;
                    State = DataSynchronizerState.Exception;
                    return;
                }
                
                lock (__sync_property_access_lock)
                {
                    for (int i = 0; i < __process_data.Count; ++i)
                    {
                        if (__process_data[i].access == DataSyncMode.Read || __process_data[i].access == DataSyncMode.Readback)
                            __process_data[i].data.CopyTo(__internal_process_data[i], 0);
                        else
                            __internal_process_data[i].CopyTo(__process_data[i].data, 0);
                    }
                    __internal_process_data_end = __process_data_end;
                }
                int ms = (int)(sw.ElapsedMilliseconds);
                if (ms < interval)
                    Thread.Sleep(interval - ms);
                PollingInterval = (int)sw.ElapsedMilliseconds;
                sw.Restart();
            }
        }

        private void __data_sync_delay(object param)
        {
            uint counter = 0;
            Random r = new Random();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while(true)
            {
                if (__stop_event.WaitOne(0))
                {
                    break;
                }

                Counter = counter++;

                Thread.Sleep(5000);

                for(int i = 0; i < __process_data[1].size; ++i)
                {
                    __process_data[1].data[i] = (ushort)r.Next(0, 65536);
                    //__process_data[1].data[i] = 0xFFFF;
                }

                lock (__sync_property_access_lock)
                {
                    for (int i = 0; i < __process_data.Count; ++i)
                    {
                        if (__process_data[i].access == DataSyncMode.Read || __process_data[i].access == DataSyncMode.Readback)
                            __process_data[i].data.CopyTo(__internal_process_data[i], 0);
                        else
                            __internal_process_data[i].CopyTo(__process_data[i].data, 0);
                    }
                    __internal_process_data_end = __process_data_end;
                }
                int ms = (int)(sw.ElapsedMilliseconds);
                PollingInterval = (int)sw.ElapsedMilliseconds;
            }
        }

        public DataSynchronizerState State
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __sync_state;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __sync_state = value;
                }
            }
        }

        public string ExceptionMessage
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __sync_exception_message;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __sync_exception_message = value;
                }
            }
        }

        public int PollingInterval
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __polling_interval;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __polling_interval = value;
                }
            }
        }

        public uint Counter
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __heartbeat_counter;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __heartbeat_counter = value;
                }
            }
        }
    }

}
