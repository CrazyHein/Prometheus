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
        public override Task<StatusSummary> QueryStatus(Dummy request, ServerCallContext context)
        {
            return Task.FromResult<StatusSummary>(new StatusSummary() { 
                Status = __background_worker.ServiceStatusSummary
            });
        }

        public override Task<ConfigurationSummary> QueryConfiguration(Dummy request, ServerCallContext context)
        {
            return Task.FromResult<ConfigurationSummary>(new ConfigurationSummary() {
                Configuration = __background_worker.ServiceConfigurationSummary
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
