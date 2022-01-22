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

        private bool __is_monitorring = false;
        public bool IsMonitorring
        {
            get { return __is_monitorring; }
            set { SetProperty(ref __is_monitorring, value); }
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
    }
}
