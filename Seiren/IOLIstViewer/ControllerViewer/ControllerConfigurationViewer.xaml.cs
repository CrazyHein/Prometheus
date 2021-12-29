using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// ControllerConfigurationViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ControllerConfigurationViewer : UserControl
    {
        public ControllerConfigurationViewer(ControllerConfiguration cc, ControllerModelCatalogue cmc)
        {
            InitializeComponent();
            DataContext = new ControllerConfigurationModel(cc, cmc);
            MainViewer.RowDragDropController.Dropped += OnMainViewer_Dropped;
        }

        private void OnMainViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var controllerConfiguration = DataContext as ControllerConfigurationModel;
                DeviceConfigurationModel targetDeviceConfiguration = controllerConfiguration.DeviceConfigurations[(int)e.TargetRecord];

                MainViewer.BeginInit();
                controllerConfiguration.Remove(draggingRecords[0] as DeviceConfigurationModel, true);
                int targetIndex = controllerConfiguration.IndexOf(targetDeviceConfiguration);
                int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                controllerConfiguration.Insert(insertionIndex, draggingRecords[0] as DeviceConfigurationModel);
                MainViewer.EndInit();
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, new DeviceConfigurationModel(), InputDialogDisplayMode.Add);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
            {
                MainViewer.SelectedItem = wnd.Result;
                MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                    MainViewer.ResolveToRowIndex(MainViewer.SelectedItem), MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, new DeviceConfigurationModel(), InputDialogDisplayMode.Insert, MainViewer.SelectedIndex);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
            {
                MainViewer.SelectedItem = wnd.Result;
                MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                    MainViewer.ResolveToRowIndex(MainViewer.SelectedItem), MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, MainViewer.SelectedItem as DeviceConfigurationModel, InputDialogDisplayMode.Edit);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
                MainViewer.SelectedItem = wnd.Result;
        }

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private void MainViewer_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if (e.Record as DeviceConfigurationModel != null)
            {
                DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, e.Record as DeviceConfigurationModel, InputDialogDisplayMode.Edit);
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (wnd.ShowDialog() == true)
                    MainViewer.SelectedItem = wnd.Result;
            }
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = MainViewer.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as ControllerConfigurationModel).Remove(MainViewer.SelectedItem as DeviceConfigurationModel);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
