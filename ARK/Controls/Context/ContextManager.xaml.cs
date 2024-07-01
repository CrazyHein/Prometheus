using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Context;
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
    /// ContextManager.xaml 的交互逻辑
    /// </summary>
    public partial class ContextManager : UserControl
    {
        ContentControl __context_context_control;

        public ContextManager(GlobalEventModelCollection globals, ControlBlockModelCollection blocks, ContentControl content)
        {
            InitializeComponent();
            __context_context_control = content;
        }

        private void ContextListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                if ((e.AddedItems[0] as ListBoxItem).Tag.ToString() == "Labels")
                    __context_context_control.Content = new LabelsControl();
            }
        }
    }
}
