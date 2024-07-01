using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls
{
    /// <summary>
    /// GlobalEventControl.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalEventControl : UserControl
    {

        public GlobalEventControl(GlobalEventModel evt)
        {
            InitializeComponent();
            DataContext = evt;
            EventControl.Content = new EventControl(evt.Event);
        }

        public void ResetDataModel(GlobalEventModel evt)
        {
            DataContext = evt;
            (EventControl.Content as EventControl).ResetDataModel(evt.Event);
        }

        public void UpdateBindingSource()
        {
            var binding = txtGlobalEventName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }
}
