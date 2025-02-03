using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAQPort = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility.TCP;
using DAQMaster = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol.Master;
using System.Diagnostics;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol;
using System.Runtime.InteropServices;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System.Reflection;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    public enum AcquisitionUnitState : byte
    {
        Exception = 0x01,
        Idle = 0x02,
        Connecting = 0x03,
        Collecting = 0x04,
    }

    public struct AcquisitionUnitStatus
    {
        public int tx_received;
        public int tx_residual;
        public int tx_corrupted;
        public int rx_received;
        public int rx_residual;
        public int rx_corrupted;
        public AcquisitionUnitStatus(int txreceived, int txresidual, int txcorrupted, int rxreceived, int rxresidual, int rxcorrupted)
        {
            this.tx_received = txreceived;
            this.tx_residual = txresidual;
            this.tx_corrupted = txcorrupted;
            this.rx_received = rxreceived;
            this.rx_residual = rxresidual;
            this.rx_corrupted = rxcorrupted;
        }

        public override string ToString()
        {
            return $"[Tx: {tx_received}/{tx_residual}/{tx_corrupted}] [Rx: {rx_received}/{rx_residual}/{rx_corrupted}]";
        }
    }

    public class DataAcquisitionUnit
    {
        private AcquisitionUnitState __acquisition_uinit_state = AcquisitionUnitState.Idle;

        private object __sync_property_access_lock = new object();
        private SemaphoreSlim __sync_operation_access_lock = new SemaphoreSlim(1);
        private AutoResetEvent __stop_event = new AutoResetEvent(false);
        private Thread __acquisition_routine_thread;
        private string __acquisition_unit_exception_message = "";
        private int __disk_write_interval = 0;
        private AcquisitionUnitStatus __status;
        private uint __heartbeat_counter = 0;

        private DAQPort __daq_interface = null;

        public DataAcquisitionUnit()
        {

        }

        public async Task<AcquisitionUnitState> Startup(DAQTargetProperty property, 
            IEnumerable<ProcessData> diag, IEnumerable<ProcessData> txbit, IEnumerable<ProcessData> txblk,
            IEnumerable<ProcessData> ctrl, IEnumerable<ProcessData> rxbit, IEnumerable<ProcessData> rxblk)
        {
            __sync_operation_access_lock.Wait();
            try
            {
                switch (State)
                {
                    case AcquisitionUnitState.Collecting:
                        return AcquisitionUnitState.Collecting;
                    case AcquisitionUnitState.Exception:
                        if (__acquisition_routine_thread != null)
                        {
                            __acquisition_routine_thread.Join();
                            __acquisition_routine_thread = null;
                        }
                        break;
                    case AcquisitionUnitState.Idle:
                        break;
                }

                __stop_event.Reset();
                
                __daq_interface = new DAQPort(new System.Net.IPEndPoint(property.SourceIPv4, 0),
                                        new System.Net.IPEndPoint(property.DestinationIPv4, property.DestinationPort),
                                        property.SendTimeoutValue, property.ReceiveTimeoutValue);
                DAQMaster daqMaster = new DAQMaster(__daq_interface, property.InternalReservedBufferSize);
                IDataStorage storage = null;
                int writeInterval = 0;
                switch(property.DAQStorageSchema)
                {
                    case DAQStorageSchema.CSV:
                        storage = new CsvDataStorage(property.DataFilePath, property.DataFileNamePrefix, property.DataFileSize,
                                diag.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                txbit.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                txblk.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                ctrl.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                rxbit.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                rxblk.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }));
                        writeInterval = property.ExpectedDiskWriteInterval;
                        break;
                    case DAQStorageSchema.MongoDB:
                        storage = new MongoDataStorage(property.MongoDBConnectionString, property.MongoDBDatabaseName, property.MongoDBCollectionName, property.MongoDBCollectionSize,
                                diag.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                txbit.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                txblk.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                ctrl.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                rxbit.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }),
                                rxblk.Where(d => d.DAQ).Select(d => new AcquisitionDataIndex(d.ProcessObject.Variable.Type.Name) { BitPos = d.BitPos, FriendlyName = d.ProcessObject.Variable.Name }));
                        writeInterval = property.ExpectedDatabaseWriteInterval;
                        break;
                }

                Status = new AcquisitionUnitStatus();
                State = AcquisitionUnitState.Connecting;
                await Task.Run(() => __daq_interface.Connect());

                State = AcquisitionUnitState.Collecting;
                ExceptionMessage = "N/A";

                __acquisition_routine_thread = new Thread(new ParameterizedThreadStart(__data_acquisition_routine));
                __acquisition_routine_thread.Start(Tuple.Create(daqMaster, storage, writeInterval));
                return State;
            }
            catch (Exception ex)
            {
                State = AcquisitionUnitState.Exception;
                ExceptionMessage = ex.Message;
                return AcquisitionUnitState.Exception;
            }
            finally
            {
                __sync_operation_access_lock.Release();
            }
        }

        public async Task<AcquisitionUnitState> Stop()
        {
            __sync_operation_access_lock.Wait();
            try
            {
                switch (State)
                {
                    case AcquisitionUnitState.Idle:
                        return AcquisitionUnitState.Idle;
                    case AcquisitionUnitState.Collecting:
                        __stop_event.Set();
                        break;
                    case AcquisitionUnitState.Exception:
                        __stop_event.Reset();
                        break;
                }

                if (__acquisition_routine_thread != null)
                {
                    await Task.Run(() => __acquisition_routine_thread.Join());
                    __acquisition_routine_thread = null;
                }
                if (__daq_interface != null)
                {
                    __daq_interface.Dispose();
                    __daq_interface = null;
                }
                State = AcquisitionUnitState.Idle;
                return AcquisitionUnitState.Idle;
            }
            finally
            {
                __sync_operation_access_lock.Release();
            }
        }

        public AcquisitionUnitState State
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __acquisition_uinit_state;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __acquisition_uinit_state = value;
                }
            }
        }

        public string ExceptionMessage
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __acquisition_unit_exception_message;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __acquisition_unit_exception_message = value;
                }
            }
        }

        public int DiskWriteInterval
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __disk_write_interval;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __disk_write_interval = value;
                }
            }
        }

        public AcquisitionUnitStatus Status
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __status;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __status = value;
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


        private void __data_acquisition_routine(object param)
        {
            (DAQMaster master, IDataStorage storage, int interval) = (Tuple<DAQMaster, IDataStorage, int>)param;
            DAQ_SERVER_INFO_T info = new DAQ_SERVER_INFO_T();
            ReadOnlyMemory<byte> data = null;
            int txframesize = 0, rxframesize = 0;
            int txtimepos = 0, txdiagpos = 0, txbitpos = 0, txblkpos = 0, rxctrlpos = 0, rxbitpos = 0, rxblkpos = 0;
            byte[] txframedata = null;
            int rxreceived = 0, rxresidual = 0, rxcorrupted = 0, txreceived = 0, txresidual = 0, txcorrupted = 0, expected = 0;
            ushort trans = 0;
            bool negotiation = true;

            uint counter = 0;
            Counter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                try 
                { 
                    if (__stop_event.WaitOne(0))
                    {
                        break;
                    }
                    if(negotiation)
                    {
                        master.AcquisiteServerInfo(trans++, ref info);

                        expected = info.capacity;
                        txframesize = 2 * (info.tx_timestamp_size_in_word + info.diag_size_in_word + info.tx_bit_size_in_word + info.tx_blk_size_in_word);
                        rxframesize = 2 * (info.rx_timestamp_size_in_word + info.ctrl_size_in_word + info.rx_bit_size_in_word + info.rx_blk_size_in_word);
                            
                        txdiagpos = 2 * (info.tx_timestamp_size_in_word);
                        txbitpos = 2 * (info.diag_size_in_word) + txdiagpos;
                        txblkpos = 2 * (info.tx_bit_size_in_word) + txbitpos;

                        rxctrlpos = 2 * (info.rx_timestamp_size_in_word);
                        rxbitpos = 2 * (info.ctrl_size_in_word) + rxctrlpos;
                        rxblkpos = 2 * (info.rx_bit_size_in_word) + rxbitpos;

                        if (info.capacity == 0 || (txframesize == 0 && rxframesize == 0))
                        {
                            ExceptionMessage = $"DAQ server configuration error(capacity == {info.capacity} txframesize == {txframesize} rxframesize == {rxframesize}).";
                            State = AcquisitionUnitState.Exception;
                            break;
                        }

                        txframedata = new byte[txframesize * expected];
                        //rxframedata = new byte[rxframesize * expected];

                        master.ResetAcquisitionData(trans++, 0);
                        System.DateTime now = System.DateTime.Now;

                        do
                            master.AcquisiteData(true, trans++, 1, out txreceived, out txresidual, out txcorrupted, out data);
                        while (txreceived != 1);
                        var ttx = MemoryMarshal.Read<uint>(data.Span);
                        do
                            master.AcquisiteData(false, trans++, 1, out rxreceived, out rxresidual, out rxcorrupted, out data);
                        while (rxreceived != 1);
                        var trx = MemoryMarshal.Read<uint>(data.Span);

                        storage.InitializeTimestamp(now, ttx);

                        if (ttx != trx)
                        {
                            do
                                master.AcquisiteData(true, trans++, 1, out txreceived, out txresidual, out txcorrupted, out data);
                            while (txreceived != 1);
                        }
                        ttx = MemoryMarshal.Read<uint>(data.Span);
                        Debug.Assert(ttx == trx);

                        negotiation = false;
                    }                
                    else
                    {
                        master.AcquisiteData(true, trans++, expected, out txreceived, out txresidual, out txcorrupted, out data);
                        data.CopyTo(txframedata);
                        master.AcquisiteData(false, trans++, txreceived, out rxreceived, out rxresidual, out rxcorrupted, out data);
                            
                        if(rxreceived != txreceived)
                        {
                            ExceptionMessage = $"The number of rx frames({rxreceived}) should be always equal to the number of tx frames({txreceived}).";
                            State = AcquisitionUnitState.Exception;
                            break;
                        }
                        else if (txreceived > 0 && rxreceived > 0)
                        {
                            var t1 = MemoryMarshal.Read<uint>(txframedata.AsSpan());
                            var t2 = MemoryMarshal.Read<uint>(data.Span);
                            if(t1 != t2)
                            {
                                ExceptionMessage = $"The rx frame timestamp({t2}) should be always equal to tx frame timestamp({t1}).";
                                State = AcquisitionUnitState.Exception;
                                break;
                            }
                        }
                        else if (txcorrupted != 0 || rxcorrupted != 0)
                        {
                            ExceptionMessage = $"The rx({rxcorrupted}) or tx({txcorrupted}) acquisition data is corrupted.";
                            State = AcquisitionUnitState.Exception;
                            break;
                        }
                        for (int  i = 0; i < rxreceived; ++i)
                            storage.WriteRecord(MemoryMarshal.Read<uint>(txframedata.AsSpan().Slice(i * txframesize + txtimepos)),
                                txframedata.AsSpan().Slice(i * txframesize + txdiagpos, info.diag_size_in_word * 2),
                                txframedata.AsSpan().Slice(i * txframesize + txbitpos, info.tx_bit_size_in_word * 2),
                                txframedata.AsSpan().Slice(i * txframesize + txblkpos, info.tx_blk_size_in_word * 2),
                                data.Span.Slice(i * rxframesize + rxctrlpos, info.ctrl_size_in_word * 2),
                                data.Span.Slice(i * rxframesize + rxbitpos, info.rx_bit_size_in_word * 2),
                                data.Span.Slice(i * rxframesize + rxblkpos, info.rx_blk_size_in_word * 2));
                        storage.Flush();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMessage = ex.Message;
                    State = AcquisitionUnitState.Exception;
                    break;
                }
                int ms = (int)(sw.ElapsedMilliseconds);
                if (ms < interval)
                    Thread.Sleep(interval - ms);
                DiskWriteInterval = (int)sw.ElapsedMilliseconds;
                sw.Restart();
                Status = new AcquisitionUnitStatus(txreceived, txresidual, txcorrupted, rxreceived, rxresidual, rxcorrupted);
                Counter = counter++;
            }
            storage.Dispose();
        }
    }
}
