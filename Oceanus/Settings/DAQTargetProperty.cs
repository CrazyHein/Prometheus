using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Oceanus.Settings
{
    public class DAQTargetProperty
    {
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

        private int __internal_reserved_buffer_size = 4 * 1024 * 1024;
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

        public string MongoDBDatabaseName { get; set; } = "DAQ";
        public string MongoDBCollectionName { get; set; } = "Oceanus";

        private long __mongo_db_collection_size = 100 * 1024 * 1024;
        public long MongoDBCollectionSize
        {
            get { return __mongo_db_collection_size; }
            set
            {
                if (value < 10 * 1024 * 1024)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 10 MB.");
                else
                    __mongo_db_collection_size = value;
            }
        }

        private int __expected_db_write_interval = 1000;
        public int ExpectedDatabaseWriteInterval
        {
            get { return __expected_db_write_interval; }
            set
            {
                if (value < 500)
                    throw new ArgumentOutOfRangeException("The setting value should be greater than or equal to 500.");
                else
                    __expected_db_write_interval = value;
            }
        }

        public string MongoDBConnectionString { get; set; } = "mongodb://localhost:27017/?connectTimeoutMS=60000&serverSelectionTimeoutMS=10000&compressors=snappy,zlib,zstd";

        private static JsonSerializerOptions __JSON_OPTION = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
                {
                    new JsonStringEnumConverter(null, false)
                }
        };

        public static DAQTargetProperty? RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<DAQTargetProperty>(ref reader, __JSON_OPTION);
        }
        public override string ToString()
        {
            return $"DAQ Property:\n{JsonSerializer.Serialize(this, __JSON_OPTION)}";
        }
    }
}
