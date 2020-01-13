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
            set { SetProperty(ref __dirty, value); }
        }


        protected TaskUserParametersHelper _data_helper;
        public IReadOnlyList<ControllerExtensionModel> AvailableExtensionModels { get; private set; }
        public IReadOnlyList<ControllerEthernetModel> AvailableEthernetModels { get; private set; }

        public TaskUserParametersDataModel(TaskUserParametersHelper dataHelper)
        {
            __extension_modules = new ObservableCollection<ExtensionModuleDataModel>();

            _data_helper = dataHelper;

            ExtensionModules = __extension_modules;
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
            if (clearDirtyFlag == true)
                Dirty = false;
        }

        public ushort HostCPUAddress
        {
            get { return __host_cpu_address; }
            set { SetProperty(ref __host_cpu_address, value); }
        }
        private ushort __host_cpu_address;
        
        public IReadOnlyList<ExtensionModuleDataModel> ExtensionModules { get; private set; }
        private ObservableCollection<ExtensionModuleDataModel> __extension_modules;
    }

    class ExtensionModuleDataModel : INotifyPropertyChanged
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

        public ExtensionModuleDataModel(TaskUserParametersDataModel host, CONTROLLER_EXTENSION_MODULE_T module)
        {
            Address = module.ADDRESS;
            Model = module.MODEL;
            AvailableModels = host.AvailableExtensionModels;
            __user_configurations = new List<ModuleUserConfiguration>(host.DataHelper.AvailableExtensionUserConfigurationFields.Count);
            if (module.USER_CONFIGURATIONS != null)
            {
                foreach (var f in host.DataHelper.AvailableExtensionUserConfigurationFields)
                {
                    if(module.USER_CONFIGURATIONS.Keys.Contains(f))
                        __user_configurations.Add(new ModuleUserConfiguration(f, module.USER_CONFIGURATIONS[f]));
                    else
                        __user_configurations.Add(new ModuleUserConfiguration(f));
                }
            }
            else
            {
                foreach (var f in host.DataHelper.AvailableExtensionUserConfigurationFields)
                    __user_configurations.Add(new ModuleUserConfiguration(f));
            }
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

        private ushort __address;
        public ushort Address
        {
            get { return __address; }
            set { SetProperty(ref __address, value); }
        }

        private ushort __bit_size;
        public ushort BitSize
        {
            get { return __bit_size; }
            set { SetProperty(ref __bit_size, value); }
        }

        private List<ModuleUserConfiguration> __user_configurations;
        public IReadOnlyList<ModuleUserConfiguration> UserConfigurations
        {
            get { return __user_configurations; }
        }
    }

    class ModuleUserConfiguration
    {
        public ModuleUserConfiguration(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public ModuleUserConfiguration(string name)
        {
            Name = name;
            Value = "";
        }

        public string Name { get; set; }
        public string Value { get; set; }
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
                ushort address = System.Convert.ToUInt16((string)value, 16);
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

}
