using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Eresia
{
    class Metadata
    {
        public static string Title { get; private set; }
        public static string Description { get; private set; }
        public static string ControllerModelCatalogue { get; private set; }
        public static string AssemblyVersion { get; private set; }
        public static string ProductVersion { get; private set; }

        static Metadata()
        {
            Title = "Eresia";
            Description = "A graphical user interface for < Task User Configuration Parameters >.";
            ControllerModelCatalogue = "Metadata/controller_model_catalogue.xml";

            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ProductVersion = Application.ProductVersion.ToString();
        }
    }
}
