using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class ObjectDictionary : Publisher<ProcessObject>, IComparable<ObjectDictionary>
    {
        public IReadOnlyDictionary<uint, ProcessObject> ProcessObjects { get; private set; }
        private Dictionary<uint, ProcessObject> __process_objects = new Dictionary<uint, ProcessObject>();
        private VariableDictionary __variable_dictionary;
        private ControllerConfiguration __controller_configuration;

        public ObjectDictionary(VariableDictionary variableDictionary, ControllerConfiguration controllerConfiguration)
        {
            ProcessObjects = __process_objects;
            __variable_dictionary = variableDictionary;
            __controller_configuration = controllerConfiguration;
            ProcessObject.Publisher = this;
        }

        public ObjectDictionary(VariableDictionary variableDictionary, ControllerConfiguration controllerConfiguration, XmlNode objectsNode)
        {
            __variable_dictionary = variableDictionary;
            __controller_configuration = controllerConfiguration;
            ProcessObjects = __load_objects(objectsNode);
            ProcessObject.Publisher = this;
        }

        public ProcessObject InspectProcessObject(uint index, string name, string? deviceName, string? channelName, uint channelIndex, ValueRange? vr, ValueConverter? vc)
        {
            if (__process_objects.ContainsKey(index) == true)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_INDEX);
            var v = InspectVariable(name);
            DeviceBinding b = null;
            if (deviceName != null)
                b = InspectDeviceBinding(deviceName, channelName, channelIndex);
            return new ProcessObject() { Index = index, Variable = v, Binding = b, Range = vr, Converter = vc };
        }

        public Variable InspectVariable(string? name)
        {
            if (name == null || __variable_dictionary.Variables.TryGetValue(name.Trim(), out var variable) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_VARIABLE_NAME);
            return variable;
        }

        public DeviceBinding InspectDeviceBinding(string? deviceName, string? channelName, uint channelIndex)
        {
            if (deviceName == null || channelName == null)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_BINDING_INFO);
            if (__controller_configuration.Configurations.TryGetValue(deviceName.Trim(), out var device) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_BINDING_MODULE_NAME);
            if (device.DeviceModel.RxVariables.ContainsKey(channelName.Trim()) == false && device.DeviceModel.TxVariables.ContainsKey(channelName.Trim()) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_BINDING_CHANNEL_NAME);
            return new DeviceBinding(device, channelName, channelIndex);
        }

        private Dictionary<uint, ProcessObject> __load_objects(XmlNode objectsNode)
        {
            __process_objects.Clear();
            try
            {
                if (objectsNode?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode objectNode in objectsNode.ChildNodes)
                    {
                        if (objectNode.NodeType != XmlNodeType.Element || objectNode.Name != "Object")
                            continue;

                        uint index = Convert.ToUInt32(objectNode.SelectSingleNode("Index").FirstChild.Value, 16);
                        Variable variable = InspectVariable(objectNode.SelectSingleNode("Name").FirstChild.Value);
                        DeviceBinding? binding = __load_device_binding(objectNode["Binding"]);
                        ValueRange? range = __load_value_range(objectNode["Range"]);
                        ValueConverter converter = __load_converter_range(objectNode["Converter"]);

                        if (__process_objects.ContainsKey(index))
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_INDEX);
                        __process_objects[index] = new ProcessObject{ Index = index, Variable = variable, Binding = binding, Range = range, Converter = converter };
                        __variable_dictionary.AddSubscriber(variable, __process_objects[index]);
                        if (binding != null)
                            __controller_configuration.AddSubscriber(binding.Device, __process_objects[index]);
                    }
                }
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }

            return __process_objects;
        }

        private DeviceBinding? __load_device_binding(XmlNode bindingNode)
        {
            string deviceName = null;
            string channelName = null;
            uint channelIndex = 0;
            try
            {
                if (bindingNode?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node in bindingNode.ChildNodes)
                    {
                        if (node.NodeType != XmlNodeType.Element)
                            continue;
                        switch (node.Name)
                        {
                            case "Module":
                                if (deviceName != null) break;
                                deviceName = node.FirstChild.Value.Trim();
                                break;
                            default:
                                if (channelName != null) break;
                                channelName = node.Name.Trim();
                                channelIndex = Convert.ToUInt32(node.FirstChild.Value, 10);
                                break;
                        }
                        if (deviceName != null && channelName != null)
                            break;
                    }
                    return InspectDeviceBinding(deviceName, channelName, channelIndex);
                }
                else
                    return null;
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

        private ValueRange? __load_value_range(XmlNode rangeNode)
        {
            try
            {
                if (rangeNode?.NodeType == XmlNodeType.Element)
                    return new ValueRange(rangeNode.SelectSingleNode("UpLimit").FirstChild.Value,
                        rangeNode.SelectSingleNode("DownLimit").FirstChild.Value);
                else
                    return null;
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

        private ValueConverter? __load_converter_range(XmlNode converterNode)
        {
            double upScale = 0, downScale = 0;
            try
            {
                if (converterNode?.NodeType == XmlNodeType.Element)
                {
                    upScale = Convert.ToDouble(converterNode.SelectSingleNode("UpScale").FirstChild.Value);
                    downScale = Convert.ToDouble(converterNode.SelectSingleNode("DownScale").FirstChild.Value);
                    return new ValueConverter(upScale, downScale);
                }
                else
                    return null;
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

        protected void Add(ProcessObject o)
        {
            if (__process_objects.TryAdd(o.Index, o) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_INDEX);
            __variable_dictionary.AddSubscriber(o.Variable, o);
            if (o.Binding != null)
                __controller_configuration.AddSubscriber(o.Binding.Device, o);
        }

        public ProcessObject Add(uint index, string name, string? deviceName, string? channelName, uint channelIndex, ValueRange? vr, ValueConverter? vc)
        {
            var o = InspectProcessObject(index, name, deviceName, channelName, channelIndex, vr, vc);
            __process_objects[o.Index] = o;
            __variable_dictionary.AddSubscriber(o.Variable, o);
            if (o.Binding != null)
                __controller_configuration.AddSubscriber(o.Binding.Device, o);
            return o;
        }

        public void Remove(ProcessObject o)
        {
            if (_subscribers.ContainsKey(o))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_BE_SUBSCRIBED);

            if (__process_objects.Remove(o.Index) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_UNFOUND);

            __variable_dictionary.RemoveSubscriber(o.Variable, o);
            if (o.Binding != null)
                __controller_configuration.RemoveSubscriber(o.Binding.Device, o);
        }

        protected void Replace(ProcessObject origin, ProcessObject o, ReplaceMode mode = ReplaceMode.Full)
        {
            if (_subscribers.ContainsKey(origin) && origin.Variable.Type != o.Variable.Type)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_BE_SUBSCRIBED);

            if (origin.Index != o.Index && __process_objects.ContainsKey(o.Index))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_INDEX);

            if (__process_objects.Remove(origin.Index) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_UNFOUND);
            __process_objects.Add(o.Index, o);


            if (_subscribers.TryGetValue(origin, out var res) == true)
            {
                List<ISubscriber<ProcessObject>> subs = new List<ISubscriber<ProcessObject>>(res);
                try
                {
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        subs[i] = subs[i].DependencyChanged(origin, o);
                    }
                }
                catch (LombardiaException)
                {
                    __process_objects.Remove(o.Index);
                    __process_objects.Add(origin.Index, origin);
                    throw;
                }
                _subscribers.Remove(origin);
                _subscribers[o] = subs;
            }
            if(mode == ReplaceMode.Variable)
            {
                if (origin.Binding != null) __controller_configuration.RemoveSubscriber(origin.Binding.Device, origin);
                if (o.Binding != null) __controller_configuration.AddSubscriber(o.Binding.Device, o);
            }
            else if(mode == ReplaceMode.DeviceConfiguration)
            {
                __variable_dictionary.RemoveSubscriber(origin.Variable, origin);
                __variable_dictionary.AddSubscriber(o.Variable, o);
            }
            else if (mode == ReplaceMode.Full)
            {
                __variable_dictionary.RemoveSubscriber(origin.Variable, origin);
                __variable_dictionary.AddSubscriber(o.Variable, o);
                if (origin.Binding != null) __controller_configuration.RemoveSubscriber(origin.Binding.Device, origin);
                if (o.Binding != null) __controller_configuration.AddSubscriber(o.Binding.Device, o);
            }
        }

        public ProcessObject Remove(uint index)
        {
            if (__process_objects.TryGetValue(index, out var o) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_UNFOUND);
            Remove(o);
            return o;
        }

        public bool IsUnused(uint index)
        {
            return __process_objects.TryGetValue(index, out var o) == false ? true : _subscribers.ContainsKey(o) == false;
        }

        protected void Replace(uint index, ProcessObject o, ReplaceMode mode = ReplaceMode.Full)
        {
            if (__process_objects.TryGetValue(index, out var origin) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.OBJECT_UNFOUND);
            Replace(origin, o, mode);
        }

        public ProcessObject Replace(uint origin, 
            uint index, string name, string? deviceName, string? channelName, uint channelIndex, ValueRange? vr, ValueConverter? vc, 
            ReplaceMode mode = ReplaceMode.Full)
        {
            var v = InspectVariable(name);
            DeviceBinding b = null;
            if (deviceName != null)
                b = InspectDeviceBinding(deviceName, channelName, channelIndex);
            ProcessObject o = new ProcessObject() { Index = index, Variable = v, Binding = b, Range = vr, Converter = vc };
            Replace(origin, o, mode);
            return o;
        }

        public void Save(XmlDocument doc, IEnumerable<uint> indexes, XmlElement objects, uint version = 1)
        {
            try
            {
                foreach(var index in indexes)
                {
                    XmlElement objectNode = doc.CreateElement("Object");
                    var o = __process_objects[index];

                    XmlElement sub = doc.CreateElement("Index");
                    sub.AppendChild(doc.CreateTextNode($"0x{o.Index:X8}"));
                    objectNode.AppendChild(sub);

                    sub = doc.CreateElement("Name");
                    sub.AppendChild(doc.CreateTextNode(o.Variable.Name));
                    objectNode.AppendChild(sub);

                    XmlElement inner;
                    if (o.Binding != null)
                    {
                        inner = doc.CreateElement("Binding");
                        sub = doc.CreateElement("Module");
                        sub.AppendChild(doc.CreateTextNode(o.Binding.Device.ReferenceName));
                        inner.AppendChild(sub);
                        sub = doc.CreateElement(o.Binding.ChannelName);
                        sub.AppendChild(doc.CreateTextNode(o.Binding.ChannelIndex.ToString()));
                        inner.AppendChild(sub);

                        objectNode.AppendChild(inner);
                    }

                    if(o.Range != null)
                    {
                        inner = doc.CreateElement("Range");
                        sub = doc.CreateElement("UpLimit");
                        sub.AppendChild(doc.CreateTextNode(o.Range.Up));
                        inner.AppendChild(sub);
                        sub = doc.CreateElement("DownLimit");
                        sub.AppendChild(doc.CreateTextNode(o.Range.Down));
                        inner.AppendChild(sub);

                        objectNode.AppendChild(inner);
                    }

                    if(o.Converter != null)
                    {
                        inner = doc.CreateElement("Converter");
                        sub = doc.CreateElement("UpScale");
                        sub.AppendChild(doc.CreateTextNode(o.Converter.UpScale.ToString("G17")));
                        inner.AppendChild(sub);
                        sub = doc.CreateElement("DownScale");
                        sub.AppendChild(doc.CreateTextNode(o.Converter.DownScale.ToString("G17")));
                        inner.AppendChild(sub);

                        objectNode.AppendChild(inner);
                    }

                    objects.AppendChild(objectNode);
                }
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, IEnumerable<uint> indexes, uint version = 1)
        {
            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("Index");
                dt.Columns.Add("Variable Name");
                dt.Columns.Add("Data Type");
                dt.Columns.Add("Unit");
                dt.Columns.Add("Binding");
                dt.Columns.Add("Range");
                dt.Columns.Add("Converter");
                dt.Columns.Add("Comment");

                foreach(var i in indexes)
                {
                    var pb = __process_objects[i];
                    dt.Rows.Add("0x" + pb.Index.ToString("X08"),
                                    pb.Variable.Name,
                                    pb.Variable.Type.Name,
                                    pb.Variable.Unit,
                                    pb.Binding?.ToString(),
                                    pb.Range?.ToString(),
                                    pb.Converter?.ToString(),
                                    pb.Variable.Comment);

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

        public bool IsEquivalent(ObjectDictionary? other)
        {
            bool res = false;
            if (other != null && other.ProcessObjects.Count == this.ProcessObjects.Count)
                res = this.ProcessObjects.All(p => other.ProcessObjects.ContainsKey(p.Key) && other.ProcessObjects[p.Key].IsEquivalent(this.ProcessObjects[p.Key]));
            return res;
        }
    }
    
    public class ProcessObject : ISubscriber<Variable>, ISubscriber<DeviceConfiguration>, IComparable<ProcessObject>
    {
        public ISubscriber<Variable>? DependencyChanged(Variable origin, Variable newcome)
        {
            Debug.Assert(this.Variable == origin);
            Debug.Assert(this.Variable.Type == newcome.Type);
            var n = (Publisher as ObjectDictionary)?.Replace(this.Index,
                this.Index, newcome.Name,
                this.Binding?.Device.ReferenceName, this.Binding?.ChannelName, this.Binding == null ? 0 : this.Binding.ChannelIndex,
                this.Range, this.Converter,
                ReplaceMode.Variable);
            return n;
        }

        public ISubscriber<DeviceConfiguration>? DependencyChanged(DeviceConfiguration origin, DeviceConfiguration newcome)
        {
            Debug.Assert(this.Binding.Device == origin);
            Debug.Assert(this.Binding.Device.DeviceModel == newcome.DeviceModel);
            var n =(Publisher as ObjectDictionary)?.Replace(this.Index,
                this.Index, this.Variable.Name,
                newcome.ReferenceName, this.Binding?.ChannelName, this.Binding == null ? 0 : this.Binding.ChannelIndex,
                this.Range, this.Converter,
                ReplaceMode.DeviceConfiguration);
            return n;
        }

        public bool IsEquivalent(ProcessObject? other)
        {
            return other != null && other.Index == Index && other.Variable.IsEquivalent(Variable) && 
                ((other.Binding == null && Binding == null) || (other.Binding != null && other.Binding == Binding)) &&
                ((other.Range == null && Range == null) || (other.Range != null && other.Range == Range)) &&
                ((other.Converter == null && Converter == null) || (other.Converter != null && other.Converter == Converter));
        }

        public uint Index { get; init; }
        public Variable Variable { get; init; } = new Variable();
        public DeviceBinding? Binding { get; init; }
        public ValueRange? Range { get; init; }
        public ValueConverter? Converter { get; init; }

        public static Publisher<ProcessObject>? Publisher { get; set; }
    }

    public record DeviceBinding(DeviceConfiguration Device, string ChannelName, uint ChannelIndex)
    {
        public virtual bool Equals(DeviceBinding? other)
        {
            if (other == null)
                return false;
            else
                return Device.IsEquivalent(other.Device) && ChannelName == other.ChannelName && ChannelIndex == other.ChannelIndex;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Device.ReferenceName} -- [{ChannelName} : {ChannelIndex}]";
        }
    }
    public record ValueRange(string Up, string Down)
    {
        public override string ToString()
        {
            return $"R[{Down} , {Up}]";
        }
    }
    public record ValueConverter(double UpScale, double DownScale)
    {
        public override string ToString()
        {
            return $"C[{DownScale} , {UpScale}]";
        }
    }
}
