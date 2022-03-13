using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    internal class RecentlyOpened
    {
        private ObservableCollection<string> __path_collection;
        public bool ContentChanged { get; private set; }
        public int Capacity { get; init; }
        public IReadOnlyList<string> PathCollection { get; private set; }

        public RecentlyOpened(int capacity = 16)
        {
            Capacity = capacity;
            __path_collection = new ObservableCollection<string>();
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.CreateSubKey(@"Software\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                if(key.GetValue("PathCollection") != null && key.GetValueKind("PathCollection") == RegistryValueKind.MultiString)
                {
                    string[] paths = key.GetValue("PathCollection") as string[];
                    for(int i = 0; i < paths.Length && i < capacity; ++i)
                        __path_collection.Add(paths[i]);
                }
                key.Close();
            }
            catch (Exception)
            {
                if (key != null) key.Close();
            }
            PathCollection = __path_collection;
        }

        public void Flush()
        {
            RegistryKey key = null;
            if (__path_collection.Count == 0 || ContentChanged == false)
                return;
            try
            {
                key = Registry.CurrentUser.CreateSubKey(@"Software\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                if (key.GetValue("PathCollection") != null && key.GetValueKind("PathCollection") != RegistryValueKind.MultiString)
                {
                    key.DeleteValue("PathCollection");
                    key.SetValue("PathCollection", __path_collection.ToArray(), RegistryValueKind.MultiString);
                }
                else
                    key.SetValue("PathCollection", __path_collection.ToArray(), RegistryValueKind.MultiString);
                key.Close();
            }
            catch (Exception)
            {
                if (key != null) key.Close();
            }
        }

        public void Add(string path)
        {
            int index = __path_collection.IndexOf(path);
            if (index > 0)
            {
                __path_collection.RemoveAt(index);
                __path_collection.Insert(0, path);
                ContentChanged = true;
            }
            else if (index == -1)
            {
                if (__path_collection.Count == Capacity)
                    __path_collection.RemoveAt(Capacity - 1);
                __path_collection.Insert(0, path);
                ContentChanged = true;
            }
        }
    }
}
