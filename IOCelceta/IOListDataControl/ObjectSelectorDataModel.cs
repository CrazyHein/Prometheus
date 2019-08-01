using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ObjectSelectorDataModel
    {
        private IReadOnlyDictionary<uint, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> __object_dictionary;
        public IEnumerable<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> AvailableObjects { get; private set; }
        public IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T SelectedObject { get; set; }
        public ObjectDefinitionFilter DataFilter { get; private set; }
        public IReadOnlyDictionary<string, DataTypeDefinition> DataTypes { get; private set; }

        public ObjectSelectorDataModel(IReadOnlyDictionary<uint, IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> dict,  IReadOnlyDictionary<string, DataTypeDefinition> dataTypes)
        {
            __object_dictionary = dict;
            AvailableObjects = dict.Values;
            DataFilter = new ObjectDefinitionFilter(null, null, null);
            DataTypes = dataTypes;
        }
    }

    public class ObjectDefinitionFilter
    {
        public DataTypeDefinition DataType { get; set; }
        public string BindingModule { get; set; }
        public string VariableName { get; set; }
        private byte __filter_mask { get; set; }

        public ObjectDefinitionFilter(DataTypeDefinition dataType, string bindingModule, string variableName)
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

        public bool DataTypeFilterEnabled { get { return (__filter_mask & 0x01) != 0; } }
        public bool BindingModuleFilterEnabled { get { return (__filter_mask & 0x02) != 0; } }
        public bool VariableNameFilterEnabled { get { return (__filter_mask & 0x04) != 0; } }

        public bool FilterItem(object item)
        {
            byte res = 0;
            if (__filter_mask == 0)
                return true;

            if ((__filter_mask & 0x01) != 0)
                if ((item as IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T).variable.DataType == DataType)
                    res |= 0x01;

            if ((__filter_mask & 0x02) != 0)
                if ((item as IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T).binding.ToString().StartsWith(BindingModule))
                    res |= 0x02;

            if ((__filter_mask & 0x04) != 0)
                if ((item as IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T).variable.Name.ToLower().Contains(VariableName.ToLower()))
                    res |= 0x04;

            return res == __filter_mask;
        }
    }
}
