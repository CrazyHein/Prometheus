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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingIndicator : Window
    {
        private Action<object> __action;
        private bool __state = false;

        public LoadingIndicator()
        {
            InitializeComponent();
            __action = new Action<object>(__default_close_action);
        }

        public void ShowIndicator()
        {
            if (__state == false)
            {
                Show();
                __state = true;
            }
        }

        public void CloseIndicator(Action<object> act, object param)
        {
            if (act == null)
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle, __action, null);
            else
            {
                act += __action;
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle, act, param);
            }
        }

        private void __default_close_action(object param)
        {
            Close();
        }

        protected void ResetState()
        {
            __state = false;
        }

        public bool State { get { return __state; } }
    }
}
