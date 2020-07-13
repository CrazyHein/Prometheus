using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

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

        public static byte[] VariableCatalogueMD5 { get; private set; }
        public static byte[] DataTypeCatalogueMD5 { get; private set; }
        public static byte[] ControllerModelCatalogueMD5 { get; private set; }

        static Metadata()
        {
            Title = "IO Celceta";
            Description = "A graphical user interface for < Foliage Ocean IO List >.";
            DataTypeCatalogue = "Metadata/data_type_catalogue.xml";
            ControllerModelCatalogue = "Metadata/controller_model_catalogue.xml";
            VariableCatalogue = "Metadata/variable_catalogue.xml";

            AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ProductVersion = Application.ProductVersion.ToString();

            using (MD5 hash = MD5.Create())
            {
                using (FileStream stream = File.Open(DataTypeCatalogue, FileMode.Open))
                {
                    DataTypeCatalogueMD5 = hash.ComputeHash(stream);
                }
                using (FileStream stream = File.Open(ControllerModelCatalogue, FileMode.Open))
                {
                    ControllerModelCatalogueMD5 = hash.ComputeHash(stream);
                }
                using (FileStream stream = File.Open(VariableCatalogue, FileMode.Open))
                {
                    VariableCatalogueMD5 = hash.ComputeHash(stream);
                }
            }
        }

        public Metadata(IOListDataHelper dataHelp)
        {
            DataHelperSupportedFileFormatVersion = dataHelp.SupportedFileFormatVersion;
        }
    }
}
