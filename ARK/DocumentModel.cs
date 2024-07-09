using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK
{
    internal class DocumentModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        RecentlyOpened __recently_opened_manager;
        int __max_output_lines = 1024;
        public DocumentModel(Settings settings) {
            __recently_opened_manager = new RecentlyOpened(settings.PreferenceProperty.RecentlyOpenedFileCollectionCapacity);
        }

        private GlobalEventModelCollection? __global_event_manager;
        public GlobalEventModelCollection? GlobalEventManager
        {
            get { return __global_event_manager; }
            set
            {
                __global_event_manager = value;
                _notify_property_changed();
            }
        }

        private ContextModel? __context_manager;
        public ContextModel? ContextManager
        {
            get { return __context_manager; }
            set
            {
                __context_manager = value;
                _notify_property_changed();
            }
        }

        public ControlBlockModelCollection? __regular_control_block_manager;
        public ControlBlockModelCollection? RegularControlBlockManager
        {
            get { return __regular_control_block_manager; }
            set
            {
                __regular_control_block_manager = value;
                _notify_property_changed();
            }
        }

        public ControlBlockModelCollection? __exception_control_block_manager;
        public ControlBlockModelCollection? ExceptionControlBlockManager
        {
            get { return __exception_control_block_manager; }
            set
            {
                __exception_control_block_manager = value;
                _notify_property_changed();
            }
        }

        public string? __file_opened;
        public string? FileOpened
        {
            get { return __file_opened; }
            set
            {
                __file_opened = value;
                if (string.IsNullOrEmpty(value) == false)
                {
                    __recently_opened_manager.Add(value.Trim());
                    __recently_opened_manager.Flush();
                }

                _notify_property_changed();
            }
        }

        public bool FileSaved => __file_opened?.Length > 0 && __global_event_manager.IsDirty == false && __regular_control_block_manager.IsDirty == false && __exception_control_block_manager.IsDirty == false && __context_manager.IsDirty == false;
        public bool IsTemporaryFile => __file_opened?.Length == 0;
        public bool UnsavedChanges => __file_opened?.Length == 0 || __global_event_manager?.IsDirty == true || __regular_control_block_manager?.IsDirty == true || __exception_control_block_manager?.IsDirty == true || __context_manager?.IsDirty == true;
        public bool IsOpened => __file_opened != null;

        public IEnumerable<string> RecentlyOpenedFiles => __recently_opened_manager.PathCollection;

        public ObservableCollection<string> __outputs = new ObservableCollection<string>();
        public IEnumerable<string> Outputs => __outputs;

        public void AddOutput(string output)
        {
            var lines = output.ReplaceLineEndings().Split("\r\n");
            if (__outputs.Count + lines.Length > __max_output_lines)
            {

                for (int i = lines.Length + __outputs.Count - __max_output_lines; i != 0; --i)
                    __outputs.RemoveAt(0);
            }
            __outputs.Add($"{DateTime.Now.ToString(@"MM/dd/yy HH:mm:ss fff")}");
            foreach (var line in lines)
            {
                __outputs.Add($"{"\t"} {line}");
            }
        }
    }
}
