using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Eresia
{
    public enum TASK_USER_PARAMETERS_ERROR_T : int
    {
        FILE_DATA_EXCEPTION                                 = 0x10000000,
        ELEMENT_MISSING                                     = 0x10000001,

        INVALID_EXTENSION_MODULE_MODEL                      = 0x00000001,
        INVALID_EXTENSION_MODULE_MODEL_ID                   = 0x00000002,
        INVALID_EXTENSION_MODULE_USERCONFIGURATION          = 0x00000003,
        INVALID_EXTENSION_MODULE_ADDRESS                    = 0x00000004,     
        DUPLICATED_EXTENSION_MODULE_USERCONFIGURATION       = 0x00000005,
        EXTENSION_MODULE_ADDRESS_OVERLAPPED                 = 0x00000006,
        

        INVALID_ETHERNET_MODULE_IPV4_ADDRESS                = 0x00000011,
        INVALID_ETHERNET_MODULE_USERCONFIGURATION           = 0x00000012,
        INVALID_ETHERNET_MODULE_MODEL                       = 0x00000013,
        INVALID_ETHERNET_MODULE_MODEL_ID                    = 0x00000014,
        DUPLICATED_ETHERNET_MODULE_USERCONFIGURATION        = 0x00000015,
        DUPLICATED_ETHERNET_MODULE_IP_PORT                  = 0x00000016,

        INVALID_HOST_CPU_ADDRESS                            = 0x00000020,             
    }


    public class TaskUserParametersExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public TASK_USER_PARAMETERS_ERROR_T ErrorCode { get; private set; }

        public TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class TaskUserParametersHelper
    {
        private ControllerModelCatalogue __model_catalogue;
        private ushort __host_cpu_address;
        private List<CONTROLLER_EXTENSION_MODULE_T> __controller_extension_modules;
        private List<CONTROLLER_ETHERNET_MODULE_T> __controller_ethernet_modules;
        private HashSet<ushort> __controller_host_cpu_addresses;

        public static Regex VALID_IPV4_ADDRESS { get; private set; }
        public static Regex VALID_EXTENSION_MODULE_ADDRESS_FORMAT { get; private set; }
        static TaskUserParametersHelper()
        {
            VALID_IPV4_ADDRESS = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$", RegexOptions.Compiled);
            VALID_EXTENSION_MODULE_ADDRESS_FORMAT = new Regex(@"^0x[0-9A-F]{3}0$", RegexOptions.Compiled);
        }

        public TaskUserParametersHelper(ControllerModelCatalogue catalogue)
        {
            __model_catalogue = catalogue;
            __controller_host_cpu_addresses = new HashSet<ushort>() { 0x3E00, 0x3E10, 0x3E20, 0x3E30 };

            HostCPUAddress = 0x3E00;
            __controller_extension_modules = new List<CONTROLLER_EXTENSION_MODULE_T>();
            __controller_ethernet_modules = new List<CONTROLLER_ETHERNET_MODULE_T>();
        }


        public void Load(string taskUserParameters)
        {
            XmlDocument xmlDoc = new XmlDocument();
            SetDefault();

            try
            {
                xmlDoc.Load(taskUserParameters);

                XmlNode hostCPUNode = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/HostCPU");
                __load_host_cpu_info(hostCPUNode);

                XmlNode extensionsNode = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/ExtensionModules");
                __load_extension_modules(extensionsNode);

                XmlNode ethernetsNode = xmlDoc.SelectSingleNode("/R2H_Task_UserParameters/EthernetModules");
                __load_ethernet_modules(ethernetsNode);

                if(__extension_modules_address_overlap_detector(__controller_extension_modules) == true)
                {
                    __controller_extension_modules.Clear();
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.EXTENSION_MODULE_ADDRESS_OVERLAPPED, null);
                }
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void ImportModules(IReadOnlyList<CONTROLLER_EXTENSION_MODULE_T> modules, bool clear)
        {
            foreach (var m in modules)
                ModuleDataVerification(m);
            if (__extension_modules_address_overlap_detector(modules) == true)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.EXTENSION_MODULE_ADDRESS_OVERLAPPED, null);

            if (clear)
                __controller_extension_modules.Clear();
            foreach (var m in modules)
                __controller_extension_modules.Add(m);
        }

        public void ImportModules(IReadOnlyList<CONTROLLER_ETHERNET_MODULE_T> modules, bool clear)
        {
            foreach (var m in modules)
                ModuleDataVerification(m);

            if (clear)
                __controller_ethernet_modules.Clear();
            foreach (var m in modules)
                __controller_ethernet_modules.Add(m);
        }


        public void Save(string taskUserParameters)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("R2H_Task_UserParameters");
                root.AppendChild(__create_host_cpu_information_node(xmlDoc));
                root.AppendChild(__create_modules_node(xmlDoc, __controller_extension_modules));
                root.AppendChild(__create_modules_node(xmlDoc, __controller_ethernet_modules));

                xmlDoc.AppendChild(root);

                xmlDoc.Save(taskUserParameters);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void SetDefault()
        {
            HostCPUAddress = 0x3E00;
            __controller_extension_modules.Clear();
            __controller_ethernet_modules.Clear();
        }

        public void ModuleDataVerification(CONTROLLER_EXTENSION_MODULE_T module)
        {
            if (module.MODEL == null || 
                __model_catalogue.ExtensionModels.Contains(new KeyValuePair<ushort, ControllerExtensionModel>(module.MODEL.ID, module.MODEL)) == false)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_MODEL, null);
            if(module.ADDRESS % 16 != 0)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_ADDRESS, null);

            if (module.USER_CONFIGURATIONS != null)
            {
                foreach (var k in module.USER_CONFIGURATIONS.Keys)
                {
                    if (__model_catalogue.ExtensionModelConfigruationFields.Contains(k) == false)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_USERCONFIGURATION, null);
                }
            }
        }

        public void ModuleDataVerification(CONTROLLER_ETHERNET_MODULE_T module)
        {
            if (module.MODEL == null || 
                __model_catalogue.EthernetModels.Contains(new KeyValuePair<ushort, ControllerEthernetModel>(module.MODEL.ID, module.MODEL)) == false)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_MODEL, null);

            if (module.USER_CONFIGURATIONS != null)
            {
                List<string> fields = new List<string>();
                foreach (var k in module.USER_CONFIGURATIONS.Keys)
                {
                    if (__model_catalogue.EthernetModelConfigruationFields.Contains(k) == false)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_USERCONFIGURATION, null);
                }
            }

            if(VALID_IPV4_ADDRESS.IsMatch(module.IP_ADDRESS) != true)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_IPV4_ADDRESS, null);
        }

        public ushort HostCPUAddress
        {
            get { return __host_cpu_address; }
            set
            { 
                if( __controller_host_cpu_addresses.Contains(value) == false)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_HOST_CPU_ADDRESS, null);
                __host_cpu_address = value;
            }
        }

        public IReadOnlyList<CONTROLLER_EXTENSION_MODULE_T> ControllerExtensionModules { get { return __controller_extension_modules; } }

        public IReadOnlyList<CONTROLLER_ETHERNET_MODULE_T> ControllerEthernetModules { get { return __controller_ethernet_modules; } }

        public IEnumerable<ControllerExtensionModel> AvailableExtensionModels { get { return __model_catalogue.ExtensionModels.Values; } }

        public IEnumerable<ControllerEthernetModel> AvailableEthernetModels { get { return __model_catalogue.EthernetModels.Values; } }

        public IReadOnlyCollection<string> AvailableExtensionUserConfigurationFields { get { return __model_catalogue.ExtensionModelConfigruationFields; } }

        public IReadOnlyCollection<string> AvailableEthernetUserConfigurationFields { get { return __model_catalogue.EthernetModelConfigruationFields; } }


        private void __load_host_cpu_info(XmlNode cpuNode)
        {
            try
            {
                if(cpuNode.NodeType == XmlNodeType.Element)
                    HostCPUAddress = Convert.ToUInt16(cpuNode.SelectSingleNode("Address").FirstChild.Value, 16);
                else
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_extension_modules(XmlNode extensionsNode)
        {
            try
            {
                if (extensionsNode.NodeType == XmlNodeType.Element)
                {
                    foreach(XmlNode extensionModule in extensionsNode.ChildNodes)
                    {
                        if (extensionModule.NodeType != XmlNodeType.Element || extensionModule.Name != "ExtensionModule")
                            continue;

                        __load_extension_module(extensionModule);
                    }
                }
                else
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_ethernet_modules(XmlNode ethernetsNode)
        {
            try
            {
                if (ethernetsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode ethernentModule in ethernetsNode.ChildNodes)
                    {
                        if (ethernentModule.NodeType != XmlNodeType.Element || ethernentModule.Name != "EthernetModule")
                            continue;

                        __load_ethernet_module(ethernentModule);
                    }
                }
                else
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_extension_module(XmlNode extensionNode)
        {
            ushort id = 0;
            string name = "";
            ushort bitSize = 0;
            ushort address = 0;
            ControllerExtensionModel model = null;
            CONTROLLER_EXTENSION_MODULE_T module = null;
            uint mask = 0;
            Dictionary<string, string> userConfigurations = new Dictionary<string, string>();
            try
            {
                if (extensionNode.NodeType != XmlNodeType.Element)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);

                foreach (XmlNode filed in extensionNode.ChildNodes)
                {
                    if (filed.NodeType != XmlNodeType.Element)
                        continue;

                    switch (filed.Name)
                    {
                        case "ID":
                            id = Convert.ToUInt16(filed.FirstChild.Value, 16);
                            mask |= 0x00000001;
                            break;
                        case "Name":
                            name = filed.FirstChild.Value;
                            mask |= 0x00000002;
                            break;
                        case "BitSize":
                            bitSize = Convert.ToUInt16(filed.FirstChild.Value, 16);
                            mask |= 0x00000004;
                            break;
                        case "Address":
                            address = Convert.ToUInt16(filed.FirstChild.Value, 16);
                            mask |= 0x00000008;
                            break;
                        default:
                            if (userConfigurations.Keys.Contains(filed.Name) == true)
                                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_EXTENSION_MODULE_USERCONFIGURATION, null);
                            userConfigurations[filed.Name] = filed.FirstChild.Value;
                            break;
                    }
                }

                if((mask & 0x0000000F) == 0x0000000F)
                {
                    model = __inquire_extension_model(id, name, bitSize);
                    if(userConfigurations.Count != 0)
                        module = new CONTROLLER_EXTENSION_MODULE_T(model, address, userConfigurations);
                    else
                        module = new CONTROLLER_EXTENSION_MODULE_T(model, address, null);
                    ModuleDataVerification(module);
                    __controller_extension_modules.Add(module);
                }
                else
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_ethernet_module(XmlNode extensionNode)
        {
            ushort id = 0;
            string name = "";
            string ip = "";
            ushort port = 0;
            ControllerEthernetModel model = null;
            CONTROLLER_ETHERNET_MODULE_T module = null;
            uint mask = 0;
            Dictionary<string, string> userConfigurations = new Dictionary<string, string>();
            try
            {
                if (extensionNode.NodeType != XmlNodeType.Element)
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);

                foreach (XmlNode filed in extensionNode.ChildNodes)
                {
                    if (filed.NodeType != XmlNodeType.Element)
                        continue;

                    switch (filed.Name)
                    {
                        case "ID":
                            id = Convert.ToUInt16(filed.FirstChild.Value, 16);
                            mask |= 0x00000001;
                            break;
                        case "Name":
                            name = filed.FirstChild.Value;
                            mask |= 0x00000002;
                            break;
                        case "IP":
                            ip = filed.FirstChild.Value;
                            mask |= 0x00000004;
                            break;
                        case "Port":
                            port = Convert.ToUInt16(filed.FirstChild.Value);
                            mask |= 0x00000008;
                            break;
                        default:
                            if (userConfigurations.Keys.Contains(filed.Name) == true)
                                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_ETHERNET_MODULE_USERCONFIGURATION, null);
                            userConfigurations[filed.Name] = filed.FirstChild.Value;
                            break;
                    }
                }

                if ((mask & 0x0000000F) == 0x0000000F)
                {
                    model = __inquire_ethernet_model(id, name);
                    if (userConfigurations.Count != 0)
                        module = new CONTROLLER_ETHERNET_MODULE_T(model, ip, port, userConfigurations);
                    else
                        module = new CONTROLLER_ETHERNET_MODULE_T(model, ip, port, null);
                    ModuleDataVerification(module);
                    __controller_ethernet_modules.Add(module);
                }
                else
                    throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (TaskUserParametersExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private ControllerExtensionModel __inquire_extension_model(ushort id, string name, ushort bitSize)
        {
            try
            {
                ControllerExtensionModel model = null;
                model = __model_catalogue.ExtensionModels[id];
                if (model.Name == name && model.BitSize == bitSize)
                    return model;
                else
                    return null;

            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private ControllerEthernetModel __inquire_ethernet_model(ushort id, string name)
        {
            try
            {
                ControllerEthernetModel model = null;
                model = __model_catalogue.EthernetModels[id];
                if (model.Name == name)
                    return model;
                else
                    return null;

            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private bool __extension_modules_address_overlap_detector(IReadOnlyList<CONTROLLER_EXTENSION_MODULE_T> modules)
        {
            int counter = modules.Count;

            Tuple<int, int>[] ranges = new Tuple<int, int>[counter];
            for (int i = 0; i < counter; ++i)
                ranges[i] = new Tuple<int, int>(modules[i].ADDRESS, modules[i].MODEL.BitSize);

            for (int i = 0; i < counter - 1; ++i)
            {
                for (int j = 0; j < counter - 1 - i; ++j)
                {
                    if (ranges[j].Item1 > ranges[j + 1].Item1)
                    {
                        Tuple<int, int> temp = ranges[j];
                        ranges[j] = ranges[j + 1];
                        ranges[j + 1] = temp;
                    }
                }
            }
            for (int i = 0; i < counter - 1; ++i)
            {
                if (ranges[i].Item1 + ranges[i].Item2 > ranges[i + 1].Item1)
                    return true;
            }

            return false;
        }

        private XmlElement __create_host_cpu_information_node(XmlDocument doc)
        {
            XmlElement hostInfo = doc.CreateElement("HostCPU");

            XmlElement sub = doc.CreateElement("Address");
            sub.AppendChild(doc.CreateTextNode($"0x{HostCPUAddress:X4}"));
            hostInfo.AppendChild(sub);

            return hostInfo;
        }

        private XmlElement __create_module_node(XmlDocument doc, CONTROLLER_EXTENSION_MODULE_T module)
        {
            XmlElement moduleNode = doc.CreateElement("ExtensionModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{module.MODEL.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(module.MODEL.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Address");
            sub.AppendChild(doc.CreateTextNode($"0x{module.ADDRESS:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("BitSize");
            sub.AppendChild(doc.CreateTextNode($"0x{module.MODEL.BitSize:X4}"));
            moduleNode.AppendChild(sub);

            foreach(var c in module.USER_CONFIGURATIONS)
            {
                sub = doc.CreateElement(c.Key);
                sub.AppendChild(doc.CreateTextNode(c.Value));
                moduleNode.AppendChild(sub);
            }

            return moduleNode;
        }

        public XmlElement __create_modules_node(XmlDocument doc, IReadOnlyList<CONTROLLER_EXTENSION_MODULE_T> modules)
        {
            XmlElement modulesNode = doc.CreateElement("ExtensionModules");
            foreach (var m in modules)
                modulesNode.AppendChild(__create_module_node(doc, m));
            return modulesNode;
        }

        private XmlElement __create_module_node(XmlDocument doc, CONTROLLER_ETHERNET_MODULE_T module)
        {
            XmlElement moduleNode = doc.CreateElement("EthernetModule");

            XmlElement sub = doc.CreateElement("ID");
            sub.AppendChild(doc.CreateTextNode($"0x{module.MODEL.ID:X4}"));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(module.MODEL.Name));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("IP");
            sub.AppendChild(doc.CreateTextNode(module.IP_ADDRESS));
            moduleNode.AppendChild(sub);

            sub = doc.CreateElement("Port");
            sub.AppendChild(doc.CreateTextNode(module.PORT.ToString()));
            moduleNode.AppendChild(sub);

            foreach (var c in module.USER_CONFIGURATIONS)
            {
                sub = doc.CreateElement(c.Key);
                sub.AppendChild(doc.CreateTextNode(c.Value));
                moduleNode.AppendChild(sub);
            }

            return moduleNode;
        }

        public XmlElement __create_modules_node(XmlDocument doc, IReadOnlyList<CONTROLLER_ETHERNET_MODULE_T> modules)
        {
            XmlElement modulesNode = doc.CreateElement("EthernetModules");
            foreach (var m in modules)
                modulesNode.AppendChild(__create_module_node(doc, m));
            return modulesNode;
        }
    }

    public class CONTROLLER_EXTENSION_MODULE_T
    {
        public ControllerExtensionModel MODEL { get; private set; }
        public ushort ADDRESS { get; private set; }
        public IReadOnlyDictionary<string, string> USER_CONFIGURATIONS { get; private set; }

        public CONTROLLER_EXTENSION_MODULE_T(ControllerExtensionModel model, ushort address, Dictionary<string, string> userConfigurations)
        {
            MODEL = model;
            ADDRESS = address;
            USER_CONFIGURATIONS = userConfigurations;
        }
    }

    public class CONTROLLER_ETHERNET_MODULE_T
    {
        public ControllerEthernetModel MODEL { get; private set; }
        public string IP_ADDRESS { get; private set; }
        public ushort PORT { get; private set; }
        public IReadOnlyDictionary<string, string> USER_CONFIGURATIONS { get; private set; }

        public CONTROLLER_ETHERNET_MODULE_T(ControllerEthernetModel model, string ip, ushort port, Dictionary<string, string> userConfigurations)
        {
            MODEL = model;
            IP_ADDRESS = ip;
            PORT = port;
            USER_CONFIGURATIONS = userConfigurations;
        }
    }
}
