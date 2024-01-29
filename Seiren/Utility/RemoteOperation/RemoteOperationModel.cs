using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    internal class RemoteOperationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RemoteOperationModel()
        {

        }

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }

        private REMOTE_OPERATION_T __remote_operation = REMOTE_OPERATION_T.RUN;
        public REMOTE_OPERATION_T RemoteOperation
        {
            get { return __remote_operation; }
            set { __remote_operation = value; OnPropertyChanged("RemoteOperation"); }
        }

        private REMOTE_CONTROL_MODE_T __remote_control_mode = REMOTE_CONTROL_MODE_T.FORCED_EXECUTION_NOT_ALLOWED;
        public REMOTE_CONTROL_MODE_T RemoteControlMode
        {
            get { return __remote_control_mode; }
            set { __remote_control_mode = value; OnPropertyChanged("RemoteControlMode"); }
        }

        private REMOTE_CLEAR_MODE_T __remote_clear_mode = REMOTE_CLEAR_MODE_T.DO_NOT_CLEAR_DEVICE;
        public REMOTE_CLEAR_MODE_T RemoteClearMode
        {
            get { return __remote_clear_mode; }
            set { __remote_clear_mode = value; OnPropertyChanged("RemoteClearMode"); }
        }

        private SMART_ECAT_COMMAND_T __ecat_command = SMART_ECAT_COMMAND_T.RESET_REQUEST;
        public SMART_ECAT_COMMAND_T ECATCommand
        {
            get { return __ecat_command; }
            set { __ecat_command = value; OnPropertyChanged("ECATCommand"); }
        }

        private ushort __ecat_module_address = 0x0000;
        public ushort ECATModuleAddress
        {
            get { return __ecat_module_address; }
            set { __ecat_module_address = value; OnPropertyChanged("ECATModuleAddress"); }
        }

        private int __ecat_command_waiting = 2000;
        public int ECATCommandWaiting
        {
            get { return __ecat_command_waiting; }
            set { __ecat_command_waiting = value; OnPropertyChanged("ECATCommandWaiting"); }
        }

        public const ushort ECATCommandRequestRegister = 16400;
        public const ushort ECATCommandResponseRegister = 32784;
    }
}
