using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings
{
    public class GRPCServerProperty
    {
        public ushort ServerPort { get; set; } = 50002;

        private static JsonSerializerOptions __JSON_OPTION = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
                {
                    new JsonStringEnumConverter(null, false)
                }
        };

        public static GRPCServerProperty? RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<GRPCServerProperty>(ref reader, __JSON_OPTION);
        }
        public override string ToString()
        {
            return $"gRPC Property:\n{JsonSerializer.Serialize(this, __JSON_OPTION)}";
        }
    }
}
