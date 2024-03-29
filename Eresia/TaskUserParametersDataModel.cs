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
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Eresia
{
    class TaskUserParametersDataModel : INotifyPropertyChanged
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
            if (propertyName != "Dirty")
                Dirty = true;
            OnPropertyChanged(propertyName);
        }

        public int FieldDataBindingErrors { get; set; }
        private bool __dirty;
        public bool Dirty
        {
            get { return __dirty; }
            set
            {
                SetProperty(ref __dirty, value);
                if(value == false)
                {
                    foreach (var m in ExtensionModules)
                        m.Dirty = value;
                    foreach (var m in EthernetModules)
                        m.Dirty = value;
                }
            }
        }


        protected TaskUserParametersHelper _data_helper;
        public IReadOnlyList<ControllerExtensionModel> AvailableExtensionModels { get; private set; }
        public IReadOnlyList<ControllerEthernetModel> AvailableEthernetModels { get; private set; }

        public TaskUserParametersDataModel(TaskUserParametersHelper dataHelper)
        {
            __extension_modules = new ObservableCollection<ExtensionModuleDataModel>();
            __ethernet_modules = new ObservableCollection<EthernetModuleDataModel>();

            _data_helper = dataHelper;

            ExtensionModules = __extension_modules;
            EthernetModules = __ethernet_modules;
            AvailableExtensionModels = new List<ControllerExtensionModel>(dataHelper.AvailableExtensionModels);
            AvailableEthernetModels = new List<ControllerEthernetModel>(dataHelper.AvailableEthernetModels);
        }

        public TaskUserParametersHelper DataHelper
        {
            get { return _data_helper; }
        }

        public void UpdateDataHelper(bool clearDirtyFlag = false)
        {
            _data_helper.HostCPUAddress = HostCPUAddress;

            List<CONTROLLER_EXTENSION_MODULE_T> extensionModules = new List<CONTROLLER_EXTENSION_MODULE_T>(__extension_modules.Count);
            foreach(var dataModel in __extension_modules)
            {
               var userConfiguration = new List<Tuple<string, string>>(TaskUserParametersHelper.EXTENSION_MODULE_USER_FIELDS.Count);

                foreach(var c in dataModel.UserConfigurations)
                    userConfiguration.Add(new Tuple<string, string>(c.Name, c.Value.Trim()));
                extensionModules.Add(new CONTROLLER_EXTENSION_MODULE_T(dataModel.Model, dataModel.Switch, dataModel.Address, userConfiguration));
            }
            _data_helper.ImportModules(extensionModules, true);

            List<CONTROLLER_ETHERNET_MODULE_T> ethernetModules = new List<CONTROLLER_ETHERNET_MODULE_T>(__ethernet_modules.Count);
            foreach (var dataModel in __ethernet_modules)
            {
                var userConfiguration = new List<Tuple<string, string>>(TaskUserParametersHelper.ETHERNET_MODULE_USER_FIELDS.Count);
                foreach (var c in dataModel.UserConfigurations)
                    userConfiguration.Add(new Tuple<string, string>(c.Name, c.Value.Trim()));
                ethernetModules.Add(new CONTROLLER_ETHERNET_MODULE_T(dataModel.Model, dataModel.Switch, dataModel.IPAddress, dataModel.Port, userConfiguration));
            }
            _data_helper.ImportModules(ethernetModules, true);

            if (clearDirtyFlag == true)
                Dirty = false;
        }

        public void UpdateDataModel(bool clearDirtyFlag = true)
        {
            HostCPUAddress = _data_helper.HostCPUAddress;
            __extension_modules.Clear();
            foreach (var o in _data_helper.ControllerExtensionModules)
            {
                ExtensionModuleDataModel temp = new ExtensionModuleDataModel(this, o);
                __extension_modules.Add(temp);
            }
            __ethernet_modules.Clear();
            foreach (var o in _data_helper.ControllerEthernetModules)
            {
                EthernetModuleDataModel temp = new EthernetModuleDataModel(this, o);
                __ethernet_modules.Add(temp);
            }
            if (clearDirtyFlag == true)
                Dirty = false;
        }

        public ushort HostCPUAddress
        {
            get { return __host_cpu_address; }
            set
            {
                if (value % 16 != 0)
                    throw new ArgumentException();
                SetProperty(ref __host_cpu_address, value);
            }
        }
        private ushort __host_cpu_address;
        
        public IReadOnlyList<ExtensionModuleDataModel> ExtensionModules { get; private set; }
        private ObservableCollection<ExtensionModuleDataModel> __extension_modules;

        public IReadOnlyList<EthernetModuleDataModel> EthernetModules { get; private set; }
        private ObservableCollection<EthernetModuleDataModel> __ethernet_modules;

        public void AddExtensionModuleDataModel()
        {
            __extension_modules.Add(new ExtensionModuleDataModel(this,
                new CONTROLLER_EXTENSION_MODULE_T(AvailableExtensionModels[0], 0, 0x0000)));
        }

        public void InsertExtensionModuleDataModel(int pos)
        {
            __extension_modules.Insert(pos, new ExtensionModuleDataModel(this,
                new CONTROLLER_EXTENSION_MODULE_T(AvailableExtensionModels[0], 0, 0x0000)));
        }

        public void RemoveExtensionModuleDataModel(int pos)
        {
            __extension_modules.RemoveAt(pos);
            Dirty = true;
        }

        public void SwapExtensionModuleDataModel(int firstPos, int secondPos)
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

        public void AddEthernetModuleDataModel()
        {
            __ethernet_modules.Add(new EthernetModuleDataModel(this,
                new CONTROLLER_ETHERNET_MODULE_T(AvailableEthernetModels[0], 0,"192.168.0.1", 8366)));
        }

        public void InsertEthernetModuleDataModel(int pos)
        {
            __ethernet_modules.Insert(pos, new EthernetModuleDataModel(this,
                new CONTROLLER_ETHERNET_MODULE_T(AvailableEthernetModels[0], 0,"192.168.0.1", 8366)));
        }

        public void RemoveEthernetModuleDataModel(int pos)
        {
            __ethernet_modules.RemoveAt(pos);
            Dirty = true;
        }

        public void SwapEthernetModuleDataModel(int firstPos, int secondPos)
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
    }

    class ModuleDataModel : INotifyPropertyChanged
    {
        protected TaskUserParametersDataModel _host_data_model;

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
            if (propertyName != "Dirty")
                Dirty = true;
            OnPropertyChanged(propertyName);
        }
        private bool __dirty;
        public bool Dirty
        {
            get { return __dirty; }
            set
            {
                SetProperty(ref __dirty, value);
                if (value == true)
                    _host_data_model.Dirty = true;
            }
        }

        public ModuleDataModel(TaskUserParametersDataModel host)
        {
            _host_data_model = host;
        }
    }

    class ExtensionModuleDataModel : ModuleDataModel
    {
        public ExtensionModuleDataModel(TaskUserParametersDataModel host, CONTROLLER_EXTENSION_MODULE_T module) : base(host)
        {
            Switch = module.SWITCH;
            Address = module.ADDRESS;
            Model = module.MODEL;
            AvailableModels = host.AvailableExtensionModels;
            __user_configurations = new List<ModuleUserConfiguration>(TaskUserParametersHelper.EXTENSION_MODULE_USER_FIELDS.Count);
            foreach (var f in TaskUserParametersHelper.EXTENSION_MODULE_USER_FIELDS)
                __user_configurations.Add(new ModuleUserConfiguration(this, f, module.USER_CONFIGURATIONS[f]));
        }

        public IReadOnlyList<ControllerExtensionModel> AvailableModels { get; private set; }

        ControllerExtensionModel __model;
        public ControllerExtensionModel Model
        {
            get { return __model; }
            set
            {
                SetProperty(ref __model, value);
                BitSize = value.BitSize;
            }
        }

        uint __switch;
        public uint Switch
        {
            get { return __switch; }
            set { SetProperty(ref __switch, value); }
        }

        private ushort __address;
        public ushort Address
        {
            get { return __address; }
            set
            {
                if (value % 16 != 0)
                    throw new ArgumentException();
                SetProperty(ref __address, value);
            }
        }

        private ushort __bit_size;
        public ushort BitSize
        {
            get { return __bit_size; }
            private set { SetProperty(ref __bit_size, value); }
        }

        private List<ModuleUserConfiguration> __user_configurations;
        public IReadOnlyList<ModuleUserConfiguration> UserConfigurations
        {
            get { return __user_configurations; }
        }
    }

    class EthernetModuleDataModel : ModuleDataModel
    {
        public EthernetModuleDataModel(TaskUserParametersDataModel host, CONTROLLER_ETHERNET_MODULE_T module) : base(host)
        {
            Switch = module.SWITCH;
            IPAddress = module.IP_ADDRESS;
            Port = module.PORT;
            Model = module.MODEL;
            AvailableModels = host.AvailableEthernetModels;
            __user_configurations = new List<ModuleUserConfiguration>(TaskUserParametersHelper.ETHERNET_MODULE_USER_FIELDS.Count);
            foreach (var f in TaskUserParametersHelper.ETHERNET_MODULE_USER_FIELDS)
                __user_configurations.Add(new ModuleUserConfiguration(this, f, module.USER_CONFIGURATIONS[f]));
        }

        public IReadOnlyList<ControllerEthernetModel> AvailableModels { get; private set; }

        ControllerEthernetModel __model;
        public ControllerEthernetModel Model
        {
            get { return __model; }
            set { SetProperty(ref __model, value); }
        }

        uint __switch;
        public uint Switch
        {
            get { return __switch; }
            set { SetProperty(ref __switch, value); }
        }

        private string __ip_address;
        public string IPAddress
        {
            get { return __ip_address; }
            set
            {
                if(TaskUserParametersHelper.VALID_IPV4_ADDRESS.IsMatch(value) == true)
                    SetProperty(ref __ip_address, value);
                else
                    throw new ArgumentException();
            }
        }

        private ushort __port;
        public ushort Port
        {
            get { return __port; }
            set { SetProperty(ref __port, value); }
        }

        private List<ModuleUserConfiguration> __user_configurations;
        public IReadOnlyList<ModuleUserConfiguration> UserConfigurations
        {
            get { return __user_configurations; }
        }
    }

    class ModuleUserConfiguration
    {
        private string __name;
        private string __value;
        private ModuleDataModel __host_data_model;

        public ModuleUserConfiguration(ModuleDataModel host, string name, string value)
        {
            __host_data_model = host;
            __name = name;
            __value = value;
        }

        public ModuleUserConfiguration(ModuleDataModel host, string name)
        {
            __host_data_model = host;
            __name = name;
            __value = "";
        }

        public string Name
        {
            get { return __name; }
            set { __name = value; __host_data_model.Dirty = true; }
        }
        public string Value
        {
            get { return __value; }
            set { __value = value; __host_data_model.Dirty = true; }
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
                string str = (string)value;
                if (TaskUserParametersHelper.VALID_EXTENSION_MODULE_ADDRESS_FORMAT.IsMatch(str))
                    return System.Convert.ToUInt16(str, 16);
                else
                    return new ArgumentException();
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
                string str = (string)value;
                if (TaskUserParametersHelper.VALID_MODULE_SWITCH_FORMAT.IsMatch(str))
                    return System.Convert.ToUInt32(str, 16);
                else
                    return new ArgumentException();
            }
            catch
            {
                return new ArgumentException();
            }
        }
    }

    class ModuleDirtyStatusToWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(value) == true)
                return 3;
            else
                return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ModuleDirtyStatusToTitle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(value) == true)
                return "DIRTY";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
