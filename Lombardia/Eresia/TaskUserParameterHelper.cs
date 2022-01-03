using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class TaskUserParameterHelper
    {
        private ControllerModelCatalogue __controller_model_catalogue;
        private ushort __host_address = 0x3E00;
        public IReadOnlySet<ushort> HostAddresses { get; init; } = new HashSet<ushort>() { 0x3E00, 0x3E10, 0x3E20, 0x3E30 };
        private List<LocalHardwareModule> __local_hardware_collection = new List<LocalHardwareModule>();
        public IReadOnlyList<LocalHardwareModule> LocalHardwareCollection { get; private set; }
        private List<RemoteHardwareModule> __remote_hardware_collection = new List<RemoteHardwareModule>();
        public IReadOnlyList<RemoteHardwareModule> RemoteHardwareCollection { get; private set; }
        public TaskUserParameterHelper(ControllerModelCatalogue models, string path, out byte[] md5code): this(models)
        {
            __load(path, out md5code);
        }
        public TaskUserParameterHelper(ControllerModelCatalogue models, Stream sm) : this(models)
        {
            __load(sm);
        }

        public TaskUserParameterHelper(ControllerModelCatalogue models)
        {
            __controller_model_catalogue = models;
            LocalHardwareCollection = __local_hardware_collection;
            RemoteHardwareCollection = __remote_hardware_collection;
        }

        public LocalHardwareModule Add(ushort id, string name, ushort bit, uint sw, ushort localAddress, List<(string, string)> customs)
        {
            LocalHardwareModule m = new LocalHardwareModule() 
            { 
                DeviceModel = InspectLocalHardwareModel(id, name, bit), 
                Switch = sw, 
                LocalAddress = localAddress,
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1.Trim(), c.Item2.Trim())))};
            __local_hardware_collection.Add(m);
            return m;
        }

        public RemoteHardwareModule Add(ushort id, string name, uint sw, string ip, ushort port, List<(string, string)> customs)
        {
            RemoteHardwareModule m  = new RemoteHardwareModule() 
            { 
                DeviceModel = InspectRemoteHardwareModel(id, name), 
                Switch = sw, 
                IPv4 = ip, 
                Port = port, 
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1.Trim(), c.Item2.Trim()))) };
            __remote_hardware_collection.Add(m);
            return m;
        }

        public LocalHardwareModule InsertAt(int index, ushort id, string name, ushort bit, uint sw, ushort localAddress, List<(string, string)> customs)
        {
            LocalHardwareModule m = new LocalHardwareModule() 
            { 
                DeviceModel = InspectLocalHardwareModel(id, name, bit), 
                Switch = sw, 
                LocalAddress = localAddress, 
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1.Trim(), c.Item2.Trim()))) };
            __local_hardware_collection.Insert(index, m);
            return m;
        }

        public RemoteHardwareModule InsertAt(int index, ushort id, string name, uint sw, string ip, ushort port, List<(string, string)> customs)
        {
            RemoteHardwareModule m = new RemoteHardwareModule()
            {
                DeviceModel = InspectRemoteHardwareModel(id, name),
                Switch = sw,
                IPv4 = ip,
                Port = port,
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1.Trim(), c.Item2.Trim())))
            };
            __remote_hardware_collection.Insert(index, m);
            return m;
        }

        public LocalHardwareModule RemoveLocalAt(int index)
        {
            var m = __local_hardware_collection[index];
            __local_hardware_collection.RemoveAt(index);
            return m;
        }

        public LocalHardwareModule ReplaceAt(int index, ushort id, string name, ushort bit, uint sw, ushort localAddress, List<(string, string)> customs)
        {
            LocalHardwareModule m = new LocalHardwareModule()
            {
                DeviceModel = InspectLocalHardwareModel(id, name, bit),
                Switch = sw,
                LocalAddress = localAddress,
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1, c.Item2)))
            };
            __local_hardware_collection[index] = m;
            return m;
        }

        public RemoteHardwareModule RemoveRemoteAt(int index)
        {
            var m = __remote_hardware_collection[index];
            __remote_hardware_collection.RemoveAt(index);
            return m;
        }

        public RemoteHardwareModule ReplaceAt(int index, ushort id, string name, uint sw, string ip, ushort port, List<(string, string)> customs)
        {
            RemoteHardwareModule m = new RemoteHardwareModule()
            {
                DeviceModel = InspectRemoteHardwareModel(id, name),
                Switch = sw,
                IPv4 = ip,
                Port = port,
                CustomFields = new Dictionary<string, string>(customs.Select(c => new KeyValuePair<string, string>(c.Item1, c.Item2)))
            };
            __remote_hardware_collection[index] = m;
            return m;
        }

        public ushort HostAddress 
        {
            get { return __host_address; }
            set
            {
                if (HostAddresses.Contains(value))
                    __host_address = value;
                else
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_HOST_CPU_ADDRESS);
            }
        }

        public byte[] Save(string path)
        {
            try
            {
                XmlDocument xmlDoc = __save_as();
                xmlDoc.Save(path);
                using (MD5 hash = MD5.Create())
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    return hash.ComputeHash(stream);
                }
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Stream sm)
        {
            try
            {
                XmlDocument xmlDoc = __save_as();
                xmlDoc.Save(sm);
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public XmlDocument __save_as()
        {
            if (__overlap_detector())
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.CONTROLLER_MODULE_LOCAL_ADDRESS_OVERLAPPING);

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(decl);

            XmlElement root = xmlDoc.CreateElement("R2H_Task_UserParameters");
            XmlElement host = xmlDoc.CreateElement("HostCPU");
            host.AppendChild(xmlDoc.CreateElement("Address"));
            host["Address"].AppendChild(xmlDoc.CreateTextNode($"0x{HostAddress:X4}"));
            root.AppendChild(host);

            XmlElement local = xmlDoc.CreateElement("ExtensionModules");
            foreach (var m in LocalHardwareCollection)
                local.AppendChild(m.ToXml(xmlDoc));
            root.AppendChild(local);

            XmlElement remote = xmlDoc.CreateElement("EthernetModules");
            foreach (var m in RemoteHardwareCollection)
                remote.AppendChild(m.ToXml(xmlDoc));
            root.AppendChild(remote);

            xmlDoc.AppendChild(root);
            return xmlDoc;
        }

        private bool __overlap_detector()
        {
            List<ValueTuple<uint, uint>> ranges = new List<(uint, uint)>(LocalHardwareCollection.Count);
            foreach (var device in LocalHardwareCollection)
            {
                ranges.Add(new ValueTuple<uint, uint>(device.LocalAddress, device.DeviceModel.BitSize));
            }
            ranges.TrimExcess();
            return Helper.OVERLAP_DETECTOR(ranges);
        }

        public void __load(string path, out byte[] code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open);
                using (MD5 hash = MD5.Create())
                {
                    code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }
                XmlNode node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/HostCPU/Address");
                HostAddress = Convert.ToUInt16(node.FirstChild.Value, 16);
                node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/ExtensionModules");
                __load_local_hardware_modules(node);
                node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/EthernetModules");
                __load_remote_hardware_modules(node);
            }
            catch(LombardiaException)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new LombardiaException(e);
;           }
        }

        public void __load(Stream sm)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(sm);
                XmlNode node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/HostCPU/Address");
                HostAddress = Convert.ToUInt16(node.FirstChild.Value, 16);
                node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/ExtensionModules");
                __load_local_hardware_modules(node);
                node = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/EthernetModules");
                __load_remote_hardware_modules(node);
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
                ;
            }
        }

        private void __load_local_hardware_modules(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode sub in node.ChildNodes)
                    {
                        if (sub.NodeType != XmlNodeType.Element || sub.Name != "ExtensionModule")
                            continue;

                        ushort id = Convert.ToUInt16(sub.SelectSingleNode("ID").FirstChild.Value, 16);
                        string name = sub.SelectSingleNode("Name").FirstChild.Value.Trim();
                        ushort bitSize = Convert.ToUInt16(sub.SelectSingleNode("BitSize").FirstChild.Value, 16);
                        var model = InspectLocalHardwareModel(id, name, bitSize);
                        uint sw = Convert.ToUInt32(sub.SelectSingleNode("Switch").FirstChild.Value, 16);
                        ushort address = Convert.ToUInt16(sub.SelectSingleNode("Address").FirstChild.Value, 16);
                        LocalHardwareModule module = new LocalHardwareModule() { DeviceModel = model, Switch = sw, LocalAddress = address };

                        foreach (XmlNode custom in sub.ChildNodes)
                        {
                            string c = custom.Name.Trim();
                            if (custom.NodeType != XmlNodeType.Element || LocalHardwareModule.SYSTEM.Contains(c) == true)
                                continue;
                            if (module.CustomFields.ContainsKey(c))
                                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_CUSTOM_FIELD_NAME_IN_LOCAL_MODULE);
                            module.CustomFields.Add(c, custom.FirstChild.Value.Trim());
                        }

                        __local_hardware_collection.Add(module);
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
        }

        private void __load_remote_hardware_modules(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode sub in node.ChildNodes)
                    {
                        if (sub.NodeType != XmlNodeType.Element || sub.Name != "EthernetModule")
                            continue;

                        ushort id = Convert.ToUInt16(sub.SelectSingleNode("ID").FirstChild.Value, 16);
                        string name = sub.SelectSingleNode("Name").FirstChild.Value.Trim();
                        var model = InspectRemoteHardwareModel(id, name);
                        uint sw = Convert.ToUInt32(sub.SelectSingleNode("Switch").FirstChild.Value, 16);
                        string ip = sub.SelectSingleNode("IP").FirstChild.Value.Trim();
                        ushort port = Convert.ToUInt16(sub.SelectSingleNode("Port").FirstChild.Value, 10);
                        RemoteHardwareModule module = new RemoteHardwareModule() { DeviceModel = model, Switch = sw, IPv4 = ip, Port = port };

                        foreach (XmlNode custom in sub.ChildNodes)
                        {
                            string c = custom.Name.Trim();
                            if (custom.NodeType != XmlNodeType.Element || RemoteHardwareModule.SYSTEM.Contains(c) == true)
                                continue;
                            if (module.CustomFields.ContainsKey(c))
                                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_CUSTOM_FIELD_NAME_IN_REMOTE_MODULE);
                            module.CustomFields.Add(c, custom.FirstChild.Value.Trim());
                        }

                        __remote_hardware_collection.Add(module);
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
        }

        public LocalExtensionModel InspectLocalHardwareModel(ushort id, string name, ushort bitSize)
        {
            if (__controller_model_catalogue.LocalExtensionModels.TryGetValue(id, out var localModel) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
            if(localModel.BitSize != bitSize || localModel.Name != name)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
            return localModel;
        }

        public RemoteEthernetModel InspectRemoteHardwareModel(ushort id, string name)
        {
            if (__controller_model_catalogue.RemoteEthernetModels.TryGetValue(id, out var remoteModel) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
            if (remoteModel.Name != name)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_CONTROLLER_EXTENSION_MODEL);
            return remoteModel;
        }
    }

    public class LocalHardwareModule
    {
        public LocalExtensionModel DeviceModel { get; init; } = new LocalExtensionModel();
        public uint Switch { get; init; }
        private ushort __local_address;
        public ushort LocalAddress
        {
            get { return __local_address; }
            init { if (value % 16 != 0) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_MODULE_LOCAL_ADDRESS); else __local_address = value; }
        }

        public Dictionary<string, string> CustomFields { get; init; } = new Dictionary<string, string>();
        public static IReadOnlyList<string> CUSTOMS { get; } = new List<string>() { "ExtraInputDataOffset", "ExtraInputDataSize", "ExtraOutputDataOffset", "ExtraOutputDataSize", "ENI"};
        public static IReadOnlyList<string> SYSTEM { get; } = new List<string>() { "ID", "Name", "Switch", "BitSize", "Address" };

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement moduleNode = doc.CreateElement("ExtensionModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{DeviceModel.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(DeviceModel.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Switch");
            sub.AppendChild(doc.CreateTextNode($"0x{Switch:X8}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Address");
            sub.AppendChild(doc.CreateTextNode($"0x{LocalAddress:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("BitSize");
            sub.AppendChild(doc.CreateTextNode($"0x{DeviceModel.BitSize:X4}"));
            moduleNode.AppendChild(sub);

            foreach (var p in CustomFields)
            {
                sub = doc.CreateElement(p.Key.Trim());
                sub.AppendChild(doc.CreateTextNode(p.Value.Trim()));
                moduleNode.AppendChild(sub);
            }

            return moduleNode;
        }
    }

    public class RemoteHardwareModule
    {
        public RemoteEthernetModel DeviceModel { get; init; } = new RemoteEthernetModel();
        public uint Switch { get; init; }

        private string __ipv4_address = "127.0.0.1";
        public string IPv4
        {
            get { return __ipv4_address; }
            init { if (Helper.VALID_IPV4_ADDRESS.IsMatch(value) == false) throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_MODULE_IPV4_ADDRESS); else __ipv4_address = value; }
        }
        public ushort Port { get; init; } = 8366;
        public Dictionary<string, string> CustomFields { get; init; } = new Dictionary<string, string>();
        public static IReadOnlyList<string> CUSTOMS { get; } = new List<string>() { "InputDataOffset", "InputDataSize", "OutputDataOffset", "OutputDataSize", "Timeout", "Timer", "Interval", "Priority" };
        public static IReadOnlyList<string> SYSTEM { get; } = new List<string>() { "ID", "Name", "Switch", "IP", "Port" };

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement moduleNode = doc.CreateElement("EthernetModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{DeviceModel.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(DeviceModel.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Switch");
            sub.AppendChild(doc.CreateTextNode($"0x{Switch:X8}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("IP");
            sub.AppendChild(doc.CreateTextNode(IPv4));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Port");
            sub.AppendChild(doc.CreateTextNode(Port.ToString()));
            moduleNode.AppendChild(sub);

            foreach (var p in CustomFields)
            {
                sub = doc.CreateElement(p.Key.Trim());
                sub.AppendChild(doc.CreateTextNode(p.Value.Trim()));
                moduleNode.AppendChild(sub);
            }

            return moduleNode;
        }
    }
}
