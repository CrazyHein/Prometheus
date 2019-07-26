using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta
{
    public class Metadata
    {
        public static string Title { get; private set; }
        public static string Description { get; private set; }
        public static string DataTypeCatalogue { get; private set; }
        public static string ControllerModelCatalogue { get; private set; }
        public static string VariableCatalogue { get; private set; }
        public uint DataHelperSupportedFileFormatVersion { get; private set; }
        public static string AssemblyVersion { get; private set; }
        public static string ProductVersion { get; private set; }

        static Metadata()
        {
            Title = "IO Celceta";
            Description = "A graphics editing tool for < AMEC IO List >.";
            DataTypeCatalogue = "data_type_catalogue.xml";
            ControllerModelCatalogue = "controller_model_catalogue.xml";
            VariableCatalogue = "variable_catalogue.xml";

            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ProductVersion = Application.ProductVersion.ToString();
        }

        public Metadata(IOListDataHelper dataHelp)
        {
            DataHelperSupportedFileFormatVersion = dataHelp.SupportedFileFormatVersion;
        }
    }
}
