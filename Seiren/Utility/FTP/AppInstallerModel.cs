using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    public class AppInstallerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AppInstallerModel(FTPUtilityModel ftp, AppInstallerProperty app)
        {
            FTPUtilityModel = ftp;
            AppInstallerProperty = app;
        }

        public FTPUtilityModel FTPUtilityModel { get; init; }
        public AppInstallerProperty AppInstallerProperty { get; init; }

        private bool __busy = false;
        public bool IsBusy
        {
            get { return __busy; }
            set { __busy = value; OnPropertyChanged("IsBusy"); }
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

        private string __application_entry_name = "PM_Start";
        public string ApplictionEntryName
        {
            get { return __application_entry_name; }
            set
            {
                if (Regex.IsMatch(value, @"^[a-zA-z_]+[a-zA-Z0-9_]*$"))
                    __application_entry_name = value;
                else
                    throw new ArgumentException("Boot entry name can only consist of letters, numbers, and underscores, and cannot begin with a number.");
            }
        }

        private List<string> __parameter_list = new List<string>();
        private static Regex __parameter_pattern = new Regex(@"(?<![\\]),", RegexOptions.Compiled);
        public string Parameters
        {
            get 
            { 
                return string.Join(", ", __parameter_list.Select(x => x.Replace(",", "\\,"))); 
            }
            set 
            {
                var matchs = __parameter_pattern.Split(value);
                __parameter_list = matchs.Select(x => x.Trim()).Where(x => x.Length != 0).Select(x => x.Replace("\\,", ",")).ToList();
                OnPropertyChanged("Parameters");
            }
        }

        public ObservableCollection<System.IO.FileInfo> ApplicationFileCollection { get; private set; } = new ObservableCollection<System.IO.FileInfo>(); 

        public void InstallApplication()
        {
            try
            {
                bool consistency = false;
                InstallationState = "Hardware Configuration Consistency Check";
                (ConsistencyResult ret, IEnumerable<DeviceConfiguration> notfound) = FTPUtilityModel.ConfigurationConsistency();
                if (ret == ConsistencyResult.Exception)
                {
                    var rsp = MessageBox.Show("Can not read hardware configuration file, so the consistency check is not performed.\nAre you sure you want to download IO List anyway?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (rsp == MessageBoxResult.Yes)
                        consistency = true;
                }
                else if (ret == ConsistencyResult.Inconsistent)
                {
                    SimpleDeviceConfigurationViewer result = new SimpleDeviceConfigurationViewer("Consistency Check Result",
                        "The following hardware configuration(s) in IO List file is(are) not found in the system hardware configuration(s).\nAre you sure you want to download IO List anyway?",
                        notfound);

                    if (result.ShowDialog() == true)
                        consistency = true;
                }
                else
                    consistency = true;

                if (!consistency) 
                    throw new InvalidOperationException("Hardware configuration consistency check failure.");

                InstallationState = "Transfer Variable Dictionary and IO List";
                FTPUtilityModel.Download(AppInstallerProperty.VariableDictionaryPath, AppInstallerProperty.IOListPath);

                foreach (var file in ApplicationFileCollection)
                {
                    InstallationState = $"Transfer {file.Name} -> ftp://{FTPUtilityModel.HostIPv4}:{FTPUtilityModel.HostPort}/2/{file.Name}";
                    FTPUtilityModel.TransferLocalFile(file.FullName, "/2/", file.Name, 4096);
                }

                InstallationState = $"Transfer STARTUP.CMD -> ftp://{FTPUtilityModel.HostIPv4}:{FTPUtilityModel.HostPort}/2/STARTUP.CMD";
                using (Stream sm = FTPUtilityModel.RemoteUploadStream("/2/", "STARTUP.CMD"))
                using (StreamWriter sw = new StreamWriter(sm, Encoding.ASCII))
                {
                    foreach(var file in AppInstallerProperty.OrbmentRuntimeBinaryFiles)
                        sw.WriteLine($"ld(1,0,\"/2/{file}\")");
                    foreach(var file in ApplicationFileCollection.Where(f => f.Name.EndsWith(".out")))
                        sw.WriteLine($"ld(1,0,\"/2/{file.Name}\")");

                    sw.Write($"taskSpawn(\"{AppInstallerProperty.BootTaskName}\",{AppInstallerProperty.BootTaskPriority},0x{AppInstallerProperty.BootTaskSpawnFlag:X8},0x{AppInstallerProperty.BootTaskStackSize:X8},{ApplictionEntryName}," +
                        $"\"{AppInstallerProperty.VariableDictionaryPath}\",\"{AppInstallerProperty.IOListPath}\"");

                    int paramCount = 0;
                    foreach (var p in __parameter_list)
                    {
                        sw.Write($",\"{p}\"");
                        if (++paramCount == 4) break;
                    }
                    while(paramCount++ != 4)
                        sw.Write(",0");
                    sw.WriteLine(")");
                }

                InstallationState = "Done: Application Installation";
            }
            catch (Exception ex)
            {
                InstallationState = $"Exception: {InstallationState}";
                InstallationExceptionInfo = ex.Message;
                throw;
            }
        }
    }
}
