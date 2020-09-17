using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;
using System.IO;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta
{
    public enum IO_LIST_CONSTANT_T : uint
    {
        NOT_A_VALID_OBJECT_INDEX                                = 0xFFFFFFFF
    }
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
        INVALID_MODULE_IPV4_ADDRESS                             = 0x00000016,
        INVALID_MODULE_LOCAL_ADDRESS                            = 0x00000017,
        MODULE_LOCAL_ADDRESS_OVERLAPPED                         = 0x00000018,                             

        INVALID_OBJECT_DATA_TYPE                                = 0x00000020,
        DUPLICATE_OBJECT_INDEX                                  = 0x00000021,
        INVALID_OBJECT_BINDING_MODULE                           = 0x00000022,
        INVALID_OBJECT_BINDING_MODULE_REFERENCE                 = 0x00000023,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL                   = 0x00000024,
        INVALID_OBJECT_BINDING_MODULE_CHANNEL_INDEX             = 0x00000025,
        INVALID_OBJECT_CONVERTER_DATA_TYPE                      = 0x00000026,
        INVALID_OBJECT_CONVERTER                                = 0x00000027,
        INVALID_OBJECT_VARIABLE                                 = 0x00000028,
        OBJECT_VARIABLE_DATA_TYPE_MISMATCH                      = 0x00000029,
        INVALID_OBJECT_INDEX                                    = 0x0000002A,
        INVALID_OBJECT_RANGE                                    = 0x0000002B,

        INVALID_BIT_OBJECT_INDEX                                = 0x00000030,
        INVALID_BLOCK_OBJECT_INDEX                              = 0x00000031,

        INVALID_OBJECT_DATA_IN_PDO                              = 0x00000040,
        INVALID_OBJECT_REFERENCE_IN_PDO                         = 0x00000041,

        INVALID_OBJECT_REFERENCE_IN_PDO_AREA                    = 0x00000042,
        PDO_AREA_OUT_OF_RANGE                                   = 0x00000043,
        PDO_AREA_OVERLAPPED                                     = 0x00000044,

        INVALID_OBJECT_REFERENCE_IN_INTERLOCK                   = 0x00000050,
        INVALID_INTERLOCK_LOGIC_EXPRESSION                      = 0x00000051,
        INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION                  = 0x00000052,
        INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE            = 0x00000053,
        INVALID_INTERLOCK_LOGIC_DEFINITION                      = 0x00000054,
        INVALID_INTERLOCK_LOGIC_DEFINITION_NAME                 = 0x00000055,

        MODULE_IS_REFERENCED_BY_OBJECT                          = 0x000000F0,
        OBJECT_IS_REFERENCED_BY_PDO                             = 0x000000F1,
        OBJECT_IS_REFERENCED_BY_INTERLOCK                       = 0x000000F2
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

    public enum IO_LIST_INTERLOCK_LOGIC_OPERATOR_T : byte
    {
        AND                     = 0x01,
        OR                      = 0x02,
        NOT                     = 0x10,
        XOR                     = 0x03,
        NAND                    = 0x11,
        NOR                     = 0x12,
    }

    public enum IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE : byte
    {
        OPERAND                 = 0x01,
        EXPRESSION              = 0x02
    }

    public enum IO_LIST_HASH_CODE : byte
    {
        MD5,
        SH256,
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
        private Dictionary<uint, int> __object_reference_counter_of_pdo;
        private Dictionary<uint, int> __object_reference_counter_of_intlk;

        public IO_LIST_TARGET_INFORMATION_T TargetInformation;
        private IO_LIST_CONTROLLER_INFORMATION_T __controller_information;
        private IO_LIST_OBJECT_COLLECTION_T __object_collection;
        private IO_LIST_CONTROLLER_PDO_COLLECTION_T __controller_pdo_collection;
        protected IO_LIST_INTERLOCK_LOGIC_COLLECTION_T __controller_interlock_collection;

        public uint SupportedFileFormatVersion { get { return __supported_file_format_version; } }

        public static Regex VALID_IPV4_ADDRESS { get; private set; }
        public static Regex VALID_MODULE_LOCAL_ADDRESS { get; private set; }
        static IOListDataHelper()
        {
            VALID_IPV4_ADDRESS = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$", RegexOptions.Compiled);
            VALID_MODULE_LOCAL_ADDRESS = new Regex(@"^0x[0-9A-F]{3}0$", RegexOptions.Compiled);
        }

        public IOListDataHelper(ControllerModelCatalogue controllerCatalogue, DataTypeCatalogue dataTypeCatalogue, VariableCatalogue variableCatalogue)
        {
            __supported_file_format_version = 1;
            __module_reference_counter = new Dictionary<string, int>();
            __object_reference_counter_of_pdo = new Dictionary<uint, int>();
            __object_reference_counter_of_intlk = new Dictionary<uint, int>();

            TargetInformation = new IO_LIST_TARGET_INFORMATION_T();
            __controller_information = new IO_LIST_CONTROLLER_INFORMATION_T("127.0.0.1", 5010);
            __object_collection = new IO_LIST_OBJECT_COLLECTION_T();
            __controller_pdo_collection = new IO_LIST_CONTROLLER_PDO_COLLECTION_T();
            __controller_interlock_collection = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T();

            ControllerCatalogue = controllerCatalogue;
            DataTypeCatalogue = dataTypeCatalogue;
            VariableCatalogue = variableCatalogue;
        }

        public void SetDefault()
        {
            __module_reference_counter.Clear();
            __object_reference_counter_of_pdo.Clear();
            __object_reference_counter_of_intlk.Clear();

            TargetInformation.Clear();
            __controller_information.Clear();
            __object_collection.Clear();
            __controller_pdo_collection.Clear();
            __controller_interlock_collection.Clear();
        }

        public ControllerModelCatalogue ControllerCatalogue { get; }
        public DataTypeCatalogue DataTypeCatalogue { get; }
        public VariableCatalogue VariableCatalogue { get; }

        private void __load_target_info(XmlNode basicNode)
        {
            try
            {
                if (basicNode.NodeType == XmlNodeType.Element)
                {
                    TargetInformation.name = basicNode.SelectSingleNode("Name").FirstChild.Value;
                    TargetInformation.description = basicNode.SelectSingleNode("Description").FirstChild.Value;
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
            try
            {
                if (mcServerInfoNode.NodeType == XmlNodeType.Element)
                {
                    MCServerIPAddress = mcServerInfoNode.SelectSingleNode("IP").FirstChild.Value;
                    MCServerPort = Convert.ToUInt16(mcServerInfoNode.SelectSingleNode("Port").FirstChild.Value, 10);
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
            ControllerExtensionModel model = null;
            uint deviceSwitch = 0;
            string referenceName = null;
            ushort localAddress = 0;
            try
            {
                if (extensionModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModule in extensionModulesNode.ChildNodes)
                    {
                        if (extensionModule.NodeType != XmlNodeType.Element || extensionModule.Name != "ExtensionModule")
                            continue;

                        ushort id = Convert.ToUInt16(extensionModule.SelectSingleNode("ID").FirstChild.Value, 16);
                        if(ControllerCatalogue.ExtensionModels.TryGetValue(id, out model) == false)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);

                        deviceSwitch = Convert.ToUInt32(extensionModule.SelectSingleNode("Switch").FirstChild.Value, 16);
                        referenceName = extensionModule.SelectSingleNode("Name").FirstChild.Value;
                        localAddress = Convert.ToUInt16(extensionModule.SelectSingleNode("Address").FirstChild.Value, 16);

                        IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(model, deviceSwitch, referenceName, localAddress);
                        ModuleDataVerification(module);
                        __controller_information.modules.Add(module.reference_name, module);
                        __module_reference_counter.Add(module.reference_name, 0);
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
            ControllerEthernetModel model = null;
            uint deviceSwitch = 0;
            string referenceName = null;
            string ip = null;
            ushort port = 0;
            try
            {
                if (ethernetModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModule in ethernetModulesNode.ChildNodes)
                    {
                        if (extensionModule.NodeType != XmlNodeType.Element || extensionModule.Name != "EthernetModule")
                            continue;

                        ushort id = Convert.ToUInt16(extensionModule.SelectSingleNode("ID").FirstChild.Value, 16);
                        if (ControllerCatalogue.EthernetModels.TryGetValue(id, out model) == false)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_CONTROLLER_EXTENSION_MODEL, null);

                        deviceSwitch = Convert.ToUInt32(extensionModule.SelectSingleNode("Switch").FirstChild.Value, 16);
                        referenceName = extensionModule.SelectSingleNode("Name").FirstChild.Value;
                        ip = extensionModule.SelectSingleNode("IP").FirstChild.Value;
                        port = Convert.ToUInt16(extensionModule.SelectSingleNode("Port").FirstChild.Value, 10);

                        IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(model, deviceSwitch, referenceName, ip, port);
                        ModuleDataVerification(module);
                        __controller_information.modules.Add(module.reference_name, module);
                        __module_reference_counter.Add(module.reference_name, 0);
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
            try
            {
                if (objectsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode objectNode in objectsNode.ChildNodes)
                    {
                        if (objectNode.NodeType != XmlNodeType.Element || objectNode.Name != "Object")
                            continue;

                        uint index = 0;
                        VariableDefinition variableDefinition = null;
                        IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T moduleBindingInfo = null;
                        IO_LIST_OBJECT_COLLECTION_T.VALUE_RANGE_T valueRangeInfo = null;
                        IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T valueConverterInfo = null;

                        index = Convert.ToUInt32(objectNode.SelectSingleNode("Index").FirstChild.Value, 16);
                        if (VariableCatalogue.Variables.TryGetValue(objectNode.SelectSingleNode("Name").FirstChild.Value, out variableDefinition) == false)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_VARIABLE, null);

                        XmlNode opt = objectNode["Binding"];
                        if(opt != null) moduleBindingInfo = __load_object_binding_info(opt);

                        opt = objectNode["Range"];
                        if (opt != null) valueRangeInfo = __load_object_range_info(opt);

                        opt = objectNode["Converter"];
                        if (opt != null) valueConverterInfo = __load_object_converter_info(opt);


                        IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T(index, variableDefinition, moduleBindingInfo, valueRangeInfo, valueConverterInfo);
                        ObjectDataVerification(objectDefinition);
                        __object_collection.objects.Add(objectDefinition.index, objectDefinition);
                        __object_reference_counter_of_pdo.Add(objectDefinition.index, 0);
                        __object_reference_counter_of_intlk.Add(objectDefinition.index, 0);
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

        private IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T __load_object_binding_info(XmlNode bindingNode)
        {
            bool enabled = false;
            IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = null;
            string channelName = null;
            int channelIndex = 0;
            try
            {
                foreach (XmlNode node in bindingNode.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;
                    if (module != null && channelName != null)
                        break;

                    switch (node.Name)
                    {
                        case "Module":
                            if (module != null)
                                break;
                            if (__controller_information.modules.TryGetValue(node.FirstChild.Value, out module) == false)
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_REFERENCE, null);
                            break;
                        default:
                            if (channelName != null)
                                break;
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                channelName = node.Name;
                                channelIndex = Convert.ToInt32(node.FirstChild.Value, 10);
                            }
                            break;
                    }
                }
                if (module == null || channelName == null)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);
                else
                    enabled = true;

                return new IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T(enabled, module, channelName, channelIndex);
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

        private IO_LIST_OBJECT_COLLECTION_T.VALUE_RANGE_T __load_object_range_info(XmlNode converterNode)
        {
            bool enabled = false;
            string upLimit = "0", downLimit = "0";
            try
            {
                upLimit = converterNode.SelectSingleNode("UpLimit").FirstChild.Value;
                downLimit = converterNode.SelectSingleNode("DownLimit").FirstChild.Value;
                    
                enabled = true;

                return new IO_LIST_OBJECT_COLLECTION_T.VALUE_RANGE_T(enabled, upLimit, downLimit);
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

        private IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T __load_object_converter_info(XmlNode converterNode)
        {
            bool enabled = false;
            double upScale = 0, downScale = 0;
            try
            {
                upScale = Convert.ToDouble(converterNode.SelectSingleNode("UpScale").FirstChild.Value);
                downScale = Convert.ToDouble(converterNode.SelectSingleNode("DownScale").FirstChild.Value);
                enabled = true;

                return new IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T(enabled, upScale, downScale);
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

        private void __load_controller_pdo_collection(XmlNode areaNode, IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, IO_LIST_PDO_AREA_T area)
        {
            uint actualPdoSizeInByte = 0;
            uint actualPdoSizeInBit = 0;
            uint objectDataOffsetInBit = 0;
            try
            {
                if (areaNode.NodeType == XmlNodeType.Element)
                {
                    pdo.offset_in_word = Convert.ToUInt32(areaNode.Attributes.GetNamedItem("WordOffset").Value, 10);
                    pdo.size_in_word = Convert.ToUInt32(areaNode.Attributes.GetNamedItem("WordSize").Value, 10);
                    foreach(XmlNode index in areaNode.ChildNodes)
                    {
                        if (index.NodeType != XmlNodeType.Element)
                            continue;

                        if (index.Name == "Index")
                        {
                            uint id = Convert.ToUInt32(index.FirstChild.Value, 16);
                            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = null;
                            if(__object_collection.objects.TryGetValue(id, out objectData) == false)
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO, null);

                            PdoObjectDataVerification(area, objectData, false);

                            if (area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT)
                            {
                                objectDataOffsetInBit = actualPdoSizeInBit;
                                actualPdoSizeInBit += objectData.variable.DataType.BitSize;
                                if (actualPdoSizeInBit % 8 == 0)
                                    actualPdoSizeInByte = actualPdoSizeInBit / 8;
                                else
                                    actualPdoSizeInByte = actualPdoSizeInBit / 8 + 1;
                            }
                            else
                            {
                                //uint objectDataBitSize = objectData.converter.enabled == false ? objectData.variable.DataType.BitSize : objectData.converter.data_type.BitSize;
                                if(actualPdoSizeInByte % objectData.variable.DataType.Alignment != 0)
                                    actualPdoSizeInByte += objectData.variable.DataType.Alignment - actualPdoSizeInByte % objectData.variable.DataType.Alignment;
                                objectDataOffsetInBit = actualPdoSizeInByte * 8;
                                actualPdoSizeInByte += objectData.variable.DataType.BitSize % 8 == 0 ? objectData.variable.DataType.BitSize / 8 : objectData.variable.DataType.BitSize / 8 + 1;
                                actualPdoSizeInBit = actualPdoSizeInByte * 8;
                            }

                            if (actualPdoSizeInByte > (pdo.size_in_word *2))
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                            __object_reference_counter_of_pdo[id]++; 
                            pdo.objects.Add(new IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T(
                                objectData, objectDataOffsetInBit));
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

        private bool __controller_extension_modules_overlap_detector()
        {
            int counter = __controller_information.modules.Count;
            Tuple<uint, uint>[] ranges = new Tuple<uint, uint>[counter];
            int i = 0;
            foreach(var module in __controller_information.modules)
            {
                if (module.Value.model is ControllerExtensionModel)
                {
                    ranges[i] = new Tuple<uint, uint>(module.Value.local_address, (module.Value.model as ControllerExtensionModel).BitSize);
                    ++i;
                }
            }

            counter = i;

            for (i = 0; i < counter - 1; ++i)
            {
                for (int j = 0; j < counter - 1 - i; ++j)
                {
                    if (ranges[j].Item1 > ranges[j + 1].Item1)
                    {
                        Tuple<uint, uint> temp = ranges[j];
                        ranges[j] = ranges[j + 1];
                        ranges[j + 1] = temp;
                    }
                }
            }
            for (i = 0; i < counter - 1; ++i)
            {
                if (ranges[i].Item1 + ranges[i].Item2 > ranges[i + 1].Item1)
                    return true;
            }

            return false;
        }

        private void __load_interlock_logics(XmlNode interlocksNode)
        {
            try
            {
                if (interlocksNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode interlock in interlocksNode.ChildNodes)
                    {
                        if (interlock.NodeType != XmlNodeType.Element || interlock.Name != "Interlock")
                            continue;

                        string name = null;
                        List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> objectDatas = null;
                        IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T statement = null;

                        name = interlock.SelectSingleNode("Name").FirstChild.Value;
                        objectDatas = __load_interlock_logic_target(interlock.SelectSingleNode("Target"));
                        statement = __load_interlock_logic_statement(interlock.SelectSingleNode("Statement").FirstChild, null) as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T;

                        if(statement == null)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.ELEMENT_MISSING, null);

                        __controller_interlock_collection.logic_definitions.Add(
                            new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T(name, objectDatas, statement));
                        __add_logic_statement_object_reference(statement);
                        __add_logic_target_object_reference(objectDatas);
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

        private List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> __load_interlock_logic_target(XmlNode rootNode)
        {
            try
            {
                var objects = new List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T>();
                if(rootNode.NodeType == XmlNodeType.Element)
                {
                    foreach(XmlNode index in rootNode.ChildNodes)
                    {
                        if(index.Name == "Index" && index.NodeType == XmlNodeType.Element)
                        {
                            uint id = Convert.ToUInt32(index.FirstChild.Value, 16);
                            InterlockLogicTargetObjectDataVerification(id);
                            objects.Add(__object_collection.objects[id]);
                        }
                    }
                }
                return objects;
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

        private IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T __load_interlock_logic_statement(XmlNode rootNode,
            IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T rootExpression)
        {
            try
            {
                if (rootNode.NodeType == XmlNodeType.Element)
                {
                    IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T expression = null;
                    if (rootExpression != null)
                    {
                        if (rootExpression.layer == IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.MAX_LAYER_OF_NESTED_LOGIC)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE, null);
                    }
                    switch (rootNode.Name)
                    {
                        case "Index":
                            uint id = Convert.ToUInt32(rootNode.FirstChild.Value, 16);
                            if (rootExpression == null)
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                            InterlockLogicOperandDataVerification(id);
                            return new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T(__object_collection.objects[id], rootExpression);
                        case "NOT":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NOT, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                            {
                                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T element = __load_interlock_logic_statement(node, expression);
                                if (expression.elements.Count == 0)
                                    expression.elements.Add(element);
                                else
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION, null);
                            }
                            return expression;
                        case "AND":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.AND, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.elements.Add(__load_interlock_logic_statement(node, expression));
                            return expression;
                        case "OR":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.OR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.elements.Add(__load_interlock_logic_statement(node, expression));
                            return expression;
                        case "NAND":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NAND, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.elements.Add(__load_interlock_logic_statement(node, expression));
                            return expression;
                        case "NOR":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NOR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.elements.Add(__load_interlock_logic_statement(node, expression));
                            return expression;
                        case "XOR":
                            expression = new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.XOR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.elements.Add(__load_interlock_logic_statement(node, expression));
                            return expression;
                        default:
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                    }
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
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

        private void __add_logic_statement_object_reference(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T statement)
        {
            foreach(var o in statement.elements)
            {
                if (o.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
                    __object_reference_counter_of_intlk[(o as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T).Operand.index]++;
                else
                    __add_logic_statement_object_reference(o as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T);
            }
        }

        private void __remove_logic_statement_object_reference(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T statement)
        {
            foreach (var o in statement.elements)
            {
                if (o.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
                    __object_reference_counter_of_intlk[(o as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T).Operand.index]--;
                else
                    __remove_logic_statement_object_reference(o as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T);
            }
        }

        private void __add_logic_target_object_reference(List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> targets)
        {
            foreach (var o in targets)
                __object_reference_counter_of_intlk[o.index]++;
        }

        private void __remove_logic_target_object_reference(List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> targets)
        {
            foreach (var o in targets)
                __object_reference_counter_of_intlk[o.index]--;
        }

        private void __add_logic_object_reference(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T logic)
        {
            __add_logic_target_object_reference(logic.target_objects);
            __add_logic_statement_object_reference(logic.statement);
        }

        private void __remove_logic_object_reference(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T logic)
        {
            __remove_logic_target_object_reference(logic.target_objects);
            __remove_logic_statement_object_reference(logic.statement);
        }

        public void InterlockLogicTargetObjectDataVerification(List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> datas)
        {
            if(datas == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
            foreach (var o in datas)
            {
                InterlockLogicTargetObjectDataVerification(o);
                /*
                if ((o.index & 0x80000000) != 0 || o.index  >= 0x00002000)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
                else if (__object_collection.objects.Keys.Contains(o.index) == false)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
                else if(__object_collection.objects[o.index] != o)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
                    */
            }
        }

        public void InterlockLogicTargetObjectDataVerification(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T data)
        {
            if(data == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
            else if(__controller_pdo_collection.rx_pdo_bit_area.objects.Find(o => o.object_reference == data) == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
        }

        public void InterlockLogicTargetObjectDataVerification(uint objectIndex)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T data;
            if (__object_collection.objects.TryGetValue(objectIndex, out data) == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);

            InterlockLogicTargetObjectDataVerification(data);
        }

        public void InterlockLogicOperandDataVerification(uint objectIndex)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T data;
            if(__object_collection.objects.TryGetValue(objectIndex, out data) == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);

            if (__controller_pdo_collection.tx_pdo_bit_area.objects.Find(o => o.object_reference.index == objectIndex) == null && 
                __controller_pdo_collection.rx_pdo_bit_area.objects.Find(o => o.object_reference.index == objectIndex) == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
        }

        public void InterlockLogicExpressionDataVerification(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T element)
        {
            if (element == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
            else if(element.layer > IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.MAX_LAYER_OF_NESTED_LOGIC)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE, null);
            else if (element.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
            {
                if (element.root == null)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                else
                    InterlockLogicOperandDataVerification((element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T).Operand.index);
            }
            else if (element.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.EXPRESSION)
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T express = element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T;
                if (express.logic_operator == IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NOT && express.elements.Count > 1)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION, null);
                foreach (var e in express.elements)
                    InterlockLogicExpressionDataVerification(e);
            }
        }

        public void InterlockLogicDataVerification(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T logic)
        {
            if(logic == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_DEFINITION, null);
            else if(logic.name == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_DEFINITION_NAME, null);
            InterlockLogicTargetObjectDataVerification(logic.target_objects);
            InterlockLogicExpressionDataVerification(logic.statement);
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

            if(VALID_IPV4_ADDRESS.IsMatch(moduleData.ip_address) == false)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_MODULE_IPV4_ADDRESS, null);
            if(moduleData.local_address % 16 != 0)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_MODULE_LOCAL_ADDRESS, null);
        }

        public void ObjectDataVerification(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool ignoreDuplicate = false)
        {
            if(objectData.index == (uint)IO_LIST_CONSTANT_T.NOT_A_VALID_OBJECT_INDEX)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_INDEX, null);
            if (ignoreDuplicate == false && __object_collection.objects.Keys.Contains(objectData.index) == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.DUPLICATE_OBJECT_INDEX, null);
            else if(objectData.variable == null || VariableCatalogue.Variables.Keys.Contains(objectData.variable.Name) == false || VariableCatalogue.Variables[objectData.variable.Name] != objectData.variable)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_VARIABLE, null);
            else if (objectData.variable.DataType.BitSize == 1)
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
                    if (objectData.binding.module.model is ControllerExtensionModel)
                    {
                        if ((objectData.index & 0x80000000) != 0)
                            variables = ControllerCatalogue.ExtensionModels[moduleID].TxVariables;
                        else
                            variables = ControllerCatalogue.ExtensionModels[moduleID].RxVariables;
                    }
                    else
                    {
                        if ((objectData.index & 0x80000000) != 0)
                            variables = ControllerCatalogue.EthernetModels[moduleID].TxVariables;
                        else
                            variables = ControllerCatalogue.EthernetModels[moduleID].RxVariables;
                    }
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_REFERENCE, null);

                if (objectData.binding.channel_name != null && variables.Keys.Contains(objectData.binding.channel_name))
                {
                    if (variables[objectData.binding.channel_name] <= objectData.binding.channel_index)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_CHANNEL_INDEX, null);
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_BINDING_MODULE_CHANNEL, null);
            }

            if (objectData.range.enabled == true)
            {
                if (objectData.variable.DataType.BitSize != 1 && objectData.range.down_limit != null && objectData.range.up_limit != null)
                {
                    try
                    {
                        switch (objectData.variable.DataType.Name)
                        {
                            case "INT":
                            case "FIXEDPOINT3201":
                            case "FIXEDPOINT3202":
                                if (Convert.ToInt32(objectData.range.up_limit) < Convert.ToInt32(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "DINT":
                            case "FIXEDPOINT6401":
                            case "FIXEDPOINT6402":
                            case "FIXEDPOINT6404":
                                if (Convert.ToInt64(objectData.range.up_limit) < Convert.ToInt64(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "UINT":
                                if (Convert.ToUInt32(objectData.range.up_limit) < Convert.ToUInt32(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "DUINT":
                            case "UDINT":
                                if (Convert.ToUInt64(objectData.range.up_limit) < Convert.ToUInt64(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "SHORT":
                                if (Convert.ToInt16(objectData.range.up_limit) < Convert.ToInt16(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "USHORT":
                                if (Convert.ToUInt16(objectData.range.up_limit) < Convert.ToUInt16(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "SBYTE":
                                if (Convert.ToSByte(objectData.range.up_limit) < Convert.ToSByte(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "BYTE":
                                if (Convert.ToByte(objectData.range.up_limit) < Convert.ToByte(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "FLOAT":
                                if (Convert.ToSingle(objectData.range.up_limit) < Convert.ToSingle(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            case "DOUBLE":
                                if (Convert.ToDouble(objectData.range.up_limit) < Convert.ToDouble(objectData.range.down_limit))
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                                break;
                            default:
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                        }
                    }
                    catch(IOListParseExcepetion)
                    {
                        throw;
                    }
                    catch
                    {
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
                    }
                }
                else
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_RANGE, null);
            }

            if (objectData.converter.enabled == true)
            {
                if(objectData.variable.DataType.BitSize == 1 || objectData.converter.down_scale == objectData.converter.up_scale)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_CONVERTER, null);
                //else if (objectData.converter.data_type == null || DataTypeCatalogue.DataTypes.Values.Contains(objectData.converter.data_type) == false)
                    //throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_CONVERTER_DATA_TYPE, null);
                //else if (objectData.converter.data_type != objectData.variable.DataType)
                    //throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_VARIABLE_DATA_TYPE_MISMATCH, null);
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
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData;
            if (__object_collection.objects.TryGetValue(objectIndex, out objectData) == true)
            {
                __object_data_area_verification(area, objectData);
                return objectData;
            }
            else
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO, null);
        }

        public void PdoObjectDataVerification(IO_LIST_PDO_AREA_T area, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, bool objectReferenceVerify = true)
        {
            if(objectData == null)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_DATA_IN_PDO, null);
            if (objectReferenceVerify)
            {
                if (__object_collection.objects.Keys.Contains(objectData.index) == false ||
                    __object_collection.objects[objectData.index] != objectData)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_PDO, null);
            }
            __object_data_area_verification(area, objectData);
        }

        public byte[] Load(string ioList, IO_LIST_HASH_CODE code = IO_LIST_HASH_CODE.MD5)
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

                overlap = __controller_extension_modules_overlap_detector();
                if (overlap == true)
                {
                    __controller_information.modules.Clear();
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.MODULE_LOCAL_ADDRESS_OVERLAPPED, null);
                }

                infoNode = xmlDoc.SelectSingleNode("/AMECIOList/Interlocks");
                __load_interlock_logics(infoNode);

                if (code == IO_LIST_HASH_CODE.MD5)
                {
                    using (MD5 hash = MD5.Create())
                    using (FileStream stream = File.Open(ioList, FileMode.Open))
                    {
                        return hash.ComputeHash(stream);
                    }
                }
                else
                    return new byte[0];
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
                if(VALID_IPV4_ADDRESS.IsMatch(value) == true)
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
                __object_reference_counter_of_pdo.Add(dataObject.index, 0);
                __object_reference_counter_of_intlk.Add(dataObject.index, 0);
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
                if (__object_reference_counter_of_pdo[index] != 0)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_PDO, null);
                else if (__object_reference_counter_of_intlk[index] != 0)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_INTERLOCK, null);
                if (__object_collection.objects[index].binding.enabled == true)
                    __module_reference_counter[__object_collection.objects[index].binding.module.reference_name]--;
                __object_collection.objects.Remove(index); 
                __object_reference_counter_of_pdo.Remove(index);
                __object_reference_counter_of_intlk.Remove(index);
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
                    if (__object_reference_counter_of_pdo[index] != 0)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_PDO, null);
                    else if(__object_reference_counter_of_intlk[index] != 0)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.OBJECT_IS_REFERENCED_BY_INTERLOCK, null);
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> TxDiagnosticArea
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> TxBitArea
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> TxBlockArea
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> RxControlArea
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> RxBitArea
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
        public IReadOnlyList<IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T> RxBlockArea
        {
            get
            {
                return __controller_pdo_collection.rx_pdo_block_area.objects;
            }
        }

        private uint __rebuild_pdo_object_offset_index(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, int startPos)
        {
            uint sizeInByte = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    for(int i = startPos; i < pdo.objects.Count; ++i)
                        pdo.objects[i].offset_in_bit = (uint)i;
                    return (uint)(pdo.objects.Count / 8 + (pdo.objects.Count % 8 != 0 ? 1u : 0u));
                default:
                    if (startPos >= 1)
                        sizeInByte = pdo.objects[startPos - 1].offset_in_byte + 
                            pdo.objects[startPos - 1].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[startPos - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u); 

                    for(int i = startPos; i < pdo.objects.Count; ++i)
                    {
                        if (sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment != 0)
                            sizeInByte += pdo.objects[i].object_reference.variable.DataType.Alignment -
                                sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment;
                        pdo.objects[i].offset_in_bit = sizeInByte * 8;
                        sizeInByte += pdo.objects[i].object_reference.variable.DataType.BitSize / 8 + 
                            (pdo.objects[i].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    }
                    return sizeInByte;
            }
        }

        private uint __cal_pdo_actual_size_in_byte_swap(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, int first, int second)
        {
            uint sizeInByte = 0;
            int start = 0;
            int counter = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    return pdo.actual_size_in_byte;
                default:
                    if (first < second)
                    {
                        if (first >= 1)
                            sizeInByte = pdo.objects[first - 1].offset_in_byte + 
                                pdo.objects[first - 1].object_reference.variable.DataType.BitSize / 8 +
                                (pdo.objects[first - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u); 
                        start = first;
                    }
                    else if (first > second)
                    {
                        if (second >= 1)
                            sizeInByte = pdo.objects[second - 1].offset_in_byte +
                                pdo.objects[second - 1].object_reference.variable.DataType.BitSize / 8 +
                                (pdo.objects[second - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                        start = second;
                    }
                    else
                        return pdo.actual_size_in_byte;

                    for (counter = start; counter < pdo.objects.Count; ++counter)
                    {
                        int pos;
                        if (counter == first)
                            pos = second;
                        else if (counter == second)
                            pos = first;
                        else
                            pos = counter;

                        if (sizeInByte % pdo.objects[pos].object_reference.variable.DataType.Alignment != 0)
                            sizeInByte += pdo.objects[pos].object_reference.variable.DataType.Alignment -
                                sizeInByte % pdo.objects[pos].object_reference.variable.DataType.Alignment;
                        sizeInByte += pdo.objects[pos].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[pos].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    }
                    return sizeInByte;
            }
        }

        private uint __cal_pdo_actual_size_in_byte_remove(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, int pos)
        {
            uint sizeInByte = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    return (uint)((pdo.objects.Count - 1) / 8 + ((pdo.objects.Count - 1) % 8 != 0 ? 1u : 0u));
                default:
                    if (pos >= 1)
                        sizeInByte = pdo.objects[pos - 1].offset_in_byte + 
                                pdo.objects[pos - 1].object_reference.variable.DataType.BitSize / 8 +
                                (pdo.objects[pos - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    for (int i = pos + 1; i < pdo.objects.Count; ++i)
                    {
                        if (sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment != 0)
                            sizeInByte += pdo.objects[i].object_reference.variable.DataType.Alignment -
                                sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment;
                        sizeInByte += pdo.objects[i].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[i].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    }
                    return sizeInByte;
            }         
        }

        private uint __cal_pdo_actual_size_in_byte_append(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData)
        {
            uint sizeInByte = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    System.Diagnostics.Debug.Assert(objectData.variable.DataType.BitSize == 1);
                    return (uint)((pdo.objects.Count + 1) / 8 + ((pdo.objects.Count + 1) % 8 != 0 ? 1u : 0u));
                default:
                    if (pdo.objects.Count != 0)
                        sizeInByte = pdo.objects.Last().offset_in_byte +
                                pdo.objects.Last().object_reference.variable.DataType.BitSize / 8 +
                                (pdo.objects.Last().object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    if (sizeInByte % objectData.variable.DataType.Alignment != 0)
                        sizeInByte += objectData.variable.DataType.Alignment - sizeInByte % objectData.variable.DataType.Alignment;
                    sizeInByte += objectData.variable.DataType.BitSize / 8 + (objectData.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    
                    return sizeInByte;
            }
        }

        private uint __cal_pdo_actual_size_in_byte_insert_before(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, int pos, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData)
        {
            uint sizeInByte = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    System.Diagnostics.Debug.Assert(objectData.variable.DataType.BitSize == 1);
                    return (uint)((pdo.objects.Count + 1) / 8 + ((pdo.objects.Count + 1) % 8 != 0 ? 1u : 0u));
                default:
                    if (pos >= 1)
                        sizeInByte = pdo.objects[pos - 1].offset_in_byte +
                                pdo.objects[pos - 1].object_reference.variable.DataType.BitSize / 8 +
                                (pdo.objects[pos - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    if (sizeInByte % objectData.variable.DataType.Alignment != 0)
                        sizeInByte += objectData.variable.DataType.Alignment - sizeInByte % objectData.variable.DataType.Alignment;
                    sizeInByte += objectData.variable.DataType.BitSize / 8 + (objectData.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    for (int i = pos; i < pdo.objects.Count; ++i)
                    {
                        if (sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment != 0)
                            sizeInByte += pdo.objects[i].object_reference.variable.DataType.Alignment -
                                sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment;
                        sizeInByte += pdo.objects[i].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[i].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    }

                    return sizeInByte;
            }
        }

        private uint __cal_pdo_actual_size_in_byte_replace(IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo, int pos, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData)
        {
            uint sizeInByte = 0;
            switch (pdo.area)
            {
                case IO_LIST_PDO_AREA_T.RX_BIT:
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    System.Diagnostics.Debug.Assert(objectData.variable.DataType.BitSize == 1);
                    return pdo.actual_size_in_byte;
                default:
                    if (pos >= 1)
                        sizeInByte = pdo.objects[pos - 1].offset_in_byte +
                            pdo.objects[pos - 1].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[pos - 1].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    if (sizeInByte % objectData.variable.DataType.Alignment != 0)
                        sizeInByte += objectData.variable.DataType.Alignment - sizeInByte % objectData.variable.DataType.Alignment;
                    sizeInByte += objectData.variable.DataType.BitSize / 8 + (objectData.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);

                    for (int i = pos + 1; i < pdo.objects.Count; ++i)
                    {
                        if (sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment != 0)
                            sizeInByte += pdo.objects[i].object_reference.variable.DataType.Alignment -
                                sizeInByte % pdo.objects[i].object_reference.variable.DataType.Alignment;
                        sizeInByte += pdo.objects[i].object_reference.variable.DataType.BitSize / 8 +
                            (pdo.objects[i].object_reference.variable.DataType.BitSize % 8 == 0 ? 0u : 1u);
                    }

                    return sizeInByte;
            }
        }

        public void SwapPDOMapping(IO_LIST_PDO_AREA_T area, int first, int second)
        {
            try
            {
                uint byteSize = 0;
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];

                byteSize = __cal_pdo_actual_size_in_byte_swap(pdo, first, second);
                if (byteSize > pdo.size_in_word * 2)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                var temp = pdo.objects[first].object_reference;
                pdo.objects[first].object_reference = pdo.objects[second].object_reference;
                pdo.objects[second].object_reference = temp;

                pdo.actual_size_in_byte = __rebuild_pdo_object_offset_index(pdo, Math.Min(first, second));
                System.Diagnostics.Debug.Assert(pdo.actual_size_in_byte == byteSize);
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
            uint byteSize = 0;
            try
            {
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];
                byteSize = __cal_pdo_actual_size_in_byte_replace(pdo, pos, objectData);
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                var oldObjectData = pdo.objects[pos].object_reference;
                pdo.objects[pos].object_reference = objectData;
                __object_reference_counter_of_pdo[oldObjectData.index]--;
                __object_reference_counter_of_pdo[objectData.index]++;

                pdo.actual_size_in_byte = __rebuild_pdo_object_offset_index(pdo, pos);
                System.Diagnostics.Debug.Assert(pdo.actual_size_in_byte == byteSize);
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
            uint byteSize = 0;
            try
            {
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];
                byteSize = __cal_pdo_actual_size_in_byte_append(pdo, objectData);
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                pdo.objects.Add(new IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T(objectData, 0));
                __object_reference_counter_of_pdo[objectData.index]++;

                pdo.actual_size_in_byte = __rebuild_pdo_object_offset_index(pdo, pdo.objects.Count - 1);
                System.Diagnostics.Debug.Assert(pdo.actual_size_in_byte == byteSize);
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
            uint byteSize = 0;
            try
            { 
                PdoObjectDataVerification(area, objectData, objectDataReferenceVerify);
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];
                byteSize = __cal_pdo_actual_size_in_byte_insert_before(pdo, pos, objectData);
                if (byteSize > (pdo.size_in_word * 2))
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

                pdo.objects.Insert(pos, new IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T(objectData, 0));
                __object_reference_counter_of_pdo[objectData.index]++;

                pdo.actual_size_in_byte = __rebuild_pdo_object_offset_index(pdo, pos);
                System.Diagnostics.Debug.Assert(pdo.actual_size_in_byte == byteSize);
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
            uint byteSize = 0;
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData;
            try
            {
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];
                
                byteSize = __cal_pdo_actual_size_in_byte_remove(pdo, pos);

                objectData = pdo.objects[pos].object_reference;
                pdo.objects.RemoveAt(pos);    
                __object_reference_counter_of_pdo[objectData.index]--;

                pdo.actual_size_in_byte = __rebuild_pdo_object_offset_index(pdo, pos);
                System.Diagnostics.Debug.Assert(pdo.actual_size_in_byte == byteSize);
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

        public void GroupPDOMappingByBindingModule(IO_LIST_PDO_AREA_T area)
        {
            try
            {
                IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area];

                pdo.objects.Sort(__binding_module_comparison);
                __rebuild_pdo_object_offset_index(pdo, 0);
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

        private int __binding_module_comparison(IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T objectData0, IO_LIST_CONTROLLER_PDO_COLLECTION_T.OBJECT_T objectData1)
        {
            if (objectData0.object_reference.binding.enabled == false && objectData1.object_reference.binding.enabled == false)
                return 0;
            else if (objectData0.object_reference.binding.enabled == false && objectData1.object_reference.binding.enabled == true)
                return 1;
            else if (objectData0.object_reference.binding.enabled == true && objectData1.object_reference.binding.enabled == false)
                return -1;

            int res = objectData0.object_reference.binding.module.reference_name.CompareTo(objectData1.object_reference.binding.module.reference_name);
            if (res == 0)
                res = objectData0.object_reference.binding.channel_name.CompareTo(objectData1.object_reference.binding.channel_name);
            if (res == 0)
            {
                if (objectData0.object_reference.binding.channel_index == objectData1.object_reference.binding.channel_index)
                    res = 0;
                else if (objectData0.object_reference.binding.channel_index > objectData1.object_reference.binding.channel_index)
                    res = 1;
                else
                    res = -1;
            }
            return res;
        }

        public IReadOnlyList<IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T> InterlockDefinitions
        {
            get { return __controller_interlock_collection.logic_definitions; }
        }

        public void AppendInterlockLogicDefinition(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T definition)
        {
            try
            {
                InterlockLogicDataVerification(definition);

                __controller_interlock_collection.logic_definitions.Add(definition);
                __add_logic_object_reference(definition);
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

        public void InsertInterlockLogicDefinition(int pos, IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T definition)
        {
            try
            {
                InterlockLogicDataVerification(definition);

                __controller_interlock_collection.logic_definitions.Insert(pos, definition);
                __add_logic_object_reference(definition);
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

        public void RemoveInterlockLogicDefinition(int pos)
        {
            try
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T temp = __controller_interlock_collection.logic_definitions[pos];
                __controller_interlock_collection.logic_definitions.RemoveAt(pos);
                __remove_logic_object_reference(temp);
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

        public void SwapInterlockLogicDefinition(int first, int second)
        {
            try
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T temp = __controller_interlock_collection.logic_definitions[first];
                __controller_interlock_collection.logic_definitions[first] = __controller_interlock_collection.logic_definitions[second];
                __controller_interlock_collection.logic_definitions[second] = temp;
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

        public void ModifyInterlockLogicDefinition(int pos, IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T logic)
        {
            try
            {
                InterlockLogicDataVerification(logic);
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T temp = __controller_interlock_collection.logic_definitions[pos];
                __controller_interlock_collection.logic_definitions[pos] = logic;
                __remove_logic_object_reference(temp);
                __add_logic_object_reference(logic);
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


        public byte[] Save(IEnumerable<string> extensionModules, IEnumerable<string> ethernetModules, IEnumerable<uint> objects, string fileName, IO_LIST_HASH_CODE code = IO_LIST_HASH_CODE.MD5)
        {
            bool overlap = __overlap_detector(new Tuple<uint, uint>[] {
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_diagnostic_area.offset_in_word, __controller_pdo_collection.tx_pdo_diagnostic_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_bit_area.offset_in_word, __controller_pdo_collection.tx_pdo_bit_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.tx_pdo_block_area.offset_in_word, __controller_pdo_collection.tx_pdo_block_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_control_area.offset_in_word, __controller_pdo_collection.rx_pdo_control_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_bit_area.offset_in_word, __controller_pdo_collection.rx_pdo_bit_area.size_in_word),
                    new Tuple<uint, uint>(__controller_pdo_collection.rx_pdo_block_area.offset_in_word, __controller_pdo_collection.rx_pdo_block_area.size_in_word)});
            if (overlap == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OVERLAPPED, null);

            if(__controller_pdo_collection.tx_pdo_diagnostic_area.size_in_word * 2 < __controller_pdo_collection.tx_pdo_diagnostic_area.actual_size_in_byte ||
                __controller_pdo_collection.tx_pdo_bit_area.size_in_word * 2 < __controller_pdo_collection.tx_pdo_bit_area.actual_size_in_byte ||
                __controller_pdo_collection.tx_pdo_block_area.size_in_word * 2 < __controller_pdo_collection.tx_pdo_block_area.actual_size_in_byte ||
                __controller_pdo_collection.rx_pdo_control_area.size_in_word * 2 < __controller_pdo_collection.rx_pdo_control_area.actual_size_in_byte ||
                __controller_pdo_collection.rx_pdo_bit_area.size_in_word * 2 < __controller_pdo_collection.rx_pdo_bit_area.actual_size_in_byte ||
                __controller_pdo_collection.rx_pdo_block_area.size_in_word * 2 < __controller_pdo_collection.rx_pdo_block_area.actual_size_in_byte)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.PDO_AREA_OUT_OF_RANGE, null);

            overlap = __controller_extension_modules_overlap_detector();
            if (overlap == true)
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.MODULE_LOCAL_ADDRESS_OVERLAPPED, null);

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(decl);

                XmlElement root = xmlDoc.CreateElement("AMECIOList");
                root.SetAttribute("FormatVersion", __supported_file_format_version.ToString());

                root.AppendChild(__create_target_information_node(xmlDoc));
                root.AppendChild(__create_controller_information_node(xmlDoc, extensionModules, ethernetModules));
                root.AppendChild(__create_objects_node(xmlDoc, objects));
                root.AppendChild(__create_tx_pdo_mapping_node(xmlDoc));
                root.AppendChild(__create_rx_pdo_mapping_node(xmlDoc));
                root.AppendChild(__create_interlock_logics_node(xmlDoc));

                xmlDoc.AppendChild(root);
                xmlDoc.Save(fileName);

                if (code == IO_LIST_HASH_CODE.MD5)
                {
                    using (MD5 hash = MD5.Create())
                    using (FileStream stream = File.Open(fileName, FileMode.Open))
                    {
                        return hash.ComputeHash(stream);
                    }
                }
                else
                    return new byte[0];
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
                        sub.AppendChild(doc.CreateTextNode($"0x{module.model.ID:X4}"));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Switch");
                        sub.AppendChild(doc.CreateTextNode($"0x{module.device_switch:X8}"));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Name");
                        sub.AppendChild(doc.CreateTextNode(module.reference_name));
                        extension.AppendChild(sub);

                        sub = doc.CreateElement("Address");
                        sub.AppendChild(doc.CreateTextNode($"0x{module.local_address:X4}"));
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
                    sub.AppendChild(doc.CreateTextNode($"0x{__controller_information.modules[reference].model.ID:X4}"));
                    extension.AppendChild(sub);

                    sub = doc.CreateElement("Switch");
                    sub.AppendChild(doc.CreateTextNode($"0x{__controller_information.modules[reference].device_switch:X8}"));
                    extension.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(reference));
                    extension.AppendChild(sub);

                    sub = doc.CreateElement("Address");
                    sub.AppendChild(doc.CreateTextNode($"0x{__controller_information.modules[reference].local_address:X4}"));
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
                        sub.AppendChild(doc.CreateTextNode($"0x{module.model.ID:X4}"));
                        ethernet.AppendChild(sub);

                        sub = doc.CreateElement("Switch");
                        sub.AppendChild(doc.CreateTextNode($"0x{module.device_switch:X8}"));
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
                    sub.AppendChild(doc.CreateTextNode($"0x{__controller_information.modules[reference].model.ID:X4}"));
                    ethernet.AppendChild(sub);

                    sub = doc.CreateElement("Switch");
                    sub.AppendChild(doc.CreateTextNode($"0x{__controller_information.modules[reference].device_switch:X8}"));
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

        private XmlElement __create_objects_node(XmlDocument doc, IEnumerable<uint> objects)
        {
            XmlElement objectsNode = doc.CreateElement("Objects");
            XmlElement objectNode;
            if(objects == null)
            {
                foreach(var objectData in __object_collection.objects.Values)
                {
                    objectNode = doc.CreateElement("Object");

                    XmlElement sub = doc.CreateElement("Index");
                    sub.AppendChild(doc.CreateTextNode($"0x{objectData.index:X8}"));
                    objectNode.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(objectData.variable.Name));
                    objectNode.AppendChild(sub);

                    if (objectData.binding.enabled == true)
                        objectNode.AppendChild(__create_object_binding_node(doc, objectData.binding));

                    if(objectData.range.enabled == true)
                        objectNode.AppendChild(__create_object_range_node(doc, objectData.range));

                    if (objectData.converter.enabled == true)
                        objectNode.AppendChild(__create_object_converter_node(doc, objectData.converter));

                    objectsNode.AppendChild(objectNode);
                }
            }
            else
            {
                foreach (var index in objects)
                {
                    objectNode = doc.CreateElement("Object");

                    XmlElement sub = doc.CreateElement("Index");
                    sub.AppendChild(doc.CreateTextNode($"0x{__object_collection.objects[index].index:X8}"));
                    objectNode.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(__object_collection.objects[index].variable.Name));
                    objectNode.AppendChild(sub);

                    if (__object_collection.objects[index].binding.enabled == true)
                        objectNode.AppendChild(__create_object_binding_node(doc, __object_collection.objects[index].binding));

                    if (__object_collection.objects[index].range.enabled == true)
                        objectNode.AppendChild(__create_object_range_node(doc, __object_collection.objects[index].range));

                    if (__object_collection.objects[index].converter.enabled == true)
                        objectNode.AppendChild(__create_object_converter_node(doc, __object_collection.objects[index].converter));

                    objectsNode.AppendChild(objectNode);
                }
            }
            return objectsNode;
        }

        private XmlElement __create_object_binding_node(XmlDocument doc, IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T bindingInfo)
        {
            XmlElement binding = doc.CreateElement("Binding");

            XmlElement sub = doc.CreateElement("Module");
            sub.AppendChild(doc.CreateTextNode(bindingInfo.module.reference_name));
            binding.AppendChild(sub);

            sub = doc.CreateElement(bindingInfo.channel_name);
            sub.AppendChild(doc.CreateTextNode(bindingInfo.channel_index.ToString()));
            binding.AppendChild(sub);

            return binding;
        }

        private XmlElement __create_object_range_node(XmlDocument doc, IO_LIST_OBJECT_COLLECTION_T.VALUE_RANGE_T rangeInfo)
        {
            XmlElement converter = doc.CreateElement("Range");

            XmlElement sub;

            sub = doc.CreateElement("UpLimit");
            sub.AppendChild(doc.CreateTextNode(rangeInfo.up_limit.ToString()));
            converter.AppendChild(sub);

            sub = doc.CreateElement("DownLimit");
            sub.AppendChild(doc.CreateTextNode(rangeInfo.down_limit.ToString()));
            converter.AppendChild(sub);

            return converter;
        }

        private XmlElement __create_object_converter_node(XmlDocument doc, IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T converterInfo)
        {
            XmlElement converter = doc.CreateElement("Converter");

            XmlElement sub;

            sub = doc.CreateElement("UpScale");
            sub.AppendChild(doc.CreateTextNode(converterInfo.up_scale.ToString("G17")));
            converter.AppendChild(sub);

            sub = doc.CreateElement("DownScale");
            sub.AppendChild(doc.CreateTextNode(converterInfo.down_scale.ToString("G17")));
            converter.AppendChild(sub);

            return converter;
        }

        private XmlElement __create_tx_pdo_mapping_node(XmlDocument doc)
        {
            XmlElement txpdo = doc.CreateElement("TxPDO");
            txpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC));
            txpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.TX_BIT));
            txpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.TX_BLOCK));

            return txpdo;
        }

        private XmlElement __create_rx_pdo_mapping_node(XmlDocument doc)
        {
            XmlElement rxpdo = doc.CreateElement("RxPDO");
            rxpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.RX_CONTROL));
            rxpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.RX_BIT));
            rxpdo.AppendChild(__create_pdo_mapping_area_node(doc, IO_LIST_PDO_AREA_T.RX_BLOCK));

            return rxpdo;
        }

        private XmlElement __create_pdo_mapping_area_node(XmlDocument doc, IO_LIST_PDO_AREA_T area)
        {
            IO_LIST_CONTROLLER_PDO_COLLECTION_T.AREA_DEFINITION_T pdo = __controller_pdo_collection.Areas[area]; ;
            XmlElement pdoArea = null;
            XmlElement index = null;
            switch (area)
            {
                case IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC:
                    pdoArea = doc.CreateElement("DiagArea");
                    break;
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    pdoArea = doc.CreateElement("BitArea");
                    break;
                case IO_LIST_PDO_AREA_T.TX_BLOCK:
                    pdoArea = doc.CreateElement("BlockArea");
                    break;
                case IO_LIST_PDO_AREA_T.RX_CONTROL:
                    pdoArea = doc.CreateElement("ControlArea");
                    break;
                case IO_LIST_PDO_AREA_T.RX_BIT:
                    pdoArea = doc.CreateElement("BitArea");
                    break;
                case IO_LIST_PDO_AREA_T.RX_BLOCK:
                    pdoArea = doc.CreateElement("BlockArea");
                    break;
            }
            pdoArea.SetAttribute("WordOffset", pdo.offset_in_word.ToString());
            pdoArea.SetAttribute("WordSize", pdo.size_in_word.ToString());

            foreach(var o in pdo.objects)
            {
                index = doc.CreateElement("Index");
                index.AppendChild(doc.CreateTextNode($"0x{o.object_reference.index:X8}"));
                pdoArea.AppendChild(index);
            }

            return pdoArea;
        }

        private XmlElement __create_interlock_logics_node(XmlDocument doc)
        {
            XmlElement interlocks = doc.CreateElement("Interlocks");
            foreach(var i in __controller_interlock_collection.logic_definitions)
            {
                XmlElement interlock = doc.CreateElement("Interlock");

                XmlElement e = doc.CreateElement("Name");
                e.AppendChild(doc.CreateTextNode(i.name));
                interlock.AppendChild(e);

                e = doc.CreateElement("Target");
                foreach (var o in i.target_objects)
                {
                    XmlElement index = doc.CreateElement("Index");
                    index.AppendChild(doc.CreateTextNode($"0x{o.index:X8}"));
                    e.AppendChild(index);
                }
                interlock.AppendChild(e);

                e = doc.CreateElement("Statement");
                e.AppendChild(__create_interlock_logic_statement_element(doc, i.statement));
                interlock.AppendChild(e);

                interlocks.AppendChild(interlock);
            }
            return interlocks;
        }

        private XmlElement __create_interlock_logic_statement_element(XmlDocument doc, IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T elememt)
        {
            XmlElement e;
            if (elememt.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
            {
                e = doc.CreateElement("Index");
                e.AppendChild(doc.CreateTextNode($"0x{(elememt as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T).Operand.index:X8}"));
                return e;
            }
            else
            {
                e = doc.CreateElement((elememt as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T).logic_operator.ToString());
                foreach (var el in (elememt as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T).elements)
                    e.AppendChild(__create_interlock_logic_statement_element(doc, el));
                return e;
            }
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
            public ControllerModel model { get; private set; }
            public uint device_switch { get; private set; }
            public string reference_name { get; private set; }
            public ushort local_address { get; private set; }
            public string ip_address { get; private set; }
            public ushort port { get; private set; }

            public override string ToString()
            {
                return reference_name;
            }

            public MODULE_T()
            {
                model = null;
                device_switch = 0;
                reference_name = "Module";
                local_address = 0x0000;
                ip_address = "127.0.0.1";
                port = 5010;
            }

            public MODULE_T(ControllerModel model, uint deviceSwitch, string referenceName, ushort localAddress)
            {
                this.model = model;
                reference_name = referenceName;
                local_address = localAddress;
                device_switch = deviceSwitch;
                ip_address = "127.0.0.1";
                port = 5010;
            }

            public MODULE_T(ControllerModel model, uint deviceSwitch, string referenceName, string ip, ushort port)
            {
                this.model = model;
                reference_name = referenceName;
                local_address = 0x0000;
                device_switch = deviceSwitch;
                ip_address = ip;
                this.port = port;
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
            public bool enabled { get; private set; }
            public IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module { get; private set; }
            public string channel_name { get; private set; }
            public int channel_index { get; private set; }

            public override string ToString()
            {
                if (enabled == false)
                    return "N/A";
                else
                    return string.Format("{0} -- [{1} : {2}]", module.reference_name, channel_name, channel_index);
            }

            public MODULE_BINDING_T(bool enabled, IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module, string channelName, int channelIndex)
            {
                this.enabled = enabled;
                this.module = module;
                channel_name = channelName;
                channel_index = channelIndex;
            }

            public MODULE_BINDING_T()
            {
                enabled = false;
            }
        }

        public class VALUE_CONVERTER_T
        {
            public bool enabled { get; private set; }
            public double up_scale { get; private set; }
            public double down_scale { get; private set; }

            public override string ToString()
            {
                if (enabled == false)
                    return "N/A";
                else
                    //return string.Format("{0} -- [{1}, {2}] ({3})", data_type.Name, down_scale, up_scale, unit_name);
                    return string.Format("S[{0:G17}, {1:G17}]", down_scale, up_scale);
            }

            public VALUE_CONVERTER_T(bool enabled, double upScale, double downScale)
            {
                this.enabled = enabled;
                down_scale = downScale;
                up_scale = upScale;
            }
            public VALUE_CONVERTER_T()
            {
                enabled = false;
            }
        }

        public class VALUE_RANGE_T
        {
            public bool enabled { get; private set; }
            public string up_limit { get; private set; }
            public string down_limit { get; private set; }

            public override string ToString()
            {
                if (enabled == false)
                    return "N/A";
                else
                    return string.Format("R[{0}, {1}]", down_limit, up_limit);
            }

            public VALUE_RANGE_T(bool enabled, string upLimit, string downLimit)
            {
                this.enabled = enabled;
                down_limit = downLimit;
                up_limit = upLimit;
            }
            public VALUE_RANGE_T()
            {
                enabled = false;
            }
        }

        public class OBJECT_DEFINITION_T
        {
            public uint index { get; private set; }
            //public string friendly_name;
            public VariableDefinition variable { get; private set; }
            //public DataTypeDefinition data_type;
            public MODULE_BINDING_T binding { get; private set; }
            public VALUE_RANGE_T range { get; private set; }
            public VALUE_CONVERTER_T converter { get; private set; }

            public OBJECT_DEFINITION_T(uint index, VariableDefinition variable, MODULE_BINDING_T binding, VALUE_RANGE_T range, VALUE_CONVERTER_T converter)
            {
                this.index = index;
                this.variable = variable;
                if (binding != null)
                    this.binding = binding;
                else
                    this.binding = new MODULE_BINDING_T();
                if (range != null)
                    this.range = range;
                else
                    this.range = new VALUE_RANGE_T();
                if (converter != null)
                    this.converter = converter;
                else
                    this.converter = new VALUE_CONVERTER_T();
            }

            public override string ToString()
            {
                if ((index & 0x80000000) == 0)
                    return string.Format("[0x{0:X8} : {1}] --> [{2} {3}]", index, variable, binding, converter);
                else
                    return string.Format("[0x{0:X8} : {1}] <-- [{2} {3}]", index, variable, binding, converter);
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

    public class IO_LIST_CONTROLLER_PDO_COLLECTION_T
    {
        public class OBJECT_T
        {
            public IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T object_reference;
            public uint offset_in_bit;

            public uint offset_in_byte { get { return offset_in_bit / 8; } }

            public OBJECT_T(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T o, uint offsetInBit)
            {
                object_reference = o;
                offset_in_bit = offsetInBit;
            }
        }

        public class AREA_DEFINITION_T
        {
            public IO_LIST_PDO_AREA_T area { get; private set; }
            public uint offset_in_word = 0;
            public uint size_in_word = 0;
            public uint actual_size_in_byte = 0;
            public List<OBJECT_T> objects;
 
            public AREA_DEFINITION_T(IO_LIST_PDO_AREA_T area)
            {
                this.area = area;
                objects = new List<OBJECT_T>();
            }
        }

        public AREA_DEFINITION_T tx_pdo_diagnostic_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC) { offset_in_word = 0, size_in_word = 128};
        public AREA_DEFINITION_T tx_pdo_bit_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.TX_BIT) { offset_in_word = 128, size_in_word = 32 };
        public AREA_DEFINITION_T tx_pdo_block_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.TX_BLOCK) { offset_in_word = 160, size_in_word = 512 };

        public AREA_DEFINITION_T rx_pdo_control_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.RX_CONTROL) { offset_in_word = 672, size_in_word = 128 };
        public AREA_DEFINITION_T rx_pdo_bit_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.RX_BIT) { offset_in_word = 800, size_in_word = 32 };
        public AREA_DEFINITION_T rx_pdo_block_area = new AREA_DEFINITION_T(IO_LIST_PDO_AREA_T.RX_BLOCK) { offset_in_word = 832, size_in_word = 512 };

        private Dictionary<IO_LIST_PDO_AREA_T, AREA_DEFINITION_T> __area_definitions;

        public IReadOnlyDictionary<IO_LIST_PDO_AREA_T, AREA_DEFINITION_T> Areas { get { return __area_definitions; } }

        public IO_LIST_CONTROLLER_PDO_COLLECTION_T()
        {
            __area_definitions = new Dictionary<IO_LIST_PDO_AREA_T, AREA_DEFINITION_T>(6);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC, tx_pdo_diagnostic_area);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.TX_BIT, tx_pdo_bit_area);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.TX_BLOCK, tx_pdo_block_area);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.RX_CONTROL, rx_pdo_control_area);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.RX_BIT, rx_pdo_bit_area);
            __area_definitions.Add(IO_LIST_PDO_AREA_T.RX_BLOCK, rx_pdo_block_area);
        }

        public void Clear()
        {
            tx_pdo_diagnostic_area.objects.Clear();
            tx_pdo_diagnostic_area.offset_in_word = 0;
            tx_pdo_diagnostic_area.size_in_word = 0;
            tx_pdo_diagnostic_area.actual_size_in_byte = 0;

            tx_pdo_bit_area.objects.Clear();
            tx_pdo_bit_area.offset_in_word = 0;
            tx_pdo_bit_area.size_in_word = 0;
            tx_pdo_bit_area.actual_size_in_byte = 0;

            tx_pdo_block_area.objects.Clear();
            tx_pdo_block_area.offset_in_word = 0;
            tx_pdo_block_area.size_in_word = 0;
            tx_pdo_block_area.actual_size_in_byte = 0;

            rx_pdo_control_area.objects.Clear();
            rx_pdo_control_area.offset_in_word = 0;
            rx_pdo_control_area.size_in_word = 0;
            rx_pdo_control_area.actual_size_in_byte = 0;

            rx_pdo_bit_area.objects.Clear();
            rx_pdo_bit_area.offset_in_word = 0;
            rx_pdo_bit_area.size_in_word = 0;
            rx_pdo_bit_area.actual_size_in_byte = 0;

            rx_pdo_block_area.objects.Clear();
            rx_pdo_block_area.offset_in_word = 0;
            rx_pdo_block_area.size_in_word = 0;
            rx_pdo_block_area.actual_size_in_byte = 0;
        }
    }

    public class IO_LIST_INTERLOCK_LOGIC_COLLECTION_T
    {
        public static readonly int MAX_LAYER_OF_NESTED_LOGIC = 5;

        public class LOGIC_DEFINITION_T
        {
            public string name { get; private set; }
            public List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> target_objects { get; private set; }
            public LOGIC_EXPRESSION_T statement { get; private set; }

            public LOGIC_DEFINITION_T(string name, List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> target, LOGIC_EXPRESSION_T statement)
            {
                this.name = name;
                target_objects = target;
                this.statement = statement;
            }

            public LOGIC_DEFINITION_T(string name, string target, string statement, IReadOnlyDictionary<uint, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> objectDictionary)
            {
                this.name = name;
                try
                {
                    target_objects = new List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T>();

                    System.IO.StringReader sr = new System.IO.StringReader(target);
                    while(true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        else
                            line = line.Trim();
                        if (line == string.Empty)
                            continue;
                        uint id = Convert.ToUInt32(line, 16);
                        IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T temp;
                        if (objectDictionary.TryGetValue(id, out temp) == true)
                            target_objects.Add(temp);
                        else
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
                    }

                    sr = new System.IO.StringReader(statement);
                    List<Tuple<string, int>> statements = new List<Tuple<string, int>>();
                    while(true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        else if (line.Trim() == string.Empty)
                            continue;
                        else
                        {
                            int i = 0;
                            for (; i < line.Count(); ++i)
                                if (line[i] != '\t')
                                    break;
                            statements.Add(new Tuple<string, int>(line.Substring(i).TrimEnd(), i));
                        }
                    }
                    int start = 0;
                    this.statement = __search_logic_element(statements, ref start, null, objectDictionary) as LOGIC_EXPRESSION_T;

                    if(this.statement == null)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
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

            private LOGIC_ELEMEMT_T __search_logic_element(List<Tuple<string, int>> statements, ref int start, LOGIC_EXPRESSION_T root, IReadOnlyDictionary<uint, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> objectDictionary)
            {
                if (root != null && root.layer == MAX_LAYER_OF_NESTED_LOGIC)
                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE, null);
               
                LOGIC_EXPRESSION_T expression = null;
                var lineInfo = statements[start];
                
                switch(lineInfo.Item1)
                {
                    case "NOT":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NOT, root);
                        while(start + 1 < statements.Count)
                        {
                            if (statements[start + 1].Item2 == lineInfo.Item2 + 1)
                            {
                                start++;
                                LOGIC_ELEMEMT_T element = __search_logic_element(statements, ref start, expression, objectDictionary);
                                if (expression.elements.Count == 0)
                                    expression.elements.Add(element);
                                else
                                    throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION, null);
                            }
                            else if (statements[start + 1].Item2 > lineInfo.Item2 + 1)
                                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                            else
                                break;
                        }
                        return expression;
                    case "AND":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.AND, root);
                        break;
                    case "OR":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.OR, root);
                        break;
                    case "NAND":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NAND, root);
                        break;
                    case "NOR":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.NOR, root);
                        break;
                    case "XOR":
                        expression = new LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T.XOR, root);
                        break;
                    default:
                        uint id = Convert.ToUInt32(lineInfo.Item1, 16);
                        if(root == null)
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                        IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T temp;
                        if (objectDictionary.TryGetValue(id, out temp) == true)
                            return new LOGIC_OPERAND_T(temp, root);
                        else
                            throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_OBJECT_REFERENCE_IN_INTERLOCK, null);
                }

                while(start + 1 < statements.Count)
                {
                    if (statements[start + 1].Item2 == lineInfo.Item2 + 1)
                    {
                        start++;
                        expression.elements.Add(__search_logic_element(statements, ref start, expression, objectDictionary));
                    }
                    else if (statements[start + 1].Item2 > lineInfo.Item2 + 1)
                        throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.INVALID_INTERLOCK_LOGIC_EXPRESSION, null);
                    else
                        break;
                }
                return expression;
            }
        }

        public abstract class LOGIC_ELEMEMT_T
        {
            public IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE type { get; private set; }
            public LOGIC_EXPRESSION_T root { get; private set; }
            public int layer { get; private set; }

            public LOGIC_ELEMEMT_T(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE type, LOGIC_EXPRESSION_T root)
            {
                this.type = type;
                this.root = root;
                if (root == null)
                    this.layer = 0;
                else
                    this.layer = root.layer + 1;
            }
        }

        public class LOGIC_OPERAND_T : LOGIC_ELEMEMT_T
        {
            public IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T Operand { get; private set; }
            public LOGIC_OPERAND_T(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData, LOGIC_EXPRESSION_T root):base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND, root)
            {
                Operand = objectData;
            }
        }

        public class LOGIC_EXPRESSION_T : LOGIC_ELEMEMT_T
        {
            public List<LOGIC_ELEMEMT_T> elements { get; private set; }
            public IO_LIST_INTERLOCK_LOGIC_OPERATOR_T logic_operator { get; private set; }
            public LOGIC_EXPRESSION_T(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T op, LOGIC_EXPRESSION_T root) : base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.EXPRESSION, root)
            {
                logic_operator = op;
                elements = new List<LOGIC_ELEMEMT_T>();
            }
        }

        public List<LOGIC_DEFINITION_T> logic_definitions = new List<LOGIC_DEFINITION_T>();

        public void Clear()
        {
            logic_definitions.Clear();
        }
    }
}
