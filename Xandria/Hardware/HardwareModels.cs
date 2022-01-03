using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    public class HardwareModels : TabContentModel
    {
        public TaskUserParameterHelper Helper { get; private set; }
        public ControllerModelCatalogue ControllerModelCatalogue { get; private set; }
        private ObservableCollection<HardwareModel> __local_hardware_model_collection;
        private ObservableCollection<HardwareModel> __remote_hardware_model_collection;
        public IReadOnlyList<HardwareModel> HardwareModelCollection { get; private set; }
        public IReadOnlyList<HardwareModel> LocalHardwareModelCollection { get; private set; }
        public IReadOnlyList<HardwareModel> RemoteHardwareModelCollection { get; private set; }

        public HardwareModels(TaskUserParameterHelper helper, ControllerModelCatalogue model)
        {
            Helper = helper;
            ControllerModelCatalogue = model;
            __local_hardware_model_collection = new ObservableCollection<HardwareModel>(
                helper.LocalHardwareCollection.Select(d => new HardwareModel()
                {
                    DeviceModel = d.DeviceModel,
                    Switch = d.Switch,
                    LocalAddress = d.LocalAddress,
                    CustomFields = new List<(string key, string value)>(d.CustomFields.Keys.Select(k => (k, d.CustomFields[k])))
                }));
            __remote_hardware_model_collection = new ObservableCollection<HardwareModel>(
                helper.RemoteHardwareCollection.Select(d => new HardwareModel()
                {
                    DeviceModel = d.DeviceModel,
                    Switch = d.Switch,
                    IPv4 = d.IPv4,
                    Port = d.Port,
                    CustomFields = new List<(string key, string value)>(d.CustomFields.Keys.Select(k => (k, d.CustomFields[k])))
                }));
            LocalHardwareModelCollection = __local_hardware_model_collection;
            RemoteHardwareModelCollection = __remote_hardware_model_collection;
        }

        public ushort HostAddress
        {
            get { return Helper.HostAddress; }
            set
            {
                if (value != Helper.HostAddress)
                {
                    Helper.HostAddress = value;
                    Modified = true;
                }
            }
        }

        public void AddLocal(HardwareModel model)
        {
            Helper.Add(model.ID, model.Name, (model.DeviceModel as LocalExtensionModel).BitSize, model.Switch, model.LocalAddress, model.CustomFields);
            __local_hardware_model_collection.Add(model);
            Modified = true;
        }

        public void InsertLocal(int index, HardwareModel model)
        {
            Helper.InsertAt(index, model.ID, model.Name, (model.DeviceModel as LocalExtensionModel).BitSize, model.Switch, model.LocalAddress, model.CustomFields);
            __local_hardware_model_collection.Insert(index, model);
            Modified = true;
        }

        public void RemoveLocal(int index)
        {
            Helper.RemoveLocalAt(index);
            __local_hardware_model_collection.RemoveAt(index);
            Modified = true;
        }

        public void ReplaceLocal(int index, HardwareModel model)
        {
            Helper.ReplaceAt(index, model.ID, model.Name, (model.DeviceModel as LocalExtensionModel).BitSize, model.Switch, model.LocalAddress, model.CustomFields);
            __local_hardware_model_collection[index] = model;
            Modified = true;
        }

        public int IndexOfLocal(HardwareModel model)
        {
            return __local_hardware_model_collection.IndexOf(model);
        }

        public void AddRemote(HardwareModel model)
        {
            Helper.Add(model.ID, model.Name, model.Switch, model.IPv4, model.Port, model.CustomFields);
            __remote_hardware_model_collection.Add(model);
            Modified = true;
        }

        public void InsertRemote(int index, HardwareModel model)
        {
            Helper.InsertAt(index, model.ID, model.Name, model.Switch, model.IPv4, model.Port, model.CustomFields);
            __remote_hardware_model_collection.Insert(index, model);
            Modified = true;
        }

        public void RemoveRemote(int index)
        {
            Helper.RemoveRemoteAt(index);
            __remote_hardware_model_collection.RemoveAt(index);
            Modified = true;
        }

        public void ReplaceRemote(int index, HardwareModel model)
        {
            Helper.ReplaceAt(index, model.ID, model.Name, model.Switch, model.IPv4, model.Port, model.CustomFields);
            __remote_hardware_model_collection[index] = model;
            Modified = true;
        }

        public int IndexOfRemote(HardwareModel model)
        {
            return __remote_hardware_model_collection.IndexOf(model);
        }
    }

    public class HardwareModel
    {
        public DeviceModel DeviceModel { get; set; } = new LocalExtensionModel();
        public ushort ID { get { return DeviceModel.ID; } }
        public string Name { get { return DeviceModel.Name; } }
        public uint Switch { get; set; }
        public ushort LocalAddress { get; set; }
        public string IPv4 { get; set; } = "127.0.0.1";
        public ushort Port { get; set; }
        public List<(string key, string value)> CustomFields { get; set; } = new List<(string key, string value)>();

        public string CustomString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var e in CustomFields)
                    sb.Append($"{e.key} : {e.value}\n");
                return sb.ToString().Trim();
            }
            set
            {
                CustomFields.Clear();
                if (value != null)
                {
                    System.IO.StringReader sr = new System.IO.StringReader(value.Trim());
                    while(true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        var fields = line.Split(":", StringSplitOptions.TrimEntries);
                        if (fields.Length != 2 || fields.Any(s => s.Length == 0) == true)
                            throw new ArgumentException("Invalid string of custom fields.");
                        CustomFields.Add((fields[0], fields[1]));
                    }
                }
                CustomFields.TrimExcess();
            }
        }

        public override string ToString()
        {
            return $"{{ ID = 0x{ID:X04} ; Name = {Name} ; Switch = 0x{Switch:X08} ; Local Address = 0x{LocalAddress:X04} ; Remote IPv4 = {IPv4} : Remote Port = {Port} }}";
        }
    }
}
