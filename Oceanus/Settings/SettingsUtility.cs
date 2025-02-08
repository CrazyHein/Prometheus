using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings
{
    internal class SettingsUtility
    {
        private static ReadOnlySpan<byte> __UTF8_BOM => new byte[] { 0xEF, 0xBB, 0xBF };
        public static (FTPTargetProperty, DAQTargetProperty, GRPCServerProperty) Restore(string? path)
        {
            FTPTargetProperty ftp = null;
            DAQTargetProperty daq = null;
            GRPCServerProperty grpc = null;

            if (path == null)
                path = Path.GetDirectoryName(Environment.ProcessPath) + "/service.settings.json";
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
                                case "DAQ":
                                    daq = DAQTargetProperty.RESTORE(ref reader); break;
                                case "FTP":
                                    ftp = FTPTargetProperty.RESTORE(ref reader); break;
                                case "GRPC":
                                    grpc = GRPCServerProperty.RESTORE(ref reader);break;
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
                ftp ??= new FTPTargetProperty();
                daq ??= new DAQTargetProperty();
                grpc ??= new GRPCServerProperty();
            }

            return (ftp, daq, grpc);
        }
    }
}
