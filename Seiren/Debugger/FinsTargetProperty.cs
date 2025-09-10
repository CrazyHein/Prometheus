using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger
{
    public class FinsTargetProperty
    {
        public FinsTargetProperty Copy()
        {
            return MemberwiseClone() as FinsTargetProperty;
        }

        [JsonIgnore]
        public IPAddress DestinationIPv4 { get; private set; } = IPAddress.Parse("192.168.3.88");
        
        [JsonPropertyName("DestinationIPv4")]
        public string DestinationIPv4String
        {
            get { return DestinationIPv4.ToString(); }
            set { 
                DestinationIPv4 = IPAddress.Parse(value);
                ServerNodeAddress = DestinationIPv4.GetAddressBytes()[3];
            }
        }
        
        public ushort DestinationPort { get; set; } = 9600;

        [JsonIgnore]
        public byte ServerNodeAddress { get; private set; } = 88;

        private int __send_timeout_value = 200;
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
        private int __receive_timeout_value = 200;
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

        private int __polling_intervale = 100;
        public int PollingInterval
        {
            get { return __polling_intervale; }
            set
            {
                if (value < 10)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 10.");
                else
                    __polling_intervale = value;
            }
        }

        public override string ToString()
        {
            var option = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(null, false)
                }
            };
            return JsonSerializer.Serialize(this, option);
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

        public static FinsTargetProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<FinsTargetProperty>(ref reader, __JSON_OPTION);
        }
    }
}
