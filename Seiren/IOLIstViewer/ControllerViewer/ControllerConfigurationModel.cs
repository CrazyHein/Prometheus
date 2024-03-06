using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class ControllerConfigurationModel : RecordContainerModel, IDeSerializableRecordModel<DeviceConfigurationModel>
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
                    ReferenceName = c.ReferenceName,
                    Unused = cc.IsUnused(c.ReferenceName)
                }));
            DeviceConfigurations = __device_configurations;
            Modified = false;
            OperatingHistory = history;
            Name = "Controller Configuration";
        }

        public void Add(DeviceConfigurationModel model, bool log = true)
        {
            var op = new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count, OriginalValue = null, NewValue = model };
            try
            {
                __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations.Add(model);
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);

            DebugConsole.WriteOperatingRecord(op);
        }

        public DeviceConfigurationModel RemoveAt(int index, bool log = true)
        {
            DeviceConfigurationModel model = __device_configurations[index];
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __controller_configuration_collection.Remove(model.ReferenceName);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations.RemoveAt(index);
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);

            DebugConsole.WriteOperatingRecord(op);
            return model;
        }

        public void Remove(DeviceConfigurationModel model, bool log = true)
        {
            int index = __device_configurations.IndexOf(model);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __controller_configuration_collection.Remove(model.ReferenceName);
            }
            catch(Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations.Remove(model);
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);

            DebugConsole.WriteOperatingRecord(op);
        }

        public bool IsUnused(string name)
        {
            return __controller_configuration_collection.IsUnused(name);
        }

        public void ReEvaluate(string name)
        {
            __device_configurations.First(o => o.ReferenceName == name).Unused = __controller_configuration_collection.IsUnused(name);
        }

        public void Insert(int index, DeviceConfigurationModel model, bool log = true)
        {
            if (index > __device_configurations.Count)
                throw new ArgumentOutOfRangeException();

            var op = new OperatingRecord() { Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, NewValue = model };
            try
            {
                __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations.Insert(index, model);
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);

            DebugConsole.WriteOperatingRecord(op);
        }

        public void Move(int srcIndex, int dstIndex, bool log = true)
        {
            if (srcIndex >= __device_configurations.Count || dstIndex >= __device_configurations.Count || srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();
            var temp = __device_configurations[srcIndex];
            __device_configurations.RemoveAt(srcIndex);
            __device_configurations.Insert(dstIndex, temp);
            Modified = true;
            var op = new OperatingRecord() { Host = this, Operation = Operation.Move, OriginaPos = srcIndex, NewPos = dstIndex, OriginalValue = temp, NewValue = temp };
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);

            DebugConsole.WriteOperatingRecord(op);
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
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = original, NewValue = newModel };
            try
            {
                __controller_configuration_collection.Replace(original.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations[index] = newModel;
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            //Notify others here
            SubscriberObjects?.UpdateBinding(original.ReferenceName);
        }

        public void Replace(DeviceConfigurationModel originalModel, DeviceConfigurationModel newModel, bool log = true)
        {
            int index = __device_configurations.IndexOf(originalModel);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = originalModel, NewValue = newModel };

            try
            {
                __controller_configuration_collection.Replace(originalModel.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __device_configurations[index] = newModel;
            Modified = true;

            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
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
                var op = new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count - 1, OriginalValue = null, NewValue = model };
                if (log && OperatingHistory != null)
                    OperatingHistory.PushOperatingRecord(op);
                DebugConsole.WriteOperatingRecord(op);
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
                var op = new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __device_configurations.Count - 1, OriginalValue = null, NewValue = model };
                if (log && OperatingHistory != null)
                    OperatingHistory.PushOperatingRecord(op);
                DebugConsole.WriteOperatingRecord(op);
            }
            if (helper.LocalHardwareCollection.Count != 0 || helper.RemoteHardwareCollection.Count != 0)
                Modified = true;
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

        public DeviceConfigurationModel? FromXml(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Element && node.Name == "DeviceConfigurationModel")
                {
                    DeviceConfigurationModel d = new DeviceConfigurationModel();
                    d.Unused = true;

                    d.Switch = Convert.ToUInt32(node.SelectSingleNode("Switch").FirstChild.Value, 16);
                    d.LocalAddress = Convert.ToUInt16(node.SelectSingleNode("Address").FirstChild.Value, 16);
                    d.IPv4 = node.SelectSingleNode("IP").FirstChild.Value;
                    d.Port = Convert.ToUInt16(node.SelectSingleNode("Port").FirstChild.Value, 10);
                    d.ReferenceName = node.SelectSingleNode("ReferenceName").FirstChild.Value;

                    ushort id = Convert.ToUInt16(node.SelectSingleNode("ID").FirstChild.Value, 16);
                    string name = node.SelectSingleNode("Name").FirstChild.Value;
                    if (ControllerModelCatalogue.LocalExtensionModels.ContainsKey(id) && ControllerModelCatalogue.LocalExtensionModels[id].Name == name)
                    {
                        d.DeviceModel = ControllerModelCatalogue.LocalExtensionModels[id];
                        return d;
                    }
                    else if (ControllerModelCatalogue.RemoteEthernetModels.ContainsKey(id) && ControllerModelCatalogue.RemoteEthernetModels[id].Name == name)
                    {
                        d.DeviceModel = ControllerModelCatalogue.RemoteEthernetModels[id];
                        return d;
                    }
                    return null;
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class DeviceConfigurationModel : IEquatable<DeviceConfigurationModel>, INotifyPropertyChanged, ISerializableRecordModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool __unused = true;
        public bool Unused
        {
            get { return __unused; }
            set { __unused = value; _notify_property_changed(); }
        }

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

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement deviceModel = doc.CreateElement("DeviceConfigurationModel");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{ID:X4}"));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("Switch");
            sub.AppendChild(doc.CreateTextNode($"0x{Switch:X8}"));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(Name));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("Address");
            sub.AppendChild(doc.CreateTextNode($"0x{LocalAddress:X4}"));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("IP");
            sub.AppendChild(doc.CreateTextNode(IPv4));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("Port");
            sub.AppendChild(doc.CreateTextNode(Port.ToString()));
            deviceModel.AppendChild(sub);

            sub = doc.CreateElement("ReferenceName");
            sub.AppendChild(doc.CreateTextNode(ReferenceName));
            deviceModel.AppendChild(sub);

            return deviceModel;
        }
    }
}
