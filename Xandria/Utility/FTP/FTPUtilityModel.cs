using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                if (value == null || value.Trim().Length == 0)
                    throw new ArgumentException("Invalid task user parameters file path");
                __task_user_parameters_path = value;
            }
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
    }
}
