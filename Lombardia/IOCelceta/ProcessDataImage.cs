using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class ProcessDataImage : Publisher<ProcessData>
    {
        public IReadOnlyList<ProcessData> ProcessDatas { get; private set; }
        public uint OffsetInWord { get; set; } = 0;
        public uint SizeInWord { get; set; } = 0;
        public uint ActualSizeInWord { get { return __actual_size_in_bit % 16 == 0 ? __actual_size_in_bit / 16 : __actual_size_in_bit / 16 + 1; } }
        private uint __actual_size_in_bit = 0;
        public uint ActualSizeInBit { get { return __actual_size_in_bit; } }
        private List<ProcessData> __process_datas = new List<ProcessData>();
        private Dictionary<ProcessObject, ProcessData> __process_object_hash;
        public IReadOnlyDictionary<ProcessObject, ProcessData> ProcessObjectHash { get; private set; }
        private ObjectDictionary __object_dictionary;
        public ProcessDataImageLayout Layout { get; private set; }
        public ProcessDataImageAccess Access { get; private set; }
        public ProcessDataImage(ObjectDictionary objectDictionary, Dictionary<ProcessObject, ProcessData> hash, ProcessDataImageLayout layout, ProcessDataImageAccess access)
        {
            ProcessDatas = __process_datas;
            __object_dictionary = objectDictionary;
            __process_object_hash = hash ?? new Dictionary<ProcessObject, ProcessData>();
            ProcessObjectHash = __process_object_hash;
            Layout = layout;
            Access = access;
        }

        public ProcessDataImage(ObjectDictionary objectDictionary, Dictionary<ProcessObject, ProcessData> hash, ProcessDataImageLayout layout, ProcessDataImageAccess access, XmlNode dataImageNode) : this(objectDictionary, hash, layout, access)
        {
            __load_process_data(dataImageNode);
        }

        public ProcessDataImage? Associated { set; get; }

        private List<ProcessData> __load_process_data(XmlNode dataImageNode)
        {
            try
            {
                __process_datas.Clear();
                uint wordsize, bitoffset;
                __actual_size_in_bit = 0;
                if (dataImageNode?.NodeType == XmlNodeType.Element)
                {
                    OffsetInWord = Convert.ToUInt32(dataImageNode.Attributes.GetNamedItem("WordOffset").Value, 10);
                    SizeInWord = Convert.ToUInt32(dataImageNode.Attributes.GetNamedItem("WordSize").Value, 10);
                    foreach (XmlNode index in dataImageNode.ChildNodes)
                    {
                        if (index.NodeType != XmlNodeType.Element)
                            continue;
                        if (index.Name == "Index")
                        {
                            ProcessObject o;
                            (o, bitoffset) = InspectProcessObject(Convert.ToUInt32(index.FirstChild.Value, 16), __actual_size_in_bit);
                            if (__process_object_hash.ContainsKey(o))
                                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_REFERENCE_IN_DATA_IMAGE);
                            __actual_size_in_bit = bitoffset + o.Variable.Type.BitSize;
                            wordsize = __actual_size_in_bit % 16 == 0 ? __actual_size_in_bit / 16 : __actual_size_in_bit / 16 + 1; 
                            if (wordsize > SizeInWord)
                                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_IMAGE_SIZE_OUT_OF_RANGE);

                            ProcessData data = new ProcessData(o, this.Access) {BitPos = bitoffset, Publisher = this };
                            __process_datas.Add(data);
                            __process_object_hash.Add(o, data);
                            __object_dictionary.AddSubscriber(o, data);
                        }
                    }
                }
                return __process_datas;
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

        private uint __rebuild_process_data_bit_pos(int pos)
        {
            uint size = 0;
            if (pos >= 1 && pos - 1 < __process_datas.Count)
                size = __process_datas[pos - 1].BitPos + __process_datas[pos - 1].ProcessObject.Variable.Type.BitSize;

            for (int i = pos; i < __process_datas.Count; ++i)
            {
                __process_datas[i].BitPos = Align(__process_datas[i].ProcessObject.Variable.Type, size);
                size = __process_datas[i].BitPos + __process_datas[i].ProcessObject.Variable.Type.BitSize;
            }
            __actual_size_in_bit = size;
            return __actual_size_in_bit;
        }
        public ProcessData InspectProcessData(uint poindex)
        {
            if (__object_dictionary.ProcessObjects.TryGetValue(poindex, out var po) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_REFERENCE_IN_DATA_IMAGE);
            return new ProcessData(po, Access) { Publisher = this};
        }

        private void __check_process_data(ProcessData d, bool checkforsameindex = true)
        {
            if (checkforsameindex &&__process_object_hash.ContainsKey(d.ProcessObject))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.DUPLICATED_OBJECT_REFERENCE_IN_DATA_IMAGE);
            switch (Layout)
            {
                case ProcessDataImageLayout.Bit:
                    if (d.ProcessObject.Variable.Type.BitSize != 1)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_DATA_TYPE_IN_DATA_IMAGE);
                    break;
                default:
                    if (d.ProcessObject.Variable.Type.BitSize == 1)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_DATA_TYPE_IN_DATA_IMAGE);
                    break;
            }
            if (d.ProcessObject.Binding != null)
            {
                switch (Access)
                {
                    case ProcessDataImageAccess.RX:
                        if (d.ProcessObject.Binding.Device.DeviceModel.RxVariables.ContainsKey(d.ProcessObject.Binding.ChannelName) == false ||
                            d.ProcessObject.Binding.Device.DeviceModel.RxVariables[d.ProcessObject.Binding.ChannelName] <=
                            d.ProcessObject.Binding.ChannelIndex)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_BINDING_CHANNEL_IN_DATA_IMAGE);
                        break;
                    case ProcessDataImageAccess.TX:
                        if (d.ProcessObject.Binding.Device.DeviceModel.TxVariables.ContainsKey(d.ProcessObject.Binding.ChannelName) == false ||
                            d.ProcessObject.Binding.Device.DeviceModel.TxVariables[d.ProcessObject.Binding.ChannelName] <=
                            d.ProcessObject.Binding.ChannelIndex)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_BINDING_CHANNEL_IN_DATA_IMAGE);
                        break;
                }
            }
        }

        public void Save(XmlDocument doc, XmlElement dataImageNode, uint version = 1)
        {
            //check area size
            if (ActualSizeInWord > SizeInWord)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_IMAGE_SIZE_OUT_OF_RANGE);
            try
            {
                dataImageNode.SetAttribute("WordOffset", OffsetInWord.ToString());
                dataImageNode.SetAttribute("WordSize", SizeInWord.ToString());

                foreach (var data in __process_datas)
                {
                    XmlElement index = doc.CreateElement("Index");
                    index.AppendChild(doc.CreateTextNode($"0x{data.ProcessObject.Index:X8}"));
                    dataImageNode.AppendChild(index);
                }
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, uint version = 1)
        {
            if (ActualSizeInWord > SizeInWord)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_IMAGE_SIZE_OUT_OF_RANGE);
            try
            {
                sheet.Range[1, 1].Text = "Offset In Word";
                sheet.Range[2, 1].Text = "Size In Word";
                sheet.Range[3, 1].Text = "Actual Size In Word";
                sheet.Range[1, 1, 3, 1].Style = title;

                sheet.Range[1, 2].Text = OffsetInWord.ToString();
                sheet.Range[2, 2].Text = SizeInWord.ToString();
                sheet.Range[3, 2].Text = ActualSizeInWord.ToString();
                sheet.Range[1, 2, 3, 2].Style = content;

                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("Bit");
                dt.Columns.Add("Byte");
                dt.Columns.Add("Index");
                dt.Columns.Add("Variable Name");
                dt.Columns.Add("Data Type");
                dt.Columns.Add("Unit");
                dt.Columns.Add("Binding");
                dt.Columns.Add("Range");
                dt.Columns.Add("Converter");
                dt.Columns.Add("Comment");

                foreach(var p in __process_datas)
                    dt.Rows.Add(p.BitPos.ToString(),
                                p.BytePos.ToString(),
                                "0x" + p.ProcessObject.Index.ToString("X08"),
                                p.ProcessObject.Variable.Name,
                                p.ProcessObject.Variable.Type.Name,
                                p.ProcessObject.Variable.Unit,
                                p.ProcessObject.Binding?.ToString(),
                                p.ProcessObject.Range?.ToString(),
                                p.ProcessObject.Converter?.ToString(),
                                p.ProcessObject.Variable.Comment);

                int rows = sheet.InsertDataTable(dt, true, 5, 1, false);
                sheet.Range[5, 1, 5, dt.Columns.Count].Style = title;
                if (rows > 0) sheet.Range[6, 1, 5 + rows, dt.Columns.Count].Style = content;
                sheet.AllocatedRange.AutoFitColumns();
                sheet.Range[6, 1].FreezePanes();
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        protected void Add(ProcessData d)
        {
            __check_process_data(d);
            if(d.Publisher == null) d.Publisher = this;
            __process_datas.Add(d);
            __actual_size_in_bit = __rebuild_process_data_bit_pos(__process_datas.Count - 1);
            __process_object_hash.Add(d.ProcessObject, d);
            __object_dictionary.AddSubscriber(d.ProcessObject, d);
        }

        public ProcessData Add(uint poindex)
        {
            ProcessData d = InspectProcessData(poindex);
            __check_process_data(d);
            __process_datas.Add(d);
            __actual_size_in_bit = __rebuild_process_data_bit_pos(__process_datas.Count - 1);
            __process_object_hash.Add(d.ProcessObject, d);
            __object_dictionary.AddSubscriber(d.ProcessObject, d);
            return d;
        }

        public void Remove(ProcessData d, bool force = false)
        {
            if (!force && _subscribers.ContainsKey(d))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_BE_SUBSCRIBED);

            int pos = __process_datas.IndexOf(d);
            if (__process_datas.Remove(d) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_UNFOUND);
            __process_object_hash.Remove(d.ProcessObject);
            __actual_size_in_bit = __rebuild_process_data_bit_pos(pos);

            __object_dictionary.RemoveSubscriber(d.ProcessObject, d);
        }

        public (ProcessData, ProcessObject) ProcessDataAddEx(uint index, string name, string? deviceName, string? channelName, uint channelIndex, ValueRange? vr, ValueConverter? vc)
        {
            var po = __object_dictionary.Add(index, name, deviceName, channelName, channelIndex, vr, vc);
            var pd = Add(index);
            return (pd, po);
        }

        public ProcessData Remove(ProcessObject reference, bool force = false)
        {
            if (__process_object_hash.TryGetValue(reference, out var d) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_UNFOUND);
            Remove(d, force);
            return d;
        }

        protected void Insert(int index, ProcessData d)
        {
            __check_process_data(d);
            if (d.Publisher == null) d.Publisher = this;
            __process_datas.Insert(index, d);
            __actual_size_in_bit = __rebuild_process_data_bit_pos(index);
            __process_object_hash.Add(d.ProcessObject, d);
            __object_dictionary.AddSubscriber(d.ProcessObject, d);
        }

        public ProcessData Insert(int index, uint poindex)
        {
            ProcessData d = InspectProcessData(poindex);
            __check_process_data(d);
            __process_datas.Insert(index, d);
            __actual_size_in_bit = __rebuild_process_data_bit_pos(index);
            __process_object_hash.Add(d.ProcessObject, d);
            __object_dictionary.AddSubscriber(d.ProcessObject, d);
            return d;
        }

        protected void Replace(ProcessData origin, ProcessData d, ReplaceMode mode = ReplaceMode.Full)
        {
            if (_subscribers.ContainsKey(origin) && origin.ProcessObject.Variable.Type != d.ProcessObject.Variable.Type)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_BE_SUBSCRIBED);
            __check_process_data(d, origin.ProcessObject.Index != d.ProcessObject.Index);
            int index = __process_datas.IndexOf(origin);
            if (__process_datas.Remove(origin) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_UNFOUND);
            __process_object_hash.Remove(origin.ProcessObject); 
            __process_datas.Insert(index, d);
            __process_object_hash.Add(d.ProcessObject, d);
            

            if (_subscribers.TryGetValue(origin, out var res) == true)
            {
                List<ISubscriber<ProcessData>> subs = new List<ISubscriber<ProcessData>>(res);

                try
                {
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        subs[i] = subs[i].DependencyChanged(origin, d);
                    }
                }
                catch (LombardiaException)
                {
                    __process_datas.RemoveAt(index);
                    __process_object_hash.Remove(d.ProcessObject);
                    __process_datas.Insert(index, origin);
                    __process_object_hash.Add(origin.ProcessObject, origin);
                    throw;
                }
                for (int i = 0; i < subs.Count; ++i)
                {
                    foreach (var k in _subscribers.Keys)
                    {
                        if (k != origin)
                        {
                            while (_subscribers[k].Remove(res[i]))
                                _subscribers[k].Add(subs[i]);  
                        }
                    }
                    if (Associated != null)
                    {
                        List<ProcessData> keys = new List<ProcessData>(Associated.KeyCollection);
                        foreach(var k in keys)
                        {
                            while (Associated.RemoveSubscriber(k, res[i]))
                                Associated.AddSubscriber(k, subs[i]);
                        }
                    }
                }
                _subscribers.Remove(origin);
                _subscribers[d] = subs;
            }

            __actual_size_in_bit = __rebuild_process_data_bit_pos(index);

            if (mode == ReplaceMode.Full)
            {
                __object_dictionary.RemoveSubscriber(origin.ProcessObject, origin);
                __object_dictionary.AddSubscriber(d.ProcessObject, d);
            }
        }

        protected void Replace(ProcessObject reference, ProcessData d, ReplaceMode mode = ReplaceMode.Full)
        {
            if (__process_object_hash.TryGetValue(reference, out var origin) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.PROCESS_DATA_UNFOUND);
            Replace(origin, d, mode);
        }

        public ProcessData Replace(ProcessObject reference, uint newpoindex, ReplaceMode mode = ReplaceMode.Full)
        {
            ProcessData d = InspectProcessData(newpoindex);
            Replace(reference, d, mode);
            return d;
        }

        public (ProcessObject, uint) InspectProcessObject(uint index, uint start)
        {
            if (__object_dictionary.ProcessObjects.TryGetValue(index, out var o) == false)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_REFERENCE_IN_DATA_IMAGE);
            uint bits = Align(o.Variable.Type, start);
            return (o, bits);
        }

        public uint Align(DataType type, uint startbit)
        {
            switch (Layout)
            {
                case ProcessDataImageLayout.Bit:
                    if (type.BitSize != 1)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_DATA_TYPE_IN_DATA_IMAGE);
                    break;
                case ProcessDataImageLayout.Block:
                case ProcessDataImageLayout.System:
                    if (type.BitSize == 1)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_OBJECT_DATA_TYPE_IN_DATA_IMAGE);
                    uint offset = startbit % (type.Alignment * 8);
                    if (offset != 0)
                        startbit += type.Alignment * 8 - offset;
                    break;
            }
            return startbit;
        }
    }

    public class ProcessData : ISubscriber<ProcessObject>
    {
        public ISubscriber<ProcessObject>? DependencyChanged(ProcessObject origin, ProcessObject newcome)
        {
            Debug.Assert(this.ProcessObject == origin);
            Debug.Assert(this.ProcessObject.Variable.Type == newcome.Variable.Type);
            var n =(Publisher as ProcessDataImage)?.Replace(this.ProcessObject, newcome.Index, ReplaceMode.Half);
            return n;
        }

        public ProcessObject ProcessObject { get; private set; }
        public uint BitPos { get; set; }
        public uint BytePos { get { return BitPos / 8; } }

        public ProcessDataImageAccess Access { get; private set; }

        public Publisher<ProcessData>? Publisher { get; set; }

        public ProcessData(ProcessObject o, ProcessDataImageAccess access)
        {
            ProcessObject = o;
            Access = access;
        }

        public string DeviceBindingInfo
        {
            get
            {
                if (ProcessObject.Binding != null)
                    return $"{ProcessObject.Binding.Device.ReferenceName} -- [{ProcessObject.Binding.ChannelName} : {ProcessObject.Binding.ChannelIndex}]";
                else
                    return "N/A";
            }
        }

        public override string ToString()
        {
            switch (Access)
            {
                case ProcessDataImageAccess.TX:
                    return $"[0x{ProcessObject.Index:X08} : {ProcessObject.Variable.Name}] <-- [{DeviceBindingInfo}]";
                case ProcessDataImageAccess.RX:
                    return $"[0x{ProcessObject.Index:X08} : {ProcessObject.Variable.Name}] --> [{DeviceBindingInfo}]";
                default:
                    return "N/A";
            }
        }
    }
}
