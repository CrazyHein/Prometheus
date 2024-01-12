using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public enum SmartECATPlatform
    {
        R12CCPU_V,
        RD55UP06_V,
        RD55UP12_V
    }

    public enum SmartECATMainPort
    {
        CH1,
        CH2,
    }

    public class IsDualNetworkPortPlatform : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is SmartECATPlatform)
            {
                SmartECATPlatform platform = (SmartECATPlatform)value;
                if (platform == SmartECATPlatform.RD55UP06_V)
                    return false;
                return true;
            }   
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SmartECATProperty
    {
        public string SettingsPath { get; init; } = "smart_ecat.json";
        public string UserSettingsPath { get; init; } = "USettings";
        private static ReadOnlySpan<byte> __UTF8_BOM => new byte[] { 0xEF, 0xBB, 0xBF };
        public FTPTargetProperty FTPTargetProperty { get; set; }
        public SmartECATInstallerProperty InstallerProperty { get; set; }

        public SmartECATProperty(string? path = null)
        {
            Restore(path);
        }

        public void Restore(string? path = null)
        {
            if (path == null)
                path = SettingsPath;
            try
            {
                DebugConsole.WriteInfo($"Read SMART-ECAT settings from configuration file : '{SettingsPath}'.");
                ReadOnlySpan<byte> fs = File.ReadAllBytes(SettingsPath);
                if (fs.StartsWith(__UTF8_BOM)) fs = fs.Slice(__UTF8_BOM.Length);

                var reader = new Utf8JsonReader(fs, new JsonReaderOptions() { CommentHandling = JsonCommentHandling.Skip });
                while (reader.Read())
                {
                    switch (reader.TokenType, reader.CurrentDepth)
                    {
                        case (JsonTokenType.PropertyName, 1):
                            switch (reader.GetString())
                            {
                                case "FTP":
                                    FTPTargetProperty = FTPTargetProperty.RESTORE(ref reader); break;
                                case "Installer":
                                    InstallerProperty = SmartECATInstallerProperty.RESTORE(ref reader); break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException($"At least one exception has occurred while reading SMART-ECAT settings from configuration file : '{SettingsPath}'.\n{ex.Message}\nThe default settings will be used.");
            }
            finally
            {
                FTPTargetProperty ??= new FTPTargetProperty() { ReadWriteTimeoutValue = 300000};
                InstallerProperty ??= new SmartECATInstallerProperty();
            }
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
                writer.WritePropertyName("FTP");
                FTPTargetProperty.Save(writer);
                writer.WritePropertyName("Installer");
                InstallerProperty.Save(writer);
                writer.WriteEndObject();
                writer.Flush();

                using var fs = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
                fs.Flush();
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }

            DebugConsole.WriteInfo($"Save SMART-ECAT settings to configuration file : '{SettingsPath}'.");
        }
    }

    public class SmartECATInstallerProperty
    {
        private static JsonSerializerOptions __JSON_OPTION = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(null, false)
            }
        };

        public void Save(Utf8JsonWriter writer)
        {
            JsonSerializer.Serialize(writer, this, __JSON_OPTION);
        }

        public static SmartECATInstallerProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<SmartECATInstallerProperty>(ref reader, __JSON_OPTION);
        }

        public const string DEFAULT_LIC_NAME = "EtherCAT.lic";
        public const string DEFAULT_ENI_NAME = "masterENI.xml";
        public const string DEFAULT_RT_NIC_DRIVER_NAME = "emllDW3504.out";
        public const string DEFAULT_ECAT_ALP_NAME = "ecatAPL_ClassA.out";
        public const string DEFAULE_IDOL_INI_NAME = "idol.ini";
        public const string DEFAULT_RETRY_INI_NAME = "retry.ini";
        public const string DEFAULT_SETTING_INI_NAME = "setting.ini";
        public const string DEFAULT_STARTUP_CMD_NAME = "STARTUP.CMD";

        public SmartECATPlatform? PlatformModel { get; set; } =  SmartECATPlatform.RD55UP12_V;
        public bool BootFromSD { get; set; } = true;
        public bool TransferNIC { get; set; } = false;
        public bool TransferAPP { get; set; } = false;
        public bool TransferCFG { get; set; } = true;

        private int __cycle_time = 1000;
        public int CycleTime
        {
            get { return __cycle_time; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("The CycleTime value should be greater than 0.");
                else
                    __cycle_time = value;
            }
        }
        public bool EnableDCM { get; set; } = true;
        public bool EnableCableRedundancy { get; set; } = false;

        private int __log_file_size_in_kbyte = 100;
        public int LogFileSize
        {
            get { return __log_file_size_in_kbyte; }
            set
            {
                if (value < 100)
                    throw new ArgumentOutOfRangeException("The LogFileSize value should be greater than or equal to 100.");
                else
                    __log_file_size_in_kbyte = value;
            }
        }

        private int __number_of_network_scan_reties = 10;
        public int NumOfNetworkScanReties
        {
            get { return __number_of_network_scan_reties; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The number of network scan reties should be greater than or equal to 0.");
                else
                    __number_of_network_scan_reties = value;
            }
        }

        public SmartECATMainPort MainPort { get; set; } = SmartECATMainPort.CH1;
        public bool TransferNetworkInformation { get; set; } = true;
        public string LocalNetworkInformationFilePath { set; get; } = "";

        public bool TransferLicense { get; set; } = false;
        public string LocalLicenseFilePath { set; get; } = "";

        public override string ToString()
        {
            string eni = $"\\\"{ENI_PATH}{DEFAULT_ENI_NAME}\\\"";
            string clk = $"-sysclk {CycleTime}";
            string dcm = $"-dcmmode {(EnableDCM ? "busshift" : "off")}";
            string log = $"-logout {(BootFromSD ? "1" : "0")}";
            string unknown = "-t 0";
            string cable;
            switch (PlatformModel)
            {
                case SmartECATPlatform.R12CCPU_V:
                    if(EnableCableRedundancy)
                    {
                        if (MainPort == SmartECATMainPort.CH1)
                            cable = "-DW3504 2 1 r12ccpu -DW3504 1 1 r12ccpu";
                        else
                            cable = "-DW3504 1 1 r12ccpu -DW3504 2 1 r12ccpu";
                    }
                    else
                    {
                        if (MainPort == SmartECATMainPort.CH1)
                            cable = "-DW3504 2 1 r12ccpu";
                        else
                            cable = "-DW3504 1 1 r12ccpu";
                    }
                    break;
                case SmartECATPlatform.RD55UP06_V:
                    cable = "-DW3504 2 1 rd55up06";
                    break;
                case SmartECATPlatform.RD55UP12_V:
                    if (EnableCableRedundancy)
                    {
                        if (MainPort == SmartECATMainPort.CH1)
                            cable = "-DW3504 2 1 rd55up12 -DW3504 1 1 rd55up12";
                        else
                            cable = "-DW3504 1 1 rd55up12 -DW3504 2 1 rd55up12";
                    }
                    else
                    {
                        if (MainPort == SmartECATMainPort.CH1)
                            cable = "-DW3504 2 1 rd55up12";
                        else
                            cable = "-DW3504 1 1 rd55up12";
                    }
                    break;
                default:
                    cable = string.Empty;
                    break;
            }

            return string.Join(" ", eni, cable, clk, unknown, dcm, log);
        }

        [JsonIgnore]
        public string SDCARD_PATH
        {
            get
            {
                switch (PlatformModel)
                {
                    case SmartECATPlatform.R12CCPU_V:
                        return @"/2/";
                    default:
                        return @"/SD/";
                }
            }
        }

        [JsonIgnore]
        public string BUILT_IN_MEMORY_PATH
        {
            get
            {
                switch (PlatformModel)
                {
                    case SmartECATPlatform.R12CCPU_V:
                        return @"/0/";
                    default:
                        return @"/ROM/";
                }
            }
        }

        [JsonIgnore]
        public string ENI_PATH
        {
            get
            {
                if (BootFromSD)
                    return SDCARD_PATH;
                else
                    return BUILT_IN_MEMORY_PATH;
            }
        }

        [JsonIgnore]
        public string LIC_PATH { get { return BUILT_IN_MEMORY_PATH; } }

        [JsonIgnore]
        public string IDOL_PATH { get { return ENI_PATH; } }

        [JsonIgnore]
        public string RETRY_PATH { get { return ENI_PATH; } }

        [JsonIgnore]
        public string SETTING_PATH { get { return ENI_PATH; } }

        [JsonIgnore]
        public string STARTUP_PATH { get { return ENI_PATH; } }

        [JsonIgnore]
        public string LOG_PATH { get { return $"{SDCARD_PATH}Log/"; } }

    }
}
