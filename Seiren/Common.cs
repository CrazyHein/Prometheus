﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Group = Syncfusion.Data.Group;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public enum InputDialogDisplayMode
    {
        Add,
        Insert,
        Edit,
        ReadOnly
    }

    internal class ConsoleControl
    {
        public static RoutedUICommand New { get; private set; }
        public static RoutedUICommand Open { get; private set; }
        public static RoutedUICommand Save { get; private set; }
        public static RoutedUICommand SaveAs { get; private set; }
        public static RoutedUICommand Close { get; private set; }
        public static RoutedUICommand Quit { get; private set; }
        public static RoutedUICommand Import { get; private set; }
        public static RoutedUICommand Export { get; private set; }

        public static RoutedUICommand UploadviaFTP { get; private set; }
        public static RoutedUICommand DownloadviaFTP { get; private set; }
        public static RoutedUICommand AddRecord { get; private set; }
        public static RoutedUICommand AddRecordEx { get; private set; }
        public static RoutedUICommand EditRecord { get; private set; }
        public static RoutedUICommand RemoveRecord { get; private set; }
        public static RoutedUICommand RemoveRecordEx { get; private set; }
        public static RoutedUICommand RemoveRecords { get; private set; }
        public static RoutedUICommand InsertRecord { get; private set; }
        public static RoutedUICommand InsertRecordEx { get; private set; }
        public static RoutedUICommand SearchRecord { get; private set; }
        public static RoutedUICommand Cancel { get; private set; }
        public static RoutedUICommand Confirm { get; private set; }
        public static RoutedUICommand About { get; private set; }

        public static RoutedUICommand StartDebugging { get; private set; }
        public static RoutedUICommand StopDebugging { get; private set; }


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

            InputGestureCollection gestureImport = new InputGestureCollection
            {
                new KeyGesture(Key.F2, ModifierKeys.Control, "Ctrl+F2")
            };
            Import = new RoutedUICommand("Import", "Import XML", typeof(ConsoleControl), gestureImport);
            InputGestureCollection gestureExport = new InputGestureCollection
            {
                new KeyGesture(Key.F3, ModifierKeys.Control, "Ctrl+F3")
            };
            Export = new RoutedUICommand("Export", "Export XML/XLS", typeof(ConsoleControl), gestureExport);

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

            InputGestureCollection gestureAddRecord = new InputGestureCollection
            {
                new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")
            };
            AddRecord = new RoutedUICommand("Add", "Add Record", typeof(ConsoleControl), gestureAddRecord);
            InputGestureCollection gestureAddRecordEx = new InputGestureCollection
            {
                new KeyGesture(Key.P, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+P")
            };
            AddRecordEx = new RoutedUICommand("Add", "Add Record", typeof(ConsoleControl), gestureAddRecordEx);

            InputGestureCollection gestureInsertRecord = new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")
            };
            InsertRecord = new RoutedUICommand("Insert", "Insert Record", typeof(ConsoleControl), gestureInsertRecord);
            InputGestureCollection gestureInsertRecordEx = new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+I")
            };
            InsertRecordEx = new RoutedUICommand("Insert", "Insert Record", typeof(ConsoleControl), gestureInsertRecordEx);

            InputGestureCollection gestureEditRecord = new InputGestureCollection
            {
                new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E")
            };
            EditRecord = new RoutedUICommand("Edit", "Edit Record", typeof(ConsoleControl), gestureEditRecord);

            InputGestureCollection gestureRemoveRecord = new InputGestureCollection
            {
                new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R")
            };
            RemoveRecord = new RoutedUICommand("Remove", "Remove Record", typeof(ConsoleControl), gestureRemoveRecord);

            Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(ConsoleControl));

            Confirm = new RoutedUICommand("Confirm", "Confirm", typeof(ConsoleControl));

            InputGestureCollection gestureAbout = new InputGestureCollection
            {
                new KeyGesture(Key.F1, ModifierKeys.Control, "Ctrl+F1")
            };
            About = new RoutedUICommand("About", "About", typeof(ConsoleControl), gestureAbout);

            InputGestureCollection gestureStartDebugging = new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Control, "Ctrl+F6")
            };
            StartDebugging = new RoutedUICommand("Start Debugging", "Start Debugging", typeof(ConsoleControl), gestureStartDebugging);

            InputGestureCollection gestureStopDebugging = new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+F6")
            };
            StopDebugging = new RoutedUICommand("Stop Debugging", "Stop Debugging", typeof(ConsoleControl), gestureStopDebugging);
        }
    }

    internal class ModifiedIndicatorToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = (bool)value;
            if (res)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ModifiedIndicatorToColor : IValueConverter
    {
        static SolidColorBrush red = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        static SolidColorBrush black = new SolidColorBrush(Color.FromArgb(255,0,0,0));
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

    class DataTypeComparer : IComparer<Object>, ISortDirection
    {
        public int Compare(Object x, Object y)
        {
            int res = 0;
            if (x.GetType() == typeof(VariableModel))
                res = (x as VariableModel).DataType.ToString().CompareTo((y as VariableModel).DataType.ToString());
            else if (x.GetType() == typeof(Group))
                res = (x as Group).Key.ToString().CompareTo((y as Group).Key.ToString());
            else
                res = x.ToString().CompareTo(y.ToString());
            if (res > 0)
                res = SortDirection == ListSortDirection.Ascending ? 1 : -1;
            else if (res < 0)
                res = SortDirection == ListSortDirection.Ascending ? -1 : 1;

            return res;
        }

        private ListSortDirection __sort_direction;

        public ListSortDirection SortDirection
        {
            get { return __sort_direction; }
            set { __sort_direction = value; }
        }
    }

    public class DataTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as DataType).Name;
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
            if(value is byte[] && targetType == typeof(string) && parameter is string)
            {
                byte[] array = value as byte[];
                if(array.Length == 0)
                    return string.Empty;
                else if(array.Length == 1)
                    return array[0].ToString("X02");

                StringBuilder sb = new StringBuilder(array[0].ToString("X02"));
                string sep = parameter as string;

                for(int i = 1; i < array.Length; ++i)
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

    public class ProcessDataImageAllowEditing : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessDataImageAccess && targetType == typeof(bool))
            {
                var access = (ProcessDataImageAccess)value;
                if (access == ProcessDataImageAccess.TX)
                    return false;
                else
                    return true;
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideStringValueColumn : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is bool && values[1] is ProcessDataImageLayout && targetType == typeof(bool))
            {
                if ((bool)values[0] == true || (ProcessDataImageLayout)values[1] == ProcessDataImageLayout.Bit)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideBooleanValueColumn : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is bool && values[1] is ProcessDataImageLayout && targetType == typeof(bool))
            {
                if ((bool)values[0] == true || (ProcessDataImageLayout)values[1] != ProcessDataImageLayout.Bit)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanValueIndicator : IValueConverter
    {
        public SolidColorBrush True { get; set; }
        public SolidColorBrush False { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean && targetType == typeof(Brush))
            {
                if ((bool)value == true)
                    return True;
                else
                    return False;
            }
            else
                return False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanValueToVisibility : IValueConverter
    {
        public System.Windows.Visibility True { get; set; } = Visibility.Visible;
        public System.Windows.Visibility False { get; set; } = Visibility.Collapsed;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(System.Windows.Visibility))
                return (bool)value ? True : False;
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WarningValueColumn : IMultiValueConverter
    {
        public SolidColorBrush True { get; set; }
        public SolidColorBrush False { get; set; }
        public SolidColorBrush Warning { get; set; }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is bool && values[1] is Boolean && targetType == typeof(Brush))
            {
                if ((bool)values[0] == true && (bool)values[1] == true)
                    return Warning;
                else if ((bool)values[0] == false)
                    return False;
                else
                    return True;
            }
            else
                return False;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HeartbeatIndicator : IValueConverter
    {
        private static char[] __indicator = new char[] { '|', '/', '-', '\\' };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint && targetType == typeof(string))
            {
                return __indicator[(uint)value/2 % (__indicator.Length)];
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
