using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Workloads;
using Grpc.Core;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Management
{
    public class RemoteManagementService : RemoteManagement.RemoteManagementBase
    {
        private DAQBackgroundWorker __background_worker;

        public RemoteManagementService(DAQBackgroundWorker worker)
        {
            __background_worker = worker;
        }
        public override Task<Status> QueryStatue(Dummy request, ServerCallContext context)
        {
            return Task.FromResult<Status>(new Status() { 
                WorkerStage = __background_worker.WorkerStage.ToString(),
                AcquisitionUnitState = __background_worker.AcquisitionUnitState.ToString(),
                AcquisitionUnitExceptionMessage = __background_worker.AcquisitionUnitExceptionMessage,
                AcquisitionUnitWriteInterval = __background_worker.AcquisitionUnitWriteInterval,
                AcquisitionUnitHeartbeat = __background_worker.AcquisitionUnitHeartbeat,
                AcquisitionUnitStatus = __background_worker.AcquisitionUnitStatus.ToString()
            });
        }

        public static Server Start(string host, int port, DAQBackgroundWorker worker)
        {
            var server = new Server()
            {
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
                Services = { RemoteManagement.BindService(new RemoteManagementService(worker)) }
            };
            server.Start();
            return server;
        }

        public static async Task StopAsync(Server server)
        {
            await server.ShutdownAsync();
        }
    }
}
