using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
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
    public class ControllerModuleDataModel
    {
        private ControllerInformationDataModel __host_data_model;
        private ControllerExtensionModuleDataModel __extension_module_data_model;
        private ControllerEthernetModuleDataModel __ethernet_module_data_model;

        private IEnumerable<ControllerExtensionModel> __available_extension_modules;
        private IEnumerable<ControllerEthernetModel> __available_ethernet_modules;

        private bool __edit_mode;

        public ControllerModuleDataModel(ControllerInformationDataModel hostDataModel,
            ControllerExtensionModuleDataModel extensionDataModel, ControllerEthernetModuleDataModel ethernetDataModel,
            bool edit = true)
        {
            __host_data_model = hostDataModel;
            __extension_module_data_model = extensionDataModel;
            __ethernet_module_data_model = ethernetDataModel;

            __edit_mode = edit;

            if (__extension_module_data_model != null)
            {
                __available_extension_modules = __host_data_model.DataHelper.ControllerCatalogue.ExtensionModels.Values;
                ModuleModelInfo = __host_data_model.DataHelper.ControllerCatalogue.ExtensionModels[__extension_module_data_model.ID];
                LocalAddress = __extension_module_data_model.Address;
                ReferenceName = __extension_module_data_model.ReferenceName;
                IPAddress = "127.0.0.1";
            }
            else if(ethernetDataModel != null)
            {
                __available_ethernet_modules = __host_data_model.DataHelper.ControllerCatalogue.EthernetModels.Values;
                ModuleModelInfo = __host_data_model.DataHelper.ControllerCatalogue.EthernetModels[__ethernet_module_data_model.ID];
                ReferenceName = __ethernet_module_data_model.ReferenceName;
                IPAddress = __ethernet_module_data_model.IPAddress;
                Port = __ethernet_module_data_model.Port;
            }

        }

        public bool IsExtensionModule
        {
            get { return __extension_module_data_model != null; }
        }

        public ControllerModel ModuleModelInfo { get; set; }
        public ushort LocalAddress{get; set;}
        public string ReferenceName { get; set; }
        public string __ip_address;
        public string IPAddress
        {
            get
            {
                return __ip_address;
            }
            set
            {
                if (Regex.IsMatch(value, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$") == true)
                    __ip_address = value;
                else
                    throw new ArgumentException();
            }
        }
        public ushort Port { get; set; }

        public IEnumerable<ControllerExtensionModel> AvailableExtensionModules
        {
            get
            {
                if (__extension_module_data_model != null)
                    return __available_extension_modules;
                else
                    return null;
            }
        }

        public IEnumerable<ControllerEthernetModel> AvailableEthernetModules
        {
            get
            {
                if (__extension_module_data_model == null)
                    return __available_ethernet_modules;
                else
                    return null;
            }
        }

        public void UpdateHostDataModel()
        {
            IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T module;
            if (__edit_mode == true)
            {
                if (__extension_module_data_model != null)
                {
                    module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T() {
                        model = ModuleModelInfo, reference_name = ReferenceName, local_address = LocalAddress};
                    __host_data_model.DataHelper.ModifyControllerModule(__extension_module_data_model.ReferenceName, module);
                    __extension_module_data_model.ID = ModuleModelInfo.ID;
                    __extension_module_data_model.Name = ModuleModelInfo.Name;
                    __extension_module_data_model.Address = LocalAddress;
                    __extension_module_data_model.ReferenceName = ReferenceName;    
                }
                else if (__ethernet_module_data_model != null)
                {
                     module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T() {
                         model = ModuleModelInfo, reference_name = ReferenceName, ip_address = IPAddress, port = Port};
                    __host_data_model.DataHelper.ModifyControllerModule(__ethernet_module_data_model.ReferenceName, module);
                    __ethernet_module_data_model.ID = ModuleModelInfo.ID;
                    __ethernet_module_data_model.Name = ModuleModelInfo.Name;
                    __ethernet_module_data_model.IPAddress = IPAddress;
                    __ethernet_module_data_model.Port = Port;
                    __ethernet_module_data_model.ReferenceName = ReferenceName;
                }
            }
            else
            {
                if (__extension_module_data_model != null)
                {
                    module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T() {
                        model = ModuleModelInfo, reference_name = ReferenceName, local_address = LocalAddress};
                    __host_data_model.DataHelper.AddControllerModule(module);
                    __host_data_model.ExtensionModules.Add(new ControllerExtensionModuleDataModel(
                        ModuleModelInfo.ID, ModuleModelInfo.Name, ReferenceName, LocalAddress));
                }
                else if (__ethernet_module_data_model != null)
                {
                    module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T() {
                        model = ModuleModelInfo, reference_name = ReferenceName, ip_address = IPAddress, port = Port};
                    __host_data_model.DataHelper.AddControllerModule(module);
                    __host_data_model.EthernetModules.Add(new ControllerEthernetModuleDataModel(
                        ModuleModelInfo.ID, ModuleModelInfo.Name, ReferenceName, IPAddress, Port)); 
                }
            }
        }
    }

    class IsExtensionModuleDataField : IValueConverter
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

    class IsEthernetModuleDataField : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
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
