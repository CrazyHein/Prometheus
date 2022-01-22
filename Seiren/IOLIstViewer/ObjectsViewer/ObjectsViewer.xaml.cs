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
    /// ObjectsViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectsViewer : UserControl
    {
        private ProcessDataImageViewer __tx_diagnostic_area_viewer;
        private ProcessDataImageViewer __tx_bit_area_viewer;
        private ProcessDataImageViewer __tx_block_area_viewer;
        private ProcessDataImageViewer __rx_control_area_viewer;
        private ProcessDataImageViewer __rx_bit_area_viewer;
        private ProcessDataImageViewer __rx_block_area_viewer;
        private InterlockCollectionViewer __interlock_viewer;

        public ObjectsViewer(ObjectDictionary od, VariableDictionary vd, ControllerConfiguration cmc,
            ProcessDataImage txdiagnostic, ProcessDataImage txbit, ProcessDataImage txblock,
            ProcessDataImage rxcontrol, ProcessDataImage rxbit, ProcessDataImage rxblock,
            InterlockCollection interlock, OperatingHistory history = null)
        {
            InitializeComponent();
            DataContext = new ObjectsModel(od, vd, cmc, history);
            MainViewer.RowDragDropController.DragStart += OnMainViewer_DragStart;
            MainViewer.RowDragDropController.DragOver += OnMainViewer_DragOver;
            MainViewer.RowDragDropController.Drop += OnMainViewer_Drop;
            MainViewer.RowDragDropController.Dropped += OnMainViewer_Dropped;

            __tx_diagnostic_area_viewer = new ProcessDataImageViewer(txdiagnostic, od, MainViewer, DataContext as ObjectsModel, history);
            __tx_bit_area_viewer = new ProcessDataImageViewer(txbit, od, MainViewer, DataContext as ObjectsModel, history);
            __tx_block_area_viewer = new ProcessDataImageViewer(txblock, od, MainViewer, DataContext as ObjectsModel, history);
            __rx_control_area_viewer = new ProcessDataImageViewer(rxcontrol, od, MainViewer, DataContext as ObjectsModel, history);
            __rx_bit_area_viewer = new ProcessDataImageViewer(rxbit, od, MainViewer, DataContext as ObjectsModel, history);
            __rx_block_area_viewer = new ProcessDataImageViewer(rxblock, od, MainViewer, DataContext as ObjectsModel, history);
            __interlock_viewer = new InterlockCollectionViewer(interlock, od, txbit, rxbit, MainViewer, history);

            TxDiagnosticArea.Content = __tx_diagnostic_area_viewer;
            TxDiagnosticArea.DataContext = __tx_diagnostic_area_viewer.DataContext;
            TxBitArea.Content = __tx_bit_area_viewer;
            TxBitArea.DataContext = __tx_bit_area_viewer.DataContext;
            TxBlockArea.Content = __tx_block_area_viewer;
            TxBlockArea.DataContext = __tx_block_area_viewer.DataContext;
            RxControlArea.Content = __rx_control_area_viewer;
            RxControlArea.DataContext = __rx_control_area_viewer.DataContext;
            RxBitArea.Content = __rx_bit_area_viewer;
            RxBitArea.DataContext = __rx_bit_area_viewer.DataContext;
            RxBlockArea.Content = __rx_block_area_viewer;
            RxBlockArea.DataContext = __rx_block_area_viewer.DataContext;
            InterlockArea.Content = __interlock_viewer;
            InterlockArea.DataContext = __interlock_viewer.DataContext;

            (DataContext as ObjectsModel).TxDiagnosticObjects = __tx_diagnostic_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).TxBitObjects = __tx_bit_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).TxBlockObjects = __tx_block_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).RxControlObjects = __rx_control_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).RxBitObjects = __rx_bit_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).RxBlockObjects = __rx_block_area_viewer.DataContext as ProcessDataImageModel;
            (DataContext as ObjectsModel).InterlockLogics = __interlock_viewer.DataContext as InterlockCollectionModel;

            (__tx_bit_area_viewer.DataContext as ProcessDataImageModel).InterlockModels = __interlock_viewer.DataContext as InterlockCollectionModel;
            (__rx_bit_area_viewer.DataContext as ProcessDataImageModel).InterlockModels = __interlock_viewer.DataContext as InterlockCollectionModel;
        }

        private bool __raw_viewer()
        {
            return MainViewer.GroupColumnDescriptions.Count == 0 && MainViewer.SortColumnDescriptions.Count == 0 && MainViewer.Columns.All(c => c.FilterPredicates.Count == 0);
        }

        private void OnMainViewer_DragStart(object sender, GridRowDragStartEventArgs e)
        {
            if (__raw_viewer() == false)
                e.Handled = true;
        }

        private void OnMainViewer_DragOver(object sender, GridRowDragOverEventArgs e)
        {

        }

        private void OnMainViewer_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (__raw_viewer() == false)
                e.Handled = true;
        }

        private void OnMainViewer_Dropped(object sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None && __raw_viewer() && e.IsFromOutSideSource == false)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var objects = DataContext as ObjectsModel;
                ObjectModel targetObject = objects.Objects[(int)e.TargetRecord];

                MainViewer.BeginInit();
                int dragIndex = objects.IndexOf(draggingRecords[0] as ObjectModel);
                objects.Remove(draggingRecords[0] as ObjectModel, true, false);
                int targetIndex = objects.IndexOf(targetObject);
                int insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                objects.Insert(insertionIndex, draggingRecords[0] as ObjectModel, false);
                MainViewer.EndInit();
                objects.OperatingHistory?.PushOperatingRecord(
                    new OperatingRecord()
                    {
                        Host = objects,
                        Operation = Operation.Move,
                        OriginaPos = dragIndex,
                        NewPos = insertionIndex,
                        OriginalValue = draggingRecords[0] as ObjectModel,
                        NewValue = draggingRecords[0] as ObjectModel
                    });
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void MainViewer_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            if (e.Record as ObjectModel != null)
            {
                ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, e.Record as ObjectModel, InputDialogDisplayMode.Edit);
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (wnd.ShowDialog() == true)
                    MainViewer.SelectedItem = wnd.Result;
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
            ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Add);
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

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
            ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Insert, MainViewer.SelectedIndex);
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

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, MainViewer.SelectedItem as ObjectModel, InputDialogDisplayMode.Edit);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
                MainViewer.SelectedItem = wnd.Result;
        }

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = MainViewer.SelectedItem.ToString();
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as ObjectsModel).Remove(MainViewer.SelectedItem as ObjectModel);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void UpdateBindingSource()
        {
            __tx_diagnostic_area_viewer.UpdateBindingSource();
            __tx_bit_area_viewer.UpdateBindingSource();
            __tx_block_area_viewer.UpdateBindingSource();
            __rx_control_area_viewer.UpdateBindingSource();
            __rx_bit_area_viewer.UpdateBindingSource();
            __rx_block_area_viewer.UpdateBindingSource();
        }

        public bool HasError { get { return __tx_diagnostic_area_viewer.HasError || __tx_bit_area_viewer.HasError || __tx_block_area_viewer.HasError || __rx_control_area_viewer.HasError || __rx_bit_area_viewer.HasError || __rx_block_area_viewer.HasError; } }
    }
}
