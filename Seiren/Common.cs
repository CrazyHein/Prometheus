﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage;
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public static RoutedUICommand DefaultRecord { get; private set; }
        public static RoutedUICommand RecordRedo { get; private set; }
        public static RoutedUICommand RecordUndo { get; private set; }
        public static RoutedUICommand Cancel { get; private set; }
        public static RoutedUICommand Confirm { get; private set; }
        public static RoutedUICommand About { get; private set; }
        public static RoutedUICommand StartMonitoring { get; private set; }
        public static RoutedUICommand StartDebugging { get; private set; }
        public static RoutedUICommand StopDebugging { get; private set; }
        public static RoutedUICommand MoveUpRecord { get; private set; }
        public static RoutedUICommand MoveDownRecord { get; private set; }
        public static RoutedUICommand UploadCompare { get; private set; }
        public static RoutedUICommand ImportCompare { get; private set; }
        public static RoutedUICommand OpenCompare { get; private set; }
        public static RoutedUICommand SaveLayoutState { get; private set; }
        public static RoutedUICommand LoadLayoutState { get; private set; }
        public static RoutedUICommand FindInInterlock { get; private set; }
        public static RoutedUICommand FindInProcessDataImage { get; private set; }
        public static RoutedUICommand RemoveUnused { get; private set; }
        public static RoutedUICommand RemoveAllUnused { get; private set; }
        public static RoutedUICommand ControllerRemoteOperation { get; private set; }
        public static RoutedUICommand BrowseEtherCATPDOs { get; private set; }
        public static RoutedUICommand AddAllSelectedRecords { get; private set; }
        public static RoutedUICommand EventHistory { get; private set; }
        public static RoutedUICommand SetDAQFlag { get; private set; }
        public static RoutedUICommand ResetDAQFlag { get; private set; }
        public static RoutedUICommand StartBackgroundDAQ { get; private set; }
        public static RoutedUICommand StopBackgroundDAQ { get; private set; }
        public static RoutedUICommand BrowseCIPAssemblyIOs { get; private set; }
        public static RoutedUICommand SmartECATUtility { get; private set; }

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
            InputGestureCollection gestureOpenCompare = new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+O")
            };
            OpenCompare = new RoutedUICommand("Compare", "Compare via Open", typeof(ConsoleControl), gestureOpenCompare);

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
            InputGestureCollection gestureImportCompare = new InputGestureCollection
            {
                new KeyGesture(Key.F2, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+F2")
            };
            ImportCompare = new RoutedUICommand("Compare", "Compare via Import", typeof(ConsoleControl), gestureImportCompare);

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

            InputGestureCollection gestureUploadCompare = new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+F4")
            };
            UploadCompare = new RoutedUICommand("Compare", "Compare via FTP", typeof(ConsoleControl), gestureUploadCompare);

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

            InputGestureCollection gestureDefaultRecord = new InputGestureCollection
            {
                new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
            };
            DefaultRecord = new RoutedUICommand("Set As Default", "Set As Default Record", typeof(ConsoleControl), gestureDefaultRecord);

            InputGestureCollection gestureRecordRedo = new InputGestureCollection
            {
                new KeyGesture(Key.Y, ModifierKeys.Control, "Ctrl+Y")
            };
            RecordRedo = new RoutedUICommand("Redo", "Redo", typeof(ConsoleControl), gestureRecordRedo);

            InputGestureCollection gestureRecordUndo = new InputGestureCollection
            {
                new KeyGesture(Key.Z, ModifierKeys.Control, "Ctrl+Z")
            };
            RecordUndo = new RoutedUICommand("Undo", "Undo", typeof(ConsoleControl), gestureRecordUndo);

            Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(ConsoleControl));

            Confirm = new RoutedUICommand("Confirm", "Confirm", typeof(ConsoleControl));

            InputGestureCollection gestureAbout = new InputGestureCollection
            {
                new KeyGesture(Key.F1, ModifierKeys.Control, "Ctrl+F1")
            };
            About = new RoutedUICommand("About", "About", typeof(ConsoleControl), gestureAbout);

            InputGestureCollection gestureStartMonitoring = new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Control, "Ctrl+F7")
            };
            StartMonitoring = new RoutedUICommand("Start Monitoring", "Start Monitoring", typeof(ConsoleControl), gestureStartMonitoring);

            InputGestureCollection gestureStartDebugging = new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Control, "Ctrl+F6")
            };
            StartDebugging = new RoutedUICommand("Start Debugging", "Start Debugging", typeof(ConsoleControl), gestureStartDebugging);

            InputGestureCollection gestureStopDebugging = new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+F6")
            };
            StopDebugging = new RoutedUICommand("Stop Monitoring/Debugging", "Stop Monitoring/Debugging", typeof(ConsoleControl), gestureStopDebugging);

            MoveUpRecord = new RoutedUICommand("Move Up", "Move Up", typeof(ConsoleControl));
            MoveDownRecord = new RoutedUICommand("Move Down", "Move Down", typeof(ConsoleControl));
            SaveLayoutState = new RoutedUICommand("Save Layout", "Save Layout", typeof(ConsoleControl));
            LoadLayoutState = new RoutedUICommand("Load Layout", "Load Layout", typeof(ConsoleControl));

            FindInInterlock = new RoutedUICommand("Find In <Interlock Area>", "Find In <Interlock Area>", typeof(ConsoleControl));
            FindInProcessDataImage = new RoutedUICommand("Find In <Process Data Image Area>", "Find In <Process Data Image Area>", typeof(ConsoleControl));
            RemoveUnused = new RoutedUICommand("Remove Unused", "Remove Unused", typeof(ConsoleControl));
            RemoveAllUnused = new RoutedUICommand("Remove All Unused", "Remove All Unused", typeof(ConsoleControl));

            InputGestureCollection gestureControllerRemoteOperation = new InputGestureCollection
            {
                new KeyGesture(Key.F8, ModifierKeys.Control, "Ctrl+F8")
            };
            ControllerRemoteOperation = new RoutedUICommand("Remote Operation", "Remote Operation", typeof(ConsoleControl), gestureControllerRemoteOperation);

            InputGestureCollection gestureImportEtherCATPDOs = new InputGestureCollection
            {
                new KeyGesture(Key.F9, ModifierKeys.Control, "Ctrl+F9")
            };
            BrowseEtherCATPDOs = new RoutedUICommand("Browse EtherCAT PDOs", "Browse PDOs in EtherCAT ENI", typeof(ConsoleControl), gestureImportEtherCATPDOs);

            AddAllSelectedRecords = new RoutedUICommand("Add All Selected Records", "Add All Selected Records", typeof(ConsoleControl));

            InputGestureCollection gestureEventHistory = new InputGestureCollection
            {
                new KeyGesture(Key.F10, ModifierKeys.Control, "Ctrl+F10")
            };
            EventHistory = new RoutedUICommand("Event History", "Browse R12CCPU-V Event History", typeof(ConsoleControl), gestureEventHistory);

            SetDAQFlag = new RoutedUICommand("Set DAQ Flag", "Set DAQ Flag", typeof(ConsoleControl));
            ResetDAQFlag = new RoutedUICommand("Reset DAQ Flag", "Reset DAQ Flag", typeof(ConsoleControl));

            StartBackgroundDAQ = new RoutedUICommand("Start Background DAQ", "Start Background DAQ", typeof(ConsoleControl));
            StopBackgroundDAQ = new RoutedUICommand("Stop Background DAQ", "Stop Background DAQ", typeof(ConsoleControl));

            InputGestureCollection gestureImportCIPAssemblyIOs = new InputGestureCollection
            {
                new KeyGesture(Key.F11, ModifierKeys.Control, "Ctrl+F11")
            };
            BrowseCIPAssemblyIOs = new RoutedUICommand("Browse CIP Assembly IO", "Browse CIP Assembly IO in IO Allocation File", typeof(ConsoleControl), gestureImportCIPAssemblyIOs);

            InputGestureCollection gestureSmartECATUtility = new InputGestureCollection
            {
                new KeyGesture(Key.F12, ModifierKeys.Control, "Ctrl+F12")
            };
            SmartECATUtility = new RoutedUICommand("SMART-ECAT Utility", "Open SMART-ECAT Utility Window", typeof(ConsoleControl), gestureSmartECATUtility);
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

        public SolidColorBrush Unchanged { get; set; } = null;
        public SolidColorBrush Changed { get; set; } = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            bool res = (bool)value;
            if (res)
                return Changed??red;
            else
                return Unchanged??black;
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

    public class DebuggingModeIndicator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Boolean && values[1] is Boolean && targetType == typeof(string))
            {
                bool isoffline = (bool)values[0];
                bool ismonitoring  = (bool)values[1];
                if (isoffline == true)
                    return "Offline";
                else if (ismonitoring == true)
                    return "Monitoring";
                return "Debugging";

            }
            else
                throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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

    public class ProcessDataImageAllowEditing : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ProcessDataImageAccess && values[1] is Boolean && values[2] is Boolean && targetType == typeof(bool))
            {
                var access = (ProcessDataImageAccess)values[0];
                var monitoring = (bool)values[1];
                var isoffline = (bool)values[2];
                if (monitoring == true || isoffline == true)
                    return false;
                else
                {
                    if (access == ProcessDataImageAccess.TX)
                        return false;
                    else
                        return true;
                }
            }
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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

    public class CanOpenOrNew : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is bool && values[1] is bool && targetType == typeof(bool))
            {
                //__main_model.IsOffline && !__main_model.IsBusy && 
                bool offline = (bool)values[0];
                bool busy = (bool)values[1];
                return offline && !busy;
            }
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal enum REMOTE_OPERATION_T
    {
        RUN,
        STOP,
        RESET,
        PAUSE,
        LATCH_CLEAR,
        READ_TYPE
    }

    internal enum SMART_ECAT_COMMAND_T: UInt16
    {
        RESET_REQUEST = 0x0000,
        REQUEST_COMMUNICATION_STOP = 0x0001,
        REQUEST_CLEAR_EVENT_INFO = 0x0002,
        REQUEST_REBOOT = 0x0003
    }

    public class HEX16StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ushort)value).ToString("X4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToUInt16((string)value, 16);
            }
            catch(Exception ex)
            { 
                return ex ; 
            }
        }
    }

    public class HEX32StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((uint)value).ToString("X8");
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

    public class RemoteOperationImage : IValueConverter
    {
        static System.Windows.Media.Imaging.BitmapImage RUN = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/startdebugging.png", UriKind.Relative));
        static System.Windows.Media.Imaging.BitmapImage STOP = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/stop.png", UriKind.Relative));
        static System.Windows.Media.Imaging.BitmapImage RESET = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/reset.png", UriKind.Relative));
        static System.Windows.Media.Imaging.BitmapImage PAUSE = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/stopdebugging.png", UriKind.Relative));
        static System.Windows.Media.Imaging.BitmapImage LATCH_CLEAR = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/clear.png", UriKind.Relative));
        static System.Windows.Media.Imaging.BitmapImage READ_TYPE = new System.Windows.Media.Imaging.BitmapImage(new Uri("/imgs/type.png", UriKind.Relative));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is REMOTE_OPERATION_T && targetType == typeof(System.Windows.Media.ImageSource))
            {
                switch((REMOTE_OPERATION_T)value)
                {
                    case REMOTE_OPERATION_T.RUN:
                        return RUN;
                    case REMOTE_OPERATION_T.STOP:
                        return STOP;
                    case REMOTE_OPERATION_T.RESET:
                        return RESET;
                    case REMOTE_OPERATION_T.PAUSE:
                        return PAUSE;
                    case REMOTE_OPERATION_T.LATCH_CLEAR:
                        return LATCH_CLEAR;
                    case REMOTE_OPERATION_T.READ_TYPE:
                        return READ_TYPE;
                    default:
                        return null;
                }
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RemoteControlModeAvailable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is REMOTE_OPERATION_T && targetType == typeof(Visibility))
            {
                switch ((REMOTE_OPERATION_T)value)
                {
                    case REMOTE_OPERATION_T.RUN:
                        return Visibility.Visible;
                    case REMOTE_OPERATION_T.STOP:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.RESET:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.PAUSE:
                        return Visibility.Visible;
                    case REMOTE_OPERATION_T.LATCH_CLEAR:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.READ_TYPE:
                        return Visibility.Hidden;
                    default:
                        return Visibility.Hidden;
                }
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RemoteClearModeAvailable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is REMOTE_OPERATION_T && targetType == typeof(Visibility))
            {
                switch ((REMOTE_OPERATION_T)value)
                {
                    case REMOTE_OPERATION_T.RUN:
                        return Visibility.Visible;
                    case REMOTE_OPERATION_T.STOP:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.RESET:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.PAUSE:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.LATCH_CLEAR:
                        return Visibility.Hidden;
                    case REMOTE_OPERATION_T.READ_TYPE:
                        return Visibility.Hidden;
                    default:
                        return Visibility.Hidden;
                }
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StorageSchemaVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DAQStorageSchema res = (DAQStorageSchema)value;
            DAQStorageSchema schema = (DAQStorageSchema)parameter;
            if (res == schema)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
