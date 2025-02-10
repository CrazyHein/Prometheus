using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Management;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Workloads;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Net;
using static System.Net.WebRequestMethods;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus
{
    public class DAQBackgroundWorker : BackgroundService
    {
        public enum Stage: int
        {
            ReadOfflineIOConfiguration = 0,
            ReadOnlineIOConfiguration = 1,
            StartupDataAcquisionTask = 2,
            DataAcquisiting = 3
        }

        private readonly ILogger<DAQBackgroundWorker> _logger;
        private object __sync_property_access_lock = new object();
        private FTPTargetProperty __ftp_property;
        private DAQTargetProperty __daq_property;
        private GRPCServerProperty __grpc_property;
        private DataTypeCatalogue __data_types;
        private ControllerModelCatalogue __controller_models;
        private DataAcquisitionUnit? __data_acquisition_unit;


        private Stage __stage = Stage.ReadOfflineIOConfiguration;
        private AcquisitionRange __range = AcquisitionRange.None;

        private (VariableDictionary? vd, ControllerConfiguration? cc, ObjectDictionary? od,
                ProcessDataImage? txdiag, ProcessDataImage? txbit, ProcessDataImage? txblk,
                ProcessDataImage? rxctl, ProcessDataImage? rxbit, ProcessDataImage? rxblk, 
                InterlockCollection? intlk,Miscellaneous? misc, byte[]? varHash, byte[]? ioHash,
                Exception? ex) __io_offline_configuration;

        private (DAQ_SERVER_INFO_T? info, DAQ_SERVER_IO_HASH_T? varHash, DAQ_SERVER_IO_HASH_T? ioHash,
                uint sampleRate, Exception? ex) __io_online_configuration;

        private Grpc.Core.Server? __management_service;

        public DAQBackgroundWorker(FTPTargetProperty ftp, DAQTargetProperty daq, GRPCServerProperty grpc, DataTypeCatalogue datatypes, ControllerModelCatalogue models, ILogger<DAQBackgroundWorker> logger)
        {
            _logger = logger;
            __ftp_property = ftp;
            __daq_property = daq;
            __grpc_property = grpc;

            __data_types = datatypes;
            __controller_models = models;

            __management_service = RemoteManagementService.Start("0.0.0.0", grpc.ServerPort, this);

            _logger.LogInformation($"{DateTimeOffset.Now}: \n{ftp.ToString()}\n{daq.ToString()}\n{grpc.ToString()}");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    switch(WorkerStage)
                    {
                        case Stage.ReadOfflineIOConfiguration:
                            await Task.Run(() => __io_offline_configuration = IOConfiguration.UploadOfflineIOConfiguration(__ftp_property, __data_types, __controller_models));
                            if(__io_offline_configuration.ex == null)
                                WorkerStage = Stage.ReadOnlineIOConfiguration;
                            else
                                _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\n{__io_offline_configuration.ex.Message}");
                            break;
                        case Stage.ReadOnlineIOConfiguration:
                            await Task.Run(() => __io_online_configuration = IOConfiguration.UploadOnlineIOConfiguration(__daq_property));
                            if (__io_online_configuration.ex == null)
                            {
                                string offlineVar = IOConfiguration.MD5String(__io_offline_configuration.varHash);
                                string offlineIO = IOConfiguration.MD5String(__io_offline_configuration.ioHash);
                                string onlineVar = IOConfiguration.MD5String(ref __io_online_configuration.varHash);
                                string onlineIO = IOConfiguration.MD5String(ref __io_online_configuration.ioHash);
                                if (offlineVar == onlineVar && offlineIO == onlineIO)
                                    WorkerStage = Stage.StartupDataAcquisionTask;
                                else
                                {
                                    _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\nIO Configuration Mismatch:\n{offlineVar} <--> {onlineVar}\n{offlineIO} <--> {onlineIO}");
                                    WorkerStage = Stage.ReadOfflineIOConfiguration;
                                }
                            }
                            else
                            {
                                _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\n{__io_online_configuration.ex.Message}");
                                WorkerStage = Stage.ReadOfflineIOConfiguration;
                            }
                            break;
                        case Stage.StartupDataAcquisionTask:
                            __data_acquisition_unit = IOAcquisition.BuildDataAcquisitionUnit();
                            AcquisitionRange range = AcquisitionRange.IOConfiguration;
                            if (__io_offline_configuration.txdiag.ProcessDatas.Concat(__io_offline_configuration.txbit.ProcessDatas).Concat(__io_offline_configuration.txblk.ProcessDatas).
                                Concat(__io_offline_configuration.rxctl.ProcessDatas).Concat(__io_offline_configuration.rxbit.ProcessDatas).Concat(__io_offline_configuration.rxblk.ProcessDatas).
                                Any(d => d.DAQ == true) == false)
                                range = AcquisitionRange.FullIOImage;

                            var ret = await __data_acquisition_unit.Startup(__daq_property,
                                __io_offline_configuration.txdiag.ProcessDatas, __io_offline_configuration.txbit.ProcessDatas, __io_offline_configuration.txblk.ProcessDatas,
                                __io_offline_configuration.rxctl.ProcessDatas, __io_offline_configuration.rxbit.ProcessDatas, __io_offline_configuration.rxblk.ProcessDatas,
                                range);
                            if (ret != AcquisitionUnitState.Exception)
                            {
                                _logger.LogInformation($"{DateTimeOffset.Now}: \n{WorkerStage}:\nEntering into [{Stage.DataAcquisiting}]\nIO acquisition range: {range}");
                                WorkerStage = Stage.DataAcquisiting;
                                Range = range;
                            }
                            else
                            {
                                _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\n{__data_acquisition_unit.ExceptionMessage}");

                                await __data_acquisition_unit.Stop();
                                WorkerStage = Stage.ReadOfflineIOConfiguration;
                                Range = AcquisitionRange.None;
                            }
                            break;
                        case Stage.DataAcquisiting:
                            if(__data_acquisition_unit.State == AcquisitionUnitState.Exception)
                            {
                                _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\n{__data_acquisition_unit.ExceptionMessage}");

                                await __data_acquisition_unit.Stop();
                                WorkerStage = Stage.ReadOfflineIOConfiguration;
                                Range = AcquisitionRange.None;
                            }
                            else
                            {

                            }
                            break;
                    }

                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch(OperationCanceledException)
            {

            }
            catch(Exception ex)
            {
                _logger.LogError($"{DateTimeOffset.Now}: \n{WorkerStage}:\n{ex.Message}");
                Environment.Exit(1);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (__management_service != null)
                await RemoteManagementService.StopAsync(__management_service);
            __management_service = null;
            if (__data_acquisition_unit != null)
                await __data_acquisition_unit.Stop();
            __data_acquisition_unit = null;
            await base.StopAsync(cancellationToken);
        }

        public Stage WorkerStage
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __stage;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __stage = value;
                }
            }
        }

        public AcquisitionRange Range
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    return __range;
                }
            }
            private set
            {
                lock (__sync_property_access_lock)
                {
                    __range = value;
                }
            }
        }

        public AcquisitionUnitState AcquisitionUnitState
        {
            get
            {
                lock (__sync_property_access_lock)
                    return __data_acquisition_unit.State;
            }
        }

        public string AcquisitionUnitExceptionMessage
        {
            get
            {
                lock (__sync_property_access_lock)
                    return __data_acquisition_unit.ExceptionMessage;
            }
        }

        public AcquisitionUnitStatus AcquisitionUnitStatus
        {
            get
            {
                lock (__sync_property_access_lock)
                    return __data_acquisition_unit.Status;
            }
        }

        public int AcquisitionUnitWriteInterval
        {
            get
            {
                lock (__sync_property_access_lock)
                    return __data_acquisition_unit.DiskWriteInterval;
            }
        }

        public uint AcquisitionUnitHeartbeat
        {
            get
            {
                lock (__sync_property_access_lock)
                    return __data_acquisition_unit.Counter;
            }
        }

        public string ServiceStatusSummary
        {
            get
            {
                lock (__sync_property_access_lock)
                {
                    if (__stage != Stage.DataAcquisiting)
                        return $"Current Task: {__stage}";
                    else
                        return $"Current Task: {__stage}\nRange: {__range}\nTask Status: {__data_acquisition_unit.StatusSummary}";
                }
            }
        }

        public string ServiceConfigurationSummary
        {
            get
            {
                return $"{__ftp_property.ToString()}\n{__daq_property.ToString()}\n{__grpc_property.ToString()}";
            }
        }
    }
}
