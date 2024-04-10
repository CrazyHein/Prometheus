using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable SYSLIB0014

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria.Utility
{
    public enum FTPMode
    {
        Upload,
        Download,
    }
    class FTPUtilityModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ControllerModelCatalogue __controller_model_catalogue;
        private TaskUserParameterHelper __task_user_parameter_helper;

        public FTPUtilityModel(FTPMode mode, ControllerModelCatalogue cc, TaskUserParameterHelper helper)
        {
            Mode = mode;
            __controller_model_catalogue = cc;
            __task_user_parameter_helper = helper;

            var list = new List<string>();
            DirectoryInfo info = new DirectoryInfo(__LOCAL_ORBMENT_BINARY_PATH);
            foreach (var sub in info.GetDirectories())
                list.Add(sub.Name);
            AvailableOrbmentVersions = list.OrderByDescending(v => v);
            if (list.Count != 0)
                SelectedOrbmentVersion = AvailableOrbmentVersions.First();
        }

        public FTPMode Mode { get; private set; }

        private bool __has_error = false;
        public bool HasError
        {
            get { return __has_error; }
            set { __has_error = value; OnPropertyChanged("HasError"); }
        }

        public string HostIPv4 { get; set; } = "192.168.3.3";
        public ushort HostPort { get; set; } = 21;
        public string User { get; set; } = "target";
        public string Password { get; set; } = "password";
        public int Timeout { get; set; } = 5000;
        public int ReadWriteTimeout { get; set; } = 5000;
        private string __task_user_parameters_path = "/2/r2h_task_user_parameters.xml";
        public string TaskUserParametersPath
        {
            get { return __task_user_parameters_path; }
            set
            {
                if (Regex.IsMatch(value, @"^(/[^/]+)+$") == false)
                    throw new ArgumentException("Invalid task user parameters file path");
                __task_user_parameters_path = value;
            }
        }

        private string __LOCAL_ORBMENT_BINARY_PATH { get { return @"Metadata/Orbment/"; } }
        private string[] __ORBMENT_BINARY_FILES { get; init; } = new string[]
        {
            "vx_R2H_DLink.out",
            "vx_R2H_EthModule.out",
            "vx_R2H_ExtModule.out",
            "vx_R2H_ILink.out",
            "vx_R2H_Task.out"
        };
        public string SelectedOrbmentVersion { get; set; }
        public IEnumerable<string> AvailableOrbmentVersions { get; }

        private string __transfer_state = "Done";
        public string TransferState
        {
            get { return __transfer_state; }
            set { __transfer_state = value; OnPropertyChanged("TransferState"); }
        }

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }

        public TaskUserParameterHelper Upload()
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            TaskUserParameterHelper helper;
            request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __task_user_parameters_path);
            request.Credentials = cred;
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (System.IO.Stream sm = response.GetResponseStream())
            {
                helper = new TaskUserParameterHelper(__controller_model_catalogue, sm);
                sm.Close();
                response.Close();
            }
            return helper;
        }

        public void Upload(string remote, string local, int bufferSize = 16 * 1024, bool keepAlive = false)
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + remote);
            request.Credentials = cred;
            request.KeepAlive = keepAlive;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (System.IO.Stream sm = response.GetResponseStream())
            using (System.IO.FileStream fs = System.IO.File.Create(local))
            {
                byte[] buffer = new byte[bufferSize];
                int read = 0;
                do
                {
                    read = sm.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, read);
                } while (read != 0);
            }
        }

        public void Download()
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __task_user_parameters_path);
            request.Credentials = cred;
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            using (System.IO.Stream sm = request.GetRequestStream())
            {
                __task_user_parameter_helper.Save(sm);
                sm.Close();
            }
        }

        public void Download(string local, string remote, int bufferSize = 16 * 1024, bool keepAlive = false)
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + remote);
            request.Credentials = cred;
            request.KeepAlive = keepAlive;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;

            using (System.IO.FileStream fs = System.IO.File.OpenRead(local))
            using (System.IO.Stream sm = request.GetRequestStream())
            {
                byte[] buffer = new byte[bufferSize];
                int read = 0;
                do
                {
                    read = fs.Read(buffer, 0, buffer.Length);
                    sm.Write(buffer, 0, read);
                } while (read != 0);
            }
        }

        public void DownloadOrbmentBinary()
        {
            foreach (var f in __ORBMENT_BINARY_FILES)
            {
                TransferState = $"Download {SelectedOrbmentVersion}/{f}";
                Download(string.Join(null, __LOCAL_ORBMENT_BINARY_PATH, SelectedOrbmentVersion, '/', f), $"/2/{f}", 16 * 1024, true);
            }
        }
    }
}
