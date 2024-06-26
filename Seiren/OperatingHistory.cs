﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public enum Operation
    {
        Add,
        Insert,
        Replace,
        Remove,
        Move,
        ReviseDAQ
    }
    public class OperatingRecord
    {
        public RecordContainerModel Host { get; init; }
        public Operation Operation { get; init; }
        public int OriginaPos { get; init; }
        public int NewPos { get; init; }
        public object OriginalValue { get; init; }
        public object NewValue { get; init; }

        public char Marker { get; set; } = OperatingRecordMarker.DefaultMarker;

        public override string ToString()
        {
            if (Host != null)
                return $"{Host.Name} - {Operation}";
            else
                return $"Unnamed Container - {Operation}";
        }

        public string Info
        {
            get { return this.ToString(); }
        }

        public string DetailedDescription
        {
            get 
            {
                if (Host != null)
                    return $"{Host.Name} - {Operation}\n[{OriginaPos}]\n{OriginalValue}\n->\n[{NewPos}]\n{NewValue}";
                else
                    return $"Unnamed Container - {Operation}\n[{OriginaPos}]\n{OriginalValue}\n->\n[{NewPos}]\n{NewValue}";
            }
        }
    }
    public class OperatingRecordMarker : IValueConverter
    {
        public const char DefaultMarker = '\x20';
        public const char EvenBatchMarker = '0';
        public const char OddBatchMarker = '1';

        private static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.LightYellow);
        private static SolidColorBrush EvenBatchBrush = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush OddBatchBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            char marker = (char)value;
            switch(marker)
            {
                case DefaultMarker:
                    return DefaultBrush;
                case EvenBatchMarker:
                    return EvenBatchBrush;
                case OddBatchMarker: 
                    return OddBatchBrush;
                default:
                    return DefaultBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OperatingHistory
    {
        private OperatingRecord[] __operating_records;
        private int __deep = -1;
        private int __bottom_record_index = -1;
        private int __top_record_index = -1;
        private char[] __markers = new char[] { OperatingRecordMarker.EvenBatchMarker, OperatingRecordMarker.OddBatchMarker };
        private int __batch_marker_index = 0;
        private bool __batch_operating = false;
        private bool __batch_marker_used = false;
        private ObservableCollection<OperatingRecord> __undo_operating_records;
        private ObservableCollection<OperatingRecord> __redo_operating_records;
        public IReadOnlyList<OperatingRecord> UndoOperatingRecords { get; init; }
        public IReadOnlyList<OperatingRecord> RedoOperatingRecords { get; init; }

        public OperatingHistory(int capacity)
        {
            System.Diagnostics.Debug.Assert(capacity > 1);
            __operating_records = new OperatingRecord[capacity];
            __undo_operating_records = new ObservableCollection<OperatingRecord>();
            __redo_operating_records = new ObservableCollection<OperatingRecord>();
            UndoOperatingRecords = __undo_operating_records;
            RedoOperatingRecords = __redo_operating_records;
        }

        public void Clear()
        {
            __deep = -1;
            __bottom_record_index = -1;
            __top_record_index = -1;
            __undo_operating_records.Clear();
            __redo_operating_records.Clear();
        }
        public bool CanUndo
        {
            get
            {
                return __deep != -1;
            }
        }

        public bool CanRedo
        {
            get
            {
                return __deep < Length - 1;
            }
        }

        public OperatingRecord Undo()
        {
            if (CanUndo)
            {
                var op = __operating_records[(__bottom_record_index + __deep) % __operating_records.Length];
                __deep--;
                __undo_operating_records.RemoveAt(0);
                __redo_operating_records.Insert(0, op);
                return op;
            }
            else
                return null;
        }

        public OperatingRecord Redo()
        {
            if (CanRedo)
            {
                __deep++;
                var op = __operating_records[(__bottom_record_index + __deep) % __operating_records.Length];
                __redo_operating_records.RemoveAt(0);
                __undo_operating_records.Insert(0, op);
                return op;
            }
            else
                return null;
        }

        public void PushOperatingRecord(OperatingRecord r)
        {
            if (__top_record_index == -1)
            {
                __operating_records[0] = r;
                __top_record_index = __bottom_record_index = 0;
                __deep = 0;
            }
            else
            {
                __deep++;
                if (__deep == __operating_records.Length)
                {
                    __deep = __operating_records.Length - 1;
                    __bottom_record_index = (__bottom_record_index + 1) % __operating_records.Length;
                }
                __top_record_index = (__bottom_record_index + __deep) % __operating_records.Length;
                __operating_records[__top_record_index] = r;     
            }
            if(__undo_operating_records.Count == __operating_records.Length)
                __undo_operating_records.RemoveAt(__undo_operating_records.Count - 1);
            if(__batch_operating)
            {
                __batch_marker_used = true;
                r.Marker = __markers[__batch_marker_index % 2];
            }
            __undo_operating_records.Insert(0, r);
            __redo_operating_records.Clear();
        }

        public int Length
        {
            get
            {
                if (__top_record_index == -1)
                    return 0;
                else if (__top_record_index >= __bottom_record_index)
                    return __top_record_index - __bottom_record_index + 1;
                else
                    return __top_record_index - __bottom_record_index + 1 + __operating_records.Length;
            }
        }

        public void EnterBatchOperating()
        {
            __batch_operating = true;
        }

        public void ExitBatchOperating()
        {
            __batch_operating = false;
            if (__batch_marker_used)
            {
                __batch_marker_used = false;
                __batch_marker_index++;
            }
        }
        public bool IsBatchOperating { get { return __batch_operating == true; } }
    }
}
