using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class ProcessDataImageModel : RecordContainerModel
    {
        private ProcessDataImage __process_data_image;
        private ObjectsModel __objects_source;
        public ObjectDictionary ObjectDictionary { get; private set; }
        private ObservableCollection<ProcessDataModel> __process_data_models;
        public IReadOnlyList<ProcessDataModel> ProcessDataModels { get; private set; }
        public ProcessDataImageAccess Access { get; private set; }
        public InterlockCollectionModel InterlockModels { get; set; }
        public bool DirectModeOperation { get; set; }

        private bool __is_offline = true;
        public bool IsOffline 
        {
            get { return __is_offline; }
            set 
            {
                if(value != __is_offline)
                {
                    __is_offline = value;
                    OnPropertyChanged("IsOffline");
                }
            }
        }

        private bool __is_monitoring = true;
        public bool IsMonitoring
        {
            get { return __is_monitoring; }
            set
            {
                if (value != __is_monitoring)
                {
                    __is_monitoring = value;
                    OnPropertyChanged("IsMonitoring");
                }
            }
        }
        public ProcessDataImageLayout Layout { get; private set; }

        public ProcessDataImageModel(ProcessDataImage pdi, ObjectDictionary od, ObjectsModel objectsSource)
        {
            __process_data_image = pdi;
            __objects_source = objectsSource;
            ObjectDictionary = od;
            __process_data_models = new ObservableCollection<ProcessDataModel>(
                pdi.ProcessDatas.Select(
                    d => new ProcessDataModel(d.ProcessObject, __process_data_image.Access, __process_data_image.Layout) { Bit = d.BitPos }));;
            Access = __process_data_image.Access;
            Layout = pdi.Layout;
            ProcessDataModels = __process_data_models;
            __actual_size_in_word = __process_data_image.ActualSizeInWord;
            __process_data_image.OffsetInWord = __process_data_image.OffsetInWord;
            __process_data_image.SizeInWord = __process_data_image.SizeInWord;
            Modified = false;
            __objects_source.SubsModified = false;
        }

        private void __copy_bitpos(IReadOnlyList<ProcessData> source, IList<ProcessDataModel> destination, int start)
        {
            for (int i = start; i < source.Count; ++i)
                destination[i].Bit = source[i].BitPos;
        }

        public bool UpdateProcessData(uint origin, uint newcome)
        {
            bool res = false;
            for (int i = 0; i < __process_data_models.Count; ++i)
            {
                if (__process_data_models[i].Index == origin)
                {
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[newcome], __process_data_models[i].Access, __process_data_image.Layout)
                    {
                        Bit = __process_data_models[i].Bit
                    };
                    if(Layout == ProcessDataImageLayout.Bit)
                        InterlockModels?.UpdateInterlockLogic();
                    Modified = true;
                    __objects_source.SubsModified = true;
                    res = true;
                    break;
                }
            }
            return res;
        }

        public void UpdateVariable(string origin)
        {
            for (int i = 0; i < __process_data_models.Count; ++i)
            {
                if (__process_data_models[i].VariableName == origin)
                {
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[__process_data_models[i].Index], __process_data_models[i].Access, __process_data_image.Layout)
                    {
                        Bit = __process_data_models[i].Bit
                    };
                    InterlockModels?.UpdateInterlockLogic();
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
        }

        public void UpdateBinding(string origin)
        {
            for (int i = 0; i < __process_data_models.Count; ++i)
            {
                if (__process_data_models[i].EnableBinding && __process_data_models[i].BindingDeviceName == origin)
                {
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[__process_data_models[i].Index], __process_data_models[i].Access, __process_data_image.Layout)
                    {
                        Bit = __process_data_models[i].Bit
                    };
                    InterlockModels?.UpdateInterlockLogic();
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
        }

        public void ProcessDataValueChanged(ushort[] dataSource)
        {
            foreach(var d in __process_data_models)
            {
                d.ValueChanged(dataSource, IsMonitoring);
            }
        }

        public void InitProcessDataValue(ushort[] dataSource)
        {
            foreach (var d in __process_data_models)
            {
                d.InitValue(dataSource);
            }
        }

        public void ResetProcessDataValue()
        {
            foreach (var d in __process_data_models)
            {
                d.ResetDataStorage();
            }
        }

        public void Add(ProcessDataModel model)
        {
            Debug.Assert(model.Access == __process_data_image.Access);
            __process_data_image.Add(model.Index);
            __process_data_models.Add(model);
            __copy_bitpos(__process_data_image.ProcessDatas, __process_data_models, __process_data_image.ProcessDatas.Count - 1);
            ActualSizeInWord = __process_data_image.ActualSizeInWord;
            Modified = true;
            __objects_source.SubsModified = true;
        }

        public ObjectModel RemoveAt(int index, bool force = false)
        {
            ProcessDataModel model = __process_data_models[index];
            __process_data_image.Remove(ObjectDictionary.ProcessObjects[model.Index], force);
            __process_data_models.RemoveAt(index);
            __copy_bitpos(__process_data_image.ProcessDatas, __process_data_models, index);
            ActualSizeInWord = __process_data_image.ActualSizeInWord;
            Modified = true;
            __objects_source.SubsModified = true;
            return model;
        }

        public void Remove(ProcessDataModel model, bool force = false)
        {
            __process_data_image.Remove(ObjectDictionary.ProcessObjects[model.Index], force);
            int index = __process_data_models.IndexOf(model);
            __process_data_models.Remove(model);
            __copy_bitpos(__process_data_image.ProcessDatas, __process_data_models, index);
            ActualSizeInWord = __process_data_image.ActualSizeInWord;
            Modified = true;
            __objects_source.SubsModified = true;
        }

        public void Insert(int index, ProcessDataModel model)
        {
            Debug.Assert(model.Access == __process_data_image.Access);
            if (index > __process_data_models.Count)
                throw new ArgumentOutOfRangeException();
            __process_data_image.Insert(index, model.Index);
            __process_data_models.Insert(index, model);
            __copy_bitpos(__process_data_image.ProcessDatas, __process_data_models, index);
            ActualSizeInWord = __process_data_image.ActualSizeInWord;
            Modified = true;
            __objects_source.SubsModified = true;
        }

        public int IndexOf(ProcessDataModel model)
        {
            return __process_data_models.IndexOf(model);
        }

        public uint OffsetInWord
        {
            get { return __process_data_image.OffsetInWord; }
            set 
            {
                if (__process_data_image.OffsetInWord != value)
                {
                    __process_data_image.OffsetInWord = value;
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
        }

        public uint SizeInWord
        {
            get { return __process_data_image.SizeInWord; }
            set 
            {
                if (__process_data_image.SizeInWord != value)
                {
                    __process_data_image.SizeInWord = value;
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
        }

        public void Save(XmlDocument doc, XmlElement root)
        {
            __process_data_image.Save(doc, root);
            Modified = false;
        }

        private uint __actual_size_in_word = 0;
        public uint ActualSizeInWord
        {
            get { return __process_data_image.ActualSizeInWord; }
            set { SetProperty(ref __actual_size_in_word, value); }
        }
    }

    public class ProcessDataModel : ObjectModel
    {
        public ProcessDataModel(ProcessObject o, ProcessDataImageAccess access, ProcessDataImageLayout layout) : base(o)
        {
            Access = access;
            Layout = layout;
            __data_type = o.Variable.Type;
            //__data_size_in_byte = (int)__data_type.BitSize / 8;
            if(__data_type.BitSize == 1)
                __data_storage = new byte[2];
            else
                __data_storage = new byte[__data_type.BitSize / 8];
        }

        private DataType __data_type;
        //private int __data_size_in_byte;
        public ProcessDataImageAccess Access { get; private set; }
        public ProcessDataImageLayout Layout { get; private set; }
        public uint Byte { get { return Bit / 8; } }
        
        private uint __bits;
        public uint Bit
        {
            get { return __bits; }
            set
            {
                __bits = value;
                _notify_property_changed();
                _notify_property_changed("Byte");
            }
        }

        private byte[] __data_storage;
        //private byte[] __unsupported_data_type;
        private bool __rx_pending = false;

        public void ResetDataStorage()
        {
            Array.Clear(__data_storage, 0, __data_storage.Length);
            _notify_property_changed("DataBooleanValue");
            _notify_property_changed("DataStringValue");
            __rx_pending = true;
        }

        public bool DataBooleanValue 
        {
            get 
            {
                if (Layout == ProcessDataImageLayout.Bit)
                {
                    ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                    var data = MemoryMarshal.Read<ushort>(__data_storage);
                    if ((data & bitpos) == 0)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            set
            {
                if (Layout == ProcessDataImageLayout.Bit)
                {
                    ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                    var data = MemoryMarshal.Read<ushort>(__data_storage);
                    if (value == true && (data & bitpos) == 0)
                    {
                        data |= bitpos;
                        MemoryMarshal.Write<ushort>(__data_storage, ref data);
                        _notify_property_changed("DataBooleanValue");
                        __rx_pending = true;
                    }
                    else if (value == false && (data & bitpos) != 0)
                    {
                        data &= (ushort)(~bitpos);
                        MemoryMarshal.Write<ushort>(__data_storage, ref data);
                        _notify_property_changed("DataBooleanValue");
                        __rx_pending = true;
                    }
                }
            }
        }

        public string DataStringValue 
        {
            get 
            {
                if (Layout == ProcessDataImageLayout.Bit)
                    return "N/A";
                string res;
                switch (__data_type.Name)
                {
                    case "BYTE":
                        res = MemoryMarshal.Read<byte>(__data_storage).ToString();
                        break;
                    case "SBYTE":
                        res = MemoryMarshal.Read<sbyte>(__data_storage).ToString();
                        break;
                    case "USHORT":
                        res = MemoryMarshal.Read<ushort>(__data_storage).ToString();
                        break;
                    case "SHORT":
                        res = MemoryMarshal.Read<short>(__data_storage).ToString();
                        break;
                    case "UINT":
                        res = MemoryMarshal.Read<uint>(__data_storage).ToString();
                        break;
                    case "INT":
                        res = MemoryMarshal.Read<int>(__data_storage).ToString();
                        break;
                    case "UDINT":
                    case "DUINT":
                        res = MemoryMarshal.Read<ulong>(__data_storage).ToString();
                        break;
                    case "DINT":
                        res = MemoryMarshal.Read<long>(__data_storage).ToString();
                        break;
                    case "FLOAT":
                        res = MemoryMarshal.Read<float>(__data_storage).ToString("G9");
                        break;
                    case "DOUBLE":
                        res = MemoryMarshal.Read<double>(__data_storage).ToString("G17");
                        break;
                    case "FIXEDPOINT3201":
                        res = (MemoryMarshal.Read<int>(__data_storage) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT3202":
                        res = (MemoryMarshal.Read<int>(__data_storage) / 100.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6401":
                        res = (MemoryMarshal.Read<long>(__data_storage) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT6402":
                        res = (MemoryMarshal.Read<long>(__data_storage) / 100.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6404":
                        res = (MemoryMarshal.Read<long>(__data_storage.AsSpan()) / 10000.0).ToString("F4");
                        break;
                    case "FINGERPRINT":
                        StringBuilder sb = new StringBuilder(__data_storage.Length * 2);
                        for (int i = 0; i < __data_storage.Length; ++i)
                            sb.Append(__data_storage[i].ToString("X2"));
                        res = sb.ToString();
                        break;
                    default:
                        res = "Not yet supported";
                        break;
                }
                return res; 
            }
            set
            {
                if (Access == ProcessDataImageAccess.RX && Layout != ProcessDataImageLayout.Bit)
                {
                    bool res = false;
                    ProcessDataStorage storage = new ProcessDataStorage();
                    switch (__data_type.Name)
                    {
                        case "BYTE":
                            res = byte.TryParse(value, out storage.byteData);
                            if (res)
                                MemoryMarshal.Write<byte>(__data_storage, ref storage.byteData);
                            break;
                        case "SBYTE":
                            res = sbyte.TryParse(value, out storage.sbyteData);
                            if(res)
                                MemoryMarshal.Write<sbyte>(__data_storage, ref storage.sbyteData);
                            break;
                        case "USHORT":
                            res = ushort.TryParse(value, out storage.ushortData);
                            if(res)
                                MemoryMarshal.Write<ushort>(__data_storage, ref storage.ushortData);
                            break;
                        case "SHORT":
                            res = short.TryParse(value, out storage.shortData);
                            if(res)
                                MemoryMarshal.Write<short>(__data_storage, ref storage.shortData);
                            break;
                        case "UINT":
                            res = uint.TryParse(value, out storage.uintData);
                            if(res)
                                MemoryMarshal.Write<uint>(__data_storage, ref storage.uintData);
                            break;
                        case "INT":
                            res = int.TryParse(value, out storage.intData);
                            if (res)
                                MemoryMarshal.Write<int>(__data_storage, ref storage.intData);
                            break;
                        case "UDINT":
                        case "DUINT":
                            res = ulong.TryParse(value, out storage.ulongData);
                            if (res)
                                MemoryMarshal.Write<ulong>(__data_storage, ref storage.ulongData);
                            break;
                        case "DINT":
                            res = long.TryParse(value, out storage.longData);
                            if (res)
                                MemoryMarshal.Write<long>(__data_storage, ref storage.longData);
                            break;
                        case "FLOAT":
                            res = float.TryParse(value, out storage.floatData);
                            if (res)
                                MemoryMarshal.Write<float>(__data_storage, ref storage.floatData);
                            break;
                        case "DOUBLE":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                                MemoryMarshal.Write<double>(__data_storage, ref storage.doubleData);
                            break;
                        case "FIXEDPOINT3201":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                            {
                                storage.intData = (int)(storage.doubleData * 10);
                                MemoryMarshal.Write<int>(__data_storage, ref storage.intData);
                            }
                            break;
                        case "FIXEDPOINT3202":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                            {
                                storage.intData = (int)(storage.doubleData * 100);
                                MemoryMarshal.Write<int>(__data_storage, ref storage.intData);
                            }
                            break;
                        case "FIXEDPOINT6401":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                            {
                                storage.longData = (long)(storage.doubleData * 10);
                                MemoryMarshal.Write<long>(__data_storage, ref storage.longData);
                            }
                            break;
                        case "FIXEDPOINT6402":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                            {
                                storage.longData = (long)(storage.doubleData * 100);
                                MemoryMarshal.Write<long>(__data_storage, ref storage.longData);
                            }
                            break;
                        case "FIXEDPOINT6404":
                            res = double.TryParse(value, out storage.doubleData);
                            if (res)
                            {
                                storage.longData = (long)(storage.doubleData * 10000);
                                MemoryMarshal.Write<long>(__data_storage, ref storage.longData);
                            }
                            break;
                        case "FINGERPRINT":
                            int i = 0;
                            byte[] buffer = new byte[__data_storage.Length];
                            try
                            {
                                for (i = 0; i < __data_storage.Length; ++i)
                                {
                                    var byteData = Convert.ToByte(value.Substring(i * 2, 2), 16);
                                    buffer[i] = byteData;
                                }
                            }
                            catch
                            {
                                res = false;
                            }
                            buffer.CopyTo(__data_storage, 0);
                            res = true;
                            break;
                        default:
                            break;
                    }
                    if (res)
                    {
                        _notify_property_changed("DataStringValue");
                        __rx_pending = true;
                    }
                }
            }
        }


        public override string ToString()
        {
            switch(Access)
            {
                case ProcessDataImageAccess.TX:
                    return $"[0x{Index:X08} : {VariableName}] <-- [{DeviceBindingInfo}]";
                case ProcessDataImageAccess.RX:
                    return $"[0x{Index:X08} : {VariableName}] --> [{DeviceBindingInfo}]";
                default:
                    return base.ToString();
            }
        }

        public void InitValue(ushort[] dataSource)
        {
            if (dataSource != null && dataSource.Length * 16 - Bit >= __data_type.BitSize && Access == ProcessDataImageAccess.RX)
            {
                var span = MemoryMarshal.AsBytes(dataSource.AsSpan()).Slice((int)Byte);
                switch (__data_type.Name)
                {
                    case "BIT":
                        ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                        var data = MemoryMarshal.Read<ushort>(__data_storage.AsSpan());
                        if ((dataSource[Bit / 16] & bitpos) != 0 && (data & bitpos) == 0)
                        {

                            data |= bitpos;
                            MemoryMarshal.Write<ushort>(__data_storage.AsSpan(), ref data);
                            _notify_property_changed("DataBooleanValue");
                        }
                        else if ((dataSource[Bit / 16] & bitpos) == 0 && (data & bitpos) != 0)
                        {
                            data &= (ushort)(~bitpos);
                            MemoryMarshal.Write<ushort>(__data_storage.AsSpan(), ref data);
                            _notify_property_changed("DataBooleanValue");
                        }
                        break;
                    default:
                        span.Slice(0, __data_storage.Length).CopyTo(__data_storage);
                        _notify_property_changed("DataStringValue");
                        break;
                }
            }
        }

        public void ValueChanged(ushort[] dataSource, bool isMonitoring)
        {
            if(dataSource != null && dataSource.Length * 16 - Bit >= __data_type.BitSize)
            {
                var span = MemoryMarshal.AsBytes(dataSource.AsSpan()).Slice((int)Byte);
                if (Access == ProcessDataImageAccess.TX || isMonitoring == true)
                {
                    switch (__data_type.Name)
                    {
                        case "BIT":
                            ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                            var data = MemoryMarshal.Read<ushort>(__data_storage.AsSpan());
                            if ((dataSource[Bit / 16] & bitpos) != 0 && (data & bitpos) == 0)
                            {

                                data |= bitpos;
                                MemoryMarshal.Write<ushort>(__data_storage.AsSpan(), ref data);
                                _notify_property_changed("DataBooleanValue");
                            }
                            else if ((dataSource[Bit / 16] & bitpos) == 0 && (data & bitpos) != 0)
                            {
                                data &= (ushort)(~bitpos);
                                MemoryMarshal.Write<ushort>(__data_storage.AsSpan(), ref data);
                                _notify_property_changed("DataBooleanValue");
                            }
                            break;
                        default:
                            span.Slice(0, __data_storage.Length).CopyTo(__data_storage);
                            _notify_property_changed("DataStringValue");
                            break;
                    }
                }
                else if (Access == ProcessDataImageAccess.RX && isMonitoring == false && __rx_pending)
                {
                    switch (__data_type.Name)
                    {
                        case "BIT":
                            if(DataBooleanValue)
                                dataSource[Bit / 16] |= (ushort)(1 << (int)(Bit % 16));
                            else
                                dataSource[Bit / 16] &= (ushort)(~(1 << (int)(Bit % 16)));
                            break;
                        default:
                            __data_storage.AsSpan().CopyTo(span);
                            break;
                    }
                    __rx_pending = false;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct ProcessDataStorage
    {
        [FieldOffset(0)]
        public byte byteData;
        [FieldOffset(0)]
        public sbyte sbyteData;
        [FieldOffset(0)]
        public ushort ushortData;
        [FieldOffset(0)]
        public short shortData;
        [FieldOffset(0)]
        public uint uintData;
        [FieldOffset(0)]
        public int intData;
        [FieldOffset(0)]
        public ulong ulongData;
        [FieldOffset(0)]
        public long longData;
        [FieldOffset(0)]
        public float floatData;
        [FieldOffset(0)]
        public double doubleData;
    }
}
