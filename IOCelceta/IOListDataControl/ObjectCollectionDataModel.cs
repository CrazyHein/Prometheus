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
        public ObservableCollection<ObjectItemDataModel> Objects { get; private set; }

        public PropertyGroupDescription DataTypeGroupDescription { get; private set; }
        public PropertyGroupDescription BindingModuleGroupDescription { get; private set; }
        public ObjectItemFilter ItemFilter { get; private set; }

        public ObjectCollectionDataModel(IOListDataHelper helper):base(helper)
        {
            Objects = new ObservableCollection<ObjectItemDataModel>();
            DataTypeGroupDescription = new PropertyGroupDescription("DataType");
            BindingModuleGroupDescription = new PropertyGroupDescription("Binding", new ObjectItemBindingModule());
            ItemFilter = new ObjectItemFilter(null, null);
        }

        public override void UpdateDataHelper()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDataModel()
        {
            Objects.Clear();
            foreach(var o in _data_helper.IOObjectCollection)
            {
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T definition = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T()
                {
                    index = o.index,
                    binding = o.binding,
                    converter = o.converter,
                    data_type = o.data_type,
                    friendly_name = o.friendly_name
                };
                ObjectItemDataModel temp = new ObjectItemDataModel(definition);

                Objects.Add(temp);
            }
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

        public ObjectItemDataModel(DataTypeDefinition dataType, uint index = 0, string friendlyName = "New Object")
        {
            DataType = dataType;
            Index = index;
            FriendlyName = friendlyName;
            Binding = new IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T();
            Converter = new IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T();
        }

        public ObjectItemDataModel(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition)
        {
            UpdataDataModel(objectDefinition);
        }

        public void UpdataDataModel(IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectDefinition)
        {
            Index = objectDefinition.index;
            FriendlyName = objectDefinition.friendly_name;
            DataType = objectDefinition.data_type;
            Binding = objectDefinition.binding;
            Converter = objectDefinition.converter;
        }

        private uint __index;
        private string __friendly_name;
        private DataTypeDefinition __data_type;
        private IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T __binding_definition;
        private IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T __converter_definition;

        public uint Index
        {
            get { return __index; }
            set { SetProperty(ref __index, value); }
        }

        public string FriendlyName
        {
            get { return __friendly_name; }
            set { SetProperty(ref __friendly_name, value); }
        }
        public DataTypeDefinition DataType
        {
            get { return __data_type; }
            set { SetProperty(ref __data_type, value); }
        }
        public IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T Binding
        {
            get { return __binding_definition; }
            set { SetProperty(ref __binding_definition, value); }
        }
        public IO_LIST_OBJECT_COLLECTION_T.VALUE_CONVERTER_T Converter
        {
            get { return __converter_definition; }
            set { SetProperty(ref __converter_definition, value); }
        }
    }

    class ObjectItemBindingModule : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T binding = (IO_LIST_OBJECT_COLLECTION_T.MODULE_BINDING_T)value;
            if (binding.enabled == true)
                return binding.module.reference_name;
            else
                return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectItemFilter
    {
        public string DataTypeName { get; set; }
        public string BindingModule { get; set; }
        private byte __filter_mask { get; set; }

        public ObjectItemFilter(string dataTypeName, string bindingModule)
        {
            DataTypeName = dataTypeName;
            BindingModule = bindingModule;
            __filter_mask = 0;
        }

        public void EnableDataTypeFilter() { __filter_mask |= 0x01; }
        public void DisableDataTypeFilter() { __filter_mask &= 0xFE; }
        public void EnableBindingModuleFilter() { __filter_mask |= 0x02; }
        public void DisableBindingModuleFilter() { __filter_mask &= 0xFD; }

        public bool FilterItem(object item)
        {
            if (__filter_mask == 0)
                return true;
            else if (__filter_mask == 0x01)
                return (item as ObjectItemDataModel).DataType.Name == DataTypeName;
            else if (__filter_mask == 0x02)
                return (item as ObjectItemDataModel).Binding.ToString().StartsWith(BindingModule);
            else
                return (item as ObjectItemDataModel).DataType.Name == DataTypeName && (item as ObjectItemDataModel).Binding.ToString().StartsWith(BindingModule);
        }
    }
}
