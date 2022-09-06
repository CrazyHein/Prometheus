using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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
    /// VariablesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class VariablesViewer : UserControl
    {
        private VariableModel __default_variable_model;
        public VariablesViewer(VariableDictionary dic, DataTypeCatalogue dtc, OperatingHistory history = null)
        {
            InitializeComponent();
            DataContext = new VariablesModel(dic, dtc, history);
            MainViewer.RowDragDropController.DragStart += OnMainViewer_DragStart;
            MainViewer.RowDragDropController.Dropped += OnMainViewer_Dropped;
            MainViewer.RowDragDropController.DragOver += OnMainViewer_DragOver;
        }

        private bool __raw_viewer()
        {
            return MainViewer.GroupColumnDescriptions.Count == 0 && MainViewer.SortColumnDescriptions.Count == 0 && MainViewer.Columns.All(c => c.FilterPredicates.Count == 0);
        }

        private void OnMainViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var variablesModel = DataContext as VariablesModel;
                VariableModel targetVariable = variablesModel.Variables[(int)e.TargetRecord];

                MainViewer.BeginInit();
                int dragIndex = variablesModel.IndexOf(draggingRecords[0] as VariableModel);
                int targetIndex = variablesModel.IndexOf(targetVariable);
                int insertionIndex = 0;
                if(dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;
                variablesModel.Move(dragIndex, insertionIndex);
                MainViewer.EndInit();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void OnMainViewer_DragStart(object sender, Syncfusion.UI.Xaml.Grid.GridRowDragStartEventArgs e)
        {
            if (__raw_viewer() == false)
                e.Handled = true;
        }
        private void OnMainViewer_DragOver(object sender, Syncfusion.UI.Xaml.Grid.GridRowDragOverEventArgs e)
        {
            if(e.IsFromOutSideSource)
            {
                //e.Handled = true;
                e.ShowDragUI = false;
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataType dt = (DataContext as VariablesModel).DataTypeCatalogue.DataTypes.Values.FirstOrDefault();
            VariableViewer wnd = new VariableViewer(DataContext as VariablesModel, __default_variable_model ?? new VariableModel() { DataType = dt }, InputDialogDisplayMode.Add);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
            {
                MainViewer.SelectedItem = wnd.Result;
                if (__raw_viewer() == true)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                        MainViewer.ResolveToRowIndex(MainViewer.SelectedItem), MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null;
        }

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            VariableViewer wnd = new VariableViewer(DataContext as VariablesModel, MainViewer.SelectedItem as VariableModel, InputDialogDisplayMode.Edit);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
                MainViewer.SelectedItem = wnd.Result;
        }

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataType dt = (DataContext as VariablesModel).DataTypeCatalogue.DataTypes.Values.FirstOrDefault();
            VariableViewer wnd = new VariableViewer(DataContext as VariablesModel, __default_variable_model ?? new VariableModel() { DataType = dt }, InputDialogDisplayMode.Insert, MainViewer.SelectedIndex);
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
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null && __raw_viewer();
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = MainViewer.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as VariablesModel).Remove(MainViewer.SelectedItem as VariableModel);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DefaultRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            __default_variable_model = MainViewer.SelectedItem as VariableModel;
        }

        private void MainViewer_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if (e.Record as VariableModel != null)
            {
                //var res = object.ReferenceEquals((DataContext as VariablesModel).Variables[0], e.Record as VariableModel);
                VariableViewer wnd = new VariableViewer(DataContext as VariablesModel, e.Record as VariableModel, InputDialogDisplayMode.Edit);
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (wnd.ShowDialog() == true)
                    MainViewer.SelectedItem = wnd.Result;
            }
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void MainViewer_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (MainViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
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
            var m = DataContext as VariablesModel;
            List<VariableModel> unused = new List<VariableModel>();
            foreach (var v in m.Variables)
            {
                if (m.IsUnused(v.Name))
                    unused.Add(v);
            }
            if (unused.Count == 0)
            {
                MessageBox.Show("There's no record that fits the criteria.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            foreach (var v in unused)
            {
                var res = MessageBox.Show("Are you sure you want to remove the record :\n" + v.ToString(), "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        (DataContext as VariablesModel).Remove(v);
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
            var m = DataContext as VariablesModel;
            string record = string.Empty;
            List<VariableModel> unused = new List<VariableModel>();
            foreach (var v in m.Variables)
            {
                if (m.IsUnused(v.Name))
                    unused.Add(v);
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
            foreach (var v in unused)
            {
                try
                {
                    (DataContext as VariablesModel).Remove(v);
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
