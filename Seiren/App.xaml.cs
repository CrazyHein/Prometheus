using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using System.Windows;
using System.Windows.Threading;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledExceptionEventHandler;
        }

        void App_DispatcherUnhandledExceptionEventHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DebugConsole.WriteException("An unhandled exception has occurred.");
            DebugConsole.WriteException(e.Exception);
            DebugConsole.Flush();
        }
    }
}
