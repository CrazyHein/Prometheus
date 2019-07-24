﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
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
        NO_ERROR                                                = 0x00000000,
        FILE_LOAD_ERROR                                         = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION                         = 0x00000002,
        ELEMENT_MISSING                                         = 0x00000003,
        FILE_DATA_EXCEPTION                                     = 0x00000004,

        INVALID_CONTROLLER_EXTENSION_MODEL                      = 0x00000010,
        INVALID_CONTROLLER_ETHERNET_MODEL                       = 0x00000011,
        DUPLICATE_MODULE_REFERENCE_NAME                         = 0x00000012,
        INVALID_CONTROLLER_MODEL                                = 0x00000013,
        INVALID_CONTROLLER_MODEL_ID                             = 0x00000014,
        INVALID_MC_SERVER_IPV4_ADDRESS                          = 0x00000015,

        INVALID_OBJECT_DATA_TYPE                                = 0x00000020,
        DUPLICATE_OBJECT_INDEX                                  = 0x00000021,
        INVALID_OBJECT_BINDING_MODULE                           = 0x00000022,
        INVALID_OBJECT_BINDING_MODULE_REFERENCE_NAME            = 0x00000023,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL                   = 0x00000024,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL_INDEX             = 0x00000025,
        INVALID_OBJECT_CONVERTER_DATA_TYPE                      = 0x00000026,
        INVALID_OBJECT_CONVERTER                                = 0x00000027,
        INVALID_BIT_OBJECT_INDEX                                = 0x00000030,
        INVALID_BLOCK_OBJECT_INDEX                              = 0x00000031,

        INVALID_OBJECT_REFERENCE_IN_PDO                         = 0x00000040,
        INVALID_OBJECT_REFERENCE_ID_IN_PDO                      = 0x00000041,

        INVALID_OBJECT_REFERENCE_IN_PDO_AREA                    = 0x00000042,
        PDO_AREA_OUT_OF_RANGE                                   = 0x00000043,
        PDO_AREA_OVERLAPPED                                     = 0x00000044,

        MODULE_IS_REFERENCED_BY_OBJECT                          = 0x000000F0,
        OBJECT_IS_REFERENCED_BY_PDO                             = 0x000000F1,
        
    }

    public enum IO_LIST_PDO_AREA_T : byte
    {
        TX_DIAGNOSTIC           = 0x01,
        TX_BIT                  = 0x02,
        TX_BLOCK                = 0x03,

        RX_CONTROL              = 0x11,
        RX_BIT                  = 0x12,
        RX_BLOCK                = 0x13,
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
        private IO_LIST_CONTROLLER_PDO_COLLECTION __controller_pdo_collection;

        public uint SupportedFileFormatVersion { get { return __supported_file_format_version; } }

        public IOListDataHelper(ControllerModelCatalogue controllerCatalogue, DataTypeCatalogue dataTypeCatalogue)
        {
            __supported_file_format_version = 1;
            __module_reference_counter = new Dictionary<string, int>();
            __object_reference_counter = new Dictionary<uint, int>();

            TargetInformation = new IO_LIST_TARGET_INFORMATION_T();
            __controller_information = new IO_LIST_CONTROLLER_INFORMATION_T("127.0.0.1", 5010);
            __object_collection = new IO_LIST_OBJECT_COLLECTION_T();
            __controller_pdo_collection = new IO_LIST_CONTROLLER_PDO_COLLECTION();

            ControllerCatalogue = controllerCatalogue;
            DataTypeCatalogue = dataTypeCatalogue;
        }

        public void SetDefault()
        {
            __module_reference_counter.Clear();
            __object_reference_counter.Clear();

            TargetInformation.Clear();
            __controller_information.Clear();
            __object_collection.Clear();
            __controller_pdo_collection.Clear();
        }

        public ControllerModelCatalogue ControllerCatalogue { get; }
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
                                MCServerIPAddress = node.FirstChild.Value;
                                mask |= 0x00000001;
                                break;
                            case "Port":
                                MCServerPort = Convert.ToUInt16(node.FirstChild.Value, 10);
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
                                    if (ControllerCatalogue.ExtensionModels.Keys.Contains(id) == false)
                                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);
                                    module.model = ControllerCatalogue.ExtensionModels[id];
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
                                    if (ControllerCatalogue.EthernetModels.Keys.Contains(id) == false)
                                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);
                                    module.model = ControllerCatalogue.EthernetModels[id];
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
                                    __load_object_converter_info(node, objectDefinition.converter);
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

        private void __load_object_converter_info(XmlNode converterNode, IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T converterData)
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

        private void __load_controller_pdo_collection(XmlNode areaNode, IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo, IO_LIST_PDO_AREA_T area)
        {
            uint actualPdoSizeInByte = 0;
            uint actualPdoSizeInBit = 0;

            try
            {
                if (areaNode.NodeType == XmlNodeType.Element)
                {
                    pdo.offset_in_word = Convert.ToUInt32(areaNode.Attributes.GetNamedItem("WordOffset").Value, 10);
                    pdo.size_in_word = Convert.ToUInt32(areaNode.Attributes.GetNamedItem("WordSize").Value, 10);
                    foreach(XmlNode index in areaNode.ChildNodes)
                    {
                        if(index.Name == "Index")
                        {
                            uint id = Convert.ToUInt32(index.FirstChild.Value, 16);
                            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = __object_collection.objects[id];
                            PdoObjectDataVerification(area, objectData, false);

                            if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                            {
                                actualPdoSizeInBit += objectData.data_type.BitSize;
                                if (actualPdoSizeInBit % 8 == 0)
                                    actualPdoSizeInByte = actualPdoSizeInBit / 8;
                                else
                                    actualPdoSizeInByte = actualPdoSizeInBit / 8 + 1;
                            }
                            else
                            {
                                uint objectDataBitSize = objectData.converter.enabled == false ? objectData.data_type.BitSize : objectData.converter.data_type.BitSize;
                                if (objectDataBitSize % 8 == 0)
                                    actualPdoSizeInByte += objectDataBitSize / 8;
                                else
                                    actualPdoSizeInByte += objectDataBitSize / 8 + 1;
                            }

                            if (actualPdoSizeInByte > (pdo.size_in_word *2))
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                            __object_reference_counter[id]++;
                            pdo.objects.Add(__object_collection.objects[id]);
                            pdo.actual_size_in_byte = actualPdoSizeInByte;
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

        private bool __overlap_detector(Tuple<uint, uint>[] ranges)
        {
            int counter = ranges.Length;

            for(int i = 0; i < counter - 1; ++i)
            {
                for(int j = 0; j < counter - 1 - i; ++j)
                {
                    if(ranges[j].Item1 > ranges[j + 1].Item1)
                    {
                        Tuple<uint, uint> temp = ranges[j];
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

        public void ModuleDataVerification(IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T moduleData, bool ignoreDuplicate = false)
        {
            if (ignoreDuplicate == false && __controller_information.modules.Keys.Contains(moduleData.reference_name) == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.DUPLICATE_MODULE_REFERENCE_NAME, null);
            if (moduleData.model == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_MODEL, null);

            bool con0 = (ControllerCatalogue.ExtensionModels.Keys.Contains(moduleData.model.ID) &&
                ControllerCatalogue.ExtensionModels[moduleData.model.ID] == moduleData.model);
            bool con1 = (ControllerCatalogue.EthernetModels.Keys.Contains(moduleData.model.ID) &&
                ControllerCatalogue.EthernetModels[moduleData.model.ID] == moduleData.model);

            if (con0 == false && con1 == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_MODEL_ID, null);
        }

        public void ObjectDataVerification(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool ignoreDuplicate = false)
        {
            if (ignoreDuplicate == false && __object_collection.objects.Keys.Contains(objectData.index) == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.DUPLICATE_OBJECT_INDEX, null);
            else if(objectData.data_type == null || DataTypeCatalogue.DataTypes.Values.Contains(objectData.data_type) == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_DATA_TYPE, null);
            else if (objectData.data_type.BitSize == 1)
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
                        variables = ControllerCatalogue.ExtensionModels[moduleID].TxVariables;
                    else
                        variables = ControllerCatalogue.ExtensionModels[moduleID].RxVariables;
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
                if(objectData.data_type.BitSize == 1)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_CONVERTER, null);
                else if (objectData.converter.data_type == null || DataTypeCatalogue.DataTypes.Values.Contains(objectData.converter.data_type) == false)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_CONVERTER_DATA_TYPE, null);
            }
        }

        private void __object_data_area_verification(IO_LIST_PDO_AREA_T area, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData)
        {
            uint index = objectData.index & 0x7FFFFFFF;
            bool isTx = ((objectData.index & 0x80000000) != 0);
            bool isRx = ((objectData.index & 0x80000000) == 0);
            switch (area)
            {
                case IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC:
                case IO_LIST_PDO_AREA_T.TX_BLOCK:
                    if (isRx == true || index < 0x00002000)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO_AREA, null);
                    break;
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    if (isRx == true || index >= 0x00002000)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO_AREA, null);
                    break;

                case IO_LIST_PDO_AREA_T.RX_CONTROL:
                case IO_LIST_PDO_AREA_T.RX_BLOCK:
                    if (isTx == true || index < 0x00002000)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO_AREA, null);
                    break;
                case IO_LIST_PDO_AREA_T.RX_BIT:
                    if (isTx == true || index >= 0x00002000)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO_AREA, null);
                    break;
            }
        }

        public IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T PdoObjectReferenceVerification(IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            if (__object_collection.objects.Keys.Contains(objectIndex))
            {
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = __object_collection.objects[objectIndex];
                __object_data_area_verification(area, objectData);
                return objectData;
            }
            else
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_ID_IN_PDO, null);
        }

        public void PdoObjectDataVerification(IO_LIST_PDO_AREA_T area, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool objectReferenceVerify = true)
        {
            if(objectData == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO, null);
            if (objectReferenceVerify)
            {
                if (__object_collection.objects.Keys.Contains(objectData.index) == false ||
                    __object_collection.objects[objectData.index] != objectData)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_ID_IN_PDO, null);
            }
            __object_data_area_verification(area, objectData);
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

                SetDefault();

                XmlNode infoNode =  xmlDoc.SelectSingleNode("/AMECIOList/TargetInfo");
                __load_target_info(infoNode);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/MCServer");
                __load_mc_server_info(infoNode);

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/ExtensionModules");
                __load_controller_extension_modules(infoNode);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/ControllerInfo/EthernetModules");
                __load_controller_ethernet_modules(infoNode);

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/Objects");
                __load_object_collection(infoNode);

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/TxPDO/DiagArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.tx_pdo_diagnostic_area, IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/TxPDO/BitArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.tx_pdo_bit_area, IO_LIST_PDO_AREA_T.TX_BIT);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/TxPDO/BlockArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.tx_pdo_block_area, IO_LIST_PDO_AREA_T.TX_BLOCK);

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/RxPDO/ControlArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.rx_pdo_control_area, IO_LIST_PDO_AREA_T.RX_CONTROL);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/RxPDO/BitArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.rx_pdo_bit_area, IO_LIST_PDO_AREA_T.RX_BIT);
                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/RxPDO/BlockArea");
                __load_controller_pdo_collection(infoNode, __controller_pdo_collection.rx_pdo_block_area, IO_LIST_PDO_AREA_T.RX_BLOCK);

                bool overlap = __overlap_detector(new Tuple<uint, uint>[] {
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_diagnostic_area.offset_in_word, __controller_pdo_collection.tx_pdo_diagnostic_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_bit_area.offset_in_word, __controller_pdo_collection.tx_pdo_bit_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_block_area.offset_in_word, __controller_pdo_collection.tx_pdo_block_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_control_area.offset_in_word, __controller_pdo_collection.rx_pdo_control_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_bit_area.offset_in_word, __controller_pdo_collection.rx_pdo_bit_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_block_area.offset_in_word, __controller_pdo_collection.rx_pdo_block_area.size_in_word)});
                if(overlap == true)
                {
                    __controller_pdo_collection.Clear();
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OVERLAPPED, null);
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

        public void AddControllerModule(IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module)
        {
            ModuleDataVerification(module);
            try
            {
                __controller_information.modules.Add(module.reference_name, module);
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
                try
                {
                    if (__module_reference_counter[referenceName] != 0)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.MODULE_IS_REFERENCED_BY_OBJECT, null);
                    __controller_information.modules[referenceName] = module;
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
        }

        public IReadOnlyCollection<IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T> ControllerModuleCollection
        {
            get
            {
                return __controller_information.modules.Values;
            }
        }

        public string MCServerIPAddress
        {
            get { return __controller_information.mc_server_ip_address; }
            set
            {
                if (Regex.IsMatch(value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") == true)
                    __controller_information.mc_server_ip_address = value;
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_MC_SERVER_IPV4_ADDRESS, null);
            }
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

        public IReadOnlyDictionary<uint, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> IOObjectDictionary
        {
            get
            {
                return __object_collection.objects;
            }
        }

        public void AddObjectData(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T dataObject)
        {
            ObjectDataVerification(dataObject);
            try
            {
                __object_collection.objects.Add(dataObject.index, dataObject);
                if(dataObject.binding.enabled == true)
                    __module_reference_counter[dataObject.binding.module.reference_name]++;
                __object_reference_counter.Add(dataObject.index, 0);
            }
            catch (Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        public void RemoveObjectData(uint index)
        {
            try
            {
                if (__object_reference_counter[index] != 0)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_PDO, null);
                if (__object_collection.objects[index].binding.enabled == true)
                    __module_reference_counter[__object_collection.objects[index].binding.module.reference_name]--;
                __object_collection.objects.Remove(index); 
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

        public void ModifyObjectData(uint index, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T dataObject)
        {
            if (index != dataObject.index)
            {
                ObjectDataVerification(dataObject, false);
                RemoveObjectData(index);
                AddObjectData(dataObject);
            }
            else
            {
                ObjectDataVerification(dataObject, true);
                try
                {
                    if (__object_reference_counter[index] != 0)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_PDO, null);
                    __object_collection.objects[index] = dataObject;
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
        }

        public uint TxDiagnosticAreaOffset
        {
            get { return __controller_pdo_collection.tx_pdo_diagnostic_area.offset_in_word; }
            set { __controller_pdo_collection.tx_pdo_diagnostic_area.offset_in_word = value; }
        }
        public uint TxDiagnosticAreaSize
        {
            get { return __controller_pdo_collection.tx_pdo_diagnostic_area.size_in_word; }
            set { __controller_pdo_collection.tx_pdo_diagnostic_area.size_in_word = value; }
        }
        public uint TxDiagnosticAreaActualSize
        {
            get { return __controller_pdo_collection.tx_pdo_diagnostic_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> TxDiagnosticArea
        {
            get
            {
                return __controller_pdo_collection.tx_pdo_diagnostic_area.objects;
            }
        }

        public uint TxBitAreaOffset
        {
            get { return __controller_pdo_collection.tx_pdo_bit_area.offset_in_word; }
            set { __controller_pdo_collection.tx_pdo_bit_area.offset_in_word = value; }
        }
        public uint TxBitAreaSize
        {
            get { return __controller_pdo_collection.tx_pdo_bit_area.size_in_word; }
            set { __controller_pdo_collection.tx_pdo_bit_area.size_in_word = value; }
        }
        public uint TxBitAreaActualSize
        {
            get { return __controller_pdo_collection.tx_pdo_bit_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> TxBitArea
        {
            get
            {
                return __controller_pdo_collection.tx_pdo_bit_area.objects;
            }
        }

        public uint TxBlockAreaOffset
        {
            get { return __controller_pdo_collection.tx_pdo_block_area.offset_in_word; }
            set { __controller_pdo_collection.tx_pdo_block_area.offset_in_word = value; }
        }
        public uint TxBlockAreaSize
        {
            get { return __controller_pdo_collection.tx_pdo_block_area.size_in_word; }
            set { __controller_pdo_collection.tx_pdo_block_area.size_in_word = value; }
        }
        public uint TxBlockAreaActualSize
        {
            get { return __controller_pdo_collection.tx_pdo_block_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> TxBlockArea
        {
            get
            {
                return __controller_pdo_collection.tx_pdo_block_area.objects;
            }
        }

        public uint RxControlAreaOffset
        {
            get { return __controller_pdo_collection.rx_pdo_control_area.offset_in_word; }
            set { __controller_pdo_collection.rx_pdo_control_area.offset_in_word = value; }
        }
        public uint RxControlAreaSize
        {
            get { return __controller_pdo_collection.rx_pdo_control_area.size_in_word; }
            set { __controller_pdo_collection.rx_pdo_control_area.size_in_word = value; }
        }
        public uint RxControlAreaActualSize
        {
            get { return __controller_pdo_collection.rx_pdo_control_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> RxControlArea
        {
            get
            {
                return __controller_pdo_collection.rx_pdo_control_area.objects;
            }
        }

        public uint RxBitAreaOffset
        {
            get { return __controller_pdo_collection.rx_pdo_bit_area.offset_in_word; }
            set { __controller_pdo_collection.rx_pdo_bit_area.offset_in_word = value; }
        }
        public uint RxBitAreaSize
        {
            get { return __controller_pdo_collection.rx_pdo_bit_area.size_in_word; }
            set { __controller_pdo_collection.rx_pdo_bit_area.size_in_word = value; }
        }
        public uint RxBitAreaActualSize
        {
            get { return __controller_pdo_collection.rx_pdo_bit_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> RxBitArea
        {
            get
            {
                return __controller_pdo_collection.rx_pdo_bit_area.objects;
            }
        }

        public uint RxBlockAreaOffset
        {
            get { return __controller_pdo_collection.rx_pdo_block_area.offset_in_word; }
            set { __controller_pdo_collection.rx_pdo_block_area.offset_in_word = value; }
        }
        public uint RxBlockAreaSize
        {
            get { return __controller_pdo_collection.rx_pdo_block_area.size_in_word; }
            set { __controller_pdo_collection.rx_pdo_block_area.size_in_word = value; }
        }
        public uint RxBlockAreaActualSize
        {
            get { return __controller_pdo_collection.rx_pdo_block_area.actual_size_in_byte; }
        }
        public IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> RxBlockArea
        {
            get
            {
                return __controller_pdo_collection.rx_pdo_block_area.objects;
            }
        }

        public void SwapPDOMapping(IO_LIST_PDO_AREA_T area, int first, int second)
        {
            try
            {
                IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo = __controller_pdo_collection.PDOs[area];
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T temp = pdo.objects[first];
                pdo.objects[first] = pdo.objects[second];
                pdo.objects[second] = temp;
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

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, uint objectIndex)
        {
            var objectData = PdoObjectReferenceVerification(area, objectIndex);
            ReplacePDOMapping(area, pos, objectData, false);           
        }

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool objectDataReferenceVerify = true)
        {
            uint bitSize = 0, byteSize = 0;
            try
            {
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo = __controller_pdo_collection.PDOs[area];
                var oldObjectData = pdo.objects[pos];

                if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                {
                    bitSize = (uint)pdo.objects.Count();
                    bitSize -= oldObjectData.data_type.BitSize;
                    bitSize += objectData.data_type.BitSize;
                    if (bitSize % 8 == 0)
                        byteSize = bitSize / 8;
                    else
                        byteSize = bitSize / 8 + 1;
                }
                else
                {
                    byteSize = pdo.actual_size_in_byte;
                    uint oldObjectDataBitSize = oldObjectData.converter.enabled == false ? oldObjectData.data_type.BitSize : oldObjectData.converter.data_type.BitSize;
                    uint newObjectDataBitSize = objectData.converter.enabled == false ? objectData.data_type.BitSize : objectData.converter.data_type.BitSize;
                    if (oldObjectDataBitSize % 8 == 0)
                        byteSize -= oldObjectDataBitSize / 8;
                    else
                        byteSize -= oldObjectDataBitSize / 8 + 1;
                    if (newObjectDataBitSize % 8 == 0)
                        byteSize += newObjectDataBitSize / 8;
                    else
                        byteSize += newObjectDataBitSize / 8 + 1;
                }
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                pdo.objects[pos] = objectData;
                pdo.actual_size_in_byte = byteSize;
                __object_reference_counter[oldObjectData.index]--;
                __object_reference_counter[objectData.index]++;
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

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            var objectData = PdoObjectReferenceVerification(area, objectIndex);
            AppendPDOMapping(area, objectData, false);
        }

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool objectDataReferenceVerify = true)
        {
            uint bitSize = 0, byteSize = 0;
            try
            {
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo = __controller_pdo_collection.PDOs[area];

                if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                {
                    bitSize = (uint)pdo.objects.Count();
                    bitSize += objectData.data_type.BitSize;
                    if (bitSize % 8 == 0)
                        byteSize = bitSize / 8;
                    else
                        byteSize = bitSize / 8 + 1;
                }
                else
                {
                    byteSize = pdo.actual_size_in_byte;
                    uint objectDataBitSize = objectData.converter.enabled == false ? objectData.data_type.BitSize : objectData.converter.data_type.BitSize;
                    if (objectDataBitSize % 8 == 0)
                        byteSize += objectDataBitSize / 8;
                    else
                        byteSize += objectDataBitSize / 8 + 1;
                }
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                pdo.objects.Add(objectData);
                pdo.actual_size_in_byte = byteSize;
                __object_reference_counter[objectData.index]++;
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

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            var objectData = PdoObjectReferenceVerification(area, objectIndex);
            InsertPDOMapping(pos, area, objectData, false);
        }

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool objectDataReferenceVerify = true)
        {
            uint bitSize = 0, byteSize = 0;
            try
            { 
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo = __controller_pdo_collection.PDOs[area];

                if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                {
                    bitSize = (uint)pdo.objects.Count();
                    bitSize += objectData.data_type.BitSize;
                    if (bitSize % 8 == 0)
                        byteSize = bitSize / 8;
                    else
                        byteSize = bitSize / 8 + 1;
                }
                else
                {
                    byteSize = pdo.actual_size_in_byte;
                    uint objectDataBitSize = objectData.converter.enabled == false ? objectData.data_type.BitSize : objectData.converter.data_type.BitSize;
                    if (objectDataBitSize % 8 == 0)
                        byteSize += objectDataBitSize / 8;
                    else
                        byteSize += objectDataBitSize / 8 + 1;
                }
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                pdo.objects.Insert(pos, objectData);
                pdo.actual_size_in_byte = byteSize;
                __object_reference_counter[objectData.index]++;
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

        public void RemovePDOMapping(int pos, IO_LIST_PDO_AREA_T area)
        {
            uint bitSize = 0, byteSize = 0;
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData;
            try
            {
                IO_LIST_CONTROLLER_PDO_COLLECTION.PDO_DEFINITION_T pdo = __controller_pdo_collection.PDOs[area];
                objectData = pdo.objects[pos];

                if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                {
                    bitSize = (uint)pdo.objects.Count();
                    bitSize -= objectData.data_type.BitSize;
                    if (bitSize % 8 == 0)
                        byteSize = bitSize / 8;
                    else
                        byteSize = bitSize / 8 + 1;
                }
                else
                {
                    byteSize = pdo.actual_size_in_byte;
                    uint objectDataBitSize = objectData.converter.enabled == false ? objectData.data_type.BitSize : objectData.converter.data_type.BitSize;
                    if (objectDataBitSize % 8 == 0)
                        byteSize -= objectDataBitSize / 8;
                    else
                        byteSize -= objectDataBitSize / 8 + 1;
                }

                pdo.objects.RemoveAt(pos);
                pdo.actual_size_in_byte = byteSize;
                __object_reference_counter[objectData.index]--;
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

        public void Save(IEnumerable<string> extensionModules, IEnumerable<string> ethernetModules, IEnumerable<uint> objects, string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("AMECIOList");
                root.SetAttribute("FormatVersion", __supported_file_format_version.ToString());

                root.AppendChild(__create_target_information_node(xmlDoc));
                root.AppendChild(__create_controller_information_node(xmlDoc, extensionModules, ethernetModules));


                xmlDoc.AppendChild(root);
                xmlDoc.Save(fileName);
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

        private XmlElement __create_target_information_node(XmlDocument doc)
        {
            XmlElement targetInfo = doc.CreateElement("TargetInfo");

            XmlElement sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(TargetInformation.name));
            targetInfo.AppendChild(sub);

            sub = doc.CreateElement("Description");
            sub.AppendChild(doc.CreateTextNode(TargetInformation.description));
            targetInfo.AppendChild(sub);

            return targetInfo;
        }

        private XmlElement __create_controller_information_node(XmlDocument doc, IEnumerable<string> extensionModules, IEnumerable<string> ethernetModules)
        {
            XmlElement controller = doc.CreateElement("ControllerInfo");
            controller.AppendChild(__create_mc_server_information_node(doc));
            controller.AppendChild(__create_extension_modules_node(doc, extensionModules));
            controller.AppendChild(__create_ethernet_modules_node(doc, ethernetModules));
            return controller;
        }

        private XmlElement __create_mc_server_information_node(XmlDocument doc)
        {
            XmlElement mcserver = doc.CreateElement("MCServer");

            XmlElement sub = doc.CreateElement("IP");
            sub.AppendChild(doc.CreateTextNode(__controller_information.mc_server_ip_address));
            mcserver.AppendChild(sub);

            sub = doc.CreateElement("Port");
            sub.AppendChild(doc.CreateTextNode(__controller_information.mc_server_port.ToString()));
            mcserver.AppendChild(sub);

            return mcserver;
        }

        private XmlElement __create_extension_modules_node(XmlDocument doc, IEnumerable<string> extensionModules)
        {
            XmlElement extensions = doc.CreateElement("ExtensionModules");
            XmlElement extension;
            if (extensionModules == null)
            {
                foreach(var module in __controller_information.modules.Values)
                {
                    if (module.model is ControllerExtensionModel)
                    {
                        extension = doc.CreateElement("ExtensionModule");

                        XmlElement sub = doc.CreateElement("ID");
                        sub.AppendChild(doc.CreateTextNode("0x" + module.model.ID.ToString("X4")));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Name");
                        sub.AppendChild(doc.CreateTextNode(module.reference_name));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Address");
                        sub.AppendChild(doc.CreateTextNode("0x" + module.local_address.ToString("X4")));
                        extension.AppendChild(sub);

                        extensions.AppendChild(extension);
                    }
                }
            }
            else
            {
                foreach(var reference in extensionModules)
                {
                    extension = doc.CreateElement("ExtensionModule");

                    XmlElement sub = doc.CreateElement("ID");
                    sub.AppendChild(doc.CreateTextNode("0x"+__controller_information.modules[reference].model.ID.ToString("X4")));
                    extension.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(reference));
                    extension.AppendChild(sub);

                    sub = doc.CreateElement("Address");
                    sub.AppendChild(doc.CreateTextNode("0x"+__controller_information.modules[reference].local_address.ToString("X4")));
                    extension.AppendChild(sub);

                    extensions.AppendChild(extension);
                }
            }
            return extensions;
        }

        private XmlElement __create_ethernet_modules_node(XmlDocument doc, IEnumerable<string> ethernetModules)
        {
            XmlElement ethernets = doc.CreateElement("EthernetModules");
            XmlElement ethernet;
            if (ethernetModules == null)
            {
                foreach (var module in __controller_information.modules.Values)
                {
                    if (module.model is ControllerEthernetModel)
                    {
                        ethernet = doc.CreateElement("EthernetModule");

                        XmlElement sub = doc.CreateElement("ID");
                        sub.AppendChild(doc.CreateTextNode("0x" + module.model.ID.ToString("X4")));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Name");
                        sub.AppendChild(doc.CreateTextNode(module.reference_name));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("IP");
                        sub.AppendChild(doc.CreateTextNode(module.ip_address));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Port");
                        sub.AppendChild(doc.CreateTextNode(module.port.ToString()));
                        ethernet.AppendChild(sub);

                        ethernets.AppendChild(ethernet);
                    }
                }
            }
            else
            {
                foreach (var reference in ethernetModules)
                {
                    ethernet = doc.CreateElement("EthernetModule");

                    XmlElement sub = doc.CreateElement("ID");
                    sub.AppendChild(doc.CreateTextNode("0x" + __controller_information.modules[reference].model.ID.ToString("X4")));
                    ethernet.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(reference));
                    ethernet.AppendChild(sub);

                    sub = doc.CreateElement("IP");
                    sub.AppendChild(doc.CreateTextNode(__controller_information.modules[reference].ip_address));
                    ethernet.AppendChild(sub);

                    sub = doc.CreateElement("Port");
                    sub.AppendChild(doc.CreateTextNode(__controller_information.modules[reference].port.ToString()));
                    ethernet.AppendChild(sub);

                    ethernets.AppendChild(ethernet);
                }
            }
            return ethernets;
        }
    }

    public class IO_LIST_TARGET_INFORMATION_T
    {
        public string name;
        public string description;

        public IO_LIST_TARGET_INFORMATION_T()
        {
            name = "AMEC Etch Tool";
            description = "Input Tool Description Here";
        }

        public void Clear()
        {
            name = "AMEC Etch Tool";
            description = "Input Tool Description Here";
        }
    }

    public class IO_LIST_CONTROLLER_INFORMATION_T
    {
        public string mc_server_ip_address;
        public ushort mc_server_port;

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

        public IO_LIST_CONTROLLER_INFORMATION_T()
        {
            mc_server_ip_address = "127.0.0.1";
            mc_server_port = 5010;
            modules = new Dictionary<string, MODULE_T>();
        }

        public void Clear()
        {
            mc_server_ip_address = "127.0.0.1";
            mc_server_port = 5010;
            modules.Clear();
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

        public IO_LIST_OBJECT_COLLECTION_T()
        {
            objects = new Dictionary<uint, OBJECT_DEFINITION_T>();
        }

        public void Clear()
        {
            objects.Clear();
        }
    }

    public class IO_LIST_CONTROLLER_PDO_COLLECTION
    {
        public class PDO_DEFINITION_T
        {
            public uint offset_in_word = 0;
            public uint size_in_word = 0;
            public uint actual_size_in_byte = 0;
            public List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> objects = new List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T>();
        }

        public PDO_DEFINITION_T tx_pdo_diagnostic_area = new PDO_DEFINITION_T() { offset_in_word = 0, size_in_word = 128 };
        public PDO_DEFINITION_T tx_pdo_bit_area = new PDO_DEFINITION_T() { offset_in_word = 128, size_in_word = 32 };
        public PDO_DEFINITION_T tx_pdo_block_area = new PDO_DEFINITION_T() { offset_in_word = 160, size_in_word = 512 };

        public PDO_DEFINITION_T rx_pdo_control_area = new PDO_DEFINITION_T() { offset_in_word = 672, size_in_word = 128 };
        public PDO_DEFINITION_T rx_pdo_bit_area = new PDO_DEFINITION_T() { offset_in_word = 800, size_in_word = 32 };
        public PDO_DEFINITION_T rx_pdo_block_area = new PDO_DEFINITION_T() { offset_in_word = 832, size_in_word = 512 };

        private Dictionary<IO_LIST_PDO_AREA_T, PDO_DEFINITION_T> __area_pdo_definitions;

        public IReadOnlyDictionary<IO_LIST_PDO_AREA_T, PDO_DEFINITION_T> PDOs { get { return __area_pdo_definitions; } }

        public IO_LIST_CONTROLLER_PDO_COLLECTION()
        {
            __area_pdo_definitions = new Dictionary<IO_LIST_PDO_AREA_T, PDO_DEFINITION_T>(6);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC, tx_pdo_diagnostic_area);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.TX_BIT, tx_pdo_bit_area);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.TX_BLOCK, tx_pdo_block_area);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.RX_CONTROL, rx_pdo_control_area);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.RX_BIT, rx_pdo_bit_area);
            __area_pdo_definitions.Add(IO_LIST_PDO_AREA_T.RX_BLOCK, rx_pdo_block_area);
        }

        public void Clear()
        {
            tx_pdo_diagnostic_area.objects.Clear();
            tx_pdo_diagnostic_area.offset_in_word = 0;
            tx_pdo_diagnostic_area.size_in_word = 0;

            tx_pdo_bit_area.objects.Clear();
            tx_pdo_bit_area.offset_in_word = 0;
            tx_pdo_bit_area.size_in_word = 0;

            tx_pdo_block_area.objects.Clear();
            tx_pdo_block_area.offset_in_word = 0;
            tx_pdo_block_area.size_in_word = 0;

            tx_pdo_diagnostic_area.objects.Clear();
            tx_pdo_diagnostic_area.offset_in_word = 0;
            tx_pdo_diagnostic_area.size_in_word = 0;

            rx_pdo_control_area.objects.Clear();
            rx_pdo_control_area.offset_in_word = 0;
            rx_pdo_control_area.size_in_word = 0;

            rx_pdo_bit_area.objects.Clear();
            rx_pdo_bit_area.offset_in_word = 0;
            rx_pdo_bit_area.size_in_word = 0;

            rx_pdo_block_area.objects.Clear();
            rx_pdo_block_area.offset_in_word = 0;
            rx_pdo_block_area.size_in_word = 0;
        }
    }
}
