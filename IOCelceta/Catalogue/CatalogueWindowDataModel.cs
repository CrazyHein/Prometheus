using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    public class CatalogueWindowDataModel
    {
        private readonly IEnumerable<DataTypeDefinition> __data_types;
        private readonly IEnumerable<ControllerExtensionModel> __extension_modules;
        private readonly IEnumerable<ControllerEthernetModel> __ethernet_modules;

        public bool UsedForDataTypesOnly { get; private set; }

        public CatalogueWindowDataModel(DataTypeCatalogue dataTypes)
        {
            __data_types = dataTypes.DataTypes.Values;
            UsedForDataTypesOnly = true;
        }

        public CatalogueWindowDataModel(IReadOnlyList<DataTypeDefinition> list)
        {
            __data_types = list;
            UsedForDataTypesOnly = true;
        }

        public CatalogueWindowDataModel(ControllerModelCatalogue modules, DataTypeCatalogue dataTypes)
        {
            __data_types = dataTypes.DataTypes.Values;
            __extension_modules = modules.ExtensionModels.Values;
            __ethernet_modules = modules.EthernetModels.Values;
            UsedForDataTypesOnly = false;
        }


        public IEnumerable<DataTypeDefinition> DataTypes
        {
            get { return __data_types; }
        }

        public IEnumerable<ControllerExtensionModel> ExtensionModules
        {
            get { return __extension_modules; }
        }

        public IEnumerable<ControllerEthernetModel> EthernetModules
        {
            get { return __ethernet_modules; }
        }
    }

    class DataTypeSubItemsToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class RxTxVariablesToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<string, int> dict = value as Dictionary<string, int>;
            if(dict != null && dict.Count != 0)
            {
                StringBuilder str = new StringBuilder();
                foreach(string key in dict.Keys)
                    str.AppendFormat("{0} : {1}\n", key, dict[key]);
                str.Remove(str.Length - 1, 1);
                return str;
            }
            else
                return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
