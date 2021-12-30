using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class Settings
    {
        public Settings()
        {
            Restore(null);
        }
        public string Description { get; init; } = "A graphical user interface for [Foliage Ocean IO List]";
        public string DataTypeCataloguePath { get; init; } = "Metadata/data_type_catalogue.xml";
        public string ControllerModelCataloguePath { get; init; } = "Metadata/controller_model_catalogue.xml";
        public string SettingsPath { get; init; } = "settings.json";

        public byte[] DataTypeCatalogueHash { get; set; }
        public byte[] ControllerModelCatalogueHash { get; set; }

        public string SeirenVersion { get; init; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LombardiaVersion { get; init; } = System.Reflection.Assembly.GetAssembly(typeof(IOCelcetaHelper)).GetName().Version.ToString();

        public uint SupporedFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedFileFormatVersion;
        public uint SupportedVariableFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedVariableFileFormatVersion;
        public uint SupportedIOFileFormatVersion { get; init; } = IOCelcetaHelper.SupportedIOFileFormatVersion;

        public SlmpTargetProperty SlmpTargetProperty { get; private set; }

        private static ReadOnlySpan<byte> __UTF8_BOM => new byte[] { 0xEF, 0xBB, 0xBF };

        public void Save()
        {
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true });
            writer.WriteStartObject();
            writer.WritePropertyName("Debugger");
            SlmpTargetProperty.Save(writer);
            writer.WriteEndObject();
            writer.Flush();

            using var fs = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
            fs.Flush();
        }

        public void Restore(string path)
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
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                SlmpTargetProperty = new SlmpTargetProperty();
                //throw;
            }
        }
    }
}
