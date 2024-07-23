using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;

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
        public static uint SupporedSourceFileFormatVersion { get; } = RecipeDocument.SupportedSourceFileFormatVersion;
        public static uint SupporedScriptFileFormatVersion { get; } = RecipeDocument.SupportedScriptFileFormatVersion;
        public static uint SupportedVariableFileFormatVersion { get; } = IOCelcetaHelper.SupportedVariableFileFormatVersion;
        public static uint SupportedIOFileFormatVersion { get; } = IOCelcetaHelper.SupportedIOFileFormatVersion;
        public static string GagharvVersion { get; } = System.Reflection.Assembly.GetAssembly(typeof(RemoteOperationMaster)).GetName().Version.ToString();

        public static DataTypeCatalogue DataTypeCatalogue { get; } = new DataTypeCatalogue(Settings.DataTypeCataloguePath);
        public static ControllerModelCatalogue ControllerModelCatalogue { get; } = new ControllerModelCatalogue(Settings.ControllerModelCataloguePath);
        public static byte[] DataTypeCatalogueHash { get; } = DataTypeCatalogue.MD5Code;
        public static byte[] ControllerModelCatalogueHash { get; } = ControllerModelCatalogue.MD5Code;

        private static ReadOnlySpan<byte> __UTF8_BOM => new byte[] { 0xEF, 0xBB, 0xBF };
        public PreferenceProperty? PreferenceProperty { get; set; }
        public ILinkProperty? ILinkProperty { get; set; }


        public Settings(string? path = null)
        {
            Restore(path);
        }

        public void Save(string? path = null)
        {
            if (path == null)
                path = SettingsPath;
            try
            {
                using var ms = new MemoryStream();
                using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true });
                writer.WriteStartObject();
                writer.WritePropertyName("Preference");
                PreferenceProperty.Save(writer);
                writer.WritePropertyName("ILinkProperty");
                ILinkProperty.Save(writer);
                writer.WriteEndObject();
                writer.Flush();

                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
                fs.Flush();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Restore(string? path = null)
        {
            if (path == null)
                path = SettingsPath;
            try
            {
                ReadOnlySpan<byte> fs = File.ReadAllBytes(path);
                if (fs.StartsWith(__UTF8_BOM)) fs = fs.Slice(__UTF8_BOM.Length);

                var reader = new Utf8JsonReader(fs, new JsonReaderOptions() { CommentHandling = JsonCommentHandling.Skip });

                while (reader.Read())
                {
                    switch (reader.TokenType, reader.CurrentDepth)
                    {
                        case (JsonTokenType.PropertyName, 1):
                            switch (reader.GetString())
                            {
                                case "Preference":
                                    PreferenceProperty = PreferenceProperty.RESTORE(ref reader); break;
                                case "ILinkProperty":
                                    ILinkProperty = ILinkProperty.RESTORE(ref reader); break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
                PreferenceProperty ??= new PreferenceProperty();
                ILinkProperty ??= new ILinkProperty();
            }
        }
    }

    public class PreferenceProperty
    {
        public PreferenceProperty Copy()
        {
            return (MemberwiseClone() as PreferenceProperty)!;
        }

        private static JsonSerializerOptions __JSON_OPTION = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(null, false) }
        };
        public void Save(Utf8JsonWriter writer)
        {
            JsonSerializer.Serialize(writer, this, __JSON_OPTION);
        }

        public static PreferenceProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<PreferenceProperty>(ref reader, __JSON_OPTION)!;
        }

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
        public ILinkProperty Copy()
        {
            return (MemberwiseClone() as ILinkProperty)!;
        }

        private static JsonSerializerOptions __JSON_OPTION = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(null, false) }
        };
        public void Save(Utf8JsonWriter writer)
        {
            JsonSerializer.Serialize(writer, this, __JSON_OPTION);
        }

        public static ILinkProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<ILinkProperty>(ref reader, __JSON_OPTION)!;
        }

        [JsonIgnore]
        public IPAddress IPv4 { get; set; } = IPAddress.Parse("192.168.3.3");
        [JsonPropertyName("IPv4")]
        public string IPv4s
        {
            get { return IPv4.ToString(); }
            set { IPv4 = IPAddress.Parse(value); }
        }

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
