using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public abstract class IOListDataModel : INotifyPropertyChanged
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
            if(propertyName != "Dirty")
                Dirty = true;
            OnPropertyChanged(propertyName);
        }

        public IOListDataModel(IOListDataHelper dataHelper)
        {
            _data_helper = dataHelper;
        }

        public IOListDataHelper DataHelper
        {
            get { return _data_helper; }
        }

        protected IOListDataHelper _data_helper;
        public abstract void UpdateDataModel(bool clearDirtyFlag = true);
        public abstract void UpdateDataHelper(bool clearDirtyFlag = false);

        public int FieldDataBindingErrors { get; set; }
        private bool __dirty;
        public bool Dirty
        {
            get { return __dirty; }
            set { SetProperty(ref __dirty, value); }
        }
    }
}
