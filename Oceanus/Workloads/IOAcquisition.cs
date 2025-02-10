using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;
using System.Diagnostics;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Workloads
{
    internal class IOAcquisition
    {
        public static DataAcquisitionUnit BuildDataAcquisitionUnit()
        {
            return new DataAcquisitionUnit();
        }
    }
}
