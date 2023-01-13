using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public enum FTPMode
    {
        Upload,
        Download,
        Compare
    }
    class FTPUtilityModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private VariableDictionary __variable_dictionary;
        private ControllerConfiguration __controller_configuration;
        private ObjectDictionary __object_dictionary;
        private ProcessDataImage __tx_diagnotic_area;
        private ProcessDataImage __tx_bit_area;
        private ProcessDataImage __tx_block_area;
        private ProcessDataImage __rx_control_area;
        private ProcessDataImage __rx_bit_area;
        private ProcessDataImage __rx_block_area;
        private InterlockCollection __interlock_area;
        private Miscellaneous __misc_info;
        private IEnumerable<string> __variable_names;
        private IEnumerable<string> __configuration_names;
        private IEnumerable<uint> __object_indexes;
        private DataTypeCatalogue __data_type_catalogue;
        private ControllerModelCatalogue __controller_model_catalogue;

        public FTPUtilityModel(FTPMode mode,
            VariableDictionary vd, IEnumerable<string> variableNames,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk,
            InterlockCollection intlk, Miscellaneous misc,
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            Mode = mode;
            __variable_dictionary = vd;
            __controller_configuration = cc;
            __object_dictionary = od;
            __tx_diagnotic_area = txdiag;
            __tx_bit_area = txbit;
            __tx_block_area = txblk;
            __rx_control_area = rxctl;
            __rx_bit_area = rxbit;
            __rx_block_area = rxblk;
            __interlock_area = intlk;
            __misc_info = misc;
            __variable_names = variableNames;
            __configuration_names = configurationNames;
            __object_indexes = objectIndexes;
            __data_type_catalogue = dataTypes;
            __controller_model_catalogue = models;
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
        private string __variable_dictionary_path = "/2/variable_catalogue.xml";
        public string VariableDictionaryPath
        {
            get { return __variable_dictionary_path; }
            set
            {
                if (value == null || value.Trim().Length == 0)
                    throw new ArgumentException("Invalid variable dictionary file path");
                __variable_dictionary_path = value;
            }
        }
        private string __io_list_path = "/2/io_list.xml";
        public string IOListPath
        {
            get { return __io_list_path; }
            set
            {
                if (value == null || value.Trim().Length == 0)
                    throw new ArgumentException("Invalid io list file path");
                __io_list_path = value;
            }
        }

        public bool VAR { get; set; } = true;
        public bool IO { get; set; } = true;

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
        }

        public (VariableDictionary, ControllerConfiguration, ObjectDictionary,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
                    Miscellaneous) Upload()
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            VariableDictionary vd;
            ControllerConfiguration cc;
            ObjectDictionary od;
            ProcessDataImage txdiag, txbit, txblk, rxctl, rxbit, rxblk;
            InterlockCollection intlk;
            Miscellaneous misc;
            if (VAR)
            {
                request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __variable_dictionary_path);
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.Timeout = Timeout;
                request.ReadWriteTimeout = ReadWriteTimeout;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (System.IO.Stream sm = response.GetResponseStream())
                {
                    vd = IOCelcetaHelper.Import(sm, __data_type_catalogue);
                    sm.Close();
                    response.Close();
                }

                if (IO)
                {
                    request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __io_list_path);
                    request.Credentials = cred;
                    request.KeepAlive = false;
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.UseBinary = true;


                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    using (System.IO.Stream sm = response.GetResponseStream())
                    {
                        (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = IOCelcetaHelper.Import(sm, vd, __data_type_catalogue, __controller_model_catalogue);
                        sm.Close();
                        response.Close();
                    }
                }
                else
                    (cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc) = IOCelcetaHelper.Default(__data_type_catalogue, __controller_model_catalogue, vd);
                return (vd, cc, od, txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc);
            }
            else
                throw new NotImplementedException("<Variable Dictionary> must be selected.");   
        }

        public void Download()
        {
            NetworkCredential cred = null;
            if (User != null && User.Trim().Length > 0 && Password != null && Password.Trim().Length > 0)
                cred = new NetworkCredential(User.Trim(), Password.Trim());
            FtpWebRequest request;
            if (VAR)
            {
                request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __variable_dictionary_path);
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.Timeout = Timeout;
                request.ReadWriteTimeout = ReadWriteTimeout;

                using (System.IO.Stream sm = request.GetRequestStream())
                {
                    IOCelcetaHelper.Export(sm, true, __variable_dictionary, __variable_names);
                    sm.Close();
                }
            }

            if (IO)
            {
                request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIPv4 + ":" + HostPort.ToString() + __io_list_path);
                request.Credentials = cred;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;

                using (System.IO.Stream sm = request.GetRequestStream())
                {
                    IOCelcetaHelper.Export(sm, __controller_configuration, __configuration_names, __object_dictionary, __object_indexes,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area, __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info);
                    sm.Close();
                }
            }
        }
    }
}
