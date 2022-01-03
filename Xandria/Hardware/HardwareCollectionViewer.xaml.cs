using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    /// <summary>
    /// HardwareViewer.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareCollectionViewer : UserControl
    {
        public HardwareCollectionViewer(TaskUserParameterHelper helper, ControllerModelCatalogue model)
        {
            InitializeComponent();
            DataContext = new HardwareModels(helper, model);
            LocalModuleViewer.RowDragDropController.Dropped += OnLocalModuleViewer_Dropped;
            RemoteModuleViewer.RowDragDropController.Dropped += OnRemoteModuleViewer_Dropped;
        }

        private void OnLocalModuleViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var hardwareModels = DataContext as HardwareModels;
                HardwareModel targetHardwareModel = hardwareModels.LocalHardwareModelCollection[(int)e.TargetRecord];

                LocalModuleViewer.BeginInit();
                hardwareModels.RemoveLocal(hardwareModels.IndexOfLocal(draggingRecords[0] as HardwareModel));
                int targetIndex = hardwareModels.IndexOfLocal(targetHardwareModel);
                int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                hardwareModels.InsertLocal(insertionIndex, draggingRecords[0] as HardwareModel);
                LocalModuleViewer.EndInit();
            }
        }

        private void OnRemoteModuleViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var hardwareModels = DataContext as HardwareModels;
                HardwareModel targetHardwareModel = hardwareModels.RemoteHardwareModelCollection[(int)e.TargetRecord];

                RemoteModuleViewer.BeginInit();
                hardwareModels.RemoveRemote(hardwareModels.IndexOfRemote(draggingRecords[0] as HardwareModel));
                int targetIndex = hardwareModels.IndexOfRemote(targetHardwareModel);
                int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                hardwareModels.InsertRemote(insertionIndex, draggingRecords[0] as HardwareModel);
                RemoteModuleViewer.EndInit();
            }
        }

        public void UpdateBindingSource()
        {
            var binding = HostAddress.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }

        private int __errors = 0;
        private void Input_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }

        public bool HasError { get { return __errors != 0; } }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.OriginalSource.GetType() == typeof(TextBox))
            {
                var binding = (e.OriginalSource as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }
        }

        private void LocalModuleViewer_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if (e.Record as HardwareModel != null)
            {
                HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, e.Record as HardwareModel, InputDialogDisplayMode.EditLocal, LocalModuleViewer.SelectedIndex);
                wnd.ShowDialog();
            }
        }

        private void RemoteModuleViewer_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if (e.Record as HardwareModel != null)
            {
                HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, e.Record as HardwareModel, InputDialogDisplayMode.EditRemote, RemoteModuleViewer.SelectedIndex);
                wnd.ShowDialog();
            }
        }

        private void AddLocalModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = (DataContext as HardwareModels).ControllerModelCatalogue.LocalExtensionModels.Values.FirstOrDefault();
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, new HardwareModel() { DeviceModel = m}, InputDialogDisplayMode.AddLocal);
            wnd.ShowDialog();
        }
        private void AddLocalModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as HardwareModels)?.LocalHardwareModelCollection != null;
        }
        private void InsertLocalModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = (DataContext as HardwareModels).ControllerModelCatalogue.LocalExtensionModels.Values.FirstOrDefault();
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, new HardwareModel() { DeviceModel = m }, InputDialogDisplayMode.InsertLocal, LocalModuleViewer.SelectedIndex);
            wnd.ShowDialog();
        }
        private void InsertLocalModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LocalModuleViewer?.SelectedItem != null;
        }
        private void EditLocalModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, LocalModuleViewer.SelectedItem as HardwareModel, InputDialogDisplayMode.EditLocal, LocalModuleViewer.SelectedIndex);
            wnd.ShowDialog();
        }
        private void EditLocalModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LocalModuleViewer?.SelectedItem != null;
        }
        private void RemoveLocalModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = LocalModuleViewer.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as HardwareModels).RemoveLocal(LocalModuleViewer.SelectedIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void RemoveLocalModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LocalModuleViewer?.SelectedItem != null;
        }

        private void AddRemoteModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = (DataContext as HardwareModels).ControllerModelCatalogue.RemoteEthernetModels.Values.FirstOrDefault();
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, new HardwareModel() { DeviceModel = m }, InputDialogDisplayMode.AddRemote);
            wnd.ShowDialog();
        }
        private void AddRemoteModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as HardwareModels)?.RemoteHardwareModelCollection != null;
        }
        private void InsertRemoteModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = (DataContext as HardwareModels).ControllerModelCatalogue.RemoteEthernetModels.Values.FirstOrDefault();
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, new HardwareModel() { DeviceModel = m }, InputDialogDisplayMode.InsertRemote, RemoteModuleViewer.SelectedIndex);
            wnd.ShowDialog();
        }
        private void InsertRemoteModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = RemoteModuleViewer?.SelectedItem != null;
        }
        private void EditRemoteModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HardwareViewer wnd = new HardwareViewer(DataContext as HardwareModels, RemoteModuleViewer.SelectedItem as HardwareModel, InputDialogDisplayMode.EditRemote, RemoteModuleViewer.SelectedIndex);
            wnd.ShowDialog();
        }
        private void EditRemoteModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = RemoteModuleViewer?.SelectedItem != null;
        }
        private void RemoveRemoteModuleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = RemoteModuleViewer.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as HardwareModels).RemoveRemote(RemoteModuleViewer.SelectedIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void RemoveRemoteModuleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = RemoteModuleViewer?.SelectedItem != null;
        }
    }
}
