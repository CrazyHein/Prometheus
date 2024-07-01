using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK
{
    public class Settings
    {
        public static string SettingsPath { get; } = "settings.json";
        public static string Description { get; } = "A graphical user interface for [CCPU Napishtim Script]";
        public static string DataTypeCataloguePath { get; } = "Metadata/data_type_catalogue.xml";
        public static string ControllerModelCataloguePath { get; } = "Metadata/controller_model_catalogue.xml";
        public static string ArkVersion { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string NapishtimVersion { get; } = System.Reflection.Assembly.GetAssembly(typeof(Step)).GetName().Version.ToString();
        public static string LombardiaVersion { get; } = System.Reflection.Assembly.GetAssembly(typeof(IOCelcetaHelper)).GetName().Version.ToString();
        public static uint SupporedFileFormatVersion { get; } = IOCelcetaHelper.SupportedFileFormatVersion;
        public static uint SupportedVariableFileFormatVersion { get; } = IOCelcetaHelper.SupportedVariableFileFormatVersion;
        public static uint SupportedIOFileFormatVersion { get; } = IOCelcetaHelper.SupportedIOFileFormatVersion;
        public static string GagharvVersion { get; } = System.Reflection.Assembly.GetAssembly(typeof(RemoteOperationMaster)).GetName().Version.ToString();

        public static DataTypeCatalogue DataTypeCatalogue { get; } = new DataTypeCatalogue(Settings.DataTypeCataloguePath);
        public static ControllerModelCatalogue ControllerModelCatalogue { get; } = new ControllerModelCatalogue(Settings.ControllerModelCataloguePath);
        public static byte[] DataTypeCatalogueHash { get; } = DataTypeCatalogue.MD5Code;
        public static byte[] ControllerModelCatalogueHash { get; } = ControllerModelCatalogue.MD5Code;

        public PreferenceProperty PreferenceProperty { get; private set; }
        public ILinkProperty ILinkProperty { get; private set; }

        public Settings()
        {
            PreferenceProperty = new PreferenceProperty();
            ILinkProperty = new ILinkProperty();
        }
    }

    public class PreferenceProperty
    {
        private int __recently_opened_file_collection_capacity = 16;
        public int RecentlyOpenedFileCollectionCapacity
        {
            get { return __recently_opened_file_collection_capacity; }
            set
            {
                if (value < 2)
                    throw new ArgumentOutOfRangeException("The capacity value should be greater than or equal to 2.");
                else if (value > 32)
                    throw new ArgumentOutOfRangeException("The capacity value should be less than or equal to 32.");
                else
                    __recently_opened_file_collection_capacity = value;
            }
        }
    }

    public class ILinkProperty
    {
        public IPAddress IPv4 { get; set; } = IPAddress.Parse("192.168.3.3");
        public ushort Port { get; set; } = 8367;
        private int __send_timeout_value = 5000;
        public int SendTimeoutValue
        {
            get { return __send_timeout_value; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 0.");
                else
                    __send_timeout_value = value;
            }
        }

        private int __receive_timeout_value = 5000;
        public int ReceiveTimeoutValue
        {
            get { return __receive_timeout_value; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 0.");
                else
                    __receive_timeout_value = value;
            }
        }
    }
}
