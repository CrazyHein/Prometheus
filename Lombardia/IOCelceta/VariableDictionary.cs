using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class VariableDictionary : Publisher<Variable>, IEquatable<VariableDictionary>
    {
        public IReadOnlyDictionary<string, Variable> Variables { get; private set; }
        private Dictionary<string, Variable> __variables = new Dictionary<string, Variable>();
        private DataTypeCatalogue __data_type_catalogue;
        public static uint SupportedFileFormatVersion { get; private set; } = 1;
        public uint FileFormatVersion { get; private set; } = 0;

        public VariableDictionary(DataTypeCatalogue dataTypeCatalogue)
        {
            Variables = __variables;
            __data_type_catalogue = dataTypeCatalogue;
        }

        public VariableDictionary(DataTypeCatalogue dataTypeCatalogue, string dictionary, out byte[] md5code)
        {
            __data_type_catalogue = dataTypeCatalogue;
            __load(dictionary, out md5code);
            Variables = __variables;
        }

        public VariableDictionary(DataTypeCatalogue dataTypeCatalogue, XmlNode variablesNode)
        {
            __data_type_catalogue = dataTypeCatalogue;
            Variables = __load_variables(variablesNode);
        }

        public VariableDictionary(DataTypeCatalogue dataTypeCatalogue, Stream sm)
        {
            __data_type_catalogue = dataTypeCatalogue;
            __load(sm);
            Variables = __variables;
        }

        public Variable InspectVariable(string name, string dataTypeName, string unit, string comment)
        {
            if (__data_type_catalogue.DataTypes.TryGetValue(dataTypeName, out var dataType) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_VARIABLE_DATA_TYPE);
            if (__variables.ContainsKey(name) == true)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_VARIABLE);
            return new Variable { Name = name, Type = dataType, Unit = unit, Comment = comment };
        }

        public XmlDocument __save_as(IEnumerable<string> names, bool enableSerialNumber = false, uint version = 1)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(decl);

            XmlElement root = xmlDoc.CreateElement("AMECVariables");
            root.SetAttribute("FormatVersion", SupportedFileFormatVersion.ToString());
            xmlDoc.AppendChild(root);
            uint i = 0;
            foreach (var n in names)
            {
                var varNode = xmlDoc.CreateElement("Variable");
                var variable = __variables[n];

                if (enableSerialNumber)
                {
                    var index = xmlDoc.CreateElement("ID");
                    index.AppendChild(xmlDoc.CreateTextNode(i.ToString()));
                    varNode.AppendChild(index);
                    i++;
                }

                var sub = xmlDoc.CreateElement("Name");
                sub.AppendChild(xmlDoc.CreateTextNode(variable.Name));
                varNode.AppendChild(sub);

                sub = xmlDoc.CreateElement("DataType");
                sub.AppendChild(xmlDoc.CreateTextNode(variable.Type.Name));
                varNode.AppendChild(sub);

                sub = xmlDoc.CreateElement("Unit");
                sub.AppendChild(xmlDoc.CreateTextNode(variable.Unit));
                varNode.AppendChild(sub);

                sub = xmlDoc.CreateElement("Comment");
                sub.AppendChild(xmlDoc.CreateTextNode(variable.Comment));
                varNode.AppendChild(sub);

                root.AppendChild(varNode);
            }
            return xmlDoc;
        }
        public byte[] Save(string path, IEnumerable<string> names, bool enableSerialNumber = false, uint version = 1)
        { 
            try
            {
                XmlDocument xmlDoc = __save_as(names, enableSerialNumber, version);
                xmlDoc.Save(path);
                using (MD5 hash = MD5.Create())
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    return hash.ComputeHash(stream);
                }
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Stream sm, IEnumerable<string> names, bool enableSerialNumber = false, uint version = 1)
        {
            try
            {
                XmlDocument xmlDoc = __save_as(names, enableSerialNumber, version);
                xmlDoc.Save(sm);
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }

        }

        public void Save(XmlDocument doc, IEnumerable<string> names, XmlElement variablesInfo, uint version = 1)
        {
            try
            {
                foreach (var n in names)
                {
                    var varNode = doc.CreateElement("Variable");
                    var variable = __variables[n];

                    var sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(variable.Name));
                    varNode.AppendChild(sub);

                    sub = doc.CreateElement("DataType");
                    sub.AppendChild(doc.CreateTextNode(variable.Type.Name));
                    varNode.AppendChild(sub);

                    sub = doc.CreateElement("Unit");
                    sub.AppendChild(doc.CreateTextNode(variable.Unit));
                    varNode.AppendChild(sub);

                    sub = doc.CreateElement("Comment");
                    sub.AppendChild(doc.CreateTextNode(variable.Comment));
                    varNode.AppendChild(sub);

                    variablesInfo.AppendChild(varNode);
                }
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, IEnumerable<string> names, uint version = 1)
        {
            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("Data Type");
                dt.Columns.Add("Unit");
                dt.Columns.Add("Comment");
                foreach (var n in names)
                {
                    var variable = __variables[n];
                    dt.Rows.Add(variable.Name, variable.Type, variable.Unit, variable.Comment);
                }
                int rows = sheet.InsertDataTable(dt, true, 1, 1, false);
                sheet.Range[1, 1, 1, dt.Columns.Count].Style = title;
                if (rows > 0) sheet.Range[2, 1, 1 + rows, dt.Columns.Count].Style = content;
                sheet.AllocatedRange.AutoFitColumns();
                sheet.Range[2, 1].FreezePanes();
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        protected void Remove(Variable variable, bool force = false)
        {
            if (!force && _subscribers.ContainsKey(variable))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_BE_SUBSCRIBED);

            if (__variables.Remove(variable.Name) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_UNFOUND);
        }

        public Variable Remove(string name, bool force = false)
        {
            if (__variables.TryGetValue(name, out var variable) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_UNFOUND);
            Remove(variable, force);
            return variable;
        }

        protected void Add(Variable v)
        {
            if (__variables.TryAdd(v.Name, v) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_VARIABLE);
        }

        public Variable Add(string name, string dataTypeName, string unit, string comment)
        {
            var v = InspectVariable(name, dataTypeName, unit, comment);
            __variables[v.Name] = v;
            return v;
        }

        protected void Replace(Variable origin, Variable v)
        {
            if (_subscribers.ContainsKey(origin) && origin.Type != v.Type)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_BE_SUBSCRIBED);
            if (origin.Name != v.Name && __variables.ContainsKey(v.Name))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_VARIABLE);
            if (__variables.Remove(origin.Name) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_UNFOUND);
            __variables.Add(v.Name, v);
            if (_subscribers.TryGetValue(origin, out var res) == true)
            {
                List<ISubscriber<Variable>> subs = new List<ISubscriber<Variable>>(res);
                try
                { 
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        subs[i] = subs[i].DependencyChanged(origin, v);
                    }
                }
                catch(LombardiaException)
                {
                    __variables.Remove(v.Name);
                    __variables.Add(origin.Name, origin);
                    throw;
                }
                _subscribers.Remove(origin);
                _subscribers[v] = subs;
            }
        }

        protected void Replace(string name, Variable v)
        {
            if (__variables.TryGetValue(name, out var origin) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.VARIABLE_UNFOUND);
            Replace(origin, v);
        }

        public Variable Replace(string origin, string name, string dataTypeName, string unit, string comment)
        {
            if (__data_type_catalogue.DataTypes.TryGetValue(dataTypeName, out var dataType) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_VARIABLE_DATA_TYPE);
            var v = new Variable { Name = name, Type = dataType, Unit = unit, Comment = comment };
            Replace(origin, v);
            return v;
        }

        public override IReadOnlyList<ISubscriber<Variable>>? CurrentSubscribers(Variable key)
        {
            _subscribers.TryGetValue(key, out var res);
            return res;
        }

        private Dictionary<string, Variable> __load(string dictionary, out byte[] code)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using FileStream stream = File.Open(dictionary, FileMode.Open);
                using (MD5 hash = MD5.Create())
                {
                    code = hash.ComputeHash(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                }
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECVariables");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (SupportedFileFormatVersion < FileFormatVersion)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);

                XmlNode variablesNode = xmlDoc.SelectSingleNode("/AMECVariables");
                return __load_variables(variablesNode);
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

        private Dictionary<string, Variable> __load(Stream sm)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(sm);
                XmlNode rootNode = xmlDoc.SelectSingleNode("/AMECVariables");
                FileFormatVersion = uint.Parse(rootNode.Attributes.GetNamedItem("FormatVersion").Value);

                if (SupportedFileFormatVersion < FileFormatVersion)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.UNSUPPORTED_FILE_FORMAT_VERSION);

                XmlNode variablesNode = xmlDoc.SelectSingleNode("/AMECVariables");
                return __load_variables(variablesNode);
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

        private Dictionary<string, Variable> __load_variables(XmlNode variablesNode)
        {
            try
            {
                __variables.Clear();
                if (variablesNode?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode variableNode in variablesNode.ChildNodes)
                    {
                        if (variableNode.NodeType != XmlNodeType.Element || variableNode.Name != "Variable")
                            continue;

                        string name = null;
                        string dataType = null;
                        string unit = null, comment = null;

                        name = variableNode.SelectSingleNode("Name").FirstChild?.Value;
                        dataType = variableNode.SelectSingleNode("DataType").FirstChild?.Value;
                        unit = variableNode.SelectSingleNode("Unit").FirstChild?.Value;
                        comment = variableNode.SelectSingleNode("Comment").FirstChild?.Value;

                        Add(name, dataType, unit, comment);
                    }
                }
                return __variables;
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

        public bool Equals(VariableDictionary? other)
        {
            bool res = false;
            if(other!= null && other.Variables.Count == this.Variables.Count)
                res = this.Variables.All(p => other.Variables.ContainsKey(p.Key) && other.Variables[p.Key].Equals(this.Variables[p.Key]));
            return res;
        }
    }

    public class Variable : IEquatable<Variable>
    {
        private string __name = "unnamed";
        public string Name
        {
            get { return __name; }
            init
            {
                var name = value?.Trim();
                if (name == null || name.Length == 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_VARIABLE_NAME);
                __name = name;
            }
        }
        public DataType Type { get; init; } = new DataType();
        private string __unit = "N/A";
        public string Unit
        {
            get { return __unit; }
            init
            {
                var unit = value?.Trim();
                if (unit == null ||unit.Length == 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_VARIABLE_UNIT);
                __unit = unit;
            }
        }
        private string __comment = "N/A";
        public string Comment 
        {
            get { return __comment; }
            init
            {
                if (value == null) value = String.Empty;
                __comment = value;
            }
        }

        public bool Equals(Variable? other)
        {
            return other != null && Name == other.Name && Type == other.Type &&
                Unit == other.Unit && Comment == other.Comment;
        }
    }
}
