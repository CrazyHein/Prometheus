using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class AppInstallerProperty
    {
        public AppInstallerProperty Copy()
        {
            return MemberwiseClone() as AppInstallerProperty;
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

        public static AppInstallerProperty RESTORE(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<AppInstallerProperty>(ref reader, __JSON_OPTION);
        }

        private string __boot_task_name = "SEIREN_CONTROLLER";

        public string BootTaskName
        {
            get { return __boot_task_name; } 
            set 
            {
                if (Regex.IsMatch(value, @"^[a-zA-Z_]+[a-zA-Z0-9_]*$"))
                    __boot_task_name = value;
                else
                    throw new ArgumentException("Task name can only consist of letters, numbers, and underscores, and cannot begin with a number.");
            }
        }

        private int __boot_task_stack_size = 0x20000;
        public int BootTaskStackSize 
        { 
            get { return __boot_task_stack_size; } 
            set
            {
                if (value > 0)
                    __boot_task_stack_size = value;
                else
                    throw new ArgumentOutOfRangeException("Task stack size should be greater than 0.");
            }
        }

        private int __boot_task_priority = 200;
        public int BootTaskPriority
        {
            get { return __boot_task_priority; }
            set
            {
                if (value > 255 || value < 0)
                    throw new ArgumentOutOfRangeException("The value of sask priority must be between 0 and 255.");
                __boot_task_priority = value;
            }
        }

        public uint BootTaskSpawnFlag { get; set; } = 0x1000000;

        [JsonIgnore]
        public static IReadOnlyList<string> OrbmentRuntimeBinaryFiles { get; } = new string[]
        {
            "vx_R2H_DLink.out",
            "vx_R2H_EthModule.out",
            "vx_R2H_ExtModule.out",
            "vx_R2H_ILink.out",
            "vx_R2H_Task.out"
        };

        [JsonIgnore]
        public string VariableDictionaryPath { get; } = "/2/variable_catalogue.xml";
        
        [JsonIgnore]
        public string IOListPath { get; } = "/2/io_list.xml";
    }
}
