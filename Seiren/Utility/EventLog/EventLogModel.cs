using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility.MXRL;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility.R12CCPU;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Logging;
using Renci.SshNet;
using Syncfusion.UI.Xaml.Diagram.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
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

        public Platform Platform { get; set; } = Platform.R12CCPU;
        public string LocalEventLogPath { get; set; } = "";

        private const string __EVENT_HISTORY_DATA_MEMORY = "/4/MELPRJ/EVENT.LOG";
        private const string __EVENT_HISTORY_MEMORY_CARD = "/2/MELPRJ/EVENT.LOG";

        private const string __MXRL_LOGS_PATH = "/home/mxr/orbment_logs/";

        public IEnumerable<OrbmemtEventLog> Records
        {
            get
            {
                switch(Platform)
                {
                    case Platform.R12CCPU:
                        if (ViewOrbmentLogOnly)
                            return __records.Reverse<OrbmemtEventLog>().Where(r => r.EventCode == 0x5000 && r.Source == 0x00004820);
                        else
                            return __records.Reverse<OrbmemtEventLog>();
                    case Platform.MXRL:
                        return __records;
                    default:
                        return __records;
                }
                
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
            List<OrbmemtEventLog> records = new List<OrbmemtEventLog>();
            switch (Platform)
            {
                case Platform.R12CCPU:
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
                        R12CCPU.EventLog r12log = new R12CCPU.EventLog(br);
                        
                        foreach (var r in r12log.Records)
                        {
                            records.Add(new OrbmemtEventLog(r.Date, r.EventType, r.EventCode, r.Source, r.StartIO, r.Raw));
                        }
                        __records = records;
                        OnPropertyChanged("Records");
                    }
                    break;
                case Platform.MXRL:
                    MXRL.EventLog mxrlog = new MXRL.EventLog();
                    using (var client = new SftpClient(HostIPv4, HostPort, User.Trim(), Password.Trim()) { OperationTimeout = TimeSpan.FromMilliseconds(ReadWriteTimeout) })
                    {
                        client.Connect();
                        foreach (var f in client.ListDirectory(__MXRL_LOGS_PATH))
                        {
                            if (f.IsDirectory || f.Name.EndsWith("txt") == false)
                                continue;
                            using (System.IO.MemoryStream mm = new MemoryStream())
                            {
                                client.DownloadFile(f.FullName, mm);
                                mm.Position = 0;
                                mxrlog.Append(Encoding.ASCII.GetString(mm.GetBuffer()));
                                mm.Close();
                            }
                        }
                        client.Disconnect();
                    }

                    foreach (var r in mxrlog.Records)
                    {
                        records.Add(new OrbmemtEventLog(r.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), r.Cls, r.Error, r.Source, r.Content));
                    }
                    __records = records;
                    OnPropertyChanged("Records");
                    break;
                default:
                    break;
            }
        }

        public void ReadLocal()
        {
            using (System.IO.FileStream fs = System.IO.File.OpenRead(LocalEventLogPath))
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
            {
                R12CCPU.EventLog log = new R12CCPU.EventLog(br);
                List<OrbmemtEventLog> records = new List<OrbmemtEventLog>();
                foreach (var r in log.Records)
                {
                    records.Add(new OrbmemtEventLog(r.Date, r.EventType, r.EventCode, r.Source, r.StartIO, r.Raw));
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
        public string Date { get; private init; }
        public ushort EventType { get; private init; }
        public ushort EventCode { get; private init; } 
        public uint Source { get; private init; }
        public ushort StartIO { get; private init; }
        public uint OrbmentErrorCode { get; private init; }
        public string OrbmentEventDetails { get; private init; }

        public OrbmemtEventLog(string date, ushort eventType, ushort eventCode, uint source, ushort startIO, byte[] details)
        {
            Date = date;
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

        public OrbmemtEventLog(string date, ushort cls, uint error, string source, string details)
        {
            Date = date;
            EventType = cls;
            OrbmentErrorCode = error;
            OrbmentEventDetails = $"{source} -> {details}";
        }
    }
}
