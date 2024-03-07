using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ControllerConfigurationViewer(ControllerConfiguration cc, ControllerModelCatalogue cmc, OperatingHistory history = null)
        {
            InitializeComponent();
            DataContext = new ControllerConfigurationModel(cc, cmc, history);
            MainViewer.RowDragDropController.Dropped += OnMainViewer_Dropped;
            MainViewer.RowDragDropController.DragOver += OnMainViewer_DragOver;
        }

        private DeviceConfigurationModel? __DefaultDeviceConfigurationModel
        {
            get
            {
                return RecordUtility.DefaultRecord<ControllerConfigurationModel, DeviceConfigurationModel>(DataContext as ControllerConfigurationModel);
            }
        }
        private void OnMainViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var controllerConfiguration = DataContext as ControllerConfigurationModel;
                DeviceConfigurationModel targetDeviceConfiguration = controllerConfiguration.DeviceConfigurations[(int)e.TargetRecord];

                MainViewer.View.BeginInit();
                int dragIndex = controllerConfiguration.IndexOf(draggingRecords[0] as DeviceConfigurationModel);       
                int targetIndex = controllerConfiguration.IndexOf(targetDeviceConfiguration);
                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;
                controllerConfiguration.Move(dragIndex, insertionIndex);
                MainViewer.View.EndInit();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void OnMainViewer_DragOver(object sender, GridRowDragOverEventArgs e)
        {
            if (e.IsFromOutSideSource)
            {
                //e.Handled = true;
                e.ShowDragUI = false;
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LocalExtensionModel m = (DataContext as ControllerConfigurationModel).ControllerModelCatalogue.LocalExtensionModels.Values.FirstOrDefault();
            DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, __DefaultDeviceConfigurationModel??new DeviceConfigurationModel() { DeviceModel = m}, InputDialogDisplayMode.Add);
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
            LocalExtensionModel m = (DataContext as ControllerConfigurationModel).ControllerModelCatalogue.LocalExtensionModels.Values.FirstOrDefault();
            DeviceConfigurationViewer wnd = new DeviceConfigurationViewer(DataContext as ControllerConfigurationModel, __DefaultDeviceConfigurationModel??new DeviceConfigurationModel() { DeviceModel = m }, InputDialogDisplayMode.Insert, MainViewer.SelectedIndex);
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

        private void RemoveRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null && (MainViewer.SelectedItem as DeviceConfigurationModel).Unused == true;
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

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Task User Parameters File(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainViewer.View.BeginInit();
                try
                {
                    (DataContext as ControllerConfigurationModel).ImportDevices(open.FileName); 
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                MainViewer.View.EndInit();
            }
        }

        private void DefaultRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ex = RecordUtility.CopyRecords(MainViewer.SelectedItems.Select(r => r as DeviceConfigurationModel).OrderBy(r => (DataContext as ControllerConfigurationModel).IndexOf(r)));
            if (ex != null)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DefaultRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private int __unique_index = 1;
        public string __unique_naming(string name)
        {
            string originalName = name;
            string revisedName = originalName;

            while ((DataContext as ControllerConfigurationModel).Contains(revisedName))
            {
                revisedName = originalName + $"({__unique_index})";
                __unique_index++;
            }
            return revisedName;
        }

        private void AddRecordExCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var records = RecordUtility.PasteAllRecords<ControllerConfigurationModel, DeviceConfigurationModel>(DataContext as ControllerConfigurationModel);
            if (records.Count != 0)
            {
                MainViewer.ClearSelections(false);
                foreach (var r in records)
                {
                    string revisedName = __unique_naming(r.ReferenceName);
                    if (r.ReferenceName != revisedName)
                    {
                        DebugConsole.WriteInfo($"Module reference name [{r.ReferenceName}] -> [{revisedName}]");
                        r.ReferenceName = revisedName;
                    }
                    try
                    {
                        (DataContext as ControllerConfigurationModel).Add(r);
                    }
                    catch (LombardiaException ex)
                    {
                        if (MessageBox.Show($"At least one exception has occurred when adding the following record:\n{r.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            continue;
                        else
                            break;
                    }
                    MainViewer.SelectedItems.Add(r);
                }
                if (MainViewer.SelectedItems.Count > 0)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                               MainViewer.ResolveToRowIndex(MainViewer.SelectedItems.First()),
                               MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void InsertRecordExCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var records = RecordUtility.PasteAllRecords<ControllerConfigurationModel, DeviceConfigurationModel>(DataContext as ControllerConfigurationModel);
            int insertPos = MainViewer.SelectedIndex;
            if (records.Count != 0)
            {
                MainViewer.ClearSelections(false);
                foreach (var r in records.Reverse<DeviceConfigurationModel>())
                {
                    string revisedName = __unique_naming(r.ReferenceName);
                    if (r.ReferenceName != revisedName)
                    {
                        DebugConsole.WriteInfo($"Module reference name [{r.ReferenceName}] -> [{revisedName}]");
                        r.ReferenceName = revisedName;
                    }
                    try
                    {
                        (DataContext as ControllerConfigurationModel).Insert(insertPos, r);
                    }
                    catch (LombardiaException ex)
                    {
                        if (MessageBox.Show($"At least one exception has occurred when inserting the following record:\n{r.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            continue;
                        else
                            break;
                    }
                    MainViewer.SelectedItems.Add(r);
                }
                if (MainViewer.SelectedItems.Count > 0)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                               MainViewer.ResolveToRowIndex(MainViewer.SelectedItems.First()),
                               MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private LoadingIndicator __loading_dialog;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (__loading_dialog != null)
            {
                __loading_dialog.CloseIndicator(null, null);
                __loading_dialog = null;
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && __loading_dialog == null && IsLoaded == false)
            {
                __loading_dialog = new LoadingIndicator();
                __loading_dialog.ShowIndicator();
            }
        }

        private void RemoveUnusedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = DataContext as ControllerConfigurationModel;
            List<DeviceConfigurationModel> unused = new List<DeviceConfigurationModel>();
            foreach (var d in m.DeviceConfigurations)
            {
                if (m.IsUnused(d.ReferenceName))
                    unused.Add(d);
            }
            if (unused.Count == 0)
            {
                MessageBox.Show("There's no record that fits the criteria.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            foreach (var d in unused)
            {
                var res = MessageBox.Show("Are you sure you want to remove the record :\n" + d.ToString(), "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        (DataContext as ControllerConfigurationModel).Remove(d);
                    }
                    catch (LombardiaException ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (res == MessageBoxResult.Cancel)
                    break;
            }
        }

        private void RemoveAllUnusedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = DataContext as ControllerConfigurationModel;
            string record = string.Empty;
            List<DeviceConfigurationModel> unused = new List<DeviceConfigurationModel>();
            foreach (var d in m.DeviceConfigurations)
            {
                if (m.IsUnused(d.ReferenceName))
                    unused.Add(d);
            }
            if (unused.Count == 0)
            {
                MessageBox.Show("There's no record that fits the criteria.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (unused.Count == 1)
                record = unused[0].ToString();
            else
            {
                int i = 0;
                for (i = 0; i < unused.Count; ++i)
                {
                    if (i >= 5)
                        break;
                    record += unused[i].ToString() + "\n";
                }
                if (i != unused.Count)
                {
                    record += "...\n";
                    record += unused[unused.Count - 1].ToString() + "\n";
                }
            }
            if (MessageBox.Show("Are you sure you want to remove the " + unused.Count.ToString() + " record(s) :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            foreach (var d in unused)
            {
                try
                {
                    (DataContext as ControllerConfigurationModel).Remove(d);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
        }
    }
}
