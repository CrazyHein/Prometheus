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
                    d => new ProcessDataModel(d.ProcessObject, __process_data_image.Access) { Bit = d.BitPos }));
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
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[newcome], __process_data_models[i].Access)
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
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[__process_data_models[i].Index], __process_data_models[i].Access)
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
                    __process_data_models[i] = new ProcessDataModel(ObjectDictionary.ProcessObjects[__process_data_models[i].Index], __process_data_models[i].Access)
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
                d.DataStringDisplay = "N/A";
                d.DataStringValue = "0";
                d.DataBooleanValue = false;
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
        public ProcessDataModel(ProcessObject o, ProcessDataImageAccess access) : base(o)
        {
            Access = access;
            __data_type = o.Variable.Type;
        }

        private DataType __data_type;
        private bool __rx_changed = false;
        public ProcessDataImageAccess Access { get; private set; }
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

        private bool __data_boolean_value = false;
        public bool DataBooleanValue 
        {
            get { return __data_boolean_value; }
            set
            {
                if (__data_boolean_value != value)
                {
                    __data_boolean_value = value;
                    __rx_changed = true;
                    _notify_property_changed("DataBooleanValue");
                }
            }
        }

        private string __data_string_value = "0";
        public string DataStringValue 
        {
            get { return __data_string_value; }
            set
            {
                if (Access == ProcessDataImageAccess.RX)
                {
                    __data_string_value = value;
                    __rx_changed = true;
                    //_notify_property_changed("DataStringValue");
                }
            }
        }
        private ProcessDataStorage __non_bit_data_storage;

        private string __data_string_display = "N/A";
        public string DataStringDisplay
        {
            get { return __data_string_display; }
            set
            {
                if (__data_string_display != value)
                {
                    __data_string_display = value;
                    _notify_property_changed("DataStringDisplay");
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
            if (dataSource != null && dataSource.Length * 16 - Bit >= __data_type.BitSize)
            {
                var span = MemoryMarshal.AsBytes(dataSource.AsSpan()).Slice((int)Byte);
                switch (__data_type.Name)
                {
                    case "BIT":
                        if (Access == ProcessDataImageAccess.RX)
                        {
                            ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                            if ((dataSource[Bit / 16] & bitpos) == 0)
                                DataBooleanValue = false;
                            else
                                DataBooleanValue = true;
                        }
                        break;
                    case "BYTE":
                        DataStringValue = MemoryMarshal.Read<byte>(span).ToString();
                        break;
                    case "SBYTE":
                        DataStringValue = MemoryMarshal.Read<sbyte>(span).ToString();
                        break;
                    case "USHORT":
                        DataStringValue = MemoryMarshal.Read<ushort>(span).ToString();
                        break;
                    case "SHORT":
                        DataStringValue = MemoryMarshal.Read<short>(span).ToString();
                        break;
                    case "UINT":
                        DataStringValue = MemoryMarshal.Read<uint>(span).ToString();
                        break;
                    case "INT":
                        DataStringValue = MemoryMarshal.Read<int>(span).ToString();
                        break;
                    case "UDINT":
                    case "DUINT":
                        DataStringValue = MemoryMarshal.Read<ulong>(span).ToString();
                        break;
                    case "DINT":
                        DataStringValue = MemoryMarshal.Read<long>(span).ToString();
                        break;
                    case "FLOAT":
                        DataStringValue = MemoryMarshal.Read<float>(span).ToString("G9");
                        break;
                    case "DOUBLE":
                        DataStringValue = MemoryMarshal.Read<double>(span).ToString("G17");
                        break;
                    case "FIXEDPOINT3201":
                        DataStringValue = (MemoryMarshal.Read<int>(span) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT3202":
                        DataStringValue = (MemoryMarshal.Read<int>(span) / 10.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6401":
                        DataStringValue = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT6402":
                        DataStringValue = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6404":
                        DataStringValue = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F4");
                        break;
                    case "FINGERPRINT":
                        StringBuilder sb = new StringBuilder(32);
                        for (int i = 0; i < 16; ++i)
                            sb.Append(span[i].ToString("X2"));
                        DataStringValue = sb.ToString();
                        break;
                    default:
                        DataStringValue = "0";
                        break;
                }
            }
        }

        public void ValueChanged(ushort[] dataSource, bool isMonitoring)
        {
            if(dataSource != null && dataSource.Length * 16 - Bit >= __data_type.BitSize)
            {
                var span = MemoryMarshal.AsBytes(dataSource.AsSpan()).Slice((int)Byte);
                switch (__data_type.Name)
                {
                    case "BIT":
                        if (Access == ProcessDataImageAccess.TX || isMonitoring == true)
                        {
                            ushort bitpos = (ushort)(1 << (int)(Bit % 16));
                            if ((dataSource[Bit / 16] & bitpos) == 0)
                                DataBooleanValue = false;
                            else
                                DataBooleanValue = true;
                        }
                        break;
                    case "BYTE":
                        DataStringDisplay = MemoryMarshal.Read<byte>(span).ToString();
                        break;
                    case "SBYTE":
                        DataStringDisplay = MemoryMarshal.Read<sbyte>(span).ToString();
                        break;
                    case "USHORT":
                        DataStringDisplay = MemoryMarshal.Read<ushort>(span).ToString();
                        break;
                    case "SHORT":
                        DataStringDisplay = MemoryMarshal.Read<short>(span).ToString();
                        break;
                    case "UINT":
                        DataStringDisplay = MemoryMarshal.Read<uint>(span).ToString();
                        break;
                    case "INT":
                        DataStringDisplay = MemoryMarshal.Read<int>(span).ToString();
                        break;
                    case "UDINT":
                    case "DUINT":
                        DataStringDisplay = MemoryMarshal.Read<ulong>(span).ToString();
                        break;
                    case "DINT":
                        DataStringDisplay = MemoryMarshal.Read<long>(span).ToString();
                        break;
                    case "FLOAT":
                        DataStringDisplay = MemoryMarshal.Read<float>(span).ToString("G9");
                        break;
                    case "DOUBLE":
                        DataStringDisplay = MemoryMarshal.Read<double>(span).ToString("G17");
                        break;
                    case "FIXEDPOINT3201":
                        DataStringDisplay = (MemoryMarshal.Read<int>(span) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT3202":
                        DataStringDisplay = (MemoryMarshal.Read<int>(span) / 10.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6401":
                        DataStringDisplay = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F1");
                        break;
                    case "FIXEDPOINT6402":
                        DataStringDisplay = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F2");
                        break;
                    case "FIXEDPOINT6404":
                        DataStringDisplay = (MemoryMarshal.Read<long>(span) / 10.0).ToString("F4");
                        break;
                    case "FINGERPRINT":
                        StringBuilder sb = new StringBuilder(32);
                        for (int i = 0; i < 16; ++i)
                            sb.Append(span[i].ToString("X2"));
                        DataStringDisplay = sb.ToString();
                        break;
                    default:
                        DataStringDisplay = "Not yet supported";
                        break;
                }
                if (Access == ProcessDataImageAccess.RX && __rx_changed && isMonitoring == false)
                {
                    ProcessDataStorage storage = new ProcessDataStorage();
                    switch (__data_type.Name)
                    {
                        case "BIT":
                            if(DataBooleanValue)
                                dataSource[Bit / 16] |= (ushort)(1 << (int)(Bit % 16));
                            else
                                dataSource[Bit / 16] &= (ushort)(~(1 << (int)(Bit % 16)));
                            break;
                        case "BYTE":
                            if (byte.TryParse(DataStringValue, out storage.byteData))
                                MemoryMarshal.Write<byte>(span, ref storage.byteData);
                            break;
                        case "SBYTE":
                            if(sbyte.TryParse(DataStringValue, out storage.sbyteData))
                                MemoryMarshal.Write<sbyte>(span, ref storage.sbyteData);
                            break;
                        case "USHORT":
                            if (ushort.TryParse(DataStringValue, out storage.ushortData))
                                MemoryMarshal.Write<ushort>(span, ref storage.ushortData);
                            break;
                        case "SHORT":
                            if (short.TryParse(DataStringValue, out storage.shortData))
                                MemoryMarshal.Write<short>(span, ref storage.shortData);
                            break;
                        case "UINT":
                            if (uint.TryParse(DataStringValue, out storage.uintData))
                                MemoryMarshal.Write<uint>(span, ref storage.uintData);
                            break;
                        case "INT":
                            if (int.TryParse(DataStringValue, out storage.intData))
                                MemoryMarshal.Write<int>(span, ref storage.intData);
                            break;
                        case "UDINT":
                        case "DUINT":
                            if (ulong.TryParse(DataStringValue, out storage.ulongData))
                                MemoryMarshal.Write<ulong>(span, ref storage.ulongData);
                            break;
                        case "DINT":
                            if (long.TryParse(DataStringValue, out storage.longData))
                                MemoryMarshal.Write<long>(span, ref storage.longData);
                            break;
                        case "FLOAT":
                            if (float.TryParse(DataStringValue, out storage.floatData))
                                MemoryMarshal.Write<float>(span, ref storage.floatData);
                            break;
                        case "DOUBLE":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                                MemoryMarshal.Write<double>(span, ref storage.doubleData);
                            break;

                        case "FIXEDPOINT3201":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                            {
                                storage.intData = (int)(storage.doubleData * 10);
                                MemoryMarshal.Write<int>(span, ref storage.intData);
                            }
                            break;
                        case "FIXEDPOINT3202":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                            {
                                storage.intData = (int)(storage.doubleData * 100);
                                MemoryMarshal.Write<int>(span, ref storage.intData);
                            }
                            break;
                        case "FIXEDPOINT6401":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                            {
                                storage.longData = (long)(storage.doubleData * 10);
                                MemoryMarshal.Write<long>(span, ref storage.longData);
                            }
                            break;
                        case "FIXEDPOINT6402":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                            {
                                storage.longData = (long)(storage.doubleData * 100);
                                MemoryMarshal.Write<long>(span, ref storage.longData);
                            }
                            break;
                        case "FIXEDPOINT6404":
                            if (double.TryParse(DataStringValue, out storage.doubleData))
                            {
                                storage.longData = (long)(storage.doubleData * 10000);
                                MemoryMarshal.Write<long>(span, ref storage.longData);
                            }
                            break;
                        case "FINGERPRINT":
                            byte[] bytes = new byte[16];
                            int i = 0;
                            if (DataStringValue.Length == 32)
                            {
                                for (i = 0; i < 16; ++i)
                                {
                                    if (byte.TryParse(DataStringValue.Substring(i*2, 2), out storage.byteData))
                                        bytes[i] = storage.byteData;
                                }
                                if(i == 16)
                                    for (i = 0; i < 16; ++i)
                                        MemoryMarshal.Write(span.Slice(i, 1), ref bytes[i]);
                            }
                            break;
                        default:
                            //DataStringValue = "Not yet supported";
                            break;
                    }
                    __rx_changed = false;
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
        [FieldOffset(0)]
        public unsafe fixed byte FINGERPRINT[16];
    }
}
