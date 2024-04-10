using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility.R12CCPU;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

#pragma warning disable SYSLIB0014

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class EventLogModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string HostIPv4 { get; set; } = "192.168.3.3";
        public ushort HostPort { get; set; } = 21;
        public string User { get; set; } = "target";
        public string Password { get; set; } = "password";
        public int Timeout { get; set; } = 5000;
        public int ReadWriteTimeout { get; set; } = 5000;
        public EventLogDestination HistoryDestination { get; set; } = EventLogDestination.DATA_MEMORY;
        public string LocalEventLogPath { get; set; } = "";

        private const string __EVENT_HISTORY_DATA_MEMORY = "/4/MELPRJ/EVENT.LOG";
        private const string __EVENT_HISTORY_MEMORY_CARD = "/2/MELPRJ/EVENT.LOG";

        public IEnumerable<OrbmemtEventLog> Records
        {
            get
            { 
                if(ViewOrbmentLogOnly)
                    return __records.Reverse<OrbmemtEventLog>().Where(r => r.EventCode == 0x5000 && r.Source == 0x00004820);
                else
                    return __records.Reverse<OrbmemtEventLog>();
            }
        }
        private List<OrbmemtEventLog> __records = new List<OrbmemtEventLog>();

        private bool __view_orbment_log_only = true;
        public bool ViewOrbmentLogOnly
        {
            get { return __view_orbment_log_only; }
            set
            {
                __view_orbment_log_only = value;
                OnPropertyChanged("ViewOrbmentLogOnly");
                OnPropertyChanged("Records");
            }

        }

        public void Upload()
        {
            FtpWebRequest request;

            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());

            request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + 
                (HistoryDestination == EventLogDestination.DATA_MEMORY ? __EVENT_HISTORY_DATA_MEMORY : __EVENT_HISTORY_MEMORY_CARD));
            request.Credentials = cred;
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (System.IO.Stream sm = response.GetResponseStream())
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(sm))
            {
                EventLog log = new EventLog(br);
                List<OrbmemtEventLog> records = new List<OrbmemtEventLog>();
                foreach (var r in log.Records)
                {
                    records.Add(new OrbmemtEventLog(r.Data, r.EventType, r.EventCode, r.Source, r.StartIO, r.Raw));
                }
                __records = records;
                OnPropertyChanged("Records");
            }

        }

        public void ReadLocal()
        {
            using (System.IO.FileStream fs = System.IO.File.OpenRead(LocalEventLogPath))
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
            {
                EventLog log = new EventLog(br);
                List<OrbmemtEventLog> records = new List<OrbmemtEventLog>();
                foreach (var r in log.Records)
                {
                    records.Add(new OrbmemtEventLog(r.Data, r.EventType, r.EventCode, r.Source, r.StartIO, r.Raw));
                }
                __records = records;
                OnPropertyChanged("Records");
            }
        }

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }
    }

    public enum EventLogDestination
    {
        DATA_MEMORY,
        MEMORY_CARD
    }

    public class OrbmemtEventLog
    {
        public string Data { get; private init; }
        public ushort EventType { get; private init; }
        public ushort EventCode { get; private init; } 
        public uint Source { get; private init; }
        public ushort StartIO { get; private init; }
        public uint OrbmentErrorCode { get; private init; }
        public string OrbmentEventDetails { get; private init; }

        public OrbmemtEventLog(string data, ushort eventType, ushort eventCode, uint source, ushort startIO, byte[] details)
        {
            Data = data;
            EventType = eventType;
            EventCode = eventCode; 
            Source = source;
            StartIO = startIO;
            if(eventCode == 0x5000 && source == 0x00004820 && details.Length >= 24 + Marshal.SizeOf<RecordHeader>() + 2)
            {
                OrbmentErrorCode = MemoryMarshal.Read<uint>(new ReadOnlySpan<byte>(details, Marshal.SizeOf<RecordHeader>() + 12, 4));
                OrbmentEventDetails = Encoding.Unicode.GetString(details, Marshal.SizeOf<RecordHeader>() + 24, details.Length - Marshal.SizeOf<RecordHeader>() - 24 - 2);
            }
            else
            {
                OrbmentErrorCode = 0;
                OrbmentEventDetails = "N/A";
            }
        }
    }
}
