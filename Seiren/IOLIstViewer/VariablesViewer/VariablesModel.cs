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
        public VariablesModel(VariableDictionary dic, DataTypeCatalogue dtc)
        {
            __variable_dictionary = dic;
            DataTypeCatalogue = dtc;
            __variables = new ObservableCollection<VariableModel>(dic.Variables.Values.Select(v => new VariableModel { Name = v.Name, DataType = v.Type, Unit = v.Unit, Comment = v.Comment }));
            Variables = __variables;
            Modified = false;
        }

        public void Add(VariableModel model)
        {
            __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            __variables.Add(model);
            Modified = true;
        }

        public VariableModel RemoveAt(int index, bool force = false)
        {
            VariableModel model = __variables[index];
            __variable_dictionary.Remove(model.Name, force);
            __variables.RemoveAt(index);
            Modified = true;
            return model;
        }

        public void Remove(VariableModel model, bool force = false)
        {
            __variable_dictionary.Remove(model.Name, force);
            __variables.Remove(model);
            Modified = true;
        }

        public void Insert(int index, VariableModel model)
        {
            if (index > __variables.Count)
                throw new ArgumentOutOfRangeException();
            __variable_dictionary.Add(model.Name, model.DataType.Name, model.Unit, model.Comment);
            __variables.Insert(index, model);
            Modified = true;
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

        public void Replace(int index, VariableModel newModel)
        {
            VariableModel original = __variables[index];
            __variable_dictionary.Replace(original.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            __variables[index] = newModel;
            Modified = true;
            //Notify others here
            SubscriberObjects?.UpdateVariable(original.Name);
        }

        public void Replace(VariableModel originalModel, VariableModel newModel)
        {
            __variable_dictionary.Replace(originalModel.Name, newModel.Name, newModel.DataType.Name, newModel.Unit, newModel.Comment);
            __variables[__variables.IndexOf(originalModel)] = newModel;
            Modified = true;
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
