using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    public enum VARIABLE_CATALOGUE_FILE_ERROR_CODE_T : int
    {
        NO_ERROR = 0x00000000,
        FILE_LOAD_ERROR = 0x00000001,
        UNSUPPORTED_FILE_FORMAT_VERSION = 0x00000002,
        ELEMENT_MISSING = 0x00000003,
        FILE_DATA_EXCEPTION = 0x00000004,
        INVALID_VARIABLE_DATA_TYPE = 0x00000005
    }

    public class VariableCatalogueParseExcepetion : Exception
    {
        public Exception DataException { get; private set; }
        public VARIABLE_CATALOGUE_FILE_ERROR_CODE_T ErrorCode { get; private set; }

        public VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T errorCode, Exception dataException)
        {
            ErrorCode = errorCode;
            DataException = dataException;
        }
    }

    public class VariableCatalogue
    {
        public IReadOnlyDictionary<string, VariableDefinition> Variables { get; private set; }
        private Dictionary<string, VariableDefinition> __variables = null;
        private readonly uint __supported_file_format_version;
        public uint FileFormatVersion { get; private set; }
        private DataTypeCatalogue __data_type_catalogue;

        public VariableCatalogue(DataTypeCatalogue dataTypeCatalogue)
        {
            __supported_file_format_version = 1;
            __variables = new Dictionary<string, VariableDefinition>();
            Variables = __variables;
            __data_type_catalogue = dataTypeCatalogue;
        }

        public void Load(string catalogueConfiguration)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(catalogueConfiguration);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECVariables");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (__supported_file_format_version < FileFormatVersion)
                    throw new VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION, null);

                XmlNode dataTypesNode = xmlDoc.SelectSingleNode("/AMECVariables");
                __load(dataTypesNode);
            }
            catch (VariableCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
        }


        public void __load(XmlNode variablesNode)
        {
            try
            {
                __variables.Clear();
                if (variablesNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode variableNode in variablesNode.ChildNodes)
                    {
                        if (variableNode.NodeType != XmlNodeType.Element || variableNode.Name != "Variable")
                            continue;

                        uint id = 0;
                        string name = null;
                        DataTypeDefinition dataType = null;
                        string unit = null, comment = null;
                        uint mask = 0;
                        foreach (XmlNode node in variableNode.ChildNodes)
                        {
                            switch (node.Name)
                            {
                                case "ID":
                                    id = uint.Parse(node.FirstChild.Value);
                                    mask |= 0x00000001;
                                    break;
                                case "Name":
                                    name = node.FirstChild.Value;
                                    mask |= 0x00000002;
                                    break;
                                case "DataType":
                                    if (__data_type_catalogue.DataTypes.Keys.Contains(node.FirstChild.Value))
                                    {
                                        dataType = __data_type_catalogue.DataTypes[node.FirstChild.Value];
                                        mask |= 0x00000004;
                                    }
                                    else
                                        throw new VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.INVALID_VARIABLE_DATA_TYPE, null);
                                    break;
                                case "Unit":
                                    unit = node.FirstChild.Value;
                                    mask |= 0x00000008;
                                    break;
                                case "Comment":
                                    comment = node.FirstChild.Value;
                                    break;
                            }
                        }
                        if ((mask & 0x0000000F) == 0x0000000F)
                            __variables.Add(name, new VariableDefinition(id, name, dataType, unit, comment));
                        else
                            throw new VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.ELEMENT_MISSING, null);
                    }
                }
            }
            catch (VariableCatalogueParseExcepetion e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new VariableCatalogueParseExcepetion(VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION, e);
            }
            finally
            {
                Variables = __variables;
            }
        }
    }

    public class VariableDefinition
    {
        public DataTypeDefinition DataType { get; private set; }
        public string Name { get; private set; }
        public uint ID { get; private set; }
        public string Unit { get; private set; }
        public string Comment { get; private set; }

        public VariableDefinition(uint id, string name, DataTypeDefinition dataType, string unit, string comment)
        {
            ID = id;
            Name = name;
            DataType = dataType;
            Unit = unit;
            if (comment != null && comment != string.Empty)
                Comment = comment;
            else
                Comment = "N/A";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
