using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// ProcessDataImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessDataImageViewer : UserControl
    {
        private SfDataGrid __object_source;
        private ObjectDictionary __object_dictionary;
        private ProcessDataImageAccess __process_data_access;
        private ProcessDataImageLayout __process_data_layout;
        private ObjectsModel __objects_model;
        public ProcessDataImageViewer(ProcessDataImage pdi, ObjectDictionary od, SfDataGrid source, ObjectsModel objects, OperatingHistory history)
        {
            InitializeComponent();
            DataContext = new ProcessDataImageModel(pdi, od, source.DataContext as ObjectsModel, history);
            __process_data_access = pdi.Access;
            __process_data_layout = pdi.Layout;
            __object_source = source;
            __object_dictionary = od;
            __objects_model = objects;
            ProcessDataImageGrid.RowDragDropController.DragStart += OnMainViewer_DragStart;
            ProcessDataImageGrid.RowDragDropController.DragOver += OnMainViewer_DragOver;
            ProcessDataImageGrid.RowDragDropController.Dropped += OnMainViewer_Dropped;
        }

        private void OnMainViewer_DragStart(object sender, GridRowDragStartEventArgs e)
        {
            if ((DataContext as ProcessDataImageModel).IsOffline == false || ProcessDataImageGrid.SelectedItems.Count != 1)
                e.Handled = true;
        }

        private void OnMainViewer_DragOver(object sender, GridRowDragOverEventArgs e)
        {
            if (e.IsFromOutSideSource)
            {
                //e.Handled = true;
                e.ShowDragUI = false;
            }
        }

        private void OnMainViewer_Dropped(object sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None && e.IsFromOutSideSource == false)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var dataImage = DataContext as ProcessDataImageModel;
                ProcessDataModel targetData = dataImage.ProcessDataModels[(int)e.TargetRecord];

                ProcessDataImageGrid.BeginInit();
                int dragIndex = dataImage.IndexOf(draggingRecords[0] as ProcessDataModel);
                int targetIndex = dataImage.IndexOf(targetData);
                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;
                dataImage.Move(dragIndex, insertionIndex);
                ProcessDataImageGrid.EndInit();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ObjectModel o;
                if((DataContext as ProcessDataImageModel).DirectModeOperation)
                {
                    uint mindex = 0;
                    if (__process_data_access == ProcessDataImageAccess.TX)
                        mindex = __objects_model.Objects.AsParallel().Select(o => o.Index).Where(i => (i & 0x80000000) != 0).DefaultIfEmpty<uint>(0x7FFFFFFF).Max(i => i) + 1;
                    else
                        mindex = __objects_model.Objects.AsParallel().Select(o => o.Index).Where(i => (i & 0x80000000) == 0).DefaultIfEmpty<uint>(0xFFFFFFFF).Max(i => i) + 1;
                    ObjectViewer wnd = new ObjectViewer(__objects_model, new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Add);
                    wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    if (wnd.ShowDialog() == true)
                        o = wnd.Result;
                    else
                        return;
                }
                else
                    o = __object_source.SelectedItem as ObjectModel;
                ProcessDataModel p = new ProcessDataModel(__object_dictionary.ProcessObjects[o.Index], __process_data_access, __process_data_layout);
                (DataContext as ProcessDataImageModel).Add(p);
                ProcessDataImageGrid.SelectedItem = p;
                ProcessDataImageGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                                 ProcessDataImageGrid.ResolveToRowIndex(ProcessDataImageGrid.SelectedItem),
                                 ProcessDataImageGrid.ResolveToStartColumnIndex()));
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext != null) && ((DataContext as ProcessDataImageModel).DirectModeOperation || __object_source?.SelectedItem != null) && (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                ObjectModel o;
                if ((DataContext as ProcessDataImageModel).DirectModeOperation)
                {
                    uint mindex = 0;
                    if (__process_data_access == ProcessDataImageAccess.TX)
                        mindex = __objects_model.Objects.AsParallel().Select(o => o.Index).Where(i => (i & 0x80000000) != 0).DefaultIfEmpty<uint>(0x7FFFFFFF).Max(i => i) + 1;
                    else
                        mindex = __objects_model.Objects.AsParallel().Select(o => o.Index).Where(i => (i & 0x80000000) == 0).DefaultIfEmpty<uint>(0xFFFFFFFF).Max(i => i) + 1;
                    ObjectViewer wnd = new ObjectViewer(__objects_model, new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Add);
                    wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    if (wnd.ShowDialog() == true)
                        o = wnd.Result;
                    else
                        return;
                }
                else
                    o = __object_source.SelectedItem as ObjectModel;
                ProcessDataModel p = new ProcessDataModel(__object_dictionary.ProcessObjects[o.Index], __process_data_access, __process_data_layout);
                (DataContext as ProcessDataImageModel).Insert(ProcessDataImageGrid.SelectedIndex, p);
                ProcessDataImageGrid.SelectedItem = p;
                ProcessDataImageGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                                 ProcessDataImageGrid.ResolveToRowIndex(ProcessDataImageGrid.SelectedItem),
                                 ProcessDataImageGrid.ResolveToStartColumnIndex()));
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext != null) && ((DataContext as ProcessDataImageModel).DirectModeOperation || __object_source?.SelectedItem != null) && ProcessDataImageGrid?.SelectedItems.Count == 1 && (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectModel o = __objects_model.Objects.Single(m => m.Index == (ProcessDataImageGrid.SelectedItem as ProcessDataModel).Index);
            ObjectViewer wnd = new ObjectViewer(__objects_model, o, InputDialogDisplayMode.Edit);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            wnd.ShowDialog();
        }

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && (DataContext as ProcessDataImageModel).DirectModeOperation && ProcessDataImageGrid?.SelectedItems.Count == 1 && (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = string.Empty;
            if (ProcessDataImageGrid.SelectedItems.Count == 1)
                record = ProcessDataImageGrid.SelectedItem.ToString();
            else
            {
                int i = 0;
                ProcessDataModel m;
                for (i = 0; i < ProcessDataImageGrid.SelectedItems.Count; ++i)
                {
                    if (i >= 15)
                        break;
                    m = ProcessDataImageGrid.SelectedItems[i] as ProcessDataModel;
                    record += m.Index.ToString("X08") + " : " + m.VariableName + "\n";
                }
                if (i != ProcessDataImageGrid.SelectedItems.Count)
                {
                    record += "...\n";
                    m = ProcessDataImageGrid.SelectedItems[ProcessDataImageGrid.SelectedItems.Count - 1] as ProcessDataModel;
                    record += m.Index.ToString("X08") + " : " + m.VariableName + "\n";
                }
            }
            if (MessageBox.Show("Are you sure you want to remove the record(s) " +
                (((DataContext as ProcessDataImageModel).DirectModeOperation) ? "(DIRECT MODE) ":"" )+
                ":\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var indexes = ProcessDataImageGrid.SelectedItems.Select(r => r as ProcessDataModel).ToList();
            try
            {
                foreach (var i in indexes)
                {
                    (DataContext as ProcessDataImageModel).Remove(i);
                    if ((DataContext as ProcessDataImageModel).DirectModeOperation)
                        __objects_model.Remove(i.Index);
                }
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ProcessDataImageGrid?.SelectedItem != null && (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void MoveUpRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessDataImageGrid.BeginInit();
            var dataImage = DataContext as ProcessDataImageModel;

            var indexes = ProcessDataImageGrid.SelectedItems.Select(r => r as ProcessDataModel).OrderBy(r => dataImage.IndexOf(r)).ToList();
            foreach(var i in indexes)
            {
                int sourceIndex = dataImage.IndexOf(i);
                int targetIndex = sourceIndex - 1;
                dataImage.Move(sourceIndex, targetIndex);
            }
            ProcessDataImageGrid.SelectedItems = new ObservableCollection<object>(indexes);
            ProcessDataImageGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                 ProcessDataImageGrid.ResolveToRowIndex(indexes[0]),
                 ProcessDataImageGrid.ResolveToStartColumnIndex()));
            ProcessDataImageGrid.EndInit();
            CommandManager.InvalidateRequerySuggested();
        }

        private void MoveUpRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null &&
                (ProcessDataImageGrid.SelectedItems.Count > 0 && ProcessDataImageGrid.SelectedItems.Min(i => (DataContext as ProcessDataImageModel).IndexOf(i as ProcessDataModel)) != 0) && 
                (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void MoveDownRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessDataImageGrid.BeginInit();
            var dataImage = DataContext as ProcessDataImageModel;

            var indexes = ProcessDataImageGrid.SelectedItems.Select(r => r as ProcessDataModel).OrderByDescending(r => dataImage.IndexOf(r)).ToList();
            foreach (var i in indexes)
            {
                int sourceIndex = dataImage.IndexOf(i);
                int targetIndex = sourceIndex + 1;
                dataImage.Move(sourceIndex, targetIndex);
            }
            ProcessDataImageGrid.SelectedItems = new ObservableCollection<object>(indexes);
            ProcessDataImageGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                 ProcessDataImageGrid.ResolveToRowIndex(indexes[0]),
                 ProcessDataImageGrid.ResolveToStartColumnIndex()));
            ProcessDataImageGrid.EndInit();
            CommandManager.InvalidateRequerySuggested();
        }

        private void MoveDownRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null &&
                (ProcessDataImageGrid.SelectedItems.Count > 0 && ProcessDataImageGrid.SelectedItems.Max(i => (DataContext as ProcessDataImageModel).IndexOf(i as ProcessDataModel)) != (DataContext as ProcessDataImageModel).ProcessDataModels.Count - 1) &&
                (DataContext as ProcessDataImageModel).IsOffline == true;
        }

        private void FindInInterlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(ProcessDataImageGrid?.SelectedItems.Count == 1)
            {
                e.Handled = false;
            }
            else
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        public void UpdateBindingSource()
        {
            var binding = InputOffsetInWord.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = InputSizeInWord.GetBindingExpression(TextBox.TextProperty);
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
            if(e.Key == Key.Enter && e.OriginalSource.GetType() == typeof(TextBox))
            {
                var binding = (e.OriginalSource as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }
        }

        private void ProcessDataImageGrid_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if(DataContext != null && (DataContext as ProcessDataImageModel).DirectModeOperation && (DataContext as ProcessDataImageModel).IsOffline == true)
            {
                ObjectModel o = __objects_model.Objects.Single(m => m.Index == (e.Record as ProcessDataModel).Index);
                ObjectViewer wnd = new ObjectViewer(__objects_model, o, InputDialogDisplayMode.Edit);
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                wnd.ShowDialog();
            }
        }
    }
}
