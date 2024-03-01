using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class VariablesModel : RecordContainerModel
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
    }

    public class VariableModel : IEquatable<VariableModel>, INotifyPropertyChanged
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

        public string Name { get; set; } = "unnamed";
        public DataType DataType { get; set; } = new DataType();
        public string Unit { get; set; } = "N/A";
        public string Comment { get; set; } = "N/A";

        public bool Equals(VariableModel? other)
        {
            return Name == other.Name && DataType == other.DataType && Unit == other.Unit && Comment == other.Comment;
        }

        public override string ToString()
        {
            return String.Format("{{ Name = {0} ; Data Type = {1} ; Unit = {2} ; Comment = {3} }}", Name, DataType, Unit, Comment);
        }

        public byte[] ToBinary()
        {
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = false });
            writer.WriteStartObject();
            writer.WriteString("Name", Name);
            writer.WriteString("DataType", DataType.Name);
            writer.WriteString("Unit", Unit);
            writer.WriteString("Comment", Comment);
            writer.WriteEndObject();
            writer.Flush();
            //Encoding.UTF8.GetString(ms.ToArray());

            return ms.ToArray();
        }

        public static VariableModel? FromBinary(byte[] array, DataTypeCatalogue types)
        {
            try
            {
                var reader = new Utf8JsonReader(array, new JsonReaderOptions() { CommentHandling = JsonCommentHandling.Skip });
                VariableModel v = new VariableModel();
                v.Unused = true;
                while (reader.Read())
                {
                    switch (reader.TokenType, reader.CurrentDepth)
                    {
                        case (JsonTokenType.PropertyName, 1):
                            switch (reader.GetString())
                            {
                                case "Name":
                                    reader.Read();
                                    if (reader.TokenType == JsonTokenType.String)
                                        v.Name = reader.GetString();
                                    else
                                        throw new ArgumentException("Invalid variable name");
                                    break;
                                case "DataType":
                                    reader.Read();
                                    if (reader.TokenType == JsonTokenType.String)
                                    {
                                        var typename = reader.GetString();
                                        if (types.DataTypes.ContainsKey(typename))
                                            v.DataType = types.DataTypes[typename];
                                        else
                                            throw new ArgumentException("Invalid variable data type");
                                    }
                                    else
                                        throw new ArgumentException("Invalid variable data type");
                                    break;
                                case "Unit":
                                    reader.Read();
                                    if (reader.TokenType == JsonTokenType.String)
                                        v.Unit = reader.GetString();
                                    else
                                        throw new ArgumentException("Invalid variable unit");
                                    break;
                                case "Comment":
                                    reader.Read();
                                    if (reader.TokenType == JsonTokenType.String)
                                        v.Comment = reader.GetString();
                                    else
                                        throw new ArgumentException("Invalid variable comment");
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                return v;
            }
            catch
            { 
                return null; 
            }
        }
    }
}
