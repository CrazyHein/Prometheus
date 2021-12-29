using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public abstract class RecordContainerModel : INotifyPropertyChanged
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
            if (propertyName != "Modified")
                Modified = true;
            OnPropertyChanged(propertyName);
        }

        private bool __modified;
        public bool Modified
        {
            get { return __modified; }
            protected set { SetProperty(ref __modified, value); }
        }

        public virtual void CommitChanges()
        {
            Modified = false;
        }
    }
}
