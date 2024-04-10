using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

#pragma warning disable SYSLIB0011

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
        private UserControl[] __process_data_image_controls;
        OperatingHistory __operating_history;

        public ObjectsViewer(ObjectDictionary od, VariableDictionary vd, ControllerConfiguration cmc, VariablesModel vmodels, ControllerConfigurationModel cmodels,
            ProcessDataImage txdiagnostic, ProcessDataImage txbit, ProcessDataImage txblock,
            ProcessDataImage rxcontrol, ProcessDataImage rxbit, ProcessDataImage rxblock,
            InterlockCollection interlock, OperatingHistory history = null)
        {
            InitializeComponent();
            DataContext = new ObjectsModel(od, vd, cmc, vmodels, cmodels, history);
            __operating_history = history;
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

            __process_data_image_controls = new UserControl[] 
            {
                TxDiagnosticArea,
                TxBitArea,
                TxBlockArea,
                RxControlArea,
                RxBitArea,
                RxBlockArea
            };
        }

        private bool __raw_viewer()
        {
            return MainViewer.GroupColumnDescriptions.Count == 0 && MainViewer.SortColumnDescriptions.Count == 0 && MainViewer.Columns.All(c => c.FilterPredicates.Count == 0);
        }

        private ObjectModel? __default_object_model(uint index)
        {
            if(RecordUtility.ContainsRecord<ObjectModel>() )
            {
                ObjectModel o = RecordUtility.DefaultRecord<ObjectsModel, ObjectModel>(DataContext as ObjectsModel);
                if (o != null)
                {
                    o.Index = index;
                    return o;
                }
            }
            else if (RecordUtility.ContainsRecord<VariableModel>())
            {
                VariableModel v = RecordUtility.DefaultRecord<VariablesModel, VariableModel>((DataContext as ObjectsModel).VariableModelSource);
                if (v != null && (DataContext as ObjectsModel).VariableModelSource.Contains(v.Name))
                {
                    return new ObjectModel()
                    {
                        Index = index,
                        Unused = true,
                        VariableName = v.Name,
                        VariableDataType = (DataContext as ObjectsModel).Variables.Variables[v.Name].Type.Name,
                        VariableUnit = (DataContext as ObjectsModel).Variables.Variables[v.Name].Unit,
                        VariableComment = (DataContext as ObjectsModel).Variables.Variables[v.Name].Comment,
                        EnableBinding = false,
                        EnableValueRange = false,
                        EnableValueConverter = false,
                    };
                }
            }
            return null;
        }

        private void OnMainViewer_DragStart(object sender, GridRowDragStartEventArgs e)
        {
            if (__raw_viewer() == false || MainViewer.SelectedItems.Count != 1)
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

        private void OnMainViewer_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnMainViewer_Dropped(object sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None && __raw_viewer() && e.IsFromOutSideSource == false)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                var objects = DataContext as ObjectsModel;
                ObjectModel targetObject = objects.Objects[(int)e.TargetRecord];

                MainViewer.View.BeginInit();
                int dragIndex = objects.IndexOf(draggingRecords[0] as ObjectModel);
                int targetIndex = objects.IndexOf(targetObject);
                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;
                objects.Move(dragIndex, insertionIndex);
                MainViewer.View.EndInit();
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
            ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, __default_object_model(mindex)?? new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Add);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
            {
                MainViewer.SelectedItem = wnd.Result;
                var line = MainViewer.ResolveToRowIndex(MainViewer.SelectedItem);
                if (line != -1)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(line, MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
            ObjectViewer wnd = new ObjectViewer(DataContext as ObjectsModel, __default_object_model(mindex)??new ObjectModel() { Index = mindex }, InputDialogDisplayMode.Insert, MainViewer.SelectedIndex);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wnd.ShowDialog() == true)
            {
                MainViewer.SelectedItem = wnd.Result;
                var line = MainViewer.ResolveToRowIndex(MainViewer.SelectedItem);
                if(line != -1)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(line, MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null && __raw_viewer() && MainViewer.SelectedItems.Count == 1;
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
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null && MainViewer.SelectedItems.Count == 1;
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = string.Empty;
            if (MainViewer.SelectedItems.Count == 1)
                record = MainViewer.SelectedItem.ToString();
            else
            {
                int i = 0;
                for (i = 0; i < MainViewer.SelectedItems.Count; ++i)
                {
                    if (i >= 7)
                        break;
                    record += (MainViewer.SelectedItems[i] as ObjectModel).ToString() + "\n";
                }
                if (i != MainViewer.SelectedItems.Count)
                {
                    record += "...\n";
                    record += (MainViewer.SelectedItems[MainViewer.SelectedItems.Count - 1] as ObjectModel).ToString() + "\n";
                }
            }
            if (MessageBox.Show("Are you sure you want to remove the record(s) :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var list = MainViewer.SelectedItems.Select(r => r as ObjectModel).ToList();
            if(list.Count > 1)
                __operating_history.EnterBatchOperating();
            try
            {
                foreach (var i in list)
                    (DataContext as ObjectsModel).Remove(i);
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (list.Count > 1)
                __operating_history.ExitBatchOperating();
        }

        private void RemoveRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null && MainViewer.SelectedItems.All(v => (v as ObjectModel).Unused == true);
        }

        private void DefaultRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ex = RecordUtility.CopyRecords(MainViewer.SelectedItems.Select(r => r as ObjectModel).OrderBy(r => (DataContext as ObjectsModel).IndexOf(r)));
            if (ex != null)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DefaultRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer != null && MainViewer.SelectedItem != null;
        }

        private void AddRecordExCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(RecordUtility.ContainsRecord<ObjectModel>())
            {
                var records = RecordUtility.PasteAllRecords<ObjectsModel, ObjectModel>(DataContext as ObjectsModel);
                if (records.Count != 0)
                {
                    uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
                    MainViewer.ClearSelections(false);
                    if (records.Count > 1)
                        __operating_history.EnterBatchOperating();
                    foreach (var r in records)
                    {
                        DebugConsole.WriteInfo($"Apply object index [0x{mindex:X8}]");
                        r.Index = mindex;

                        try
                        {
                            (DataContext as ObjectsModel).Add(r);
                        }
                        catch (LombardiaException ex)
                        {
                            if(MessageBox.Show($"At least one exception has occurred when adding the following record:\n{r.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                continue;
                            else
                                break;
                        }
                        MainViewer.SelectedItems.Add(r);
                        mindex++;
                    }
                    if (records.Count > 1)
                        __operating_history.ExitBatchOperating();
                }
            }
            else if(RecordUtility.ContainsRecord<VariableModel>())
            {
                var records = RecordUtility.PasteAllRecords<VariablesModel, VariableModel>((DataContext as ObjectsModel).VariableModelSource);
                if (records.Count != 0 && records.All(r => (DataContext as ObjectsModel).VariableModelSource.Contains(r.Name)))
                {
                    uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
                    MainViewer.ClearSelections(false);
                    if (records.Count > 1)
                        __operating_history.EnterBatchOperating();
                    foreach (var r in records)
                    {
                        DebugConsole.WriteInfo($"Apply object index [0x{mindex:X8}]");
                        ObjectModel o = null;
                        try
                        {
                            o = new ObjectModel()
                            {
                                Index = mindex,
                                Unused = true,
                                VariableName = r.Name,
                                VariableDataType = (DataContext as ObjectsModel).Variables.Variables[r.Name].Type.Name,
                                VariableUnit = (DataContext as ObjectsModel).Variables.Variables[r.Name].Unit,
                                VariableComment = (DataContext as ObjectsModel).Variables.Variables[r.Name].Comment,
                                EnableBinding = false,
                                EnableValueRange = false,
                                EnableValueConverter = false,
                            };
                            (DataContext as ObjectsModel).Add(o);
                        }
                        catch (LombardiaException ex)
                        {
                            if (MessageBox.Show($"At least one exception has occurred when adding the following record:\n{o.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                continue;
                            else
                                break;
                        }
                        MainViewer.SelectedItems.Add(o);
                        mindex++;
                    }
                    if (records.Count > 1)
                        __operating_history.ExitBatchOperating();
                }
            }
            if (MainViewer.SelectedItems.Count > 0)
            {
                var line = MainViewer.ResolveToRowIndex(MainViewer.SelectedItems.First());
                if(line != -1)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(line, MainViewer.ResolveToStartColumnIndex()));
            }
        }

        private void InsertRecordExCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int insertPos = MainViewer.SelectedIndex;
            if (RecordUtility.ContainsRecord<ObjectModel>())
            {
                var records = RecordUtility.PasteAllRecords<ObjectsModel, ObjectModel>(DataContext as ObjectsModel);
                if (records.Count != 0)
                {
                    uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
                    MainViewer.ClearSelections(false);
                    if (records.Count > 1)
                        __operating_history.EnterBatchOperating();
                    foreach (var r in records.Reverse<ObjectModel>())
                    {
                        DebugConsole.WriteInfo($"Apply object index [0x{mindex:X8}]");
                        r.Index = mindex;

                        try
                        {
                            (DataContext as ObjectsModel).Insert(insertPos, r);
                        }
                        catch (LombardiaException ex)
                        {
                            if (MessageBox.Show($"At least one exception has occurred when inserting the following record:\n{r.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                continue;
                            else
                                break;
                        }
                        MainViewer.SelectedItems.Add(r);
                        mindex++;
                    }
                    if (records.Count > 1)
                        __operating_history.ExitBatchOperating();
                }
            }
            else if (RecordUtility.ContainsRecord<VariableModel>())
            {
                var records = RecordUtility.PasteAllRecords<VariablesModel, VariableModel>((DataContext as ObjectsModel).VariableModelSource);
                if (records.Count != 0 && records.All(r => (DataContext as ObjectsModel).VariableModelSource.Contains(r.Name)))
                {
                    uint mindex = (DataContext as ObjectsModel).Objects.Count == 0 ? 0 : (DataContext as ObjectsModel).Objects.AsParallel().Max(o => o.Index) + 1;
                    MainViewer.ClearSelections(false);
                    if (records.Count > 1)
                        __operating_history.EnterBatchOperating();
                    foreach (var r in records.Reverse<VariableModel>())
                    {
                        DebugConsole.WriteInfo($"Apply object index [0x{mindex:X8}]");
                        ObjectModel o = null;
                        try
                        {
                            o = new ObjectModel()
                            {
                                Index = mindex,
                                Unused = true,
                                VariableName = r.Name,
                                VariableDataType = (DataContext as ObjectsModel).Variables.Variables[r.Name].Type.Name,
                                VariableUnit = (DataContext as ObjectsModel).Variables.Variables[r.Name].Unit,
                                VariableComment = (DataContext as ObjectsModel).Variables.Variables[r.Name].Comment,
                                EnableBinding = false,
                                EnableValueRange = false,
                                EnableValueConverter = false,
                            };
                            (DataContext as ObjectsModel).Insert(insertPos, o);
                        }
                        catch (LombardiaException ex)
                        {
                            if (MessageBox.Show($"At least one exception has occurred when inserting the following record:\n{o.ToString()}\n{ex.Message}\nDo you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                continue;
                            else
                                break;
                        }
                        MainViewer.SelectedItems.Add(o);
                        mindex++;
                    }
                    if (records.Count > 1)
                        __operating_history.ExitBatchOperating();
                }
            }
            if (MainViewer.SelectedItems.Count > 0)
            {
                var line = MainViewer.ResolveToRowIndex(MainViewer.SelectedItems.Last());
                if(line != -1)
                    MainViewer.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(line, MainViewer.ResolveToStartColumnIndex()));
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

        public bool SaveLayoutState(string path)
        {
            if (DockingManager.IsLoaded)
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                DockingManager.SaveDockState(formatter, StorageFormat.Xml, path ?? "pdo_default_layout_state.xml");
                return true;
            }
            return false;
        }

        protected bool LoadLayoutState(string path)
        {
            if (DockingManager.IsLoaded == false)
                return false;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var res = DockingManager.LoadDockState(formatter, StorageFormat.Xml, path ?? "pdo_default_layout_state.xml");
            if (res == false)
                DockingManager.ResetState();
            return res;
        }

        protected bool InitializeLayoutState()
        {
            if (LoadLayoutState(null))
                return true;
            else
            {
                Uri path = new Uri("/LayoutProfile/pdo_default_layout_state.xml", UriKind.Relative);
                var profile = Application.GetResourceStream(path).Stream;
                XmlReader reader = XmlReader.Create(profile);
                bool res = DockingManager.LoadDockState(reader);
                return res;
            }
        }

        private LoadingIndicator __loading_dialog;
        public bool LayoutFinished { get; private set; } = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (__loading_dialog != null)
            {
                __loading_dialog.CloseIndicator(new Action<object>(__initialize_layout_state), null);
                __loading_dialog = null;
            }
            else
                __initialize_layout_state(null);
        }

        private void __initialize_layout_state(object parameter)
        {
            InitializeLayoutState();
            LayoutFinished = true;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && __loading_dialog == null && IsLoaded == false)
            {
                __loading_dialog = new LoadingIndicator();
                __loading_dialog.ShowIndicator();
            }
        }

        private void FindInInterlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint index;
            int pos;
            if (e.Parameter is ProcessDataModel)
                index = (e.Parameter as ProcessDataModel).Index;
            else if (e.Parameter is ObjectModel)
                index = (e.Parameter as ObjectModel).Index;
            else
                return;
            pos = (DataContext as ObjectsModel).InterlockLogics.FindNext(index);
            if (pos == -1)
            {
                string record = e.Parameter is ProcessDataModel ? (e.Parameter as ProcessDataModel).ToString() : (e.Parameter as ObjectModel).ToString();
                MessageBox.Show("<Interlock Area> does not reference the selected record :\n" + record, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var o = (DataContext as ObjectsModel).InterlockLogics.InterlockLogicModels[pos];
                __interlock_viewer.InterlockLogicList.ScrollIntoView(o);
                __interlock_viewer.InterlockLogicList.SelectedIndex = pos;
            }
        }

        private void FindInInterlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is ProcessDataModel)
                e.CanExecute = __interlock_viewer.InterlockLogicList.IsEnabled == true;
            else if (e.Parameter is ObjectModel)
                e.CanExecute = MainViewer.SelectedItems.Count == 1 && __interlock_viewer.InterlockLogicList.IsEnabled == true;
        }

        private void FindInProcessDataImageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint index = (e.Parameter as ObjectModel).Index;
            int pos;

            foreach(var w in __process_data_image_controls)
            {
                pos = (w.DataContext as ProcessDataImageModel).Find(index);
                if(pos != -1)
                {
                    DockingManager.ActiveWindow = w;
                    ProcessDataImageViewer viewer = w.Content as ProcessDataImageViewer;
                    viewer.ProcessDataImageGrid.SelectedIndex = pos;
                    viewer.ProcessDataImageGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(
                            viewer.ProcessDataImageGrid.ResolveToRowIndex(viewer.ProcessDataImageGrid.SelectedItem), viewer.ProcessDataImageGrid.ResolveToStartColumnIndex()));
                    return;
                }
            }
            string record = (e.Parameter as ObjectModel).ToString();
            MessageBox.Show("<Process Data Image Area> does not reference the selected record :\n" + record, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FindInProcessDataImageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainViewer.SelectedItems.Count == 1;
        }

        private void RemoveUnusedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = DataContext as ObjectsModel;
            List<ObjectModel> unused = new List<ObjectModel>();
            foreach(var o in m.Objects)
            {
                if(m.IsUnused(o.Index))
                    unused.Add(o);
            }
            if (unused.Count == 0)
            {
                MessageBox.Show("There's no record that fits the criteria.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if(unused.Count > 1)
                __operating_history.EnterBatchOperating();
            foreach (var o in unused)
            {
                var res = MessageBox.Show("Are you sure you want to remove the record :\n" + o.ToString(), "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        (DataContext as ObjectsModel).Remove(o);
                    }
                    catch (LombardiaException ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (res == MessageBoxResult.Cancel)
                    break;
            }
            if (unused.Count > 1)
                __operating_history.ExitBatchOperating();
        }

        private void RemoveAllUnusedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var m = DataContext as ObjectsModel;
            string record = string.Empty;
            List<ObjectModel> unused = new List<ObjectModel>();
            foreach (var o in m.Objects)
            {
                if (m.IsUnused(o.Index))
                    unused.Add(o);
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
            if (unused.Count > 1)
                __operating_history.EnterBatchOperating();
            foreach (var o in unused)
            {
                try
                {
                    (DataContext as ObjectsModel).Remove(o);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
            if (unused.Count > 1)
                __operating_history.ExitBatchOperating();
        }
    }
}
