using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    public enum InputDialogDisplayMode
    {
        AddLocal,
        InsertLocal,
        EditLocal,
        AddRemote,
        InsertRemote,
        EditRemote,
    }
    internal class ConsoleControl
    {
        public static RoutedUICommand New { get; private set; }
        public static RoutedUICommand Open { get; private set; }
        public static RoutedUICommand Save { get; private set; }
        public static RoutedUICommand SaveAs { get; private set; }
        public static RoutedUICommand Close { get; private set; }
        public static RoutedUICommand Quit { get; private set; }
        public static RoutedUICommand UploadviaFTP { get; private set; }
        public static RoutedUICommand DownloadviaFTP { get; private set; }
        public static RoutedUICommand AddLocalModule { get; private set; }
        public static RoutedUICommand EditLocalModule { get; private set; }
        public static RoutedUICommand RemoveLocalModule { get; private set; }
        public static RoutedUICommand AddRemoteModule { get; private set; }
        public static RoutedUICommand EditRemoteModule { get; private set; }
        public static RoutedUICommand RemoveRemoteModule { get; private set; }
        public static RoutedUICommand RemoveRecords { get; private set; }
        public static RoutedUICommand InsertLocalModule { get; private set; }
        public static RoutedUICommand InsertRemoteModule { get; private set; }
        public static RoutedUICommand SearchRecord { get; private set; }
        public static RoutedUICommand Cancel { get; private set; }
        public static RoutedUICommand Confirm { get; private set; }
        public static RoutedUICommand About { get; private set; }


        static ConsoleControl()
        {
            InputGestureCollection gestureNew = new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N")
            };
            New = new RoutedUICommand("New", "Create a New File", typeof(ConsoleControl), gestureNew);

            InputGestureCollection gestureOpen = new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")
            };
            Open = new RoutedUICommand("Open", "Open Existing File", typeof(ConsoleControl), gestureOpen);

            InputGestureCollection gestureSave = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")
            };
            Save = new RoutedUICommand("Save", "Save File", typeof(ConsoleControl), gestureSave);

            InputGestureCollection gestureSaveAs = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Shift|ModifierKeys.Control, "Ctrl+Shift+S")
            };
            SaveAs = new RoutedUICommand("Save As", "Save File As", typeof(ConsoleControl), gestureSaveAs);

            Close = new RoutedUICommand("Close", "Close File", typeof(ConsoleControl));

            InputGestureCollection gestureQuit = new InputGestureCollection
            {
                new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q")
            };
            Quit = new RoutedUICommand("Quit", "Quit the Application", typeof(ConsoleControl), gestureQuit);

            InputGestureCollection gestureUploadviaFTP = new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Control, "Ctrl+F4")
            };
            UploadviaFTP = new RoutedUICommand("Upload", "Upload via FTP", typeof(ConsoleControl), gestureUploadviaFTP);

            InputGestureCollection gestureDownloadviaFTP = new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.Control, "Ctrl+F5")
            };
            DownloadviaFTP = new RoutedUICommand("Download", "Download via FTP", typeof(ConsoleControl), gestureDownloadviaFTP);

            AddLocalModule = new RoutedUICommand("Add", "Add Local Module", typeof(ConsoleControl));
            InsertLocalModule = new RoutedUICommand("Insert", "Insert Local Module", typeof(ConsoleControl));
            EditLocalModule = new RoutedUICommand("Edit", "Edit Local Module", typeof(ConsoleControl));
            RemoveLocalModule = new RoutedUICommand("Remove", "Remove Local Module", typeof(ConsoleControl));

            AddRemoteModule = new RoutedUICommand("Add", "Add Remote Module", typeof(ConsoleControl));
            InsertRemoteModule = new RoutedUICommand("Insert", "Insert Remote Module", typeof(ConsoleControl));
            EditRemoteModule = new RoutedUICommand("Edit", "Edit Remote Module", typeof(ConsoleControl));
            RemoveRemoteModule = new RoutedUICommand("Remove", "Remove Remote Module", typeof(ConsoleControl));

            Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(ConsoleControl));

            Confirm = new RoutedUICommand("Confirm", "Confirm", typeof(ConsoleControl));

            InputGestureCollection gestureAbout = new InputGestureCollection
            {
                new KeyGesture(Key.F1, ModifierKeys.Control, "Ctrl+F1")
            };
            About = new RoutedUICommand("About", "About", typeof(ConsoleControl), gestureAbout);
        }
    }

    internal class ModifiedIndicatorToColor : IValueConverter
    {
        static SolidColorBrush red = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        static SolidColorBrush black = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool res = (bool)value;
            if (res)
                return red;
            else
                return black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LocalAddressTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ushort)value).ToString("X04");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var address = System.Convert.ToUInt16((string)value, 16);
                if (address % 16 != 0)
                    throw new ArgumentException("Invalid Local Address");
                return address;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

    public class DeviceSwitchTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((uint)value).ToString("X08");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToUInt32((string)value, 16);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

    public class IPv4TextConverter : IValueConverter
    {
        public static Regex VALID_IPV4_ADDRESS { get; private set; } = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$", RegexOptions.Compiled);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (VALID_IPV4_ADDRESS.IsMatch((string)value))
                return value;
            else
                return new ArgumentException("Invalid Remote IPv4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (VALID_IPV4_ADDRESS.IsMatch((string)value))
                return value;
            else
                return new ArgumentException("Invalid Remote IPv4");
        }
    }

    public class InvertBooleanValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(bool))
                return !(bool)value;
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] && targetType == typeof(string) && parameter is string)
            {
                byte[] array = value as byte[];
                if (array.Length == 0)
                    return string.Empty;
                else if (array.Length == 1)
                    return array[0].ToString("X02");

                StringBuilder sb = new StringBuilder(array[0].ToString("X02"));
                string sep = parameter as string;

                for (int i = 1; i < array.Length; ++i)
                {
                    sb.Append(sep);
                    sb.Append(array[i].ToString("X02"));
                }
                return sb.ToString();
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class TabContentModel : INotifyPropertyChanged
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
