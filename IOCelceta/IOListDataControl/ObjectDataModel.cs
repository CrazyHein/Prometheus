using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ObjectDataModel
    {
        public DataTypeDefinition BasicDataTypeSelection { get; set; }
        public string FriendlyName { get; set; }
        public uint Index { get; set; }

        public bool BindingEnable { get; set; }
        public IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T BindingModuleSelection { get; set; }
        public string BindingChannelName { get; set; }
        public int BindingChannelIndex { get; set; }

        public bool ConverterEnable { get; set; }
        public DataTypeDefinition ConverterDataTypeSelection { get; set; }
        public int ConverterUpScale { get; set; }
        public int ConverterDownScale { get; set; }
        public string ConverterUnitName { get; set; }

        private bool __edit_mode;
        public ObjectCollectionDataModel HostDataModel { get; private set; }
        private ObjectItemDataModel __object_item_data_model;

        public ObjectDataModel(ObjectCollectionDataModel hostDataModel, ObjectItemDataModel dataModel, bool edit = true)
        {
            HostDataModel = hostDataModel;
            __object_item_data_model = dataModel;

            BasicDataTypeSelection = dataModel.DataType;
            FriendlyName = dataModel.FriendlyName;
            Index = dataModel.Index;

            BindingEnable = dataModel.Binding.enabled;
            BindingModuleSelection = dataModel.Binding.module;
            BindingChannelName = dataModel.Binding.channel_name;
            BindingChannelIndex = dataModel.Binding.channel_index;

            ConverterEnable = dataModel.Converter.enabled;
            ConverterDataTypeSelection = dataModel.Converter.data_type;
            ConverterUpScale = dataModel.Converter.up_scale;
            ConverterDownScale = dataModel.Converter.down_scale;
            ConverterUnitName = dataModel.Converter.unit_name;

            __edit_mode = edit;
        }

        public void UpdateHostDataModel()
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectItem = new IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T()
            {
                index = Index,
                friendly_name = FriendlyName,
                data_type = BasicDataTypeSelection,
                binding = { enabled = BindingEnable, module = BindingModuleSelection, channel_name = BindingChannelName, channel_index = BindingChannelIndex },
                converter = { enabled = ConverterEnable, data_type = ConverterDataTypeSelection, up_scale = ConverterUpScale, down_scale = ConverterDownScale, unit_name = ConverterUnitName }
            };
            if (__edit_mode == true)
            {
                HostDataModel.DataHelper.ModifyObjectData(__object_item_data_model.Index, objectItem);
                __object_item_data_model.UpdataDataModel(objectItem);
            }
            else
            {
                HostDataModel.DataHelper.AddObjectData(objectItem);
                HostDataModel.Objects.Add(new ObjectItemDataModel(objectItem));
            }
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
