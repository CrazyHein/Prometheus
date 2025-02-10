using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    internal class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            storage = value;
            OnPropertyChanged(propertyName);
        }

        private string __currently_open_file;
        public string CurrentlyOpenFile
        {
            get 
            {
                if (__currently_open_file == null)
                    return "<N/A>";
                else if (__currently_open_file.Length == 0)
                    return "<unnamed>";
                return __currently_open_file; 
            }
            set 
            { 
                SetProperty(ref __currently_open_file, value);
                OnPropertyChanged("IsTemporaryFile");
                OnPropertyChanged("IsOpened");
            }
        }

        public bool IsTemporaryFile
        {
            get
            {
                return __currently_open_file?.Length == 0;
            }
        }

        public bool IsNonTemporaryFile
        {
            get
            {
                return __currently_open_file != null && __currently_open_file.Length != 0;
            }
        }

        public bool IsOpened
        {
            get
            {
                return __currently_open_file != null;
            }
        }

        private bool __is_offline = true;
        public bool IsOffline
        {
            get { return __is_offline; }
            set { SetProperty(ref __is_offline, value); }
        }

        private bool __is_monitoring = false;
        public bool IsMonitoring
        {
            get { return __is_monitoring; }
            set { SetProperty(ref __is_monitoring, value); }
        }

        private bool __is_data_acquisiting = false;
        public bool IsDataAcquisiting
        {
            get { return __is_data_acquisiting; }
            set { SetProperty(ref __is_data_acquisiting, value); }
        }

        private bool __is_busy = false;
        public bool IsBusy
        {
            get { return __is_busy; }
            set { SetProperty(ref __is_busy, value); }
        }

        private string __debugger_exception_message = "N/A";
        public string DebuggerExceptionMessage
        {
            get { return __debugger_exception_message; }
            set { if (value !=  __debugger_exception_message) SetProperty(ref __debugger_exception_message, value); }
        }

        private DataSynchronizerState __debugger_state = DataSynchronizerState.Ready;
        public DataSynchronizerState DebuggerState
        {
            get { return __debugger_state; }
            set { if (value != __debugger_state) SetProperty(ref __debugger_state, value); }
        }

        public int __debugger_polling_interval = 0;
        public int DebuggerPollingInterval
        {
            get { return __debugger_polling_interval; }
            set { SetProperty(ref __debugger_polling_interval, value); }
        }

        public uint __debugger_heartbeat = 0;
        public uint DebuggerHeartbeat
        {
            get { return __debugger_heartbeat; }
            set { SetProperty(ref __debugger_heartbeat, value); }
        }

        private string __debugger_target = "N/A";
        public string DebuggerTarget
        {
            get { return __debugger_target; }
            set { SetProperty(ref __debugger_target, value); }
        }

        private string __daq_unit_exception_message = "N/A";
        public string DAQUnitExceptionMessage
        {
            get { return __daq_unit_exception_message; }
            set { if (value != __daq_unit_exception_message) SetProperty(ref __daq_unit_exception_message, value); }
        }

        private AcquisitionUnitState __daq_unit_state = AcquisitionUnitState.Idle;
        public AcquisitionUnitState DAQUnitState
        {
            get { return __daq_unit_state; }
            set { if (value != __daq_unit_state) SetProperty(ref __daq_unit_state, value); }
        }

        public int __daq_unit_disk_write_interval = 0;
        public int DAQUnitDiskWriteInterval
        {
            get { return __daq_unit_disk_write_interval; }
            set { SetProperty(ref __daq_unit_disk_write_interval, value); }
        }

        public uint __daq_unit_heartbeat = 0;
        public uint DAQUnitHeartbeat
        {
            get { return __daq_unit_heartbeat; }
            set { SetProperty(ref __daq_unit_heartbeat, value); }
        }

        public AcquisitionUnitStatus __daq_unit_status;
        public AcquisitionUnitStatus DAQUnitStatus
        {
            get { return __daq_unit_status; }
            set { SetProperty(ref __daq_unit_status, value); }
        }

        private string __daq_unit_target = "N/A";
        public string DAQUnitTarget
        {
            get { return __daq_unit_target; }
            set { SetProperty(ref __daq_unit_target, value); }
        }

        private IReadOnlyList<OperatingRecord> __undo_operating_records;
        public IReadOnlyList<OperatingRecord> UndoOperatingRecords 
        { 
            get { return __undo_operating_records; }
            set { SetProperty(ref __undo_operating_records, value); }
        }
        private IReadOnlyList<OperatingRecord> __redo_operating_records;
        public IReadOnlyList<OperatingRecord> RedoOperatingRecords
        {
            get { return __redo_operating_records; }
            set { SetProperty(ref __redo_operating_records, value); }
        }

        public bool CanUndo { get { return __undo_operating_records.Count != 0; } }
        public bool CanRedo { get { return __redo_operating_records.Count != 0; } }

        private IReadOnlyList<string> __recently_opened;
        public IReadOnlyList<string> RecentlyOpened
        {
            get { return __recently_opened; }
            set { SetProperty(ref __recently_opened, value); }
        }
    }
}
