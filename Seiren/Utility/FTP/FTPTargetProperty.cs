using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class FTPTargetProperty
    {
        [JsonIgnore]
        public IPAddress HostIPv4 { get; private set; } = IPAddress.Parse("192.168.3.3");
        [JsonPropertyName("HostIPv4")]
        public string HostIPv4String
        {
            get { return HostIPv4.ToString(); }
            set { HostIPv4 = IPAddress.Parse(value); }
        }

        public ushort HostPort { get; set; } = 21;
        public string User { get; set; } = "target";
        public string Password { get; set; } = "password";

        private int __timeout_value = 5000;
        public int TimeoutValue
        {
            get { return __timeout_value; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 0.");
                else
                    __timeout_value = value;
            }
        }
        private int __read_write_timeout_value = 5000;
        public int ReadWriteTimeoutValue
        {
            get { return __read_write_timeout_value; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 0.");
                else
                    __read_write_timeout_value = value;
            }
        }

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

        public static FTPTargetProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<FTPTargetProperty>(ref reader, __JSON_OPTION);
        }
    }
}
