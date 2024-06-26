﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class ObjectsModel : RecordContainerModel, IDeSerializableRecordModel<ObjectModel>
    {
        private ObjectDictionary __object_dictionary;
        private VariablesModel __varialble_model_source;
        private ControllerConfigurationModel __controller_configuration_model_source;
        public VariableDictionary Variables { get; private set; }
        public ControllerConfiguration ControllerConfiguration { get; private set; }
        private ObservableCollection<ObjectModel> __objects;
        public IReadOnlyList<ObjectModel> Objects { get; private set; }
        public ProcessDataImageModel? TxDiagnosticObjects { get; set; }
        public ProcessDataImageModel? TxBitObjects { get; set; }
        public ProcessDataImageModel? TxBlockObjects { get; set; }
        public ProcessDataImageModel? RxControlObjects { get; set; }
        public ProcessDataImageModel? RxBitObjects { get; set; }
        public ProcessDataImageModel? RxBlockObjects { get; set; }
        public InterlockCollectionModel? InterlockLogics { get; set; }

        private bool __subs_modified = false;
        public bool SubsModified
        {
            get => __subs_modified;
            set
            {
                __subs_modified = value;
                OnPropertyChanged("SubsModified");
                OnPropertyChanged("ContentModified");
            }
        }

        public new bool Modified
        {
            get { return base.Modified; }
            set { base.Modified = value; OnPropertyChanged("ContentModified"); }
        }

        public bool ContentModified { get { return Modified || SubsModified; } }

        public override void CommitChanges()
        {
            Modified = false;
            TxDiagnosticObjects?.CommitChanges();
            TxBitObjects?.CommitChanges();
            TxBlockObjects?.CommitChanges();
            RxControlObjects?.CommitChanges();
            RxBitObjects?.CommitChanges();
            RxBlockObjects?.CommitChanges();
            RxBlockObjects?.CommitChanges();
            InterlockLogics?.CommitChanges();
            SubsModified = false;
        }
        public ObjectsModel(ObjectDictionary od, VariableDictionary vd, ControllerConfiguration cmc, VariablesModel vmodels, ControllerConfigurationModel cmodels, OperatingHistory history)
        {
            __object_dictionary = od;
            Variables = vd;
            ControllerConfiguration = cmc;
            __varialble_model_source = vmodels;
            __controller_configuration_model_source = cmodels;
            __objects = new ObservableCollection<ObjectModel>(od.ProcessObjects.Values.Select(o => new ObjectModel(o, od.IsUnused(o.Index))));
            Objects = __objects;
            Modified = false;
            OperatingHistory = history;
            Name = "Object Dictionary";
        }

        public VariablesModel VariableModelSource { get { return __varialble_model_source; } }
        public ControllerConfigurationModel ControllerConfigurationModel { get { return __controller_configuration_model_source; } }

        protected (uint Index, string VariableName, string BindingDeviceName, string BindingChannelName, uint BindingChannelIndex,
            ValueRange Range, ValueConverter Converter) _process_object_property(ObjectModel model)
        {
            return (model.Index, model.VariableName, model.EnableBinding == true ? model.BindingDeviceName : null,
                model.BindingChannelName, model.BindingChannelIndex,
                model.EnableValueRange == true ? new ValueRange(model.ValueRangeUp, model.ValueRangeDown) : null,
                model.EnableValueConverter == true ? new ValueConverter(model.ValueConverterUp, model.ValueConverterDown) : null);
        }

        public void Add(ObjectModel model, bool log = true)
        {
            var op = new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __objects.Count, OriginalValue = null, NewValue = model };
            var p = _process_object_property(model);
            try
            {
                __object_dictionary.Add(p.Index, p.VariableName,
                    p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                    p.Range, p.Converter);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects.Add(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            __varialble_model_source.ReEvaluate(model.VariableName);
            if (model.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(model.BindingDeviceName);
        }

        public ObjectModel RemoveAt(int index, bool log = true)
        {
            ObjectModel model = __objects[index];
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __object_dictionary.Remove(model.Index);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects.RemoveAt(index);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            __varialble_model_source.ReEvaluate(model.VariableName);
            if (model.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(model.BindingDeviceName);
            return model;
        }

        public ObjectModel Remove(uint index, bool log = true)
        {
            var o = __objects.First(o => o.Index == index);
            int i = __objects.IndexOf(o);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = i, NewPos = -1, OriginalValue = o, NewValue = null };
            try
            {
                __object_dictionary.Remove(index);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects.Remove(o);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            __varialble_model_source.ReEvaluate(o.VariableName);
            if (o.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(o.BindingDeviceName);
            return o;
        }

        public void Remove(ObjectModel model, bool log = true)
        {
            int index = __objects.IndexOf(model);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = model, NewValue = null };
            try
            {
                __object_dictionary.Remove(model.Index);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects.Remove(model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            __varialble_model_source.ReEvaluate(model.VariableName);
            if (model.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(model.BindingDeviceName);
        }

        public bool IsUnused(uint index)
        {
            return __object_dictionary.IsUnused(index);
        }

        public void ReEvaluate(uint index)
        {
            __objects.First(o => o.Index == index).Unused = __object_dictionary.IsUnused(index);
        }

        public void Insert(int index, ObjectModel model, bool log = true)
        {
            if (index > __objects.Count)
                throw new ArgumentOutOfRangeException();

            var op = new OperatingRecord() { Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, NewValue = model };
            var p = _process_object_property(model);
            try
            {
                __object_dictionary.Add(p.Index, p.VariableName,
                    p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                    p.Range, p.Converter);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects.Insert(index, model);
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
            __varialble_model_source.ReEvaluate(model.VariableName);
            if (model.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(model.BindingDeviceName);
        }

        public void Move(int srcIndex, int dstIndex, bool log = true)
        {
            if (srcIndex >= __objects.Count || dstIndex >= __objects.Count || srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();
            var temp = __objects[srcIndex];
            __objects.RemoveAt(srcIndex);
            __objects.Insert(dstIndex, temp);
            Modified = true;
            var op = new OperatingRecord() { Host = this, Operation = Operation.Move, OriginaPos = srcIndex, NewPos = dstIndex, OriginalValue = temp, NewValue = temp };
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);
        }

        public int IndexOf(ObjectModel model)
        {
            return __objects.IndexOf(model);
        }

        public bool Contains(ObjectModel o)
        {
            return __object_dictionary.ProcessObjects.ContainsKey(o.Index);
        }

        public bool Contains(uint index)
        {
            return __object_dictionary.ProcessObjects.ContainsKey(index);
        }

        public void Replace(int index, ObjectModel newModel, bool log = true)
        {
            ObjectModel original = __objects[index];
            var p = _process_object_property(newModel);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = original, NewValue = newModel };
            try
            {
                __object_dictionary.Replace(original.Index,
                    p.Index, p.VariableName,
                    p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                    p.Range, p.Converter);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);

            __varialble_model_source.ReEvaluate(original.VariableName);
            if (original.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(original.BindingDeviceName);
            __varialble_model_source.ReEvaluate(newModel.VariableName);
            if (newModel.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(newModel.BindingDeviceName);
            //Notify others here
            if (original.VariableDataType == "BIT")
            {
                if (TxBitObjects?.UpdateProcessData(original.Index, newModel.Index) == true)
                    return;
                else
                    RxBitObjects?.UpdateProcessData(original.Index, newModel.Index);
            }
            else
            {
                if (TxDiagnosticObjects?.UpdateProcessData(original.Index, newModel.Index) == true)
                    return;
                else if (TxBlockObjects?.UpdateProcessData(original.Index, newModel.Index) == true)
                    return;
                else if (RxControlObjects?.UpdateProcessData(original.Index, newModel.Index) == true)
                    return;
                else
                    RxBlockObjects?.UpdateProcessData(original.Index, newModel.Index);
            }
        }

        public void Replace(ObjectModel originalModel, ObjectModel newModel, bool log = true)
        {
            var p = _process_object_property(newModel);
            int index = __objects.IndexOf(originalModel);
            var op = new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, OriginalValue = originalModel, NewValue = newModel };

            try
            {
                __object_dictionary.Replace(originalModel.Index,
                p.Index, p.VariableName,
                p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                p.Range, p.Converter);
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, op);
                throw;
            }
            __objects[index] = newModel;
            Modified = true;
            if (log && OperatingHistory != null)
                OperatingHistory?.PushOperatingRecord(op);
            DebugConsole.WriteOperatingRecord(op);

            __varialble_model_source.ReEvaluate(originalModel.VariableName);
            if (originalModel.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(originalModel.BindingDeviceName);
            __varialble_model_source.ReEvaluate(newModel.VariableName);
            if (newModel.EnableBinding)
                __controller_configuration_model_source.ReEvaluate(newModel.BindingDeviceName);
            //Notify others here
            //TxDiagnosticObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            //TxBitObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            //TxBlockObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            //RxControlObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            //RxBitObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            //RxBlockObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            if (originalModel.VariableDataType == "BIT")
            {
                if (TxBitObjects?.UpdateProcessData(originalModel.Index, newModel.Index) == true)
                    return;
                else
                    RxBitObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            }
            else
            {
                if (TxDiagnosticObjects?.UpdateProcessData(originalModel.Index, newModel.Index) == true)
                    return;
                else if (TxBlockObjects?.UpdateProcessData(originalModel.Index, newModel.Index) == true)
                    return;
                else if (RxControlObjects?.UpdateProcessData(originalModel.Index, newModel.Index) == true)
                    return;
                else
                    RxBlockObjects?.UpdateProcessData(originalModel.Index, newModel.Index);
            }
        }

        public IEnumerable<uint> ObjectIndexes
        {
            get { return __objects.Select(o => o.Index); }
        }

        public void Save(XmlDocument doc, XmlElement root)
        {
            __object_dictionary.Save(doc, ObjectIndexes, root);
            Modified = false;
        }
        public void UpdateVariable(string origin)
        {
            for (int i = 0; i < __objects.Count; ++i)
            {
                if (__objects[i].VariableName == origin)
                {
                    __objects[i] = new ObjectModel(__object_dictionary.ProcessObjects[__objects[i].Index], __objects[i].Unused);
                    Modified = true;
                }
            }
            TxDiagnosticObjects?.UpdateVariable(origin);
            TxBitObjects?.UpdateVariable(origin);
            TxBlockObjects?.UpdateVariable(origin);
            RxControlObjects?.UpdateVariable(origin);
            RxBitObjects?.UpdateVariable(origin);
            RxBlockObjects?.UpdateVariable(origin);
        }

        public void UpdateBinding(string origin)
        {
            for (int i = 0; i < __objects.Count; ++i)
            {
                if (__objects[i].EnableBinding && __objects[i].BindingDeviceName == origin)
                {
                    __objects[i] = new ObjectModel(__object_dictionary.ProcessObjects[__objects[i].Index], __objects[i].Unused);
                    Modified = true;
                }
            }
            TxDiagnosticObjects?.UpdateBinding(origin);
            TxBitObjects?.UpdateBinding(origin);
            TxBlockObjects?.UpdateBinding(origin);
            RxControlObjects?.UpdateBinding(origin);
            RxBitObjects?.UpdateBinding(origin);
            RxBlockObjects?.UpdateBinding(origin);
        }

        public override void Undo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Remove(r.NewValue as ObjectModel, false);
                    break;
                case Operation.Move:
                    Move(r.NewPos, r.OriginaPos, false);
                    break;
                case Operation.Remove:
                    if (r.OriginaPos == __objects.Count)
                        Add(r.OriginalValue as ObjectModel, false);
                    else
                        Insert(r.OriginaPos, r.OriginalValue as ObjectModel, false);
                    break;
                case Operation.Insert:
                    Remove(r.NewValue as ObjectModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.NewValue as ObjectModel, r.OriginalValue as ObjectModel, false);
                    break;
            }
        }

        public override void Redo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Add(r.NewValue as ObjectModel, false);
                    break;
                case Operation.Move:
                    Move(r.OriginaPos, r.NewPos, false);
                    break;
                case Operation.Remove:
                    Remove(r.OriginalValue as ObjectModel, false);
                    break;
                case Operation.Insert:
                    Insert(r.NewPos, r.NewValue as ObjectModel, false);
                    break;
                case Operation.Replace:
                    Replace(r.OriginalValue as ObjectModel, r.NewValue as ObjectModel, false);
                    break;
            }
        }

        public ObjectModel? FromXml(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Element && node.Name == typeof(ObjectModel).Name)
                {
                    ObjectModel o = new ObjectModel();
                    o.Unused = true;

                    o.VariableName = node.SelectSingleNode("VariableName").FirstChild.Value;
                    o.EnableBinding = node.SelectSingleNode("EnableBinding") != null;
                    o.EnableValueRange = node.SelectSingleNode("EnableValueRange") != null;
                    o.EnableValueConverter = node.SelectSingleNode("EnableValueConverter") != null;

                    if (Variables.Variables.ContainsKey(o.VariableName))
                    {
                        o.VariableDataType = Variables.Variables[o.VariableName].Type.Name;
                        o.VariableUnit = Variables.Variables[o.VariableName].Unit;
                        o.VariableComment = Variables.Variables[o.VariableName].Comment;
                    }
                    else
                        return null;

                    XmlNode sub = null;
                    if(o.EnableBinding)
                    {
                        sub = node.SelectSingleNode("EnableBinding");
                        o.BindingDeviceName = sub.SelectSingleNode("BindingDeviceName").FirstChild.Value;
                        o.BindingChannelName = sub.SelectSingleNode("BindingChannelName").FirstChild.Value;
                        o.BindingChannelIndex = Convert.ToUInt32(sub.SelectSingleNode("BindingChannelIndex").FirstChild.Value);

                        if (ControllerConfiguration.Configurations[o.BindingDeviceName].DeviceModel.RxVariables.ContainsKey(o.BindingChannelName) == false &&
                            ControllerConfiguration.Configurations[o.BindingDeviceName].DeviceModel.TxVariables.ContainsKey(o.BindingChannelName) == false)
                            return null;
                    }

                    if(o.EnableValueRange)
                    {
                        sub = node.SelectSingleNode("EnableValueRange");
                        o.ValueRangeDown = sub.SelectSingleNode("ValueRangeDown").FirstChild.Value;
                        o.ValueRangeUp = sub.SelectSingleNode("ValueRangeUp").FirstChild.Value;
                    }

                    if(o.EnableValueConverter)
                    {
                        sub = node.SelectSingleNode("EnableValueConverter");

                        byte[] value = sub.SelectSingleNode("ValueConverterDown").FirstChild.Value.Split('-').Select(s => Convert.ToByte(s, 16)).ToArray();
                        o.ValueConverterDown = MemoryMarshal.Read<double>(value);

                        value = sub.SelectSingleNode("ValueConverterUp").FirstChild.Value.Split('-').Select(s => Convert.ToByte(s, 16)).ToArray();
                        o.ValueConverterUp = MemoryMarshal.Read<double>(value);
                    }
                    return o;
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

    public class ObjectModel : INotifyPropertyChanged, IEquatable<ObjectModel>, ISerializableRecordModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObjectModel Clone()
        {
            return new ObjectModel()
            {
                Unused = this.Unused,
                Index = this.Index,
                VariableName = this.VariableName,
                VariableDataType = this.VariableDataType,
                VariableUnit = this.VariableUnit,
                VariableComment = this.VariableComment,

                EnableBinding = this.EnableBinding,
                BindingDeviceName = this.BindingDeviceName,
                BindingChannelName = this.BindingChannelName,
                BindingChannelIndex = this.BindingChannelIndex,
                EnableValueRange = this.EnableValueRange,
                ValueRangeDown = this.ValueRangeDown,
                ValueRangeUp = this.ValueRangeUp,
                EnableValueConverter = this.EnableValueConverter,
                ValueConverterDown = this.ValueConverterDown,
                ValueConverterUp = this.ValueConverterUp
            };
        }

        public ObjectModel(ProcessObject o, bool unused)
        {
            __unused = unused;
            __index = o.Index;
            __variable_name = o.Variable.Name;
            __variable_data_type = o.Variable.Type.Name;
            __variable_unit = o.Variable.Unit;
            __variable_comment = o.Variable.Comment;
            if (o.Binding != null)
            {
                EnableBinding = true;
                __binding_device_name = o.Binding.Device.ReferenceName;
                BindingChannelName = o.Binding.ChannelName;
                BindingChannelIndex = o.Binding.ChannelIndex;
            }
            if (o.Range != null)
            {
                EnableValueRange = true;
                ValueRangeUp = o.Range.Up;
                ValueRangeDown = o.Range.Down;
            }
            if (o.Converter != null)
            {
                EnableValueConverter = true;
                ValueConverterUp = o.Converter.UpScale;
                ValueConverterDown = o.Converter.DownScale;
            }
        }

        public ObjectModel()
        { }

        private bool __unused = true;
        public bool Unused
        {
            get { return __unused; }
            set { __unused = value; _notify_property_changed(); }
        }

        private uint __index;
        public uint Index 
        {
            get { return __index; }
            set { __index = value; _notify_property_changed(); } 
        }

        private string __variable_name = "unnamed";
        public string VariableName
        {
            get { return __variable_name; }
            set { __variable_name = value; _notify_property_changed(); }
        }
        private string __variable_data_type = "unnamed";
        public string VariableDataType
        {
            get { return __variable_data_type; }
            set { __variable_data_type = value; _notify_property_changed(); }
        }
        private string __variable_unit = "N/A";
        public string VariableUnit
        {
            get { return __variable_unit; }
            set { __variable_unit = value; _notify_property_changed(); }
        }
        public string __variable_comment = "N/A";
        public string VariableComment
        {
            get { return __variable_comment; }
            set { __variable_comment = value; _notify_property_changed(); }
        }
        public bool EnableBinding { get; set; }
        public string? __binding_device_name;
        public string? BindingDeviceName
        {
            get { return __binding_device_name; }
            set
            {
                __binding_device_name = value;
                _notify_property_changed();
                _notify_property_changed("DeviceBindingInfo");
            }
        }
        public string? BindingChannelName { get; set; }
        private uint __binding_channel_index;
        public uint BindingChannelIndex 
        { 
            get { return __binding_channel_index; }
            set { __binding_channel_index = value;  _notify_property_changed(); } 
        }
        public bool EnableValueRange { get; set; }
        public string? ValueRangeUp { get; set; } = "1000";
        public string? ValueRangeDown { get; set; } = "0";
        public bool EnableValueConverter { get; set; }
        public double ValueConverterUp { get; set; }
        public double ValueConverterDown { get; set; }

        public string DeviceBindingInfo
        {
            get
            {
                if (EnableBinding)
                    return $"{BindingDeviceName} -- [{BindingChannelName} : {BindingChannelIndex}]";
                else
                    return "N/A";
            }
        }
        public string ValueRangeInfo
        {
            get
            {
                if (EnableValueRange)
                    return $"R[{ValueRangeDown} , {ValueRangeUp}]";
                else
                    return "N/A";
            }
        }
        public string ValueConverterInfo
        {
            get
            {
                if (EnableValueConverter)
                    return $"C[{ValueConverterDown} , {ValueConverterUp}]";
                else
                    return "N/A";
            }
        }

        public override string ToString()
        {
            return $"{{Index = 0x{Index:X08} ; Name = {VariableName} ; Data Type = {VariableDataType} ; Unit = {VariableUnit} ; Binding = {DeviceBindingInfo} ; Range = {ValueRangeInfo} ; Converter = {ValueConverterInfo}}}";
        }

        public bool Equals(ObjectModel? other)
        {
            bool binding = EnableBinding ? (EnableBinding == other.EnableBinding && DeviceBindingInfo == other.DeviceBindingInfo) : (EnableBinding == other.EnableBinding);
            bool range = EnableValueRange ? (EnableValueRange == other.EnableValueRange && ValueRangeInfo == other.ValueRangeInfo) : (EnableValueRange == other.EnableValueRange);
            bool converter = EnableValueConverter ? (EnableValueConverter == other.EnableValueConverter && ValueConverterInfo == other.ValueConverterInfo) : (EnableValueConverter == other.EnableValueConverter);
            return Index == other.Index && VariableName == other.VariableName && binding && range && converter;
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement objectModel = doc.CreateElement(typeof(ObjectModel).Name);

            XmlElement sub = doc.CreateElement("VariableName");
            sub.AppendChild(doc.CreateTextNode(VariableName));
            objectModel.AppendChild(sub);

            XmlElement ssub = null;
            if (EnableBinding)
            {
                sub = doc.CreateElement("EnableBinding");
                
                ssub = doc.CreateElement("BindingDeviceName");
                ssub.AppendChild(doc.CreateTextNode(BindingDeviceName));
                sub.AppendChild(ssub);

                ssub = doc.CreateElement("BindingChannelName");
                ssub.AppendChild(doc.CreateTextNode(BindingChannelName));
                sub.AppendChild(ssub);

                ssub = doc.CreateElement("BindingChannelIndex");
                ssub.AppendChild(doc.CreateTextNode(BindingChannelIndex.ToString()));
                sub.AppendChild(ssub);

                objectModel.AppendChild(sub);
            }

            if(EnableValueRange)
            {
                sub = doc.CreateElement("EnableValueRange");
                
                ssub = doc.CreateElement("ValueRangeDown");
                ssub.AppendChild(doc.CreateTextNode(ValueRangeDown));
                sub.AppendChild(ssub);

                ssub = doc.CreateElement("ValueRangeUp");
                ssub.AppendChild(doc.CreateTextNode(ValueRangeUp));
                sub.AppendChild(ssub);

                objectModel.AppendChild(sub);
            }

            if(EnableValueConverter)
            {
                sub = doc.CreateElement("EnableValueConverter");

                byte[] storage = new byte[sizeof(double)];
                double value = ValueConverterDown;
                ssub = doc.CreateElement("ValueConverterDown");
                MemoryMarshal.Write<double>(storage, in value);
                ssub.AppendChild(doc.CreateTextNode(string.Join("-", storage.Select(d => d.ToString("X2")))));
                sub.AppendChild(ssub);

                value = ValueConverterUp;
                ssub = doc.CreateElement("ValueConverterUp");
                MemoryMarshal.Write<double>(storage, in value);
                ssub.AppendChild(doc.CreateTextNode(string.Join("-", storage.Select(d => d.ToString("X2")))));
                sub.AppendChild(ssub);

                objectModel.AppendChild(sub);
            }
            return objectModel;
        }
    }
}
