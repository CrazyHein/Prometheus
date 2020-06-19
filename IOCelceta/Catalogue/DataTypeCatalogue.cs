using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    public enum DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T : int
    {
        NO_ERROR = 0x00000000,
        FILE_LOAD_ERROR = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION = 0x00000002,
        ELEMENT_MISSING = 0x00000003,
        FILE_DATA_EXCEPTION = 0x00000004,
        ILLEGAL_DATA_TYPE_DEFINITION = 0x00000005,
    }

    public class DataTypeCatalogueParseExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T ErrorCode { get; private set; }

        public DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class DataTypeCatalogue
    {
        public IReadOnlyDictionary<string, DataTypeDefinition> DataTypes { get; private set; }
        private Dictionary<string, DataTypeDefinition> __data_types = null;
        private readonly uint __supported_file_format_version;
        public uint FileFormatVersion { get; private set; }

        public DataTypeCatalogue()
        {
            __supported_file_format_version = 1;
            __data_types = new Dictionary<string, DataTypeDefinition>();
            DataTypes = __data_types;
        }

        public void Load(XmlNode dataTypesNode)
        {
            try
            {
                __data_types.Clear();
                if (dataTypesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode dataTypeNode in dataTypesNode.ChildNodes)
                    {
                        if (dataTypeNode.NodeType != XmlNodeType.Element || dataTypeNode.Name != "DataType")
                            continue;

                        __search_data_type(dataTypeNode, __data_types);
                    }
                }
            }
            catch (DataTypeCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                DataTypes = __data_types;
            }
        }

        public void Load(string catalogueConfiguration)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(catalogueConfiguration);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECDataTypes");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (__supported_file_format_version < FileFormatVersion)
                    throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION, null);

                XmlNode dataTypesNode = xmlDoc.SelectSingleNode("/AMECDataTypes");
                Load(dataTypesNode);
            }
            catch (DataTypeCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch(Exception e)
            {
                throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __search_data_type(XmlNode dataTypeNode, Dictionary<string, DataTypeDefinition> dictionary)
        {
            uint bitSize = 0, subItemsByteSize = 0, alignment = 0;
            string name;
            List<DataTypeDefinition> subItems = null;
            XmlNode subItemsNode;
            try
            {
                name = dataTypeNode.SelectSingleNode("Name").FirstChild.Value;
                bitSize = uint.Parse(dataTypeNode.SelectSingleNode("BitSize").FirstChild.Value);
                alignment = uint.Parse(dataTypeNode.SelectSingleNode("Alignment").FirstChild.Value);

                subItemsNode = dataTypeNode["SubItems"];
                if (subItemsNode != null)
                {
                    foreach (XmlNode node in subItemsNode.ChildNodes)
                    {
                        if (node.NodeType != XmlNodeType.Element)
                            continue;

                        if (node.Name == "SubItem")
                        {
                            if (subItems == null) subItems = new List<DataTypeDefinition>();
                            __search_sub_items(node, dictionary, subItems, out subItemsByteSize);
                        }
                    }
                }
                if ((subItems != null && bitSize % 8 == 0 && bitSize / 8 >= subItemsByteSize) || (subItems == null))
                    dictionary.Add(name, new DataTypeDefinition(name, bitSize, alignment, 0, "N/A", subItems));
                else
                    throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.ILLEGAL_DATA_TYPE_DEFINITION, null);
            }
            catch(DataTypeCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch(Exception e)
            {
                throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private void __search_sub_items(XmlNode subItemNode, Dictionary<string, DataTypeDefinition> dataTypeDictionary,
            List<DataTypeDefinition> subItemList, out uint byteSize)
        {
            uint byteOffset = 0;
            string comment, name;

            byteSize = 0;
            try
            {
                name = subItemNode.SelectSingleNode("Name").FirstChild.Value;
                byteOffset = uint.Parse(subItemNode.SelectSingleNode("ByteOffset").FirstChild.Value);
                comment = subItemNode.SelectSingleNode("Comment").FirstChild.Value;

                subItemList.Add(new DataTypeDefinition(name, dataTypeDictionary[name].BitSize, 1, byteOffset, comment, null));
                if (dataTypeDictionary[name].BitSize % 8 == 0)
                    byteSize += dataTypeDictionary[name].BitSize / 8 + byteOffset;
                else
                    byteSize += dataTypeDictionary[name].BitSize / 8 + byteOffset + 1;
            }
            catch (DataTypeCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new DataTypeCatalogueParseExcepetion(DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
        }
    }

    public class DataTypeDefinition
    {
        public string Name { get; private set; }
        public uint BitSize { get; private set; }
        public uint Alignment { get; private set; }
        public uint ByteOffset { get; private set; }
        public string Comment { get; private set; }

        public IReadOnlyList<DataTypeDefinition> SubItems { get; private set; }

        public DataTypeDefinition(string name, uint bitSize, uint alignment, uint byteOffset, string comment, List<DataTypeDefinition> subItems)
        {
            Name = name;
            BitSize = bitSize;
            Alignment = alignment;
            ByteOffset = byteOffset;
            Comment = comment;
            if (subItems != null)
                SubItems = subItems;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
