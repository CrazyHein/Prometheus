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
        public bool UsedForDataTypesOnly { get; private set; }

        public CatalogueWindowDataModel(DataTypeCatalogue dataTypes)
        {
            DataTypes = dataTypes.DataTypes.Values;
            UsedForDataTypesOnly = true;
        }

        public CatalogueWindowDataModel(IReadOnlyList<DataTypeDefinition> list)
        {
            DataTypes = list;
            UsedForDataTypesOnly = true;
        }

        public CatalogueWindowDataModel(ControllerModelCatalogue modules, DataTypeCatalogue dataTypes, VariableCatalogue variables)
        {
            DataTypes = dataTypes.DataTypes.Values;
            ExtensionModules = modules.ExtensionModels.Values;
            EthernetModules = modules.EthernetModels.Values;
            Variables = variables.Variables.Values;
            UsedForDataTypesOnly = false;
        }


        public IEnumerable<DataTypeDefinition> DataTypes { get; }

        public IEnumerable<ControllerExtensionModel> ExtensionModules { get; }

        public IEnumerable<ControllerEthernetModel> EthernetModules { get; }

        public IEnumerable<VariableDefinition> Variables { get; }
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
            Dictionary<string, uint> dict = value as Dictionary<string, uint>;
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
