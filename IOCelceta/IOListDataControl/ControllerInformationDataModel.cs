using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class ControllerInformationDataModel : IOListDataModel
    {
        private string __mc_server_ip_address;
        public ushort __mc_server_port;

        public ObservableCollection<ControllerExtensionModuleDataModel> ExtensionModules { get; private set; }
        public ObservableCollection<ControllerEthernetModuleDataModel> EthernetModules { get; private set; }

        public ControllerInformationDataModel(IOListDataHelper helper) : base(helper)
        {
            ExtensionModules = new ObservableCollection<ControllerExtensionModuleDataModel>();
            EthernetModules = new ObservableCollection<ControllerEthernetModuleDataModel>();
        }

        public override void UpdateDataModel()
        {
            MCServerIPAddress = _data_helper.MCServerIPAddress;
            MCServerPort = _data_helper.MCServerPort;

            ExtensionModules.Clear();
            EthernetModules.Clear();
            foreach (var module in _data_helper.ControllerModuleCollection)
            {
                if(module.model as ControllerExtensionModel != null)
                {
                    ControllerExtensionModuleDataModel temp = new ControllerExtensionModuleDataModel(
                        module.model.ID, module.model.Name, module.reference_name, module.local_address);
                    ExtensionModules.Add(temp);
                }
                else if(module.model as ControllerEthernetModel != null)
                {
                    ControllerEthernetModuleDataModel temp = new ControllerEthernetModuleDataModel(
                        module.model.ID, module.model.Name, module.reference_name, module.ip_address, module.port);
                    EthernetModules.Add(temp);
                }
            }
        }

        public override void UpdateDataHelper()
        {
            throw new NotImplementedException();
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
    }

    public class ControllerExtensionModuleDataModel : INotifyPropertyChanged
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

        public ControllerExtensionModuleDataModel(ushort id, string name, string referenceName = "Extension_M0", ushort address = 0)
        {
            ID = id;
            Name = name;
            ReferenceName = referenceName;
            Address = address;
        }

        private ushort __id;
        private string __name;
        private string __reference_name;
        private ushort __address;

        public ushort ID
        {
            get { return __id; }
            set { SetProperty(ref __id, value); }
        }

        public string Name
        {
            get { return __name; }
            set { SetProperty(ref __name, value); }
        }

        public string ReferenceName
        {
            get { return __reference_name; }
            set { SetProperty(ref __reference_name, value); }
        }

        public ushort Address
        {
            get { return __address; }
            set { SetProperty(ref __address, value); }
        }
    }

    public class ControllerEthernetModuleDataModel : INotifyPropertyChanged
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

        public ControllerEthernetModuleDataModel(ushort id, string name, string referenceName = "Ethernet_M0", 
            string ip = "127.0.0.1", ushort port = 5010)
        {
            ID = id;
            Name = name;
            ReferenceName = referenceName;
            IPAddress = ip;
            Port = port;
        }

        private ushort __id;
        private string __name;
        private string __reference_name;
        private string __ip;
        private ushort __port;

        public ushort ID
        {
            get { return __id; }
            set { SetProperty(ref __id, value); }
        }

        public string Name
        {
            get { return __name; }
            set { SetProperty(ref __name, value); }
        }

        public string ReferenceName
        {
            get { return __reference_name; }
            set { SetProperty(ref __reference_name, value); }
        }

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
    }
}
