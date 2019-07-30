using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ObjectCollectionDataModel : IOListDataModel
    {
        public static PropertyGroupDescription DataTypeGroupDescription { get; private set; }
        public static PropertyGroupDescription BindingModuleGroupDescription { get; private set; }
        public ObjectItemFilter ItemFilter { get; private set; }
        public static string DataTypePropertyName { get; private set; }
        public static string BindingModulePropertyName { get; private set; }
        public static string VariablePropertyName { get; private set; }

        public IReadOnlyDictionary<uint, ObjectItemDataModel> ObjectDictionary { get; private set; }
        public IReadOnlyList<ObjectItemDataModel> Objects { get; private set; }

        private Dictionary<uint, ObjectItemDataModel> __object_dictionary;
        private ObservableCollection<ObjectItemDataModel> __objects;

        static ObjectCollectionDataModel()
        {
            DataTypePropertyName = "VariableDataType";
            BindingModulePropertyName = "BindingModuleSelection";
            VariablePropertyName = "VariableName";
            DataTypeGroupDescription = new PropertyGroupDescription(DataTypePropertyName);
            BindingModuleGroupDescription = new PropertyGroupDescription(BindingModulePropertyName, new ObjectItemBindingModule());
        }

        public ObjectCollectionDataModel(IOListDataHelper helper):base(helper)
        {
            __object_dictionary = new Dictionary<uint, ObjectItemDataModel>();
            __objects = new ObservableCollection<ObjectItemDataModel>();
            ObjectDictionary = __object_dictionary;
            Objects = __objects;
            ItemFilter = new ObjectItemFilter(null, null, null);
        }

        public override void UpdateDataHelper()
        {
            
        }

        public override void UpdateDataModel()
        {
            __objects.Clear();
            __object_dictionary.Clear();
            foreach (var o in _data_helper.IOObjectCollection)
            {
                ObjectItemDataModel temp = new ObjectItemDataModel(this, o);
                __objects.Add(temp);
                __object_dictionary.Add(o.index, temp);
            }
        }

        public void SwapDataModel(int firstPos, int secondPos)
        {
            if (firstPos < secondPos)
            {
                __objects.Move(secondPos, firstPos);
                __objects.Move(firstPos + 1, secondPos);
            }
            else if(firstPos > secondPos)
            {
                __objects.Move(firstPos, secondPos);
                __objects.Move(secondPos + 1, firstPos);
            }
        }

        public void SwapDataModel(ObjectItemDataModel first, ObjectItemDataModel second)
        {
            SwapDataModel(__objects.IndexOf(second), __objects.IndexOf(first));
        }

        public void AddDataModel(ObjectItemDataModel dataModel, int pos = -1)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectItem = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T()
            {
                index = dataModel.Index,
                variable = dataModel.VariableSelection,
                binding = { enabled = dataModel.BindingEnable, module = dataModel.BindingModuleSelection, channel_name = dataModel.BindingChannelName, channel_index = dataModel.BindingChannelIndex },       
                converter = { enabled = dataModel.ConverterEnable, up_scale = dataModel.ConverterUpScale, down_scale = dataModel.ConverterDownScale, unit_name = dataModel.ConverterUnitName }
            };
            _data_helper.AddObjectData(objectItem);
            if(pos == -1)
                __objects.Add(dataModel);
            else
                __objects.Insert(pos, dataModel);
            __object_dictionary.Add(dataModel.Index, dataModel);
        }

        public void ModifyDataModel(uint index, ObjectItemDataModel dataModel)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectItem = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T()
            {
                index = dataModel.Index,
                variable = dataModel.VariableSelection,
                binding = { enabled = dataModel.BindingEnable, module = dataModel.BindingModuleSelection, channel_name = dataModel.BindingChannelName, channel_index = dataModel.BindingChannelIndex },
                converter = { enabled = dataModel.ConverterEnable, up_scale = dataModel.ConverterUpScale, down_scale = dataModel.ConverterDownScale, unit_name = dataModel.ConverterUnitName }
            };
            _data_helper.ModifyObjectData(index, objectItem);
            var data = __object_dictionary[index];
            data.ImportObjectDefinition(objectItem);
            if (dataModel.Index != index)
            {
                __object_dictionary.Remove(index);
                __object_dictionary.Add(data.Index, data);
            }
        }

        public void RemoveDataModel(ObjectItemDataModel dataModel)
        {
            _data_helper.RemoveObjectData(dataModel.Index);
            __objects.Remove(dataModel);
            __object_dictionary.Remove(dataModel.Index);
        }

        public void RemoveDataModel(int listPos)
        {
            var dataModel = __objects[listPos];
            _data_helper.RemoveObjectData(dataModel.Index);
            __objects.RemoveAt(listPos);
            __object_dictionary.Remove(dataModel.Index);
        }
    }

    public class ObjectItemDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
                return;
            storage = value;
            OnPropertyChanged(propertyName);
        }

        public ObjectItemDataModel(ObjectCollectionDataModel host, VariableDefinition variable, uint index = 0)
        {
            Host = host;
            Index = index;
            VariableSelection = variable;
        }

        public ObjectItemDataModel(ObjectCollectionDataModel host, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition)
        {
            Host = host;
            ImportObjectDefinition(objectDefinition);
        }

        public ObjectItemDataModel Clone()
        {
            return MemberwiseClone() as ObjectItemDataModel;
        }

        public void ImportObjectDefinition(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition)
        {
            Index = objectDefinition.index;
            VariableSelection = objectDefinition.variable;

            BindingEnable = objectDefinition.binding.enabled;
            if (BindingEnable == true)
                BindingModuleSelection = objectDefinition.binding.module;
            else
                BindingModuleSelection = null;
            BindingChannelName = objectDefinition.binding.channel_name;
            BindingChannelIndex = objectDefinition.binding.channel_index;

            ConverterEnable = objectDefinition.converter.enabled;
            //ConverterDataTypeSelection = objectDefinition.converter.data_type;
            ConverterUpScale = objectDefinition.converter.up_scale;
            ConverterDownScale = objectDefinition.converter.down_scale;
            ConverterUnitName = objectDefinition.converter.unit_name;
        }

        private uint __index;
        private VariableDefinition __variable_selection;
        private string __variable_name;
        private DataTypeDefinition __variable_data_type;

        private bool __binding_enable;
        private IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T __binding_module;
        private string __binding_channel_name;
        private int __binding_channel_index;

        private bool __converter_enable;
        //private DataTypeDefinition __converter_data_type;
        private int __converter_up_scale;
        private int __converter_down_scale;
        private string __converter_unit_name;

        public uint Index
        {
            get { return __index; }
            set { SetProperty(ref __index, value); }
        }

        public VariableDefinition VariableSelection
        {
            get { return __variable_selection; }
            set
            {
                __variable_selection = value;
                if(__variable_selection != null)
                {
                    VariableName = __variable_selection.Name;
                    VariableDataType = __variable_selection.DataType;
                }
            }
        }

        public string VariableName
        {
            get
            {
                return __variable_name;
            }
            set
            {
                SetProperty(ref __variable_name, value);
                try
                {
                    __variable_selection =
                        Host.DataHelper.VariableCatalogue.Variables[value];
                    VariableDataType = __variable_selection.DataType;
                }
                catch
                {
                    __variable_selection = null;
                }
            }
        }

        public DataTypeDefinition VariableDataType
        {
            get { return __variable_data_type; }
            private set { SetProperty(ref __variable_data_type, value); }
        }

        public bool BindingEnable
        {
            get { return __binding_enable; }
            set { SetProperty(ref __binding_enable, value); }
        }
        public IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T BindingModuleSelection
        {
            get { return __binding_module; }
            set { SetProperty(ref __binding_module, value); }
        }
        public string BindingChannelName
        {
            get { return __binding_channel_name; }
            set { SetProperty(ref __binding_channel_name, value); }
        }
        public int BindingChannelIndex
        {
            get { return __binding_channel_index; }
            set { SetProperty(ref __binding_channel_index, value); }
        }
        public string Binding
        {
            get
            {
                if (BindingEnable == false)
                    return "N/A";
                else
                    return string.Format("{0} -- [{1} : {2}]", BindingModuleSelection.reference_name, BindingChannelName, BindingChannelIndex);
            }
        }

        public bool ConverterEnable
        {
            get { return __converter_enable; }
            set { SetProperty(ref __converter_enable, value); }
        }
        /*
        public DataTypeDefinition ConverterDataTypeSelection
        {
            get { return __converter_data_type; }
            set { SetProperty(ref __converter_data_type, value); }
        }
        */
        public int ConverterUpScale
        {
            get { return __converter_up_scale; }
            set { SetProperty(ref __converter_up_scale, value); }
        }
        public int ConverterDownScale
        {
            get { return __converter_down_scale; }
            set { SetProperty(ref __converter_down_scale, value); }
        }
        public string ConverterUnitName
        {
            get { return __converter_unit_name; }
            set { SetProperty(ref __converter_unit_name, value); }
        }
        public string Converter
        {
            get
            {
                if (ConverterEnable == false)
                    return "N/A";
                else
                    //return string.Format("{0} -- [{1}, {2}] ({3})", ConverterDataTypeSelection.Name, ConverterDownScale, ConverterUpScale, ConverterUnitName);
                    return string.Format("[{0}, {1}] ({2})", ConverterDownScale, ConverterUpScale, ConverterUnitName);
            }
        }

        public ObjectCollectionDataModel Host { get; private set; }

        public override string ToString()
        {
            if((Index & 0x80000000) == 0)
                return string.Format("[0x{0:X8} : {1}] --> [{2} {3}]", Index, VariableName, Binding, Converter);
            else
                return string.Format("[0x{0:X8} : {1}] <-- [{2} {3}]", Index, VariableName, Binding, Converter);
        }
    }

    class ObjectItemBindingModule : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module = (IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T)value;
            if (module != null)
                return module.reference_name;
            else
                return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ObjectItemBindingString : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)values[0] == false || values[1] == null || values[2] == null)
                return "N/A";
            else
                return string.Format("{0} -- [{1} : {2}]", 
                    ((IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T)values[1]).reference_name, 
                    (string)values[2], (int)values[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ObjectItemConverterString : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            if ((bool)values[0] == false || values[1] == null || values[4] == null)
                return "N/A";
            else
                return string.Format("{0} -- [{1}, {2}] ({3})", ((DataTypeDefinition)values[1]).Name, (int)values[2], (int)values[3], (string)values[4]);
                */
            if ((bool)values[0] == false)
                return "N/A";
            else
                return string.Format("[{0}, {1}] ({2})", (int)values[1], (int)values[2], (string)values[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectItemFilter
    {
        public DataTypeDefinition DataType { get; set; }
        public string BindingModule { get; set; }
        public string VariableName { get; set; }
        private byte __filter_mask { get; set; }

        public ObjectItemFilter(DataTypeDefinition dataType, string bindingModule, string variableName)
        {
            DataType = dataType;
            BindingModule = bindingModule;
            VariableName = variableName;
            __filter_mask = 0;
        }

        public void EnableDataTypeFilter() { __filter_mask |= 0x01; }
        public void DisableDataTypeFilter() { __filter_mask &= 0xFE; }
        public void EnableBindingModuleFilter() { __filter_mask |= 0x02; }
        public void DisableBindingModuleFilter() { __filter_mask &= 0xFD; }
        public void EnableVariableNameFilter() { __filter_mask |= 0x04; }
        public void DisableVariableNameFilter() { __filter_mask &= 0xFB; }

        public void DisableFilter() { __filter_mask = 0x00; }
        public void EnableFilter() { __filter_mask = 0x07; }

        public bool FilterItem(object item)
        {
            byte res = 0;
            if (__filter_mask == 0)
                return true;

            if ((__filter_mask & 0x01) != 0)
                if ((item as ObjectItemDataModel).VariableDataType == DataType)
                    res |= 0x01;

            if ((__filter_mask & 0x02) != 0)
                if ((item as ObjectItemDataModel).Binding.StartsWith(BindingModule))
                    res |= 0x02;

            if ((__filter_mask & 0x04) != 0)
                if ((item as ObjectItemDataModel).VariableName.ToLower().Contains(VariableName.ToLower()))
                    res |= 0x04;

            return res == __filter_mask;
        }
    }

    class ObjectItemIndexToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("0x{0:X8}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToUInt32((string)value, 10);
            }
            catch
            {

            }
            try
            {
                return System.Convert.ToUInt32((string)value, 16);
            }
            catch
            {
                return new ArgumentException();
            }
        }
    }
}
