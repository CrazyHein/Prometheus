using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
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
    }
}
