using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class ControllerConfiguration : Publisher<DeviceConfiguration>, IComparable<ControllerConfiguration>
    {
        public IReadOnlyDictionary<string, DeviceConfiguration> Configurations { get; private set; }
        private Dictionary<string, DeviceConfiguration> __configurations = new Dictionary<string, DeviceConfiguration>();
        private ControllerModelCatalogue __controller_model_catalogue;
        public ControllerConfiguration(ControllerModelCatalogue controllerModelCatalogue)
        {
            Configurations = __configurations;
            __controller_model_catalogue = controllerModelCatalogue;
        }

        public ControllerConfiguration(ControllerModelCatalogue controllerModelCatalogue, XmlNode controllerNode)
        {
            __controller_model_catalogue = controllerModelCatalogue;
            Configurations = __load_configuration(controllerNode);
        }

        public DeviceConfiguration InspectDeviceConfiguration(ushort device, uint sw, ushort localAddress, string ipv4, ushort port, string reference)
        {
            if(__configurations.ContainsKey(reference))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_CONTROLLER_MODULE_REFERENCE);
            if (__controller_model_catalogue.LocalExtensionModels.TryGetValue(device, out var localModel) == true)
                return new DeviceConfiguration() { DeviceModel = localModel, Switch = sw, LocalAddress = localAddress, ReferenceName = reference };
            else if (__controller_model_catalogue.RemoteEthernetModels.TryGetValue(device, out var remoteModel) == true)
                return new DeviceConfiguration() { DeviceModel = remoteModel, Switch = sw, IPv4 = ipv4, Port = port, ReferenceName = reference };
            else
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
        }

        public void Save(XmlDocument doc, IEnumerable<string> names, XmlElement controllerInfo, uint version = 1)
        {
            if (__overlap_detector())
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_LOCAL_ADDRESS_OVERLAPPING);
            controllerInfo.AppendChild(doc.CreateElement("ExtensionModules"));
            controllerInfo.AppendChild(doc.CreateElement("EthernetModules"));
            var extensions = controllerInfo["ExtensionModules"];
            var ethernets = controllerInfo["EthernetModules"];
            try
            {
                foreach (var n in names)
                {
                    var configuration = __configurations[n];
                    if(configuration.DeviceModel is LocalExtensionModel)
                    {
                        XmlElement extension = doc.CreateElement("ExtensionModule");

                        XmlElement sub = doc.CreateElement("ID");
                        sub.AppendChild(doc.CreateTextNode($"0x{configuration.DeviceModel.ID:X4}"));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Switch");
                        sub.AppendChild(doc.CreateTextNode($"0x{configuration.Switch:X8}"));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Name");
                        sub.AppendChild(doc.CreateTextNode(configuration.ReferenceName));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Address");
                        sub.AppendChild(doc.CreateTextNode($"0x{configuration.LocalAddress:X4}"));
                        extension.AppendChild(sub);

                        extensions.AppendChild(extension);
                    }
                    else if (configuration.DeviceModel is RemoteEthernetModel)
                    {
                        XmlElement ethernet = doc.CreateElement("EthernetModule");

                        XmlElement sub = doc.CreateElement("ID");
                        sub.AppendChild(doc.CreateTextNode($"0x{configuration.DeviceModel.ID:X4}"));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Switch");
                        sub.AppendChild(doc.CreateTextNode($"0x{configuration.Switch:X8}"));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Name");
                        sub.AppendChild(doc.CreateTextNode(configuration.ReferenceName));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("IP");
                        sub.AppendChild(doc.CreateTextNode(configuration.IPv4));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Port");
                        sub.AppendChild(doc.CreateTextNode(configuration.Port.ToString()));
                        ethernet.AppendChild(sub);

                        ethernets.AppendChild(ethernet);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, IEnumerable<string> names, uint version = 1)
        {
            if (__overlap_detector())
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_LOCAL_ADDRESS_OVERLAPPING);

            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("ID");
                dt.Columns.Add("Name");
                dt.Columns.Add("Switch");
                dt.Columns.Add("Local Address");
                dt.Columns.Add("Remote IPv4");
                dt.Columns.Add("Remote Port");
                dt.Columns.Add("Reference Name");

                foreach (var n in names)
                {
                    var device = __configurations[n];
                    dt.Rows.Add("0x" + device.DeviceModel.ID.ToString("X04"),
                        device.DeviceModel.Name,
                        "0x" + device.Switch.ToString("X08"),
                        "0x" + device.LocalAddress.ToString("X04"),
                        device.IPv4, device.Port.ToString(), device.ReferenceName);
                }
                int rows = sheet.InsertDataTable(dt, true, 1, 1, false);
                sheet.Range[1, 1, 1, dt.Columns.Count].Style = title;
                if (rows > 0) sheet.Range[2, 1, 1 + rows, dt.Columns.Count].Style = content;
                sheet.AllocatedRange.AutoFitColumns();
                sheet.Range[2, 1].FreezePanes();
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        protected void Add(DeviceConfiguration c)
        {
            if (__configurations.TryAdd(c.ReferenceName, c) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_CONTROLLER_MODULE_REFERENCE);
        }

        public DeviceConfiguration Add(ushort device, uint sw, ushort localAddress, string ipv4, ushort port, string reference)
        {
            var c = InspectDeviceConfiguration(device, sw, localAddress, ipv4, port, reference);
            __configurations[c.ReferenceName] = c;
            return c;
        }

        protected void Remove(DeviceConfiguration configuration)
        {
            if (_subscribers.ContainsKey(configuration))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_BE_SUBSCRIBED);

            if (__configurations.Remove(configuration.ReferenceName) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND);
        }

        public DeviceConfiguration Remove(string name)
        {
            if (__configurations.TryGetValue(name, out var configuration) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND);
            Remove(configuration);
            return configuration;
        }

        public LOMBARDIA_ERROR_CODE_T CanRemove(string name)
        {
            var res = LOMBARDIA_ERROR_CODE_T.NO_ERROR;
            if (__configurations.TryGetValue(name, out var configuration) == false)
                res = LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND;
            else if (_subscribers.ContainsKey(configuration))
                res = LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_BE_SUBSCRIBED;
            return res;
        }

        public bool IsUnused(string name)
        {
            return __configurations.TryGetValue(name, out var variable) == false ? true : _subscribers.ContainsKey(variable) == false;
        }

        protected void Replace(DeviceConfiguration origin, DeviceConfiguration c)
        {
            if (_subscribers.ContainsKey(origin) && origin.DeviceModel != c.DeviceModel)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_BE_SUBSCRIBED);
            if (origin.ReferenceName != c.ReferenceName && __configurations.ContainsKey(c.ReferenceName))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_CONTROLLER_MODULE_REFERENCE);
            if (__configurations.Remove(origin.ReferenceName) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND);
            __configurations.Add(c.ReferenceName, c);
            if (_subscribers.TryGetValue(origin, out var res) == true)
            {
                List<ISubscriber<DeviceConfiguration>> subs = new List<ISubscriber<DeviceConfiguration>>(res);
                try
                {
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        subs[i] = subs[i].DependencyChanged(origin, c);
                    }
                }
                catch (LombardiaException)
                {
                    __configurations.Remove(c.ReferenceName);
                    __configurations.Add(origin.ReferenceName, origin);
                    throw;
                }
                _subscribers.Remove(origin);
                _subscribers[c] = subs;
            }
        }

        protected void Replace(string name, DeviceConfiguration c)
        {
            if (__configurations.TryGetValue(name, out var origin) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND);
            Replace(origin, c);
        }

        public DeviceConfiguration Replace(string origin, ushort device, uint sw, ushort localAddress, string ipv4, ushort port, string reference)
        {
            DeviceConfiguration c = null;
            if (__controller_model_catalogue.LocalExtensionModels.TryGetValue(device, out var localModel) == true)
                c = new DeviceConfiguration() { DeviceModel = localModel, Switch = sw, LocalAddress = localAddress, ReferenceName = reference };
            else if (__controller_model_catalogue.RemoteEthernetModels.TryGetValue(device, out var remoteModel) == true)
                c = new DeviceConfiguration() { DeviceModel = remoteModel, Switch = sw, IPv4 = ipv4, Port = port, ReferenceName = reference };
            else
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
            Replace(origin, c);
            return c;
        }

        public LOMBARDIA_ERROR_CODE_T CanReplace(string name, DeviceConfiguration c)
        {
            var res = LOMBARDIA_ERROR_CODE_T.NO_ERROR;
            if (__configurations.TryGetValue(name, out var origin) == false)
                res = LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_UNFOUND;
            else if (_subscribers.ContainsKey(origin) && origin.DeviceModel != c.DeviceModel)
                res = LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_BE_SUBSCRIBED;
            else if (origin.ReferenceName != c.ReferenceName && __configurations.ContainsKey(c.ReferenceName))
                res = LOMBARDIA_ERROR_CODE_T.DUPLICATED_CONTROLLER_MODULE_REFERENCE;
            return res;
        }

        private Dictionary<string, DeviceConfiguration> __load_configuration(XmlNode controllerNode)
        {
            __configurations.Clear();
            uint sw = 0;
            string reference = null;
            ushort address = 0;
            string ip = null;
            ushort port = 0;
            DeviceConfiguration configuration;
            try
            {
                var deviceNodes = controllerNode.SelectSingleNode("ExtensionModules");
                if (deviceNodes?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode deviceNode in deviceNodes.ChildNodes)
                    {
                        if (deviceNode.NodeType != XmlNodeType.Element || deviceNode.Name != "ExtensionModule")
                            continue;
                        ushort id = Convert.ToUInt16(deviceNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        if (__controller_model_catalogue.LocalExtensionModels.TryGetValue(id, out var localModel) == false)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);

                        sw = Convert.ToUInt32(deviceNode.SelectSingleNode("Switch").FirstChild.Value, 16);
                        reference = deviceNode.SelectSingleNode("Name").FirstChild.Value;
                        address = Convert.ToUInt16(deviceNode.SelectSingleNode("Address").FirstChild.Value, 16);

                        configuration = new DeviceConfiguration() { DeviceModel = localModel, ReferenceName = reference, LocalAddress = address, Switch = sw };
                        Add(configuration);
                    }
                }

                deviceNodes = controllerNode.SelectSingleNode("EthernetModules");
                if (deviceNodes?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode deviceNode in deviceNodes.ChildNodes)
                    {
                        if (deviceNode.NodeType != XmlNodeType.Element || deviceNode.Name != "EthernetModule")
                            continue;

                        ushort id = Convert.ToUInt16(deviceNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        if (__controller_model_catalogue.RemoteEthernetModels.TryGetValue(id, out var remoteModel) == false)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);

                        sw = Convert.ToUInt32(deviceNode.SelectSingleNode("Switch").FirstChild.Value, 16);
                        reference = deviceNode.SelectSingleNode("Name").FirstChild.Value;
                        ip = deviceNode.SelectSingleNode("IP").FirstChild.Value;
                        port = Convert.ToUInt16(deviceNode.SelectSingleNode("Port").FirstChild.Value, 10);

                        configuration = new DeviceConfiguration() { DeviceModel = remoteModel, ReferenceName = reference, IPv4 = ip, Port = port, Switch = sw };
                        Add(configuration);
                    }
                }
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }

            return __configurations;
        }

        private bool __overlap_detector()
        {
            List<ValueTuple<uint, uint>> ranges = new List<(uint, uint)>(__configurations.Count);
            foreach (var device in __configurations.Values)
            {
                if (device.DeviceModel is LocalExtensionModel)
                    ranges.Add(new ValueTuple<uint, uint>(device.LocalAddress, (device.DeviceModel as LocalExtensionModel).BitSize));
            }
            ranges.TrimExcess();
            return Helper.OVERLAP_DETECTOR(ranges);
        }

        public bool IsEquivalent(ControllerConfiguration? other)
        {
            bool res = false;
            if (other != null && other.Configurations.Count == this.Configurations.Count)
                res = this.Configurations.All(p => other.Configurations.ContainsKey(p.Key) && other.Configurations[p.Key].IsEquivalent(this.Configurations[p.Key]));
            return res;
        }
    }

    public class DeviceConfiguration: IComparable<DeviceConfiguration>
    {
        public DeviceModel DeviceModel { get; init; } = new LocalExtensionModel();
        public uint Switch { get; init; }
        private string __reference_name = "unnamed_device_reference";
        public string ReferenceName
        {
            get { return __reference_name; }
            init
            {
                var name = value.Trim();
                if (name.Length == 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_MODULE_REFERENCE);
                __reference_name = name;
            }
        }
        private ushort __local_address;
        public ushort LocalAddress
        {
            get { return __local_address; }
            init { if (value % 16 != 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_MODULE_LOCAL_ADDRESS); else __local_address = value; }
        }
        private string __ipv4_address = "127.0.0.1";
        public string IPv4
        {
            get { return __ipv4_address; }
            init { if (Helper.VALID_IPV4_ADDRESS.IsMatch(value) == false) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_MODULE_IPV4_ADDRESS); else __ipv4_address = value; }
        }
        public ushort Port { get; init; }

        public bool IsEquivalent(DeviceConfiguration? other)
        {
            return other != null && DeviceModel == other.DeviceModel && Switch == other.Switch &&
                ((DeviceModel is LocalExtensionModel && LocalAddress == other.LocalAddress) || ((DeviceModel is RemoteEthernetModel && IPv4 == other.IPv4 && Port == other.Port)) &&
                ReferenceName == other.ReferenceName);
        }
    }
}
