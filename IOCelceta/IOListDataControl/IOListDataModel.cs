using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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
        public abstract void InitializeDataModel(bool clearDirtyFlag = true);
        public abstract void FinalizeDataHelper(bool clearDirtyFlag = false);

        public int FieldDataBindingErrors { get; set; }
        private bool __dirty;
        public bool Dirty
        {
            get { return __dirty; }
            set { SetProperty(ref __dirty, value); }
        }
    }

    class MD5HashToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] data = (byte[])value;
            return
                $"{data[0]:X2}{data[1]:X2} - {data[2]:X2}{data[3]:X2} - " +
                $"{data[4]:X2}{data[5]:X2} - {data[6]:X2}{data[7]:X2} - " +
                $"{data[8]:X2}{data[9]:X2} - {data[10]:X2}{data[11]:X2} - " +
                $"{data[12]:X2}{data[13]:X2} - {data[14]:X2}{data[15]:X2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
