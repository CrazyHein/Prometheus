using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger
{
    public enum DeviceAccessMode
    {
        Auto,
        Consecutive,
        Inconsecutive,
    }
    public class SlmpTargetProperty
    {
        public static List<string> MODULE_IO_NAMES;
        private static string __CPU_MODULE_IO_PREFIX = "CPU | ";
        private static string __CCIEF_MODULE_IO_PREFIX = "CCIEF | ";

        static SlmpTargetProperty()
        {
            MODULE_IO_NAMES = new List<string>();
            foreach (var name in Enum.GetNames(typeof(REQUEST_CPU_DESTINATION_MODULE_IO_T)))
                MODULE_IO_NAMES.Add(__CPU_MODULE_IO_PREFIX + name);
            foreach (var name in Enum.GetNames(typeof(REQUEST_CCIEF_DESTINATION_MODULE_IO_T)))
                MODULE_IO_NAMES.Add(__CCIEF_MODULE_IO_PREFIX + name);
        }

        [JsonIgnore]
        public IPAddress SourceIPv4 { get; private set; } = IPAddress.Any;
        [JsonPropertyName("SourceIPv4")]
        public string SourceIPv4String
        {
            get { return SourceIPv4.ToString(); }
            set { SourceIPv4 = IPAddress.Parse(value); }
        }
        public ushort SourcePort { get; set; } = 5010;

        [JsonIgnore]
        public IPAddress DestinationIPv4 { get; private set; } = IPAddress.Parse("192.168.3.3");
        [JsonPropertyName("DestinationIPv4")]
        public string DestinationIPv4String
        {
            get { return DestinationIPv4.ToString(); }
            set { DestinationIPv4 = IPAddress.Parse(value); }
        }
        public ushort DestinationPort { get; set; } = 5010;

        public bool R_DedicatedMessageFormat { get; set; } = false;
        public bool UDPTransportLayer { get; set; } = false;
        public byte NetworkNumber { get; set; } = 0;
        public byte StationNumber { get; set; } = 255;

        public string __module_io_name = __CPU_MODULE_IO_PREFIX + Enum.GetName(REQUEST_CPU_DESTINATION_MODULE_IO_T.OWN_STATION);
        public string ModuleIOName
        {
            get { return __module_io_name; }
            set
            {
                if (value.StartsWith(__CPU_MODULE_IO_PREFIX))
                    ModuleIONumber = (ushort)Enum.Parse<REQUEST_CPU_DESTINATION_MODULE_IO_T>(value[__CPU_MODULE_IO_PREFIX.Length..]);
                else if (value.StartsWith(__CCIEF_MODULE_IO_PREFIX))
                    ModuleIONumber = (ushort)Enum.Parse<REQUEST_CCIEF_DESTINATION_MODULE_IO_T>(value[__CCIEF_MODULE_IO_PREFIX.Length..]);
                else
                    throw new ArgumentException("Invalid module io string.");
                __module_io_name = value;
            }
        }
        [JsonIgnore]
        public ushort ModuleIONumber { get; private set; } = (ushort)(REQUEST_CPU_DESTINATION_MODULE_IO_T.OWN_STATION);

        public byte MultidropNumber { get; set; } = 0;
        public ushort ExtensionStationNumber { get; set; } = 0;

        public MESSAGE_FRAME_TYPE_T FrameType { get; set; } = MESSAGE_FRAME_TYPE_T.MC_3E;
        public MESSAGE_DATA_CODE_T DataCode { get; set; } = MESSAGE_DATA_CODE_T.BINARY;
        public DeviceAccessMode DeviceReadMode { get; set; } = DeviceAccessMode.Auto;
        public DeviceAccessMode DeviceWriteMode { get; set; } = DeviceAccessMode.Auto;


        public int SendTimeoutValue { get; set; } = 200;
        public int ReceiveTimeoutValue { get; set; } = 200;
        public int SendBufferSize { get; set; } = 8192;
        public int ReceiveBufferSize { get; set; } = 8192;
        public int PollingInterval { get; set; } = 100;
        public ushort MonitoringTimer { get; set; } = 500;
        
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

        public static SlmpTargetProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<SlmpTargetProperty>(ref reader, __JSON_OPTION);
        }
    }
}
