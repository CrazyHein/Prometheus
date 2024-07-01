using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// LocalEventHelper.xaml 的交互逻辑
    /// </summary>
    public partial class LocalEventHelper : Window
    {
        public LocalEventModel? Event { get; private set; }
        public IEnumerable<uint>? LocalEventIndexes { get; set; }
        public LocalEventHelper(LocalEventModel evt)
        {
            InitializeComponent();
            Event = new LocalEventModel(evt.Index, evt.Name, evt.Event.ToEvent()) { Owner = null };
            DataContext = Event;
            EventControl.Content = new EventControl(Event.Event);
        }

        public LocalEventHelper(uint index, string name)
        {
            InitializeComponent();
            Event = new LocalEventModel(index, name) { Owner = null };
            DataContext = Event;
            EventControl.Content = new EventControl(Event.Event);
        }

        private void CancelButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEventIndexes?.Any(x => x == Event.Index) == true)
                MessageBox.Show($"The local event with the same index({Event.Index}) has existed already.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                try
                {
                    Event.ApplyChanges();
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
