using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Windows.Controls;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// DataTypesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class DataTypesViewer : UserControl
    {
        public DataTypesViewer(DataTypeCatalogue dtc)
        {
            InitializeComponent();
            DataContext = dtc.DataTypes.Values;
        }

        private LoadingIndicator __loading_dialog;
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (__loading_dialog != null)
            {
                __loading_dialog.CloseIndicator(null, null);
                __loading_dialog = null;
            }
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && __loading_dialog == null && IsLoaded == false)
            {
                __loading_dialog = new LoadingIndicator();
                __loading_dialog.ShowIndicator();
            }
        }
    }
}
