using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia.OrbmentParameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    internal class RuntimeConfigurationModel : TabContentModel
    {
        public RuntimeConfiguration Configuration { get; init; }
        public RuntimeConfigurationModel(RuntimeConfiguration data) 
        {
            Configuration = new RuntimeConfiguration()
            {
                EventLogger = data.EventLogger,
                CustomEventLogger = data.CustomEventLogger,
                EventLoggerConfiguration = data.EventLoggerConfiguration.ShallowCopy(),
                DeviceIOScanTaskConfiguration = data.DeviceIOScanTaskConfiguration.ShallowCopy(),
                DeviceControlTaskConfiguration = data.DeviceControlTaskConfiguration.ShallowCopy(),
                DLinkServiceConfiguration = data.DLinkServiceConfiguration.ShallowCopy(),
                ILinkServiceConfiguration = data.ILinkServiceConfiguration.ShallowCopy(),
                RLinkServiceConfiguration = data.RLinkServiceConfiguration.ShallowCopy()
            };
        }
    }
}
