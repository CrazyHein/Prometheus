﻿using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class Settings
    {
        public Settings(string? path = null)
        {
            Restore(path);
        }

        public Settings(SlmpTargetProperty slmp, DAQTargetProperty daq, FTPTargetProperty ftp, PreferenceProperty pref )
        {
            SlmpTargetProperty = slmp;
            DAQTargetProperty = daq;
            FTPTargetProperty = ftp;
            PreferenceProperty = pref;
        }

        public string Description { get; init; } = "A graphical user interface for [Foliage Ocean IO List]";
        public string DataTypeCataloguePath { get; init; } = "Metadata/data_type_catalogue.xml";
        public string ControllerModelCataloguePath { get; init; } = "Metadata/controller_model_catalogue.xml";
        public string SettingsPath { get; init; } = "settings.json";

        public string UserSettingsPath { get; init; } = "USettings";

        public byte[] DataTypeCatalogueHash { get; set; }
        public byte[] ControllerModelCatalogueHash { get; set; }

        public string SeirenVersion { get; init; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LombardiaVersion { get; init; } = System.Reflection.Assembly.GetAssembly(typeof(IOCelcetaHelper)).GetName().Version.ToString();

        public uint SupporedFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedFileFormatVersion;
        public uint SupportedVariableFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedVariableFileFormatVersion;
        public uint SupportedIOFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedIOFileFormatVersion;
        public string GagharvVersion { get; init; } = System.Reflection.Assembly.GetAssembly(typeof(RemoteOperationMaster)).GetName().Version.ToString();
        public string TirasweelVersion { get; init; } = System.Reflection.Assembly.GetAssembly(typeof(AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol.Master)).GetName().Version.ToString();

        public SlmpTargetProperty SlmpTargetProperty { get; set; }
        public DAQTargetProperty DAQTargetProperty { get; set; }
        public FTPTargetProperty FTPTargetProperty { get; set; }
        public PreferenceProperty PreferenceProperty { get; set; }

        private static ReadOnlySpan<byte> __UTF8_BOM => new byte[] { 0xEF, 0xBB, 0xBF };

        public void Save(string? path = null)
        {
            if (path == null)
                path = SettingsPath;
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true });
            writer.WriteStartObject();
            writer.WritePropertyName("Debugger");
            SlmpTargetProperty.Save(writer);
            writer.WritePropertyName("DAQ");
            DAQTargetProperty.Save(writer);
            writer.WritePropertyName("FTP");
            FTPTargetProperty.Save(writer);
            writer.WritePropertyName("Preference");
            PreferenceProperty.Save(writer);
            writer.WriteEndObject();
            writer.Flush();

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
            fs.Flush();

            DebugConsole.WriteInfo($"Save settings to configuration file : '{path}'.");
        }

        public void Restore(string? path = null)
        {
            if(path == null)
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
                                case "Debugger":
                                    SlmpTargetProperty = SlmpTargetProperty.RESTORE(ref reader); break;
                                case "DAQ":
                                    DAQTargetProperty = DAQTargetProperty.RESTORE(ref reader); break;
                                case "FTP":
                                    FTPTargetProperty = FTPTargetProperty.RESTORE(ref reader); break;
                                case "Preference":
                                    PreferenceProperty = PreferenceProperty.RESTORE(ref reader); break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException($"At least one exception has occurred while reading settings from configuration file : '{path}'.\n{ex.Message}\nThe default settings will be used.");
            }
            finally
            {
                SlmpTargetProperty ??= new SlmpTargetProperty();
                DAQTargetProperty ??= new DAQTargetProperty();
                FTPTargetProperty ??= new FTPTargetProperty();
                PreferenceProperty ??= new PreferenceProperty();

                DebugConsole.WriteInfo($"Read settings from configuration file : '{path}'.");
            }
        }
    }

    public class PreferenceProperty
    {
        public PreferenceProperty Copy()
        {
            return MemberwiseClone() as PreferenceProperty;
        }

        private int __record_operating_undo_queue_depth = 64;
        public int RecordOperatingUndoQueueDepth
        {
            get { return __record_operating_undo_queue_depth; }
            set
            {
                if (value < 2)
                    throw new ArgumentOutOfRangeException("The depth value should be greater than or equal to 2.");
                else if (value > 1024)
                    throw new ArgumentOutOfRangeException("The depth value should be less than or equal to 1024.");
                else
                    __record_operating_undo_queue_depth = value;
            }
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
        public DataSyncMode RxBitAreaSyncMode { get; set; } = DataSyncMode.Read;
        public DataSyncMode RxBlockAreaSyncMode { get; set; } = DataSyncMode.Read;
        public DataSyncMode RxControlAreaSyncMode { get; set; } = DataSyncMode.Write;
        public bool SaveLayoutState { get; set; } = false;
        public bool SeparateHardwareInterlocks { get; set; } = false;
        public bool SeparateExclusiveInterlocks { get; set; } = false;
        public string XlsSheetProtectionPassword { get; set; } = "password";

        public string HardwareInterlocksAlias { get; set; } = "Hardware";
        public string NonHardwareInterlocksAlias { get; set; } = "Software";
        public string ExclusiveInterlocksAlias { get; set; } = "Exclusive";
        public string NonExclusiveInterlocksAlias { get; set; } = "General";

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
            return JsonSerializer.Deserialize<PreferenceProperty>(ref reader, __JSON_OPTION);
        }
    }

}
