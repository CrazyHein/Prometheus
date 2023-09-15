using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    public class DAQTargetProperty
    {
        public DAQTargetProperty Copy()
        {
            return MemberwiseClone() as DAQTargetProperty;
        }

        [JsonIgnore]
        public IPAddress SourceIPv4 { get; private set; } = IPAddress.Any;
        [JsonPropertyName("SourceIPv4")]
        public string SourceIPv4String
        {
            get { return SourceIPv4.ToString(); }
            set { SourceIPv4 = IPAddress.Parse(value); }
        }

        [JsonIgnore]
        public IPAddress DestinationIPv4 { get; private set; } = IPAddress.Parse("192.168.3.3");
        [JsonPropertyName("DestinationIPv4")]
        public string DestinationIPv4String
        {
            get { return DestinationIPv4.ToString(); }
            set { DestinationIPv4 = IPAddress.Parse(value); }
        }
        public ushort DestinationPort { get; set; } = 8368;

        private int __send_timeout_value = 2000;
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

        private int __receive_timeout_value = 10000;
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

        private int __expected_disk_write_interval = 1000;
        public int ExpectedDiskWriteInterval
        {
            get { return __expected_disk_write_interval; }
            set
            {
                if (value < 500)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 500.");
                else
                    __expected_disk_write_interval = value;
            }
        }

        private int __data_file_size = 1024;
        public int DataFileSize
        {
            get { return __data_file_size; }
            set
            {
                if (value < 4)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 4(k).");
                else
                    __data_file_size = value;
            }
        }

        public string DataFilePath { set; get; } = "DAQ";

        public string DataFileNamePrefix { set; get; } = "Data";

        private int __internal_reserved_buffer_size = 4*1024*1024;
        public int InternalReservedBufferSize
        {
            get { return __internal_reserved_buffer_size; }
            set
            {
                if (value < 1024 * 1024)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 1,048,576.");
                else
                    __internal_reserved_buffer_size = value;
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

        public static DAQTargetProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<DAQTargetProperty>(ref reader, __JSON_OPTION);
        }
    }
}
