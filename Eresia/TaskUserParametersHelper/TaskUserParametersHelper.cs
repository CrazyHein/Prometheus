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

        public TaskUserParametersHelper(ControllerModelCatalogue catalogue)
        {
            __model_catalogue = catalogue;

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

        private void SetDefault()
        {
            HostCPUAddress = 0x3E00;
            __controller_extension_modules.Clear();
            __controller_ethernet_modules.Clear();
        }

        public void ModuleDataVerification(CONTROLLER_EXTENSION_MODULE_T module)
        {
            if (module.MODEL == null || __model_catalogue.ExtensionModels.Values.Contains(module.MODEL) == false)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_MODEL, null);
            if(module.ADDRESS % 16 != 0)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_ADDRESS, null);
            if (module.USER_CONFIGURATIONS != null)
            {
                List<string> fields = new List<string>();
                foreach (var c in module.USER_CONFIGURATIONS)
                {
                    if (fields.Contains(c.Item1) == true)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_EXTENSION_MODULE_USERCONFIGURATION, null);
                    if (__model_catalogue.ExtensionModelConfigruationFields.Contains(c.Item1) == false)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_EXTENSION_MODULE_USERCONFIGURATION, null);
                    fields.Add(c.Item1);
                }
            }
        }

        public void ModuleDataVerification(CONTROLLER_ETHERNET_MODULE_T module)
        {
            if (module.MODEL == null || __model_catalogue.EthernetModels.Values.Contains(module.MODEL) == false)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_MODEL, null);

            if (module.USER_CONFIGURATIONS != null)
            {
                List<string> fields = new List<string>();
                foreach (var c in module.USER_CONFIGURATIONS)
                {
                    if (fields.Contains(c.Item1) == true)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.DUPLICATED_ETHERNET_MODULE_USERCONFIGURATION, null);
                    if (__model_catalogue.EthernetModelConfigruationFields.Contains(c.Item1) == false)
                        throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_USERCONFIGURATION, null);
                    fields.Add(c.Item1);
                }
            }

            if (Regex.IsMatch(module.IP_ADDRESS, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") != true)
                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_IPV4_ADDRESS, null);
        }

        public ushort HostCPUAddress
        {
            get { return __host_cpu_address; }
            set { __host_cpu_address = value; }
        }


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
            List<Tuple<string, string>> userConfigurations = new List<Tuple<string, string>>();
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
                            userConfigurations.Add(new Tuple<string, string>(filed.Name, filed.FirstChild.Value));
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
            List<Tuple<string, string>> userConfigurations = new List<Tuple<string, string>>();
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
                            if (Regex.IsMatch(filed.FirstChild.Value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") != true)
                                throw new TaskUserParametersExcepetion(TASK_USER_PARAMETERS_ERROR_T.INVALID_ETHERNET_MODULE_IPV4_ADDRESS, null);
                            ip = filed.FirstChild.Value;
                            mask |= 0x00000004;
                            break;
                        case "Port":
                            port = Convert.ToUInt16(filed.FirstChild.Value);
                            mask |= 0x00000008;
                            break;
                        default:
                            userConfigurations.Add(new Tuple<string, string>(filed.Name, filed.FirstChild.Value));
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
    }

    public class CONTROLLER_EXTENSION_MODULE_T
    {
        public ControllerExtensionModel MODEL { get; private set; }
        public ushort ADDRESS { get; private set; }
        public IReadOnlyList<Tuple<string, string>> USER_CONFIGURATIONS;

        public CONTROLLER_EXTENSION_MODULE_T(ControllerExtensionModel model, ushort address, IReadOnlyList<Tuple<string, string>> userConfigurations)
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
        public uint PORT { get; private set; }
        public IReadOnlyList<Tuple<string, string>> USER_CONFIGURATIONS;

        public CONTROLLER_ETHERNET_MODULE_T(ControllerEthernetModel model, string ip, uint port, IReadOnlyList<Tuple<string, string>> userConfigurations)
        {
            MODEL = model;
            IP_ADDRESS = ip;
            PORT = port;
            USER_CONFIGURATIONS = userConfigurations;
        }
    }
}
