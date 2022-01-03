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
        public ControllerConfigurationModel(ControllerConfiguration cc, ControllerModelCatalogue cmc)
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
        }

        public void Add(DeviceConfigurationModel model)
        {
            __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            __device_configurations.Add(model);
            Modified = true;
        }

        public DeviceConfigurationModel RemoveAt(int index, bool force = false)
        {
            DeviceConfigurationModel model = __device_configurations[index];
            __controller_configuration_collection.Remove(model.ReferenceName, force);
            __device_configurations.RemoveAt(index);
            Modified = true;
            return model;
        }

        public void Remove(DeviceConfigurationModel model, bool force = false)
        {
            __controller_configuration_collection.Remove(model.ReferenceName, force);
            __device_configurations.Remove(model);
            Modified = true;
        }

        public void Insert(int index, DeviceConfigurationModel model)
        {
            if (index > __device_configurations.Count)
                throw new ArgumentOutOfRangeException();
            __controller_configuration_collection.Add(model.DeviceModel.ID, model.Switch, model.LocalAddress, model.IPv4, model.Port, model.ReferenceName);
            __device_configurations.Insert(index, model);
            Modified = true;
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

        public void Replace(int index, DeviceConfigurationModel newModel)
        {
            DeviceConfigurationModel original = __device_configurations[index];
            __controller_configuration_collection.Replace(original.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            __device_configurations[index] = newModel;
            Modified = true;
            //Notify others here
            SubscriberObjects?.UpdateBinding(original.ReferenceName);
        }

        public void Replace(DeviceConfigurationModel originalModel, DeviceConfigurationModel newModel)
        {
            __controller_configuration_collection.Replace(originalModel.ReferenceName, newModel.DeviceModel.ID, newModel.Switch, newModel.LocalAddress, newModel.IPv4, newModel.Port, newModel.ReferenceName);
            __device_configurations[__device_configurations.IndexOf(originalModel)] = newModel;
            Modified = true;
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

        public void ImportDevices(string path)
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
            }
            if (helper.LocalHardwareCollection.Count != 0 || helper.RemoteHardwareCollection.Count != 0)
                Modified = false;
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
