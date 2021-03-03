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
            Dictionary<string, uint> txVariables;
            Dictionary<string, uint> rxVariables;
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
                __extension_models.Clear();

                if (extensionModelsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModelNode in extensionModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "ExtensionModel")
                            continue;
                        txVariables = new Dictionary<string, uint>();
                        rxVariables = new Dictionary<string, uint>();
                        
                        id = Convert.ToUInt16(extensionModelNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        name = extensionModelNode.SelectSingleNode("Name").FirstChild.Value;
                        bitSize = Convert.ToUInt16(extensionModelNode.SelectSingleNode("BitSize").FirstChild.Value, 16);
                        XmlNode x = extensionModelNode["TX"];
                        if (x != null)
                        {
                            foreach (XmlNode tx in x.ChildNodes)
                            {
                                if (tx.NodeType != XmlNodeType.Element)
                                    continue;
                                txVariables.Add(tx.Name, Convert.ToUInt32(tx.FirstChild.Value, 10));
                            }
                        }
                        x = extensionModelNode["RX"];
                        if (x != null)
                        {
                            foreach (XmlNode rx in x.ChildNodes)
                            {
                                if (rx.NodeType != XmlNodeType.Element)
                                    continue;
                                rxVariables.Add(rx.Name, Convert.ToUInt32(rx.FirstChild.Value, 10));
                            }
                        }
                            
                        __extension_models.Add(id, new ControllerExtensionModel(id, name, bitSize, txVariables, rxVariables));
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
                __ethernet_models.Clear();

                if (extensionModelsNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModelNode in extensionModelsNode.ChildNodes)
                    {
                        if (extensionModelNode.NodeType != XmlNodeType.Element || extensionModelNode.Name != "EthernetModel")
                            continue;
                        txVariables = new Dictionary<string, uint>();
                        rxVariables = new Dictionary<string, uint>();

                        id = Convert.ToUInt16(extensionModelNode.SelectSingleNode("ID").FirstChild.Value, 16);
                        name = extensionModelNode.SelectSingleNode("Name").FirstChild.Value;
                        XmlNode x = extensionModelNode["TX"];
                        if (x != null)
                        {
                            foreach (XmlNode tx in x.ChildNodes)
                            {
                                if (tx.NodeType != XmlNodeType.Element)
                                    continue;
                                txVariables.Add(tx.Name, Convert.ToUInt32(tx.FirstChild.Value, 10));
                            }
                        }
                        x = extensionModelNode["RX"];
                        if (x != null)
                        {
                            foreach (XmlNode rx in x.ChildNodes)
                            {
                                if (rx.NodeType != XmlNodeType.Element)
                                    continue;
                                rxVariables.Add(rx.Name, Convert.ToUInt32(rx.FirstChild.Value, 10));
                            }
                        }
                       
                        __ethernet_models.Add(id, new ControllerEthernetModel(id, name, txVariables, rxVariables));

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
                EthernetModels = __ethernet_models;
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
        public IReadOnlyDictionary<string, uint> TxVariables { get; private set; }
        public IReadOnlyDictionary<string, uint> RxVariables { get; private set; }
        public ushort BitSize { get; private set; }

        public ControllerExtensionModel(ushort id, string name, ushort bitSize, Dictionary<string, uint> txVariables, Dictionary<string, uint> rxVariables) :base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
            BitSize = bitSize;
        }
    }

    public class ControllerEthernetModel: ControllerModel
    {
        public IReadOnlyDictionary<string, uint> TxVariables { get; private set; }
        public IReadOnlyDictionary<string, uint> RxVariables { get; private set; }

        public ControllerEthernetModel(ushort id, string name, Dictionary<string, uint> txVariables, Dictionary<string, uint> rxVariables) :base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
        }
    }
}
