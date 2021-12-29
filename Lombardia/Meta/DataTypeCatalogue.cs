using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class DataTypeCatalogue
    {
        public IReadOnlyDictionary<string, DataType> DataTypes { get; private set; }
        private readonly uint __supported_file_format_version;
        public uint FileFormatVersion { get; private set; }
        private byte[] __md5;
        public byte[] MD5Code 
        { 
            get
            {
                byte[] code = new byte[__md5.Length];
                __md5.CopyTo(code, 0);
                return code;
            }
        }

        public DataTypeCatalogue(string catalogueConfiguration)
        {
            __supported_file_format_version = 1;
            DataTypes = __load(catalogueConfiguration, out __md5);
        }

        private Dictionary<string, DataType> __load(XmlNode dataTypesNode)
        {
            try
            {
                Dictionary<string, DataType> dataTypes = new Dictionary<string, DataType>();
                if (dataTypesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode dataTypeNode in dataTypesNode.ChildNodes)
                    {
                        if (dataTypeNode.NodeType != XmlNodeType.Element || dataTypeNode.Name != "DataType")
                            continue;

                        __search_data_type(dataTypeNode, dataTypes);
                    }
                }
                return dataTypes;
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

        public Dictionary<string, DataType> __load(string catalogueConfiguration, out byte[] code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using FileStream stream = File.Open(catalogueConfiguration, FileMode.Open);
                using (MD5 hash = MD5.Create())
                {
                    code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }

                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECDataTypes");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (__supported_file_format_version < FileFormatVersion)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);

                XmlNode dataTypesNode = xmlDoc.SelectSingleNode("/AMECDataTypes");
                return __load(dataTypesNode);
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

        private void __search_data_type(XmlNode dataTypeNode, Dictionary<string, DataType> dictionary)
        {
            uint bitSize = 0, subItemsByteSize = 0, alignment = 0;
            string name;
            List<DataType> subItems = null;
            XmlNode subItemsNode;
            try
            {
                name = dataTypeNode.SelectSingleNode("Name").FirstChild.Value.Trim();
                bitSize = uint.Parse(dataTypeNode.SelectSingleNode("BitSize").FirstChild.Value);
                alignment = uint.Parse(dataTypeNode.SelectSingleNode("Alignment").FirstChild.Value);
                subItemsNode = dataTypeNode["SubItems"];

                if (name.Length == 0 || bitSize == 0 || (bitSize != 1 && bitSize % 8 != 0) || (subItemsNode != null && bitSize % 8 != 0))
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.ILLEGAL_DATA_TYPE_DEFINITION);
                if (dictionary.ContainsKey(name))
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_DATA_TYPE_NAME);

                if (subItemsNode != null)
                {
                    foreach (XmlNode node in subItemsNode.ChildNodes)
                    {
                        if (node.NodeType != XmlNodeType.Element)
                            continue;

                        if (node.Name == "SubItem")
                        {
                            if (subItems == null) subItems = new List<DataType>();
                            __search_sub_items(node, dictionary, subItems, out subItemsByteSize);
                        }
                    }
                    if (bitSize / 8 < subItemsByteSize)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.ILLEGAL_DATA_TYPE_DEFINITION);
                }
                dictionary.Add(name, new DataType { Name = name, BitSize = bitSize, Alignment = alignment, ByteOffset = 0, Comment = "N/A", SubItems = subItems });
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

        private void __search_sub_items(XmlNode subItemNode, Dictionary<string, DataType> dataTypeDictionary,
            List<DataType> subItemList, out uint byteSize)
        {
            uint byteOffset = 0;
            string comment, name;

            byteSize = 0;
            try
            {
                name = subItemNode.SelectSingleNode("Name").FirstChild.Value.Trim();
                byteOffset = uint.Parse(subItemNode.SelectSingleNode("ByteOffset").FirstChild.Value);
                comment = subItemNode.SelectSingleNode("Comment").FirstChild.Value;

                if (dataTypeDictionary.ContainsKey(name) == false)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNDEFINED_DATA_TYPE_NAME);

                subItemList.Add(new DataType { Name = name, BitSize = dataTypeDictionary[name].BitSize, Alignment = 1, ByteOffset = byteOffset, Comment = comment, SubItems = null });
                if (dataTypeDictionary[name].BitSize % 8 == 0)
                    byteSize += dataTypeDictionary[name].BitSize / 8 + byteOffset;
                else
                    byteSize += dataTypeDictionary[name].BitSize / 8 + byteOffset + 1;
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
    }

    public class DataType
    {
        public string Name { get; init; } = "unnamed";
        public uint BitSize { get; init; }
        public uint Alignment { get; init; }
        public uint ByteOffset { get; init; }
        public string Comment { get; init; } = "N/A";

        public IReadOnlyList<DataType>? SubItems { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}
