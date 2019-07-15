using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    public enum MODULE_CATALOGUE_FILE_ERROR_CODE_T : int
    {
        NO_ERROR                                = 0x00000000,
        FILE_LOAD_ERROR                         = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION         = 0x00000002,
        ELEMENT_MISSING                         = 0x00000003,
        FILE_DATA_EXCEPTION                     = 0x00000004
    }

    public class ModuleCatalogueParseExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public MODULE_CATALOGUE_FILE_ERROR_CODE_T ErrorCode { get; private set; }

        public ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class ControllerModuleCatalogue
    {
        public IReadOnlyDictionary<ushort, ControllerExtensionModel> ExtensionModules { get; private set; }
        public IReadOnlyDictionary<ushort, ControllerEthernetModel> EthernetModules { get; private set; }
        public uint FileFormatVersion { get; private set; }
        private readonly uint __supported_file_format_version;

        private Dictionary<ushort, ControllerExtensionModel> __extension_modules = null;
        private Dictionary<ushort, ControllerEthernetModel> __ethernet_modules = null;

        public ControllerModuleCatalogue()
        {
            __supported_file_format_version = 1;

            __extension_modules = new Dictionary<ushort, ControllerExtensionModel>();
            __ethernet_modules = new Dictionary<ushort, ControllerEthernetModel>();

            ExtensionModules = __extension_modules;
            EthernetModules = __ethernet_modules;
        }

        public void Load(string catalogueConfiguration)
        {
            ushort id = 0;
            string name = "";
            uint mask = 0;
            Dictionary<string, int> txVariables;
            Dictionary<string, int> rxVariables;
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(catalogueConfiguration);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECControllerModules");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);
                if (__supported_file_format_version < FileFormatVersion)
                    throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION, null);
            }
            catch (ModuleCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }

            try
            { 
                XmlNode extensionModulesNode = xmlDoc.SelectSingleNode("/AMECControllerModules/ExtensionModules");
                if (extensionModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModuleNode in extensionModulesNode.ChildNodes)
                    {
                        if (extensionModuleNode.NodeType != XmlNodeType.Element || extensionModuleNode.Name != "ExtensionModule")
                            continue;
                        mask = 0;
                        txVariables = new Dictionary<string, int>();
                        rxVariables = new Dictionary<string, int>();
                        foreach (XmlNode node in extensionModuleNode.ChildNodes)
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
                            __extension_modules.Add(id, new ControllerExtensionModel(id, name, txVariables, rxVariables));
                        else
                            throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.ELEMENT_MISSING, null);
                    }
                }
            }
            catch (ModuleCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                ExtensionModules = __extension_modules;
            }

            try
            {
                XmlNode extensionModulesNode = xmlDoc.SelectSingleNode("/AMECControllerModules/EthernetModules");
                if (extensionModulesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode extensionModuleNode in extensionModulesNode.ChildNodes)
                    {
                        if (extensionModuleNode.NodeType != XmlNodeType.Element || extensionModuleNode.Name != "EthernetModule")
                            continue;
                        mask = 0;
                        txVariables = new Dictionary<string, int>();
                        rxVariables = new Dictionary<string, int>();
                        foreach (XmlNode node in extensionModuleNode.ChildNodes)
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
                            __ethernet_modules.Add(id, new ControllerEthernetModel(id, name, txVariables, rxVariables));
                        else
                            throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.ELEMENT_MISSING, null);
                    }
                }
            }
            catch (ModuleCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ModuleCatalogueParseExcepetion(MODULE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                ExtensionModules = __extension_modules;
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

        public ControllerExtensionModel(ushort id, string name, Dictionary<string, int> txVariables, Dictionary<string, int> rxVariables):base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
        }
    }

    public class ControllerEthernetModel: ControllerModel
    {
        public IReadOnlyDictionary<string, int> TxVariables { get; private set; }
        public IReadOnlyDictionary<string, int> RxVariables { get; private set; }

        public ControllerEthernetModel(ushort id, string name, Dictionary<string, int> txVariables, Dictionary<string, int> rxVariables):base(id, name)
        {
            TxVariables = txVariables;
            RxVariables = rxVariables;
        }
    }
}
