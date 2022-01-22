using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            __variables = new ObservableCollection<VariableModel>(dic.Variables.Values.Select(v => new VariableModel { Name = v.Name, DataType = v.Type, Unit = v.Unit, Comment = v.Comment }));
            Variables = __variables;
            Modified = false;
            OperatingHistory = history;
            Name = "Variable Dictionary";
        }

        public void Add(VariableModel model, bool log = true)
        {
            __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            __variables.Add(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord(){ Host = this, Operation= Operation.Add, OriginaPos = -1, NewPos = __variables.Count - 1, OriginalValue = null, NewValue = model});
        }

        public VariableModel RemoveAt(int index, bool force = false, bool log = true)
        {
            VariableModel model = __variables[index];
            __variable_dictionary.Remove(model.Name, force);
            __variables.RemoveAt(index);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord(){ Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null });
            return model;
        }

        public void Remove(VariableModel model, bool force = false, bool log = true)
        {
            __variable_dictionary.Remove(model.Name, force);
            int index = __variables.IndexOf(model);
            __variables.Remove(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord(){ Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null });
        }

        public void Insert(int index, VariableModel model, bool log = true)
        {
            if (index > __variables.Count)
                throw new ArgumentOutOfRangeException();
            __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            __variables.Insert(index, model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord(){ Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, NewValue = model });
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
            __variable_dictionary.Replace(original.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            __variables[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = original, NewValue = newModel });
            //Notify others here
            SubscriberObjects?.UpdateVariable(original.Name);
        }

        public void Replace(VariableModel originalModel, VariableModel newModel, bool log = true)
        {
            __variable_dictionary.Replace(originalModel.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            int index = __variables.IndexOf(originalModel);
            __variables[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(new OperatingRecord(){ Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = originalModel, NewValue = newModel });
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
                    Remove(r.NewValue as VariableModel, false, false);
                    break;
                case Operation.Move:
                    Remove(r.NewValue as VariableModel, true, false);
                    Insert(r.OriginaPos, r.OriginalValue as VariableModel, false);
                    break;
                case Operation.Remove:
                    if (r.OriginaPos == __variables.Count)
                        Add(r.OriginalValue as VariableModel, false);
                    else
                        Insert(r.OriginaPos, r.OriginalValue as VariableModel, false);
                    break;
                case Operation.Insert:
                    Remove(r.NewValue as VariableModel, false, false);
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
                    Remove(r.OriginalValue as VariableModel, true, false);
                    Insert(r.NewPos, r.NewValue as VariableModel, false);
                    break;
                case Operation.Remove:
                    Remove(r.OriginalValue as VariableModel, false, false);
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

    public class VariableModel : IEquatable<VariableModel>
    {
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
    }
}
