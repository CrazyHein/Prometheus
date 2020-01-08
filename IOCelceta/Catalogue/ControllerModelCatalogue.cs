using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    public enum MODEL_CATALOGUE_FILE_ERROR_CODE_T : int
    {
        NO_ERROR                                = 0x00000000,
        FILE_LOAD_ERROR                         = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION         = 0x00000002,
        ELEMENT_MISSING                         = 0x00000003,
        FILE_DATA_EXCEPTION                     = 0x00000004
    }

    public class ModelCatalogueParseExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public MODEL_CATALOGUE_FILE_ERROR_CODE_T ErrorCode { get; private set; }

        public ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class ControllerModelCatalogue
    {
        public IReadOnlyDictionary<ushort, ControllerExtensionModel> ExtensionModels { get; private set; }
        public IReadOnlyDictionary<ushort, ControllerEthernetModel> EthernetModels { get; private set; }
        public IReadOnlyList<string> ExtensionModelConfigruationFields { get; private set; }
        public IReadOnlyList<string> EthernetModelConfigruationFields { get; private set; }
        public uint FileFormatVersion { get; private set; }
        private readonly uint __supported_file_format_version;

        private Dictionary<ushort, ControllerExtensionModel> __extension_models = null;
        private Dictionary<ushort, ControllerEthernetModel> __ethernet_models = null;

        public ControllerModelCatalogue()
        {
            __supported_file_format_version = 1;

            __extension_models = new Dictionary<ushort, ControllerExtensionModel>();
            __ethernet_models = new Dictionary<ushort, ControllerEthernetModel>();

            ExtensionModels = __extension_models;
            EthernetModels = __ethernet_models;
        }

        public void Load(string catalogueConfiguration)
        {
            ushort id = 0;
            string name = "";
            ushort bitSize = 0;
            uint mask = 0;
            Dictionary<string, int> txVariables;
            Dictionary<string, int> rxVariables;
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(catalogueConfiguration);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECControllerModels");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);
                if (__supported_file_format_version < FileFormatVersion)
                    throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION, null);
            }
            catch (ModelCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }

            try
            { 
                XmlNode extensionModelsNode = xmlDoc.SelectSingleNode("/AMECControllerModels/ExtensionModels");
                if (extensionModelsNode.NodeType == XmlNodeType.Element)
                {
                    var c = extensionModelsNode.Attributes["ConfigurationFields"];
                    if (c != null)
                        ExtensionModelConfigruationFields = new List<string>(c.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    else
                        ExtensionModelConfigruationFields = new List<string>();

                    foreach (XmlNode extensionModelNode in extensionModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "ExtensionModel")
                            continue;
                        mask = 0;
                        txVariables = new Dictionary<string, int>();
                        rxVariables = new Dictionary<string, int>();
                        foreach (XmlNode node in extensionModelNode.ChildNodes)
                        {
                            if (node.NodeType != XmlNodeType.Element)
                                continue;

                            switch (node.Name)
                            {
                                case "ID":
                                    id = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "BitSize":
                                    bitSize = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    mask |= 0x00000004;
                                    break;
                                case "TX":
                                    foreach (XmlNode x in node.ChildNodes)
                                    {
                                        if (x.NodeType != XmlNodeType.Element)
                                            continue;
                                        txVariables.Add(x.Name, Convert.ToInt32(x.FirstChild.Value, 10));
                                    }
                                    mask |= 0x00000008;
                                    break;
                                case "RX":
                                    foreach (XmlNode x in node.ChildNodes)
                                    {
                                        if (x.NodeType != XmlNodeType.Element)
                                            continue;
                                        rxVariables.Add(x.Name, Convert.ToInt32(x.FirstChild.Value, 10));
                                    }
                                    mask |= 0x00000010;
                                    break;
                            }

                        }
                        if ((mask & 0x00000007) == 0x00000007)
                            __extension_models.Add(id, new ControllerExtensionModel(id, name, bitSize, txVariables, rxVariables));
                        else
                            throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.ELEMENT_MISSING, null);
                    }
                }
            }
            catch (ModelCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                ExtensionModels = __extension_models;
            }

            try
            {
                XmlNode extensionModelsNode = xmlDoc.SelectSingleNode("/AMECControllerModels/EthernetModels");
                if (extensionModelsNode.NodeType == XmlNodeType.Element)
                {
                    var c = extensionModelsNode.Attributes["ConfigurationFields"];
                    if (c != null)
                        EthernetModelConfigruationFields = new List<string>(c.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    else
                        EthernetModelConfigruationFields = new List<string>();

                    foreach (XmlNode extensionModelNode in extensionModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "EthernetModel")
                            continue;
                        mask = 0;
                        txVariables = new Dictionary<string, int>();
                        rxVariables = new Dictionary<string, int>();
                        foreach (XmlNode node in extensionModelNode.ChildNodes)
                        {
                            if (node.NodeType != XmlNodeType.Element)
                                continue;

                            switch (node.Name)
                            {
                                case "ID":
                                    id = Convert.ToUInt16(node.FirstChild.Value, 16);
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "TX":
                                    foreach (XmlNode x in node.ChildNodes)
                                    {
                                        if (x.NodeType != XmlNodeType.Element)
                                            continue;
                                        txVariables.Add(x.Name, Convert.ToInt32(x.FirstChild.Value, 10));
                                    }
                                    mask |= 0x00000004;
                                    break;
                                case "RX":
                                    foreach (XmlNode x in node.ChildNodes)
                                    {
                                        if (x.NodeType != XmlNodeType.Element)
                                            continue;
                                        rxVariables.Add(x.Name, Convert.ToInt32(x.FirstChild.Value, 10));
                                    }
                                    mask |= 0x00000008;
                                    break;
                            }

                        }
                        if ((mask & 0x00000003) == 0x00000003)
                            __ethernet_models.Add(id, new ControllerEthernetModel(id, name, txVariables, rxVariables));
                        else
                            throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.ELEMENT_MISSING, null);
                    }
                }
            }
            catch (ModelCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModelCatalogueParseExcepetion(MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                ExtensionModels = __extension_models;
            }
        }
    }

    public abstract class ControllerModel
    {
        public ushort ID { get; private set; }
        public string Name { get; private set; }

        public ControllerModel(ushort id, string name)
        {
            ID = id;
            Name = name;
        }
        public override string ToString()
        {
            return string.Format("ID = 0x{0:X4} ; Name = {1} ;", ID, Name);
        }
    }

    public class ControllerExtensionModel: ControllerModel
    {
        public IReadOnlyDictionary<string, int> TxVariables { get; private set; }
        public IReadOnlyDictionary<string, int> RxVariables { get; private set; }
        public ushort BitSize { get; private set; }

        public ControllerExtensionModel(ushort id, string name, ushort bitSize, Dictionary<string, int> txVariables, Dictionary<string, int> rxVariables) :base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
            BitSize = bitSize;
        }
    }

    public class ControllerEthernetModel: ControllerModel
    {
        public IReadOnlyDictionary<string, int> TxVariables { get; private set; }
        public IReadOnlyDictionary<string, int> RxVariables { get; private set; }

        public ControllerEthernetModel(ushort id, string name, Dictionary<string, int> txVariables, Dictionary<string, int> rxVariables) :base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
        }
    }
}
