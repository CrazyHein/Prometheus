﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
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

        public override void InitializeDataModel(bool clearDirtyFlag = true)
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
                        module.model, module.device_switch, module.reference_name, module.local_address);
                    __extension_modules.Add(temp);
                    __controller_module_dictionary.Add(temp.ReferenceName, temp);
                }
                else if(module.model as ControllerEthernetModel != null)
                {
                    ControllerEthernetModuleItemDataModel temp = new ControllerEthernetModuleItemDataModel(this,
                        module.model, module.device_switch, module.reference_name, module.ip_address, module.port);
                    __ethernet_modules.Add(temp);
                    __controller_module_dictionary.Add(temp.ReferenceName, temp);
                }
            }
            if(clearDirtyFlag == true)
                Dirty = false;
        }

        public override void FinalizeDataHelper(bool clearDirtyFlag = false)
        {
            _data_helper.MCServerIPAddress = MCServerIPAddress;
            _data_helper.MCServerPort = MCServerPort;
            if (clearDirtyFlag == true)
                Dirty = false;
        }

        public string MCServerIPAddress
        {
            get { return __mc_server_ip_address; }
            set
            {
                if(IOListDataHelper.VALID_IPV4_ADDRESS.IsMatch(value) == true)
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
            Dirty = true;
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
            Dirty = true;
        }

        public void AddDataModel(ControllerModuleItemDataModel itemDataModel, int pos = -1)
        {
            if (itemDataModel is ControllerExtensionModuleItemDataModel)
            {
                var dataModel = itemDataModel as ControllerExtensionModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(dataModel.Model, dataModel.DeviceSwitch, dataModel.ReferenceName, dataModel.LocalAddress);
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
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(dataModel.Model, dataModel.DeviceSwitch, dataModel.ReferenceName, dataModel.IPAddress, dataModel.Port);
                _data_helper.AddControllerModule(module);
                if (pos == -1)
                    __ethernet_modules.Add(dataModel);
                else
                    __ethernet_modules.Insert(pos, dataModel);
                __controller_module_dictionary.Add(dataModel.ReferenceName, dataModel);
            }
            Dirty = true;
        }

        public void ModifyDataModel(string referenceName, ControllerModuleItemDataModel dataModel)
        {
            if (dataModel is ControllerExtensionModuleItemDataModel)
            {
                var sourceDataModel = dataModel as ControllerExtensionModuleItemDataModel;
                var destDataModel = __controller_module_dictionary[referenceName] as ControllerExtensionModuleItemDataModel;
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(sourceDataModel.Model, sourceDataModel.DeviceSwitch, sourceDataModel.ReferenceName, sourceDataModel.LocalAddress);
                _data_helper.ModifyControllerModule(referenceName, module);
                destDataModel.Model = sourceDataModel.Model;
                destDataModel.DeviceSwitch = sourceDataModel.DeviceSwitch;
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
                var module = new IO_LIST_CONTROLLER_INFORMATION_T.MODULE_T(sourceDataModel.Model, sourceDataModel.DeviceSwitch, sourceDataModel.ReferenceName, sourceDataModel.IPAddress, sourceDataModel.Port);
                _data_helper.ModifyControllerModule(referenceName, module);
                destDataModel.Model = sourceDataModel.Model;
                destDataModel.DeviceSwitch = sourceDataModel.DeviceSwitch;
                destDataModel.ReferenceName = sourceDataModel.ReferenceName;
                destDataModel.IPAddress = sourceDataModel.IPAddress;
                destDataModel.Port = sourceDataModel.Port;
                if (sourceDataModel.ReferenceName != referenceName)
                {
                    __controller_module_dictionary.Remove(referenceName);
                    __controller_module_dictionary.Add(destDataModel.ReferenceName, destDataModel);
                }
            }
            Dirty = true;
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
            Dirty = true;
        }

        public void RemoveExtensionDataModel(int listPos)
        {
            var dataModel = __extension_modules[listPos];
            _data_helper.RemoveControllerModule(dataModel.ReferenceName);
            __extension_modules.RemoveAt(listPos);
            __controller_module_dictionary.Remove(dataModel.ReferenceName);
            Dirty = true;
        }

        public void RemoveEthernetDataModel(int listPos)
        {
            var dataModel = __ethernet_modules[listPos];
            _data_helper.RemoveControllerModule(dataModel.ReferenceName);
            __ethernet_modules.RemoveAt(listPos);
            __controller_module_dictionary.Remove(dataModel.ReferenceName);
            Dirty = true;
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

        public ControllerModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, uint deviceSwitch, string referenceName)
        {
            Host = host;
            __model = model;
            ID = model.ID;
            DeviceSwitch = deviceSwitch;
            Name = model.Name;
            ReferenceName = referenceName;
        }

        private ControllerModel __model;
        private ushort __id;
        private uint __device_switch;
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

        public uint DeviceSwitch
        {
            get { return __device_switch; }
            set { SetProperty(ref __device_switch, value); }
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
        public ControllerExtensionModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, uint deviceSwitch = 0, string referenceName = "Extension_M0", 
            ushort localAddress = 0): base(host, model, deviceSwitch, referenceName)
        {
            LocalAddress = localAddress;
        }
        private ushort __local_address;
        public ushort LocalAddress
        {
            get { return __local_address; }
            set
            {
                if (value % 16 != 0)
                    throw new ArgumentException();
                SetProperty(ref __local_address, value);
            }
        }

        public override bool IsExtensionModule { get { return true; } }
        public override bool IsEthernetModule { get { return false; } }
    }

    public class ControllerEthernetModuleItemDataModel : ControllerModuleItemDataModel
    {
        public ControllerEthernetModuleItemDataModel(ControllerInformationDataModel host, ControllerModel model, uint deviceSwitch = 0, string referenceName = "Ethernet_M0", 
            string ip = "127.0.0.1", ushort port = 5010) : base(host, model, deviceSwitch, referenceName)
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
                if(IOListDataHelper.VALID_IPV4_ADDRESS.IsMatch(value) == true)
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
                ushort address = System.Convert.ToUInt16((string)value, 10);
                if (address % 16 != 0)
                    return new ArgumentException();
                else
                    return address;
            }
            catch
            {

            }
            try
            {
                ushort address =  System.Convert.ToUInt16((string)value, 16);
                if (address % 16 != 0)
                    return new ArgumentException();
                else
                    return address;
            }
            catch
            {
                return new ArgumentException();
            }
        }
    }

    class ModuleHexSwitchToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("0x{0:X8}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                uint sw = System.Convert.ToUInt32((string)value, 10);
                return sw;
            }
            catch
            {

            }
            try
            {
                uint sw = System.Convert.ToUInt32((string)value, 16);
                return sw;
            }
            catch
            {
                return new ArgumentException();
            }
        }
    }
}
