using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta
{
    public enum IO_LIST_FILE_ERROR_T : int
    {
        NO_ERROR = 0x00000000,
        FILE_LOAD_ERROR = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION = 0x00000002,
        ELEMENT_MISSING = 0x00000003,
        FILE_DATA_EXCEPTION = 0x00000004,

        INVALID_CONTROLLER_EXTENSION_MODEL = 0x00000010,
        INVALID_CONTROLLER_ETHERNET_MODEL = 0x00000011,
        DUPLICATE_MODULE_REFERENCE_NAME = 0x00000012,
        INVALID_CONTROLLER_MODEL = 0x00000013,
        INVALID_CONTROLLER_MODEL_ID = 0x00000014,

        INVALID_OBJECT_DATA_TYPE = 0x00000020,
        DUPLICATE_OBJECT_INDEX = 0x00000021,
        INVALID_OBJECT_BINDING_MODULE = 0x00000022,
        INVALID_OBJECT_BINDING_MODULE_REFERENCE_NAME = 0x00000023,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL = 0x00000024,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL_INDEX = 0x00000025,
        INVALID_OBJECT_CONVERTER_DATA_TYPE = 0x00000026, 
        INVALID_BIT_OBJECT_INDEX = 0x00000030,
        INVALID_BLOCK_OBJECT_INDEX = 0x00000031,
        
        MODULE_IS_REFERENCED_BY_OBJECT = 0x000000F0,
        OBJECT_IS_REFERENCED_BY_PDO = 0x0000000F1,
        
    }

    public class IOListParseExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public IO_LIST_FILE_ERROR_T ErrorCode { get; private set; }

        public IOListParseExcepetion(IO_LIST_FILE_ERROR_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class IOListDataHelper
    {
        public uint FileFormatVersion { get; private set; }
        private readonly uint __supported_file_format_version;
        private Dictionary<string, int> __module_reference_counter;
        private Dictionary<uint, int> __object_reference_counter;

        public IO_LIST_TARGET_INFORMATION_T TargetInformation;
        private IO_LIST_CONTROLLER_INFORMATION_T __controller_information;
        private IO_LIST_OBJECT_COLLECTION_T __object_collection;
        //public IReadOnlyDictionary<string, int> ControllerModuleReferenceCounter { get { return __mudule_reference_counter; } }

        public IOListDataHelper(ControllerModuleCatalogue controllerCatalogue, DataTypeCatalogue dataTypeCatalogue)
        {
            __supported_file_format_version = 1;
            __module_reference_counter = new Dictionary<string, int>();
            __object_reference_counter = new Dictionary<uint, int>();
            SetDefault();

            ControllerCatalogue = controllerCatalogue;
            DataTypeCatalogue = dataTypeCatalogue;
        }

        public void SetDefault()
        {
            __module_reference_counter.Clear();
            __object_reference_counter.Clear();
            TargetInformation = new IO_LIST_TARGET_INFORMATION_T("AMEC Etch Tool");
            __controller_information = new IO_LIST_CONTROLLER_INFORMATION_T("127.0.0.1", 5010);
            __object_collection = new IO_LIST_OBJECT_COLLECTION_T();
        }

        public ControllerModuleCatalogue ControllerCatalogue { get; }
        public DataTypeCatalogue DataTypeCatalogue { get; }

        private void __load_target_info(XmlNode basicNode)
        {
            uint mask = 0;
            try
            {
                if (basicNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node in basicNode.ChildNodes)
                    {

                        if (node.NodeType != XmlNodeType.Element)
                            continue;
                        switch (node.Name)
                        {
                            case "Name":
                                TargetInformation.name = node.FirstChild.Value;
                                mask |= 0x00000001;
                                break;
                            case "Description":
                                TargetInformation.description = node.FirstChild.Value;
                                mask |= 0x00000002;
                                break;
                        }
                    }
                    if (mask != 3)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_mc_server_info(XmlNode mcServerInfoNode)
        {
            uint mask = 0;
            try
            {
                if (mcServerInfoNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node in mcServerInfoNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "IP":
                                __controller_information.mc_server_ip_address = node.FirstChild.Value;
                                mask |= 0x00000001;
                                break;
                            case "Port":
                                __controller_information.mc_server_port = Convert.ToUInt16(node.FirstChild.Value, 10);
                                mask |= 0x00000002;
                                break;
                        }
                    }
                    if (mask != 3)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_controller_extension_modules(XmlNode extensionModulesNode)
        {
            uint mask = 0;
            try
            {
                if (extensionModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModule in extensionModulesNode.ChildNodes)
                    {
                        if (extensionModule.NodeType != XmlNodeType.Element || extensionModule.Name != "ExtensionModule")
                            continue;
                        IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T();
                        foreach (XmlNode node in extensionModule)
                        {
                            switch (node.Name)
                            {
                                case "ID":
                                    ushort id = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    if (ControllerCatalogue.ExtensionModules.Keys.Contains(id) == false)
                                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);
                                    module.model = ControllerCatalogue.ExtensionModules[id];
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    module.reference_name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "Address":
                                    module.local_address = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    mask |= 0x00000004;
                                    break;
                            }
                        }
                        if (mask != 7)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                        else
                        {
                            __controller_information.modules.Add(module.reference_name, module);
                            __module_reference_counter.Add(module.reference_name, 0);
                        }
                    }
                }
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_controller_ethernet_modules(XmlNode ethernetModulesNode)
        {
            uint mask = 0;
            
            try
            {
                if (ethernetModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModule in ethernetModulesNode.ChildNodes)
                    {
                        if (extensionModule.NodeType != XmlNodeType.Element || extensionModule.Name != "EthernetModule")
                            continue;
                        IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T();
                        foreach (XmlNode node in extensionModule)
                        {
                            switch (node.Name)
                            {
                                case "ID":
                                    ushort id = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    if (ControllerCatalogue.EthernetModules.Keys.Contains(id) == false)
                                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);
                                    module.model = ControllerCatalogue.EthernetModules[id];
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    module.reference_name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "IP":
                                    if (Regex.IsMatch(node.FirstChild.Value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") == true)
                                    {
                                        module.ip_address = node.FirstChild.Value;
                                        mask |= 0x00000004;
                                    }
                                    else
                                        throw new ArgumentException("Invalid IPv4 Address String !");
                                    break;
                                case "Port":
                                    module.port = Convert.ToUInt16(node.FirstChild.Value, 10);
                                    mask |= 0x00000008;
                                    break;
                            }
                        }
                        if (mask != 15)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                        else
                        {
                            __controller_information.modules.Add(module.reference_name, module);
                            __module_reference_counter.Add(module.reference_name, 0);
                        }
                    }
                }
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_object_collection(XmlNode objectsNode)
        {
            uint mask = 0;
                 
            try
            {
                if (objectsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode objectNode in objectsNode.ChildNodes)
                    {
                        if (objectNode.NodeType != XmlNodeType.Element || objectNode.Name != "Object")
                            continue;
                        IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T();
                        foreach (XmlNode node in objectNode)
                        {
                            switch (node.Name)
                            {
                                case "Index":
                                    objectDefinition.index = Convert.ToUInt32(node.FirstChild.Value, 16);
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    objectDefinition.friendly_name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "DataType":
                                    if (DataTypeCatalogue.DataTypes.Keys.Contains(node.FirstChild.Value))
                                    {
                                        objectDefinition.data_type = DataTypeCatalogue.DataTypes[node.FirstChild.Value];
                                        mask |= 0x00000004;
                                    }
                                    else
                                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_DATA_TYPE, null);
                                    break;
                                case "Binding":
                                    __load_object_binding_info(node, objectDefinition.binding);
                                    mask |= 0x00000008;
                                    break;
                                case "Converter":
                                    __load_object_converter_info(node, ref objectDefinition.converter);
                                    mask |= 0x00000010;
                                    break;
                            }
                        }
                        
                        if ((mask & 0x00000007) != 0x00000007)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);

                        ObjectDataVerification(objectDefinition);
                        __object_collection.objects.Add(objectDefinition.index, objectDefinition);
                        __object_reference_counter.Add(objectDefinition.index, 0);
                        if (objectDefinition.binding.enabled == true)
                            __module_reference_counter[objectDefinition.binding.module.reference_name]++;

                    }
                }
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_object_binding_info(XmlNode bindingNode, IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T bindingData)
        {
            uint mask = 0;
            bindingData.enabled = false;
            try
            {
                if (bindingNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node in bindingNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "Module":
                                if(__controller_information.modules.Keys.Contains(node.FirstChild.Value) == false)
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_REFERENCE_NAME, null);
                                bindingData.module = __controller_information.modules[node.FirstChild.Value];
                                mask |= 0x00000001;
                                break;
                            default:
                                if (node.NodeType == XmlNodeType.Element)
                                {
                                    bindingData.channel_name = node.Name;
                                    bindingData.channel_index = Convert.ToInt32(node.FirstChild.Value, 10);
                                    mask |= 0x00000002;
                                }
                                break;
                        }
                    }
                    if (mask != 3)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                    else
                        bindingData.enabled = true;
                }
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __load_object_converter_info(XmlNode converterNode, ref IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T converterData)
        {
            uint mask = 0;
            converterData.enabled = false;
            try
            {
                if (converterNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node in converterNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "DataType":
                                if (DataTypeCatalogue.DataTypes.Keys.Contains(node.FirstChild.Value))
                                {
                                    converterData.data_type = DataTypeCatalogue.DataTypes[node.FirstChild.Value];
                                    mask |= 0x00000001;
                                }
                                else
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_DATA_TYPE, null);
                                break;
                            case "Unit":
                                converterData.unit_name = node.FirstChild.Value;
                                mask |= 0x00000002;
                                break;
                            case "UpScale":
                                converterData.up_scale = Convert.ToInt32(node.FirstChild.Value, 10);
                                mask |= 0x00000004;
                                break;
                            case "DownScale":
                                converterData.down_scale = Convert.ToInt32(node.FirstChild.Value, 10);
                                mask |= 0x00000008;
                                break;
                        }
                    }
                    if (mask != 15)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                    else
                        converterData.enabled = true;
                }
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }
        public void ModuleDataVerification(IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T moduleData, bool ignoreDuplicate = false)
        {
            if (ignoreDuplicate == false && __controller_information.modules.Keys.Contains(moduleData.reference_name) == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.DUPLICATE_MODULE_REFERENCE_NAME, null);
            if (moduleData.model == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_MODEL, null);

            bool con0 = (ControllerCatalogue.ExtensionModules.Keys.Contains(moduleData.model.ID) &&
                ControllerCatalogue.ExtensionModules[moduleData.model.ID] == moduleData.model);
            bool con1 = (ControllerCatalogue.EthernetModules.Keys.Contains(moduleData.model.ID) &&
                ControllerCatalogue.EthernetModules[moduleData.model.ID] == moduleData.model);

            if (con0 == false && con1 == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_MODEL_ID, null);
        }

        public void ObjectDataVerification(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool ignoreDuplicate = false)
        {
            if (ignoreDuplicate == false && __object_collection.objects.Keys.Contains(objectData.index) == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.DUPLICATE_OBJECT_INDEX, null);
            else if(objectData.data_type.BitSize == 1)
            {
                uint index = objectData.index & 0x7FFFFFFF;
                if(index > 0x00001FFF)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_BIT_OBJECT_INDEX, null);
            }else
            {
                uint index = objectData.index & 0x7FFFFFFF;
                if (index < 0x00002000)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_BLOCK_OBJECT_INDEX, null);
            }

            if(objectData.binding.enabled == true)
            {
                ushort moduleID;
                IReadOnlyDictionary<string, int> variables = null ;
                if(objectData.binding.module == null)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE, null);
                if (__controller_information.modules.Keys.Contains(objectData.binding.module.reference_name) && 
                    __controller_information.modules[objectData.binding.module.reference_name] == objectData.binding.module)
                {
                    moduleID = __controller_information.modules[objectData.binding.module.reference_name].model.ID;
                    if ((objectData.index & 0x80000000) != 0)
                        variables = ControllerCatalogue.ExtensionModules[moduleID].TxVariables;
                    else
                        variables = ControllerCatalogue.ExtensionModules[moduleID].RxVariables;
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_REFERENCE_NAME, null);

                if (variables.Keys.Contains(objectData.binding.channel_name))
                {
                    if (variables[objectData.binding.channel_name] <= objectData.binding.channel_index)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_CHANNEL_INDEX, null);
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_CHANNEL, null);
            }

            if (objectData.converter.enabled == true)
            {
                if (objectData.converter.data_type == null)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_CONVERTER_DATA_TYPE, null);
            }


        }

        public void Load(string ioList)
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(ioList);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECIOList");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (__supported_file_format_version < FileFormatVersion)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.UNSUPPORTED_FILE_FORMAT_VERSION, null);

                XmlNode infoNode =  xmlDoc.SelectSingleNode("/AMECIOList/TargetInfo");
                __load_target_info(infoNode);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/MCServer");
                __load_mc_server_info(infoNode);
                

                __controller_information.modules.Clear();
                __module_reference_counter.Clear();
                __object_collection.objects.Clear();
                __object_reference_counter.Clear();

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/ExtensionModules");
                __load_controller_extension_modules(infoNode);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/EthernetModules");
                __load_controller_ethernet_modules(infoNode);

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/Objects");
                __load_object_collection(infoNode);

            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void AddControllerModule(IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module)
        {
            ModuleDataVerification(module);
            try
            {
                __controller_information.modules.Add(module.reference_name, module);
                __controller_information.modules_updated = true;
                __module_reference_counter.Add(module.reference_name, 0);
            }
            catch(Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void RemoveControllerModule(string referenceName)
        {
            try
            {
                if (__module_reference_counter[referenceName] != 0)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.MODULE_IS_REFERENCED_BY_OBJECT, null);
                __controller_information.modules.Remove(referenceName);
                __controller_information.modules_updated = true;
                __module_reference_counter.Remove(referenceName);
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void ModifyControllerModule(string referenceName, IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module)
        {
            if (referenceName != module.reference_name)
            {
                ModuleDataVerification(module, false);
                RemoveControllerModule(referenceName);
                AddControllerModule(module);
            }
            else
            {
                ModuleDataVerification(module, true);
                __controller_information.modules[referenceName] = module;
            }
        }

        public IReadOnlyCollection<IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T> ControllerModuleCollection
        {
            get
            {
                return __controller_information.modules.Values;
            }
        }

        public bool ControllerModulesUpdated
        {
            get { return __controller_information.modules_updated; }
            set { __controller_information.modules_updated = value; }
        }

        public string MCServerIPAddress
        {
            get { return __controller_information.mc_server_ip_address; }
            set { __controller_information.mc_server_ip_address = value; }
        }

        public ushort MCServerPort
        {
            get { return __controller_information.mc_server_port; }
            set { __controller_information.mc_server_port = value; }
        }

        public IReadOnlyCollection<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> IOObjectCollection
        {
            get
            {
                return __object_collection.objects.Values;
            }
        }

        public bool ControllerObjectsUpdated
        {
            get { return __object_collection.objects_updated; }
            set { __object_collection.objects_updated = value; }
        }

        public void AddDataObject(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T dataObject)
        {
            ObjectDataVerification(dataObject);
            try
            {
                __object_collection.objects.Add(dataObject.index, dataObject);
                __object_collection.objects_updated = true;
                __object_reference_counter.Add(dataObject.index, 0);
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void RemoveDataObject(uint index)
        {
            try
            {
                if (__object_reference_counter[index] != 0)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_PDO, null);
                __object_collection.objects.Remove(index);
                __object_collection.objects_updated = true;
                __object_reference_counter.Remove(index);
            }
            catch (IOListParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void ModifyDataObject(uint index, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T dataObject)
        {
            if (index != dataObject.index)
            {
                ObjectDataVerification(dataObject, false);
                RemoveDataObject(index);
                AddDataObject(dataObject);
            }
            else
            {
                ObjectDataVerification(dataObject, true);
                __object_collection.objects[index] = dataObject;
            }
        }
    }

    public class IO_LIST_TARGET_INFORMATION_T
    {
        public string name;
        public string description;

        public IO_LIST_TARGET_INFORMATION_T(string targetToolName, string description)
        {
            name = targetToolName;
            this.description = description;
        }

        public IO_LIST_TARGET_INFORMATION_T(string targetToolName)
        {
            name = targetToolName;
            description = "Input Tool Description Here";
        }
    }

    public class IO_LIST_CONTROLLER_INFORMATION_T
    {
        public string mc_server_ip_address;
        public ushort mc_server_port;

        public bool modules_updated = false;

        public class MODULE_T
        {
            public ControllerModel model;
            public string reference_name;
            public ushort local_address;
            public string ip_address;
            public ushort port;

            public override string ToString()
            {
                return reference_name;
            }

            public MODULE_T()
            {
                reference_name = "Module";
                local_address = 0x0000;
                ip_address = "127.0.0.1";
                port = 5010;
            }
        }

        public Dictionary<string, MODULE_T> modules;

        public IO_LIST_CONTROLLER_INFORMATION_T(string mcServerIPAddr, ushort mcServerPort)
        {
            mc_server_ip_address = mcServerIPAddr;
            mc_server_port = mcServerPort;
            modules = new Dictionary<string, MODULE_T>();
        }
    }

    public class IO_LIST_OBJECT_COLLECTION_T
    {
        public class MODULE_BINDING_T
        {
            public bool enabled;
            public IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module;
            public string channel_name;
            public int channel_index;

            public override string ToString()
            {
                if (enabled == false)
                    return "N/A";
                else
                    return string.Format("{0} -- [{1} : {2}]", module.reference_name, channel_name, channel_index);
            }

            public MODULE_BINDING_T()
            {
                channel_name = "Any";
                channel_index = 0;
            }
        }

        public class VALUE_CONVERTER_T
        {
            public bool enabled;
            public DataTypeDefinition data_type;
            public string unit_name;
            public int up_scale;
            public int down_scale;

            public override string ToString()
            {
                if (enabled == false)
                    return "N/A";
                else
                    return string.Format("{0} -- [{1}, {2}] ({3})", data_type.Name, down_scale, up_scale, unit_name);
            }

            public VALUE_CONVERTER_T()
            {
                enabled = false;
                enabled = false;
                down_scale = 0;
                up_scale = 0;
                unit_name = "Any";
            }
        }

        public class OBJECT_DEFINITION_T
        {
            public uint index;
            public string friendly_name;
            public DataTypeDefinition data_type;
            public MODULE_BINDING_T binding;
            public VALUE_CONVERTER_T converter;

            public OBJECT_DEFINITION_T()
            {
                index = 0;
                friendly_name = "New Object";
                binding = new MODULE_BINDING_T();
                converter = new VALUE_CONVERTER_T();
            }
        }

        public Dictionary<uint, OBJECT_DEFINITION_T> objects;
        public bool objects_updated = false;

        public IO_LIST_OBJECT_COLLECTION_T()
        {
            objects = new Dictionary<uint, OBJECT_DEFINITION_T>();
        }
    }
}
