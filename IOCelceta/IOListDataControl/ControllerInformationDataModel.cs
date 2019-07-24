using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ControllerInformationDataModel : IOListDataModel
    {
        private string __mc_server_ip_address;
        private ushort __mc_server_port;

        private ObservableCollection<ControllerExtensionModuleItemDataModel> __extension_modules;
        private ObservableCollection<ControllerEthernetModuleItemDataModel> __ethernet_modules;
        private Dictionary<string, ControllerModuleItemDataModel> __controller_module_dictionary;

        public IReadOnlyList<ControllerExtensionModuleItemDataModel> ExtensionModules { get; private set; }
        public IReadOnlyList<ControllerEthernetModuleItemDataModel> EthernetModules { get; private set; }
        public IReadOnlyDictionary<string, ControllerModuleItemDataModel> ControllerModuleDictionary;

        public ControllerInformationDataModel(IOListDataHelper helper) : base(helper)
        {
            __extension_modules = new ObservableCollection<ControllerExtensionModuleItemDataModel>();
            __ethernet_modules = new ObservableCollection<ControllerEthernetModuleItemDataModel>();
            __controller_module_dictionary = new Dictionary<string, ControllerModuleItemDataModel>();
            ExtensionModules = __extension_modules;
            EthernetModules = __ethernet_modules;
            ControllerModuleDictionary = __controller_module_dictionary;
        }

        public override void UpdateDataModel()
        {
            MCServerIPAddress = _data_helper.MCServerIPAddress;
            MCServerPort = _data_helper.MCServerPort;

            __extension_modules.Clear();
            __ethernet_modules.Clear();
            foreach (var module in _data_helper.ControllerModuleCollection)
            {
                if(module.model as ControllerExtensionModel != null)
                {
                    ControllerExtensionModuleItemDataModel temp = new ControllerExtensionModuleItemDataModel(this,
                        module.model,module.reference_name, module.local_address);
                    __extension_modules.Add(temp);
                    __controller_module_dictionary.Add(temp.ReferenceName, temp);
                }
                else if(module.model as ControllerEthernetModel != null)
                {
                    ControllerEthernetModuleItemDataModel temp = new ControllerEthernetModuleItemDataModel(this,
                        module.model, module.reference_name, module.ip_address, module.port);
                    __ethernet_modules.Add(temp);
                    __controller_module_dictionary.Add(temp.ReferenceName, temp);
                }
            }
        }

        public override void UpdateDataHelper()
        {
            _data_helper.MCServerIPAddress = MCServerIPAddress;
            _data_helper.MCServerPort = MCServerPort;
        }

        public string MCServerIPAddress
        {
            get { return __mc_server_ip_address; }
            set
            {
                if (Regex.IsMatch(value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") == true)
                    SetProperty(ref __mc_server_ip_address, value);
                else
                    throw new ArgumentException();
            }
        }

        public ushort MCServerPort
        {
            get { return __mc_server_port; }
            set { SetProperty(ref __mc_server_port, value); }
        }

        public void SwapExtensionDataModel(int firstPos, int secondPos)
        {
            if (firstPos < secondPos)
            {
                __extension_modules.Move(secondPos, firstPos);
                __extension_modules.Move(firstPos + 1, secondPos);
            }
            else if (firstPos > secondPos)
            {
                __extension_modules.Move(firstPos, secondPos);
                __extension_modules.Move(secondPos + 1, firstPos);
            }
        }

        public void SwapEthernetDataModel(int firstPos, int secondPos)
        {
            if (firstPos < secondPos)
            {
                __ethernet_modules.Move(secondPos, firstPos);
                __ethernet_modules.Move(firstPos + 1, secondPos);
            }
            else if (firstPos > secondPos)
            {
                __ethernet_modules.Move(firstPos, secondPos);
                __ethernet_modules.Move(secondPos + 1, firstPos);
            }
        }

        public void AddDataModel(ControllerModuleItemDataModel itemDataModel, int pos = -1)
        {
            if (itemDataModel is ControllerExtensionModuleItemDataModel)
            {
                var dataModel = itemDataModel as ControllerExtensionModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T()
                {
                    model = dataModel.Model,
                    reference_name = dataModel.ReferenceName,
                    local_address = dataModel.LocalAddress,
                    ip_address = "127.0.0.1",
                    port = 5010
                };
                _data_helper.AddControllerModule(module);
                if (pos == -1)
                    __extension_modules.Add(dataModel);
                else
                    __extension_modules.Insert(pos, dataModel);
                __controller_module_dictionary.Add(dataModel.ReferenceName, dataModel);
            }
            else if (itemDataModel is ControllerEthernetModuleItemDataModel)
            {
                var dataModel = itemDataModel as ControllerEthernetModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T()
                {
                    model = dataModel.Model,
                    reference_name = dataModel.ReferenceName,
                    local_address = 0,
                    ip_address = dataModel.IPAddress,
                    port = dataModel.Port
                };
                _data_helper.AddControllerModule(module);
                if (pos == -1)
                    __ethernet_modules.Add(dataModel);
                else
                    __ethernet_modules.Insert(pos, dataModel);
                __controller_module_dictionary.Add(dataModel.ReferenceName, dataModel);
            }
        }

        public void ModifyDataModel(string referenceName, ControllerModuleItemDataModel dataModel)
        {
            if (dataModel is ControllerExtensionModuleItemDataModel)
            {
                var sourceDataModel = dataModel as ControllerExtensionModuleItemDataModel;
                var destDataModel = __controller_module_dictionary[referenceName] as ControllerExtensionModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T()
                {
                    model = sourceDataModel.Model,
                    reference_name = sourceDataModel.ReferenceName,
                    local_address = sourceDataModel.LocalAddress,
                    ip_address = "127.0.0.1",
                    port = 5010
                };
                _data_helper.ModifyControllerModule(referenceName, module);
                destDataModel.Model = sourceDataModel.Model;
                destDataModel.ReferenceName = sourceDataModel.ReferenceName;
                destDataModel.LocalAddress = sourceDataModel.LocalAddress;
                if (sourceDataModel.ReferenceName != referenceName)
                {
                    __controller_module_dictionary.Remove(referenceName);
                    __controller_module_dictionary.Add(destDataModel.ReferenceName, destDataModel);
                }
            }
            else if (dataModel is ControllerEthernetModuleItemDataModel)
            {
                var sourceDataModel = dataModel as ControllerEthernetModuleItemDataModel;
                var destDataModel = __controller_module_dictionary[referenceName] as ControllerEthernetModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T()
                {
                    model = sourceDataModel.Model,
                    reference_name = sourceDataModel.ReferenceName,
                    local_address = 0,
                    ip_address = sourceDataModel.IPAddress,
                    port = sourceDataModel.Port
                };
                _data_helper.ModifyControllerModule(referenceName, module);
                destDataModel.Model = sourceDataModel.Model;
                destDataModel.ReferenceName = sourceDataModel.ReferenceName;
                destDataModel.IPAddress = sourceDataModel.IPAddress;
                destDataModel.Port = sourceDataModel.Port;
                if (sourceDataModel.ReferenceName != referenceName)
                {
                    __controller_module_dictionary.Remove(referenceName);
                    __controller_module_dictionary.Add(destDataModel.ReferenceName, destDataModel);
                }
            }
        }

        public void RemoveDataModel(ControllerModuleItemDataModel dataModel)
        {  
            if(dataModel is ControllerExtensionModuleItemDataModel)
            {
                _data_helper.RemoveControllerModule(dataModel.ReferenceName);
                __extension_modules.Remove(dataModel as ControllerExtensionModuleItemDataModel);
                __controller_module_dictionary.Remove(dataModel.ReferenceName);
            }
            else if (dataModel is ControllerEthernetModuleItemDataModel)
            {
                _data_helper.RemoveControllerModule(dataModel.ReferenceName);
                __ethernet_modules.Remove(dataModel as ControllerEthernetModuleItemDataModel);
                __controller_module_dictionary.Remove(dataModel.ReferenceName);
            }
        }

        public void RemoveExtensionDataModel(int listPos)
        {
            var dataModel = __extension_modules[listPos];
            _data_helper.RemoveControllerModule(dataModel.ReferenceName);
            __extension_modules.RemoveAt(listPos);
            __controller_module_dictionary.Remove(dataModel.ReferenceName);
        }

        public void RemoveEthernetDataModel(int listPos)
        {
            var dataModel = __ethernet_modules[listPos];
            _data_helper.RemoveControllerModule(dataModel.ReferenceName);
            __ethernet_modules.RemoveAt(listPos);
            __controller_module_dictionary.Remove(dataModel.ReferenceName);
        }
    }

    public class ControllerModuleItemDataModel : INotifyPropertyChanged
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

        public ControllerModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, string referenceName)
        {
            Host = host;
            __model = model;
            ID = model.ID;
            Name = model.Name;
            ReferenceName = referenceName;
        }

        private ControllerModel __model;
        private ushort __id;
        private string __name;
        private string __reference_name;

        public ControllerModel Model
        {
            get { return __model; }
            set
            {
                __model = value;
                ID = __model.ID;
                Name = __model.Name;
            }
        }

        public ushort ID
        {
            get { return __id; }
            private set { SetProperty(ref __id, value); }
        }

        public string Name
        {
            get { return __name; }
            private set { SetProperty(ref __name, value); }
        }

        public string ReferenceName
        {
            get { return __reference_name; }
            set { SetProperty(ref __reference_name, value); }
        }

        public ControllerInformationDataModel Host { get; private set; }
        public virtual bool IsExtensionModule { get; }
        public virtual bool IsEthernetModule { get; }
    }

    public class ControllerExtensionModuleItemDataModel : ControllerModuleItemDataModel
    {
        public ControllerExtensionModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, string referenceName = "Extension_M0", 
            ushort localAddress = 0): base(host, model, referenceName)
        {
            LocalAddress = localAddress;
        }


        private ushort __local_address;
        public ushort LocalAddress
        {
            get { return __local_address; }
            set { SetProperty(ref __local_address, value); }
        }

        public override bool IsExtensionModule { get { return true; } }
        public override bool IsEthernetModule { get { return false; } }
    }

    public class ControllerEthernetModuleItemDataModel : ControllerModuleItemDataModel
    {
        public ControllerEthernetModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, string referenceName = "Ethernet_M0", 
            string ip = "127.0.0.1", ushort port = 5010) : base(host, model, referenceName)
        {
            IPAddress = ip;
            Port = port;
        }

        private string __ip;
        private ushort __port;

        public ushort Port
        {
            get { return __port; }
            set { SetProperty(ref __port, value); }
        }

        public string IPAddress
        {
            get { return __ip; }
            set
            {
                if (Regex.IsMatch(value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") == true)
                    SetProperty(ref __ip, value);
                else
                    throw new ArgumentException();
            }
        }

        public override bool IsExtensionModule { get { return false; } }
        public override bool IsEthernetModule { get { return true; } }
    }

    class ModuleDataFieldVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ModuleHexAddressToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("0x{0:X4}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToUInt16((string)value, 10);
            }
            catch
            {

            }
            try
            {
                return System.Convert.ToUInt16((string)value, 16);
            }
            catch
            {
                return new ArgumentException();
            }
        }
    }
}
