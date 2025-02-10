using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class VariablesModel : RecordContainerModel, IDeSerializableRecordModel<VariableModel>
    {
        private VariableDictionary __variable_dictionary;
        public DataTypeCatalogue DataTypeCatalogue { get; private set; }
        private ObservableCollection<VariableModel> __variables;
        public IReadOnlyList<VariableModel> Variables { get; private set; }
        public ObjectsModel? SubscriberObjects { get; set; }
        public VariablesModel(VariableDictionary dic, DataTypeCatalogue dtc, OperatingHistory history)
        {
            __variable_dictionary = dic;
            DataTypeCatalogue = dtc;
            __variables = new ObservableCollection<VariableModel>(dic.Variables.Values.Select(v => new VariableModel { Name = v.Name, DataType = v.Type, Unit = v.Unit, Comment = v.Comment, Unused = dic.IsUnused(v.Name) }));
            Variables = __variables;
            Modified = false;
            OperatingHistory = history;
            Name = "Variable Dictionary";
        }

        public void Add(VariableModel model, bool log = true)
        {
            var op = new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __variables.Count, OriginalValue = null, NewValue = model };
            try
            {
                __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables.Add(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
        }

        public VariableModel RemoveAt(int index, bool log = true)
        {
            VariableModel model = __variables[index];
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __variable_dictionary.Remove(model.Name);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables.RemoveAt(index);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            return model;
        }

        public void Remove(VariableModel model, bool log = true)
        {
            int index = __variables.IndexOf(model);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __variable_dictionary.Remove(model.Name);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables.Remove(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
        }

        public bool IsUnused(string name)
        {
            return __variable_dictionary.IsUnused(name);
        }

        public void ReEvaluate(string name)
        {
            __variables.First(o => o.Name == name).Unused = __variable_dictionary.IsUnused(name);
        }

        public void Insert(int index, VariableModel model, bool log = true)
        {
            if (index > __variables.Count)
                throw new ArgumentOutOfRangeException();

            var op = new OperatingRecord() { Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, NewValue = model };
            try
            {
                __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables.Insert(index, model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
        }

        public void Move(int srcIndex, int dstIndex, bool log = true)
        {
            if (srcIndex >= __variables.Count || dstIndex >= __variables.Count || srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();
            var temp = __variables[srcIndex];
            __variables.RemoveAt(srcIndex);
            __variables.Insert(dstIndex, temp);
            Modified = true;
            var op = new OperatingRecord() { Host = this, Operation = Operation.Move, OriginaPos = srcIndex, NewPos = dstIndex, OriginalValue = temp, NewValue = temp };
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
        }



        public int IndexOf(VariableModel model)
        {
            return __variables.IndexOf(model);
        }

        public bool Contains(VariableModel v)
        {
            return __variable_dictionary.Variables.ContainsKey(v.Name);
        }

        public bool Contains(string name)
        {
            return __variable_dictionary.Variables.ContainsKey(name);
        }

        public void Replace(int index, VariableModel newModel, bool log = true)
        {
            VariableModel original = __variables[index];
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = original, NewValue = newModel };
            try
            {
                __variable_dictionary.Replace(original.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            //Notify others here
            SubscriberObjects?.UpdateVariable(original.Name);
        }

        public void Replace(VariableModel originalModel, VariableModel newModel, bool log = true)
        {
            int index = __variables.IndexOf(originalModel);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = originalModel, NewValue = newModel };
            try
            {
                __variable_dictionary.Replace(originalModel.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __variables[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            //Notify others here
            SubscriberObjects?.UpdateVariable(originalModel.Name);
        }

        public IEnumerable<string> VariableNames
        {
            get { return __variables.Select(v => v.Name); }
        }

        public void Save(string path)
        {
            __variable_dictionary.Save(path, VariableNames);
            Modified = false;
        }

        public override void Undo(OperatingRecord r)
        {
            switch(r.Operation)
            {
                case Operation.Add:
                    Remove(r.NewValue as VariableModel, false);
                    break;
                case Operation.Move:
                    Move(r.NewPos, r.OriginaPos, false);
                    break;
                case Operation.Remove:
                    if (r.OriginaPos == __variables.Count)
                        Add(r.OriginalValue as VariableModel, false);
                    else
                        Insert(r.OriginaPos, r.OriginalValue as VariableModel, false);
                    break;
                case Operation.Insert:
                    Remove(r.NewValue as VariableModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.NewValue as VariableModel, r.OriginalValue as VariableModel, false);
                    break;
            }
        }

        public override void Redo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Add(r.NewValue as VariableModel, false);
                    break;
                case Operation.Move:
                    Move(r.OriginaPos, r.NewPos, false);
                    break;
                case Operation.Remove:
                    Remove(r.OriginalValue as VariableModel, false);
                    break;
                case Operation.Insert:
                    Insert(r.NewPos, r.NewValue as VariableModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.OriginalValue as VariableModel, r.NewValue as VariableModel, false);
                    break;
            }
        }

        public VariableModel? FromXml(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Element && node.Name == typeof(VariableModel).Name)
                {
                    VariableModel v = new VariableModel();
                    v.Unused = true;
                    v.Name = node.SelectSingleNode("Name").FirstChild.Value;
                    v.DataType = DataTypeCatalogue.DataTypes[node.SelectSingleNode("DataType").FirstChild.Value];
                    v.Unit = node.SelectSingleNode("Unit").FirstChild?.Value;
                    v.Comment = node.SelectSingleNode("Comment").FirstChild?.Value;
                    return v;
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class VariableModel : IEquatable<VariableModel>, INotifyPropertyChanged, ISerializableRecordModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool __unused = true;
        public bool Unused
        {
            get { return __unused; }
            set { __unused = value; _notify_property_changed(); }
        }

        private string __name = "unnamed";
        public string Name { get { return __name; } set { __name = value.Trim(); } } 
        public DataType DataType { get; set; } = new DataType();

        private string __unit = "N/A";
        public string Unit { get { return __unit; } set { __unit = value.Trim(); } }
        public string Comment { get; set; } = "N/A";

        public bool Equals(VariableModel? other)
        {
            return Name == other.Name && DataType == other.DataType && Unit == other.Unit && Comment == other.Comment;
        }

        public override string ToString()
        {
            return String.Format("{{ Name = {0} ; Data Type = {1} ; Unit = {2} ; Comment = {3} }}", Name, DataType, Unit, Comment);
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement variableModel = doc.CreateElement(typeof(VariableModel).Name);

            XmlElement sub = doc.CreateElement("Name");
            sub.AppendChild(doc.CreateTextNode(Name));
            variableModel.AppendChild(sub);

            sub = doc.CreateElement("DataType");
            sub.AppendChild(doc.CreateTextNode(DataType.Name));
            variableModel.AppendChild(sub);

            sub = doc.CreateElement("Unit");
            sub.AppendChild(doc.CreateTextNode(Unit));
            variableModel.AppendChild(sub);

            sub = doc.CreateElement("Comment");
            sub.AppendChild(doc.CreateTextNode(Comment));
            variableModel.AppendChild(sub);

            return variableModel;
        }
    }
}
