using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class ControllerConfigurationModel : RecordContainerModel
    {
        private ControllerConfiguration __controller_configuration_collection;
        public ControllerModelCatalogue ControllerModelCatalogue { get; private set; }
        private ObservableCollection<DeviceConfigurationModel> __device_configurations;
        public IReadOnlyList<DeviceConfigurationModel> DeviceConfigurations { get; private set; }
        public ObjectsModel? SubscriberObjects { get; set; }
        public ControllerConfigurationModel(ControllerConfiguration cc, ControllerModelCatalogue cmc, OperatingHistory history)
        {
            __controller_configuration_collection = cc;
            ControllerModelCatalogue = cmc;
            __device_configurations = new ObservableCollection<DeviceConfigurationModel>(
                cc.Configurations.Values.Select(c => new DeviceConfigurationModel()
                {
                    DeviceModel = c.DeviceModel,
                    Switch = c.Switch,
                    LocalAddress = c.LocalAddress,
                    IPv4 = c.IPv4,
                    Port = c.Port,
                    ReferenceName = c.ReferenceName
                }));
            DeviceConfigurations = __device_configurations;
            Modified = false;
            OperatingHistory = history;
            Name = "Controller Configuration";
        }

        public void Add(DeviceConfigurationModel model, bool log = true)
        {
            __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            __device_configurations.Add(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count - 1, OriginalValue = null, NewValue = model });
        }

        public DeviceConfigurationModel RemoveAt(int index, bool log = true)
        {
            DeviceConfigurationModel model = __device_configurations[index];
            __controller_configuration_collection.Remove(model.ReferenceName);
            __device_configurations.RemoveAt(index);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null });
            return model;
        }

        public void Remove(DeviceConfigurationModel model, bool log = true)
        {
            __controller_configuration_collection.Remove(model.ReferenceName);
            int index = __device_configurations.IndexOf(model);
            __device_configurations.Remove(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null });
        }

        public void Insert(int index, DeviceConfigurationModel model, bool log = true)
        {
            if (index > __device_configurations.Count)
                throw new ArgumentOutOfRangeException();
            __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            __device_configurations.Insert(index, model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, NewValue = model });
        }

        public void Move(int srcIndex, int dstIndex, bool log = true)
        {
            if (srcIndex > __device_configurations.Count || dstIndex > __device_configurations.Count || srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();
            var temp = __device_configurations[srcIndex];
            __device_configurations.RemoveAt(srcIndex);
            __device_configurations.Insert(dstIndex, temp);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Move, OriginaPos = srcIndex, NewPos = dstIndex, OriginalValue = temp, NewValue = temp });
        }

        public int IndexOf(DeviceConfigurationModel model)
        {
            return __device_configurations.IndexOf(model);
        }

        public bool Contains(DeviceConfigurationModel c)
        {
            return __controller_configuration_collection.Configurations.ContainsKey(c.ReferenceName);
        }

        public bool Contains(string name)
        {
            return __controller_configuration_collection.Configurations.ContainsKey(name);
        }

        public void Replace(int index, DeviceConfigurationModel newModel, bool log = true)
        {
            DeviceConfigurationModel original = __device_configurations[index];
            __controller_configuration_collection.Replace(original.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            __device_configurations[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = original, NewValue = newModel });
            //Notify others here
            SubscriberObjects?.UpdateBinding(original.ReferenceName);
        }

        public void Replace(DeviceConfigurationModel originalModel, DeviceConfigurationModel newModel, bool log = true)
        {
            __controller_configuration_collection.Replace(originalModel.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            int index = __device_configurations.IndexOf(originalModel);
            __device_configurations[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = originalModel, NewValue = newModel });
            //Notify others here
            SubscriberObjects?.UpdateBinding(originalModel.ReferenceName);
        }

        public IEnumerable<string> ReferenceNames
        {
            get { return __device_configurations.Select(c => c.ReferenceName); }
        }

        public void Save(XmlDocument doc, XmlElement root)
        {
            __controller_configuration_collection.Save(doc, ReferenceNames, root);
            Modified = false;
        }

        public void ImportDevices(string path, bool log = true)
        {
            TaskUserParameterHelper helper = new TaskUserParameterHelper(ControllerModelCatalogue, path, out _);
            uint i = 0;
            string refer = string.Empty;
            foreach (var d in helper.LocalHardwareCollection)
            {
                while (true)
                {
                    refer = $"unnamed_device_reference_{i}";
                    if (__controller_configuration_collection.Configurations.ContainsKey(refer))
                        i++;
                    else
                        break;
                }
                DeviceConfigurationModel model = new DeviceConfigurationModel() { DeviceModel = d.DeviceModel, Switch = d.Switch, LocalAddress = d.LocalAddress, ReferenceName = refer };
                __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
                __device_configurations.Add(model);
                if (log && OperatingHistory != null)
                    OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count - 1, OriginalValue = null, NewValue = model });
            }
            foreach (var d in helper.RemoteHardwareCollection)
            {
                while (true)
                {
                    refer = $"unnamed_device_reference_{i}";
                    if (__controller_configuration_collection.Configurations.ContainsKey(refer))
                        i++;
                    else
                        break;
                }
                DeviceConfigurationModel model = new DeviceConfigurationModel() { DeviceModel = d.DeviceModel, Switch = d.Switch, IPv4 = d.IPv4, Port = d.Port, ReferenceName = refer };
                __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
                __device_configurations.Add(model);
                if (log && OperatingHistory != null)
                    OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count - 1, OriginalValue = null, NewValue = model });
            }
            if (helper.LocalHardwareCollection.Count != 0 || helper.RemoteHardwareCollection.Count != 0)
                Modified = false;
        }

        public override void Undo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Remove(r.NewValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Move:
                    Move(r.NewPos, r.OriginaPos, false);
                    break;
                case Operation.Remove:
                    if (r.OriginaPos == __device_configurations.Count)
                        Add(r.OriginalValue as DeviceConfigurationModel, false);
                    else
                        Insert(r.OriginaPos, r.OriginalValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Insert:
                    Remove(r.NewValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.NewValue as DeviceConfigurationModel, r.OriginalValue as DeviceConfigurationModel, false);
                    break;
            }
        }

        public override void Redo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Add(r.NewValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Move:
                    Move(r.OriginaPos, r.NewPos, false);
                    break;
                case Operation.Remove:
                    Remove(r.OriginalValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Insert:
                    Insert(r.NewPos, r.NewValue as DeviceConfigurationModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.OriginalValue as DeviceConfigurationModel, r.NewValue as DeviceConfigurationModel, false);
                    break;
            }
        }
    }

    public class DeviceConfigurationModel : IEquatable<DeviceConfigurationModel>
    {
        public DeviceModel DeviceModel { get; set; } = new LocalExtensionModel();
        public ushort ID { get { return DeviceModel.ID; } }
        public string Name { get { return DeviceModel.Name; } }
        public uint Switch { get; set; }
        public ushort LocalAddress { get; set; }
        public string IPv4 { get; set; } = "127.0.0.1";
        public ushort Port { get; set; }
        private string __reference_name = "unnamed_device_reference";
        public string ReferenceName
        {
            get { return __reference_name; }
            set { __reference_name = value.Trim(); }
        }

        public override string ToString()
        {
            return $"{{ ID = 0x{ID:X04} ; Name = {Name} ; Switch = 0x{Switch:X08} ; Local Address = 0x{LocalAddress:X04} ; Remote IPv4 = {IPv4} : Remote Port = {Port} ; Reference Name = {ReferenceName} }}";
        }

        public bool Equals(DeviceConfigurationModel? other)
        {
            return DeviceModel == other.DeviceModel && Switch == other.Switch && 
                LocalAddress == other.LocalAddress && IPv4 == other.IPv4 && Port == other.Port && 
                ReferenceName == other.ReferenceName;
        }
    }
}
