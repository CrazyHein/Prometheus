using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Windows.Shapes;
using System.Xml.Linq;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using System.Threading;
using Spire.Pdf.Exporting.XPS.Schema;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Controls;
using Syncfusion.UI.Xaml.Grid;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class SmartECATUtilityModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SmartECATUtilityModel()
        {
            var list = new List<string>();
            DirectoryInfo info = new DirectoryInfo(__LOCAL_FIRMWARE_POSITION);
            foreach(var sub in info.GetDirectories())
                list.Add(sub.Name);
            AvailableFirmwareVersions = list.OrderByDescending(v =>v);
            if(list.Count != 0)
                SelectedFirmwareVersion = AvailableFirmwareVersions.First();
        }

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }

        public SmartECATProperty SmartECATProperty { get; set; } = new SmartECATProperty();

        private string __LOCAL_FIRMWARE_PATH
        {
            get
            {
                switch (SmartECATProperty.InstallerProperty.PlatformModel)
                {
                    case SmartECATPlatform.R12CCPU_V:
                        return $"{__LOCAL_FIRMWARE_POSITION}{SelectedFirmwareVersion}/R12CCPU_V/";
                    case SmartECATPlatform.RD55UP06_V:
                        return $"{__LOCAL_FIRMWARE_POSITION}{SelectedFirmwareVersion}/RD55UP06_V/";
                    case SmartECATPlatform.RD55UP12_V:
                        return $"{__LOCAL_FIRMWARE_POSITION}{SelectedFirmwareVersion}/RD55UP12_V/";
                    default:
                        return $"{__LOCAL_FIRMWARE_POSITION}{SelectedFirmwareVersion}/RD55UP12_V/";
                }
            }
        }

        private string __LOCAL_FIRMWARE_POSITION { get { return @"Metadata/SMART-ECAT/"; } }

        public string SelectedFirmwareVersion { get; set; }
        public IEnumerable<string> AvailableFirmwareVersions { get; }

        public void NotifySmartECATPropertyChanged()
        {
            OnPropertyChanged("SmartECATProperty");
        }

        private string __installation_state = "Idle";
        public string InstallationState
        {
            get { return __installation_state; }
            set { __installation_state = value; OnPropertyChanged("InstallationState"); }
        }

        private string __installation_exception_info = "N/A";
        public string InstallationExceptionInfo
        {
            get { return __installation_exception_info; }
            set { __installation_exception_info = value; OnPropertyChanged("InstallationExceptionInfo"); }
        }

        private int __installation_progress = 0;
        public int InstallationProgress
        {
            get { return __installation_progress; }
            set { __installation_progress = value; OnPropertyChanged("InstallationProgress"); }
        }

        private string __current_opened_file_name;
        public string CurrentOpenedLogFileName
        {
            get { return __current_opened_file_name; }
            set { __current_opened_file_name = value; OnPropertyChanged("CurrentOpenedLogFileName"); }
        }

        private List<SmartECATLogFileInfo>  __remote_log_file_list = new List<SmartECATLogFileInfo>();
        public IEnumerable<SmartECATLogFileInfo> RemoteLogFileList 
        {
            get
            {
                return __remote_log_file_list.OrderByDescending(f => f.Name);
            }
        }

        private List<SmartECATLogFileInfo> __local_log_file_list = new List<SmartECATLogFileInfo>();
        public IEnumerable<SmartECATLogFileInfo> LocalLogFileList
        {
            get
            {
                return __local_log_file_list.OrderByDescending(f => f.Name);
            }
        }

        private List<SmartECATLogEntry> __log_entry_list = new List<SmartECATLogEntry>();
        public IEnumerable<SmartECATLogEntry> LogEntryList
        { 
            get
            {
                return __log_entry_list;
            }
        }

        public bool LogEntryIsEmpty
        {
            get { return __log_entry_list.Count == 0; }
        }


        public void RefreshRemoteLogFileList()
        {
            try
            {
                DebugConsole.WriteInfo($"Reload Log File List via FTP : ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.LOG_PATH}.");

                NetworkCredential cred = null;
                if (SmartECATProperty.FTPTargetProperty.User != null && SmartECATProperty.FTPTargetProperty.User.Trim().Length > 0 && SmartECATProperty.FTPTargetProperty.Password != null && SmartECATProperty.FTPTargetProperty.Password.Trim().Length > 0)
                    cred = new NetworkCredential(SmartECATProperty.FTPTargetProperty.User.Trim(), SmartECATProperty.FTPTargetProperty.Password.Trim());

                FtpWebRequest request;

                request = (FtpWebRequest)FtpWebRequest.Create($"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.LOG_PATH}");
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.UseBinary = true;
                request.Timeout = SmartECATProperty.FTPTargetProperty.TimeoutValue;
                request.ReadWriteTimeout = SmartECATProperty.FTPTargetProperty.ReadWriteTimeoutValue;

                List<SmartECATLogFileInfo> files = new List<SmartECATLogFileInfo>();
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (System.IO.Stream sm = response.GetResponseStream())
                using (System.IO.StreamReader sr = new StreamReader(sm))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim().ToLower().EndsWith(".log"))
                            files.Add(new SmartECATLogFileInfo(line, request.RequestUri.AbsoluteUri));
                    }
                }
                __remote_log_file_list = files;
                OnPropertyChanged("RemoteLogFileList");
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }
        }

        public void RefreshLocalLogFileList(string folder)
        {
            try
            {
                DebugConsole.WriteInfo($"Reload Log File List from : {folder}.");
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                __local_log_file_list = directoryInfo.EnumerateFiles("*.log", SearchOption.TopDirectoryOnly).Select(f => new SmartECATLogFileInfo(f.Name, f.FullName, f.LastWriteTime, f.Length, true)).ToList(); ;
                OnPropertyChanged("LocalLogFileList");
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }
        }

        public void ReadLogContent(SmartECATLogFileInfo info)
        {
            try
            {
                if (info.Local == false)
                {
                    DebugConsole.WriteInfo($"Read Remote Log File: {info.FullName}.");
                    NetworkCredential cred = null;
                    if (SmartECATProperty.FTPTargetProperty.User != null && SmartECATProperty.FTPTargetProperty.User.Trim().Length > 0 && SmartECATProperty.FTPTargetProperty.Password != null && SmartECATProperty.FTPTargetProperty.Password.Trim().Length > 0)
                        cred = new NetworkCredential(SmartECATProperty.FTPTargetProperty.User.Trim(), SmartECATProperty.FTPTargetProperty.Password.Trim());

                    FtpWebRequest request;
                    request = (FtpWebRequest)FtpWebRequest.Create(info.FullName);
                    request.Credentials = cred;
                    request.KeepAlive = false;
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.UseBinary = true;
                    request.Timeout = SmartECATProperty.FTPTargetProperty.TimeoutValue;
                    request.ReadWriteTimeout = SmartECATProperty.FTPTargetProperty.ReadWriteTimeoutValue;

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    using (System.IO.Stream sm = response.GetResponseStream())
                    using (System.IO.StreamReader sr = new StreamReader(sm))
                    {
                        __log_entry_list = __parse_log_file(sr); ;
                        OnPropertyChanged("LogEntryList");
                        OnPropertyChanged("LogEntryIsEmpty");
                    }
                }
                else
                {
                    DebugConsole.WriteInfo($"Read Local Log File: {info.FullName}.");
                    using (FileStream fs = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                    using (System.IO.StreamReader sr = new StreamReader(fs))
                    {

                        __log_entry_list = __parse_log_file(sr); ;
                        OnPropertyChanged("LogEntryList");
                        OnPropertyChanged("LogEntryIsEmpty");
                    }
                }
                CurrentOpenedLogFileName = info.FullName;
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }
        }

        private List<SmartECATLogEntry> __parse_log_file(System.IO.StreamReader sr)
        {
            string line, subline;
            List<SmartECATLogEntry> entries = new List<SmartECATLogEntry>();
            while ((line = sr.ReadLine()) != null)
            {
                var matchs = SmartECATLogEntry.ENTRY_PATTERN.Matches(line);
                if (matchs.Count == 0)
                {
                    if (entries.Count != 0)
                        entries[entries.Count - 1].AppendContent(line);
                    continue;
                }

                foreach (Match match in matchs)
                {
                    if (match.NextMatch().Success == true)
                        subline = line.Substring(match.Groups["Content"].Index, match.NextMatch().Groups["Timestamp"].Index -1 - match.Groups["Content"].Index);
                    else
                        subline = line.Substring(match.Groups["Content"].Index);
                    entries.Add(new SmartECATLogEntry(DateTime.ParseExact(match.Groups["Timestamp"].Value, SmartECATLogEntry.TIMESTAMP_FORMAT, CultureInfo.InvariantCulture),
                        match.Groups["Category"].Value.Trim(), subline));
                }
            }
            return entries;
        }

        public void __transfer_local_file(string localfilename, string remotepath, string name, string? user, string? password, int buffersize)
        {
            try
            {
                DebugConsole.WriteInfo($"Transfer File: [{localfilename}] -> [{System.IO.Path.Combine(remotepath, name)}]");
                NetworkCredential cred = null;
                if (user != null && user.Trim().Length > 0 && password != null && password.Trim().Length > 0)
                    cred = new NetworkCredential(user.Trim(), password.Trim());

                FtpWebRequest request;
                request = (FtpWebRequest)FtpWebRequest.Create($"{System.IO.Path.Combine(remotepath, name)}");
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.Timeout = SmartECATProperty.FTPTargetProperty.TimeoutValue;
                request.ReadWriteTimeout = SmartECATProperty.FTPTargetProperty.ReadWriteTimeoutValue;

                using (System.IO.FileStream fs = System.IO.File.OpenRead(localfilename))
                using (System.IO.Stream sm = request.GetRequestStream())
                {
                    byte[] buffer = new byte[buffersize];
                    int read = 0, total = 0;
                    InstallationProgress = 0;
                    do
                    {
                        read = fs.Read(buffer, 0, buffer.Length);
                        sm.Write(buffer, 0, read);
                        sm.Flush();
                        total += read;
                        InstallationProgress =(int)(total * 100.0 / fs.Length);
                        //Thread.Sleep(100);
                    } while (total != fs.Length);
                }
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }
        }

        public Stream __ftp_upload_stream(string remotepath, string name, string? user, string? password)
        {
            try
            {
                InstallationProgress = 0;
                DebugConsole.WriteInfo($"Get FTP Upload Stream: [{System.IO.Path.Combine(remotepath, name)}]");
                NetworkCredential cred = null;
                if (user != null && user.Trim().Length > 0 && password != null && password.Trim().Length > 0)
                    cred = new NetworkCredential(user.Trim(), password.Trim());

                FtpWebRequest request;
                request = (FtpWebRequest)FtpWebRequest.Create($"{System.IO.Path.Combine(remotepath, name)}");
                request.Credentials = cred;
                request.KeepAlive = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.Timeout = SmartECATProperty.FTPTargetProperty.TimeoutValue;
                request.ReadWriteTimeout = SmartECATProperty.FTPTargetProperty.ReadWriteTimeoutValue;

                return request.GetRequestStream();
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex);
                throw;
            }
        }

        public void TransferLicenseFile()
        {
            if (SmartECATProperty.InstallerProperty.TransferLicense)
            {
                try
                {
                    InstallationState = "Transfer SMART-ECAT License File.";
                    __transfer_local_file(SmartECATProperty.InstallerProperty.LocalLicenseFilePath,
                        $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.BUILT_IN_MEMORY_PATH}",
                        SmartECATInstallerProperty.DEFAULT_LIC_NAME,
                        SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password, 4096);
                }
                catch (Exception ex)
                {
                    InstallationState = "Exception: Transfer SMART-ECAT License File.";
                    InstallationExceptionInfo = ex.Message;
                    throw;
                }
            }
        }
        public void TransferNetworkInformationFile()
        {
            if (SmartECATProperty.InstallerProperty.TransferNetworkInformation)
            {
                try
                {
                    InstallationState = "Transfer EtherCAT Network Information File.";
                    __transfer_local_file(SmartECATProperty.InstallerProperty.LocalNetworkInformationFilePath,
                        $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.ENI_PATH}",
                        SmartECATInstallerProperty.DEFAULT_ENI_NAME,
                        SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password, 4096*16);
                }
                catch(Exception ex)
                {
                    InstallationState = "Exception: Transfer EtherCAT Network Information File.";
                    InstallationExceptionInfo = ex.Message;
                    throw;
                }
            }
        }
        public void TransferFirmwareFile()
        {
            try
            {
                if (SmartECATProperty.InstallerProperty.TransferNIC)
                {
                    InstallationState = "Transfer SMART-ECAT RT NIC Driver File.";
                    __transfer_local_file(string.Join(null, __LOCAL_FIRMWARE_PATH, SmartECATInstallerProperty.DEFAULT_RT_NIC_DRIVER_NAME),
                        $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.BUILT_IN_MEMORY_PATH}",
                        SmartECATInstallerProperty.DEFAULT_RT_NIC_DRIVER_NAME,
                        SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password, 4096 * 16);
                }
                if (SmartECATProperty.InstallerProperty.TransferAPP)
                {
                    InstallationState = "Transfer SMART-ECAT Application File.";
                    __transfer_local_file(string.Join(null, __LOCAL_FIRMWARE_PATH, SmartECATInstallerProperty.DEFAULT_ECAT_ALP_NAME),
                        $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.BUILT_IN_MEMORY_PATH}",
                        SmartECATInstallerProperty.DEFAULT_ECAT_ALP_NAME,
                        SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password, 4096 * 16);
                }
            }
            catch (Exception ex)
            {
                InstallationState = $"Exception: {InstallationState}";
                InstallationExceptionInfo = ex.Message;
                throw;
            }
        }
        public void TransferConfiguration()
        {
            if (SmartECATProperty.InstallerProperty.TransferCFG == false)
                return;
            try
            {
                InstallationState = $"Transfer '{SmartECATInstallerProperty.DEFAULE_IDOL_INI_NAME}'.";
                using (Stream sm = __ftp_upload_stream(
                    $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.IDOL_PATH}",
                    SmartECATInstallerProperty.DEFAULE_IDOL_INI_NAME, SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password))
                using (StreamWriter sw = new StreamWriter(sm, Encoding.ASCII))
                {
                    sw.Write("idolRate=30");
                }

                InstallationState = $"Transfer '{SmartECATInstallerProperty.DEFAULT_RETRY_INI_NAME}'.";
                using (Stream sm = __ftp_upload_stream(
                    $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.RETRY_PATH}",
                    SmartECATInstallerProperty.DEFAULT_RETRY_INI_NAME, SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password))
                using (StreamWriter sw = new StreamWriter(sm, Encoding.ASCII))
                {
                    sw.Write($"retryNum={SmartECATProperty.InstallerProperty.NumOfNetworkScanReties}");
                }

                if (SmartECATProperty.InstallerProperty.BootFromSD)
                {
                    InstallationState = $"Transfer '{SmartECATInstallerProperty.DEFAULT_SETTING_INI_NAME}'.";
                    using (Stream sm = __ftp_upload_stream(
                        $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.SETTING_PATH}",
                        SmartECATInstallerProperty.DEFAULT_SETTING_INI_NAME, SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password))
                    using (StreamWriter sw = new StreamWriter(sm, Encoding.ASCII))
                    {
                        sw.Write($"logFileSize={SmartECATProperty.InstallerProperty.LogFileSize * 1024}");
                    }
                }

                InstallationState = $"Transfer '{SmartECATInstallerProperty.DEFAULT_STARTUP_CMD_NAME}'.";
                using (Stream sm = __ftp_upload_stream(
                    $"ftp://{SmartECATProperty.FTPTargetProperty.HostIPv4}:{SmartECATProperty.FTPTargetProperty.HostPort}{SmartECATProperty.InstallerProperty.STARTUP_PATH}",
                    SmartECATInstallerProperty.DEFAULT_STARTUP_CMD_NAME, SmartECATProperty.FTPTargetProperty.User, SmartECATProperty.FTPTargetProperty.Password))
                using (StreamWriter sw = new StreamWriter(sm, Encoding.ASCII))
                {
                    sw.WriteLine($"ld(1,0,\"{SmartECATProperty.InstallerProperty.BUILT_IN_MEMORY_PATH}{SmartECATInstallerProperty.DEFAULT_RT_NIC_DRIVER_NAME}\")");
                    sw.WriteLine($"ld(1,0,\"{SmartECATProperty.InstallerProperty.BUILT_IN_MEMORY_PATH}{SmartECATInstallerProperty.DEFAULT_ECAT_ALP_NAME}\")");
                    sw.WriteLine("ts \"miiBusMonitor\"");
                    sw.WriteLine($"taskSpawn(\"tEcat\",10,0x1000000,40000,ecatMain,\"-f " +
                        $"{SmartECATProperty.InstallerProperty.ToString()}" +
                        $"\",0,0,0,0,0,0)");
                }
            }
            catch (Exception ex)
            {
                InstallationState = $"Exception: {InstallationState}";
                InstallationExceptionInfo = ex.Message;
                throw;
            }
        }
    }

    public class SmartECATLogFileInfo
    {
        public readonly static Regex UNIX_FILE_ATTR_PATTERN = new Regex(
            @"^[d\-p\|bcs][rwx\-]{9}\x20\d+\x20\w+\x20\w+\x20+(?<Size>\d+)\x20(?<LastModified>[a-zA-Z]{3}\x20\d{2}\x20\d{2}:\d{2})\x20(?<FileName>.+)$", RegexOptions.Compiled);
        public readonly static Regex DOS_FILE_ATTR_PATTERN = new Regex(
            @"^(?<LastModified>\d{2}-\d{2}-\d{2}\x20+\d{2}:\d{2}(AM|PM))\x20+((?<Size>\d+)|(?<Dir><DIR>))\x20+(?<FileName>.+)$", RegexOptions.Compiled);
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public bool Local { get; private set; } = false;
        public long Size { get; private set; }
        public DateTime LastModified { get; private set; }
        public bool IsDirectory { get; private set; } = false;
        public const string UNIX_DATETIME_FORMAT = "MMM dd HH:mm";
        public const string DOS_DATETIME_FORMAT = "MM-dd-yy  hh:mmtt";

        public SmartECATLogFileInfo(string name, string fullname, DateTime lastModified, long size, bool local)
        {
            Name = name;
            FullName = fullname;
            LastModified = lastModified;
            Size = size;
            Local = local;
        }

        public SmartECATLogFileInfo(string ftpFileDetailsAttr, string ftpPath)
        {
            var match = UNIX_FILE_ATTR_PATTERN.Match(ftpFileDetailsAttr);
            if(match.Success)
            {
                Name = match.Groups["FileName"].Value;
                FullName = System.IO.Path.Combine(ftpPath, Name);
                Size = long.Parse(match.Groups["Size"].Value);
                LastModified = DateTime.ParseExact(match.Groups["LastModified"].Value, UNIX_DATETIME_FORMAT, CultureInfo.InvariantCulture);
                IsDirectory = ftpFileDetailsAttr.StartsWith("d");
                return;
            }
            match = DOS_FILE_ATTR_PATTERN.Match(ftpFileDetailsAttr);
            if (match.Success)
            {
                Name = match.Groups["FileName"].Value;
                FullName = System.IO.Path.Combine(ftpPath, Name);
                if (match.Groups["Size"].Success)
                {
                    Size = long.Parse(match.Groups["Size"].Value);
                    IsDirectory = false;
                }
                else
                {
                    Size = 0;
                    IsDirectory = true;
                }
                LastModified = DateTime.ParseExact(match.Groups["LastModified"].Value, DOS_DATETIME_FORMAT, CultureInfo.InvariantCulture);
                return;
            }
            throw new NotSupportedException($"The file attribute information({ftpFileDetailsAttr}) returned from FTP server cannot be recognized.");
        }
    }

    public class SmartECATLogEntry
    {
        public readonly static Regex ENTRY_PATTERN = new Regex(@"\[(?<Timestamp>\d{2}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\]\[(?<Category>.{4})\](?<Content>.+?)", RegexOptions.Compiled);
        public DateTime Timestamp { get; private set; }
        public string Category { get; private set; }
        public string Content { get; private set; }
        public const string TIMESTAMP_FORMAT = "yy/MM/dd HH:mm:ss.fff";

        public SmartECATLogEntry(DateTime timestamp, string category, string content)
        {
            Timestamp = timestamp;
            Category = category;
            Content = content;
        }

        public void AppendContent(string info)
        {
            Content += info;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yy/MM/dd HH:mm:ss.fff}][{Category,-4}]{Content}";
        }
    }

}
