using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Workloads;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tirasweel.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public override Task<DatabaseSummary> QueryDatabaseSummary(Dummy request, ServerCallContext context)
        {
            var ret = __background_worker.StorageSummay(context.CancellationToken);
            return Task.FromResult<DatabaseSummary>(new DatabaseSummary() {
                DocumentsCount = ret.counts,
                Start = ret.start.ToTimestamp(),
                End = ret.end.ToTimestamp()
            });
        }

        public override async Task QueryDatabaseLatest(LatestDocumentsRequest request, IServerStreamWriter<Document> responseStream, ServerCallContext context)
        {
            var ret = __background_worker.StorageLatest(request.Milliseconds, context.CancellationToken);
            foreach (var doc in ret)
            {
                if (context.CancellationToken.IsCancellationRequested == false)
                {
                    await responseStream.WriteAsync(new Document()
                    {
                        DocumentInBson = ByteString.CopyFrom(doc)
                    });
                }
            }
        }

        public override async Task QueryDatabaseHead(NumberOfDocumentsRequest request, IServerStreamWriter<Document> responseStream, ServerCallContext context)
        {
            var ret = __background_worker.StorageHead(request.Counts, context.CancellationToken);
            foreach (var doc in ret)
            {
                if (context.CancellationToken.IsCancellationRequested == false)
                {
                    await responseStream.WriteAsync(new Document()
                    {
                        DocumentInBson = ByteString.CopyFrom(doc)
                    });
                }
            }
        }

        public override async Task QueryDatabaseTail(NumberOfDocumentsRequest request, IServerStreamWriter<Document> responseStream, ServerCallContext context)
        {
            var ret = __background_worker.StorageTail(request.Counts, context.CancellationToken);
            foreach (var doc in ret)
            {
                if (context.CancellationToken.IsCancellationRequested == false)
                {
                    await responseStream.WriteAsync(new Document()
                    {
                        DocumentInBson = ByteString.CopyFrom(doc)
                    });
                }
            }
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
