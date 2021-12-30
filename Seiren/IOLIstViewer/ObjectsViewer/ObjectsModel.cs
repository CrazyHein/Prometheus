﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class ObjectsModel : RecordContainerModel
    {
        private ObjectDictionary __object_dictionary;
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


        public bool SubsModified { set { Modified = value; } }

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
        }
        public ObjectsModel(ObjectDictionary od, VariableDictionary vd, ControllerConfiguration cmc)
        {
            __object_dictionary = od;
            Variables = vd;
            ControllerConfiguration = cmc;
            __objects = new ObservableCollection<ObjectModel>(od.ProcessObjects.Values.Select(o => new ObjectModel(o)));
            Objects = __objects;
            Modified = false;
        }

        protected ProcessObject _create_process_object(ObjectModel model)
        {
            Variable v = __object_dictionary.InspectVariable(model.VariableName);
            DeviceBinding b = null;
            if (model.EnableBinding)
                b = __object_dictionary.InspectDeviceBinding(model.BindingDeviceName, model.BindingChannelName, model.BindingChannelIndex);
            ValueRange r = null;
            if (model.EnableValueRange)
                r = new ValueRange(model.ValueRangeUp, model.ValueRangeDown);
            ValueConverter c = null;
            if (model.EnableValueConverter)
                c = new ValueConverter(model.ValueConverterUp, model.ValueConverterDown);
            return new ProcessObject{ Index = model.Index, Variable = v, Binding = b, Range = r, Converter = c };
        }

        protected (uint Index, string VariableName, string BindingDeviceName, string BindingChannelName, uint BindingChannelIndex, 
            ValueRange Range, ValueConverter Converter) _process_object_property(ObjectModel model)
        {
            return (model.Index, model.VariableName, model.EnableBinding == true ? model.BindingDeviceName : null,
                model.BindingChannelName, model.BindingChannelIndex,
                model.EnableValueRange == true ? new ValueRange(model.ValueRangeUp, model.ValueRangeDown) : null,
                model.EnableValueConverter == true ? new ValueConverter(model.ValueConverterUp, model.ValueConverterDown) : null);
        }

        public void Add(ObjectModel model)
        {
            var p = _process_object_property(model);
            __object_dictionary.Add(p.Index, p.VariableName, 
                p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                p.Range, p.Converter);
            __objects.Add(model);
            Modified = true;
        }

        public ObjectModel RemoveAt(int index, bool force = false)
        {
            ObjectModel model = __objects[index];
            __object_dictionary.Remove(model.Index, force);
            __objects.RemoveAt(index);
            Modified = true;
            return model;
        }

        public ObjectModel Remove(uint index, bool force = false)
        {
            __object_dictionary.Remove(index, force);
            var o = __objects.First(o => o.Index == index);
            __objects.Remove(o);
            return o;
        }

        public void Remove(ObjectModel model, bool force = false)
        {
            __object_dictionary.Remove(model.Index, force);
            __objects.Remove(model);
            Modified = true;
        }

        public void Insert(int index, ObjectModel model)
        {
            if (index > __objects.Count)
                throw new ArgumentOutOfRangeException();

            var p = _process_object_property(model);

            __object_dictionary.Add(p.Index, p.VariableName,
                p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                p.Range, p.Converter);
            __objects.Insert(index, model);
            Modified = true;
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

        public void Replace(int index, ObjectModel newModel)
        {
            ObjectModel original = __objects[index];
            var p = _process_object_property(newModel);
            __object_dictionary.Replace(original.Index, 
                p.Index, p.VariableName,
                p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                p.Range, p.Converter);
            __objects[index] = newModel;
            Modified = true;
            //Notify others here
            if(original.VariableDataType == "BIT")
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

        public void Replace(ObjectModel originalModel, ObjectModel newModel)
        {
            var p = _process_object_property(newModel);
            __object_dictionary.Replace(originalModel.Index, 
                p.Index, p.VariableName,
                p.BindingDeviceName, p.BindingChannelName, p.BindingChannelIndex,
                p.Range, p.Converter);
            __objects[__objects.IndexOf(originalModel)] = newModel;
            Modified = true;
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
            for(int i = 0; i < __objects.Count; ++i)
            {
                if (__objects[i].VariableName == origin)
                {
                    __objects[i] = new ObjectModel(__object_dictionary.ProcessObjects[__objects[i].Index]);
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
                    __objects[i] = new ObjectModel(__object_dictionary.ProcessObjects[__objects[i].Index]);
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
    }

    public class ObjectModel : INotifyPropertyChanged, IEquatable<ObjectModel>
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

        public ObjectModel(ProcessObject o)
        {
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
        public uint BindingChannelIndex { get; set; }
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
    }
}