using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    public class Info
    {
        public string Description { get; init; } = "A graphical user interface for [Task User Parameters File]";
        public string ControllerModelCataloguePath { get; init; } = "Metadata/controller_model_catalogue.xml";
        public byte[] ControllerModelCatalogueHash { get; set; }
        public string XandriaVersion { get; init; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LombardiaVersion { get; init; } = System.Reflection.Assembly.GetAssembly(typeof(TaskUserParameterHelper)).GetName().Version.ToString();
    }
}
