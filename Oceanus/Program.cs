using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var (ftp, daq, grpc) = SettingsUtility.Restore(null);
            DataTypeCatalogue dataTypes = new DataTypeCatalogue(Path.GetDirectoryName(Environment.ProcessPath) + "/Metadata/data_type_catalogue.xml");
            ControllerModelCatalogue controllerModels = new ControllerModelCatalogue(Path.GetDirectoryName(Environment.ProcessPath) + "/Metadata/controller_model_catalogue.xml");

            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = ".NET Oceanus DAQ";
            });
            builder.Services.AddHostedService<DAQBackgroundWorker>();
            builder.Services.AddSingleton<DAQTargetProperty>(daq);
            builder.Services.AddSingleton<FTPTargetProperty>(ftp);
            builder.Services.AddSingleton<GRPCServerProperty>(grpc);
            builder.Services.AddSingleton<DataTypeCatalogue>(dataTypes);
            builder.Services.AddSingleton<ControllerModelCatalogue>(controllerModels);

            var host = builder.Build();
            host.Run();
        }
    }
}