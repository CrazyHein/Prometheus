using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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
    /// ControlBlockManager.xaml 的交互逻辑
    /// </summary>
    public partial class ControlBlockManager : UserControl
    {
        private ControlBlockModelCollection __control_blocks;
        private ContentControl __control_block_control;
        private ControlBlockControl? __dummy_control_block;
        private SequentialControl? __sequential_control_block;
        private LoopControl? __loop_control_block;
        private SwitchControl? __switch_control_block;
        private CompoundControl? __compound_control_block;
        private StepControl? __step;
        public ControlBlockManager(GlobalEventModelCollection globals, ControlBlockModelCollection blocks, ContentControl content)
        {
            InitializeComponent();
            __control_blocks = blocks;
            __control_block_control = content;
            sfControlBlocksTreeView.ItemsSource = blocks.ControlBlocks;
            DataContext = blocks;
        }

        private void __show_component_view(Component component)
        {
            if (component is SequentialModel)
            {
                if (__sequential_control_block == null)
                    __sequential_control_block = new SequentialControl(component as SequentialModel);
                else
                    __sequential_control_block.ResetDataModel(component as SequentialModel);

                __control_block_control.Content = __sequential_control_block;
            }
            else if (component is LoopModel)
            {
                if (__loop_control_block == null)
                    __loop_control_block = new LoopControl(component as LoopModel);
                else
                    __loop_control_block.ResetDataModel(component as LoopModel);

                __control_block_control.Content = __loop_control_block;
            }
            else if (component is SwitchModel)
            {
                if (__switch_control_block == null)
                    __switch_control_block = new SwitchControl(component as SwitchModel);
                else
                    __switch_control_block.ResetDataModel(component as SwitchModel);

                __control_block_control.Content = __switch_control_block;
            }
            else if (component is CompoundModel)
            {
                if (__compound_control_block == null)
                    __compound_control_block = new CompoundControl(component as CompoundModel);
                else
                    __compound_control_block.ResetDataModel(component as CompoundModel);

                __control_block_control.Content = __compound_control_block;
            }
            else if (component is StepModel)
            {
                if (__step == null)
                    __step = new StepControl(component as StepModel);
                else
                    __step.ResetDataModel(component as StepModel);

                __control_block_control.Content = __step;
            }
            else
            {
                if (__dummy_control_block == null)
                    __dummy_control_block = new ControlBlockControl();

                __control_block_control.Content = __dummy_control_block;
            }
        }

        private void __close_component_view()
        {
            __control_block_control.Content = null;
        }

        private void sfControlBlocksTreeView_SelectionChanged(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
                __show_component_view(e.AddedItems[0] as Component);
            else
                __close_component_view();
        }

        private void sfControlBlocksTreeView_SelectionChanging(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangingEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                if (e.RemovedItems[0] is StepModel)
                {
                    var step = e.RemovedItems[0] as StepModel;
                    if (step.Modified == true)
                    {
                        var ret = MessageBox.Show("Changes have been detected.\nClick 'Yes' to apply the changes or 'No' to discard the changes.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (ret == MessageBoxResult.No)
                            step.DiscardChanges();
                        else if (ret == MessageBoxResult.Yes)
                        {
                            try
                            {
                                step.ApplyChanges();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An exception has occurred while applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                e.Cancel = true;
                            }
                        }
                        else
                            e.Cancel = true;
                    }
                }
                else if (e.RemovedItems[0] is ControlBlockModel)
                {
                    var blk = e.RemovedItems[0] as ControlBlockModel;
                    if (blk.Modified == true)
                    {
                        var ret = MessageBox.Show("Changes have been detected.\nClick 'Yes' to apply the changes or 'No' to discard the changes.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (ret == MessageBoxResult.No)
                            blk.DiscardChanges();
                        else if (ret == MessageBoxResult.Yes)
                        {
                            try
                            {
                                blk.ApplyChanges();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An exception has occurred while applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                e.Cancel = true;
                            }
                        }
                        else
                            e.Cancel = true;
                    }
                }
            }
        }

        private void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Component? component = null;
                if (e.Parameter == DataContext)
                {
                    ControlBlockTypeSelector selector = new ControlBlockTypeSelector() { Owner = Application.Current.MainWindow };
                    if(selector.ShowDialog() == true)
                        component = (DataContext as ControlBlockModelCollection).AddControlBlock(selector.ControlBlockType);
                }
                else
                {
                    if (sfControlBlocksTreeView.SelectedItem is SequentialModel)
                    {
                        component = (sfControlBlocksTreeView.SelectedItem as SequentialModel).Add("step");
                    }
                }
                if(component != null)
                {
                    sfControlBlocksTreeView.SelectedItem = component;
                    __show_component_view(component);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void AddItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter == null || e.Parameter == DataContext)
                e.CanExecute = sfControlBlocksTreeView.SelectedItem == null || (sfControlBlocksTreeView.SelectedItem as Component).Modified == false;
            else
                e.CanExecute = (sfControlBlocksTreeView.SelectedItem as Component).Modified == false && (sfControlBlocksTreeView.SelectedItem is SequentialModel);// || sfControlBlocksTreeView.SelectedItem is SwitchModel);
            e.Handled = true;
        }

        private void InsertItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Component? component = null;
                if((sfControlBlocksTreeView.SelectedItem as Component).Owner == null)
                {
                    int pos = (DataContext as ControlBlockModelCollection).ControlBlocks.IndexOf(sfControlBlocksTreeView.SelectedItem as ControlBlockModel);
                    ControlBlockTypeSelector selector = new ControlBlockTypeSelector() { Owner = Application.Current.MainWindow };
                    if (selector.ShowDialog() == true)
                        component = (DataContext as ControlBlockModelCollection).InsertControlBlockAt(pos, selector.ControlBlockType);
                }
                else if(sfControlBlocksTreeView.SelectedItem is StepModel)
                {
                    var seq = (sfControlBlocksTreeView.SelectedItem as StepModel).Owner as SequentialModel;
                    component = seq.InsertBefore(seq.SubSteps.IndexOf(sfControlBlocksTreeView.SelectedItem as StepModel), "step");
                }

                if (component != null)
                {
                    sfControlBlocksTreeView.SelectedItem = component;
                    __show_component_view(component);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void InsertItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfControlBlocksTreeView.SelectedItem != null &&
                            (sfControlBlocksTreeView.SelectedItem as Component).Modified == false &&
                            ((sfControlBlocksTreeView.SelectedItem as Component).Owner == null || sfControlBlocksTreeView.SelectedItem is StepModel);// || ((sfControlBlocksTreeView.SelectedItem as Component)?.Owner is SwitchModel));
            e.Handled = true;
        }

        private void RemoveItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Component owner = (sfControlBlocksTreeView.SelectedItem as Component).Owner;
                if (owner == null)
                {
                    if (MessageBox.Show($"Are you sure you want to remove the Control Block:\n{(sfControlBlocksTreeView.SelectedItem as ControlBlockModel).Name}",
                        "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    (DataContext as ControlBlockModelCollection).RemoveControlBlock(sfControlBlocksTreeView.SelectedItem as ControlBlockModel);
                    __close_component_view();
                }
                else if (owner is SequentialModel)
                {
                    if (MessageBox.Show($"Are you sure you want to remove the Step:\n{(sfControlBlocksTreeView.SelectedItem as StepModel).Name}",
                        "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    (owner as SequentialModel).Remove(sfControlBlocksTreeView.SelectedItem as StepModel);
                    __close_component_view();
                }
                else if (owner is LoopModel)
                {
                    if (MessageBox.Show($"Are you sure you want to remove the Control Block:\n{(sfControlBlocksTreeView.SelectedItem as ControlBlockModel).Name}",
                        "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    (owner as LoopModel).ClearLoopBody();
                    __close_component_view();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void RemoveItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfControlBlocksTreeView.SelectedItem != null &&
                (sfControlBlocksTreeView.SelectedItem as Component).Modified == false;
            e.Handled = true;
        }

        private void CopyItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject((sfControlBlocksTreeView.SelectedItem as Component).ToJson().ToJsonString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void CopyItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfControlBlocksTreeView.SelectedItem != null && (sfControlBlocksTreeView.SelectedItem as Component).Modified == false;
            e.Handled = true;
        }

        private void PasteItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Component? component = null;
                if (sfControlBlocksTreeView.SelectedItem == null)
                {
                    component = (DataContext as ControlBlockModelCollection).PasteControlBlock();
                }
                else
                {
                    if(sfControlBlocksTreeView.SelectedItem is SequentialModel)
                    {
                        var node = JsonNode.Parse(Clipboard.GetText());
                        if (node["OWNER"].GetValue<int>() != sfControlBlocksTreeView.SelectedItem.GetHashCode())
                            if (MessageBox.Show($"The Step to be added appear to come from other Sequential Control Block.\nAre you sure you want to continue ?",
                                "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                return;
                        component = (sfControlBlocksTreeView.SelectedItem as SequentialModel).Add(node);
                    }
                    else if(sfControlBlocksTreeView.SelectedItem is SwitchModel)
                    {
                        var node = JsonNode.Parse(Clipboard.GetText());
                        (sfControlBlocksTreeView.SelectedItem as SwitchModel).AddBranch(node);
                    }
                    else if (sfControlBlocksTreeView.SelectedItem is LoopModel)
                    {
                        var node = JsonNode.Parse(Clipboard.GetText());
                        component = (sfControlBlocksTreeView.SelectedItem as LoopModel).ResetLoopBody(node);
                    }
                    else if (sfControlBlocksTreeView.SelectedItem is CompoundModel)
                    {
                        var node = JsonNode.Parse(Clipboard.GetText());
                        component = (sfControlBlocksTreeView.SelectedItem as CompoundModel).Add(node);
                    }
                }
                if (component != null)
                {
                    sfControlBlocksTreeView.SelectedItem = component;
                    __show_component_view(component);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void PasteItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool ret = false;
            var component = Component.COMPONENT_IN_CLIPBOARD();
            var selected = sfControlBlocksTreeView.SelectedItem as Component;
            if(selected == null)
            {
                if(component?.type.IsSubclassOf(typeof(ControlBlockModel)) == true)
                    ret = true;
            }
            else if(selected.Modified == false)
            {
                if (selected is SequentialModel && component?.type.IsSubclassOf(typeof(StepModel)) == true)
                    ret = true;
                else if (component?.type.IsSubclassOf(typeof(ControlBlockModel)) == true)
                {
                    if (selected is SwitchModel)
                        ret = true;
                    else if (selected is LoopModel && (selected as LoopModel).LoopBody.Count() == 0)
                        ret = true;
                    else if (selected is CompoundModel)
                        ret = true;
                }
            }

            e.CanExecute = ret;
            e.Handled = true;
        }

        private void PasteBeforeItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Component? component = null;
                if((sfControlBlocksTreeView.SelectedItem as Component).Owner == null)
                {
                    component = (DataContext as ControlBlockModelCollection).PasteControlBlockBefore(sfControlBlocksTreeView.SelectedItem as ControlBlockModel);
                }
                else if (sfControlBlocksTreeView.SelectedItem is StepModel)
                {
                    var node = JsonNode.Parse(Clipboard.GetText());
                    if (node["OWNER"].GetValue<int>() != (sfControlBlocksTreeView.SelectedItem as StepModel).Owner.GetHashCode())
                        if (MessageBox.Show($"The Step to be inserted appear to come from other Sequential Control Block.\nAre you sure you want to continue ?",
                            "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            return;
                    var seq = (sfControlBlocksTreeView.SelectedItem as StepModel).Owner as SequentialModel;
                    component = seq.InsertBefore(seq.SubSteps.IndexOf(sfControlBlocksTreeView.SelectedItem as StepModel), node);
                }
                else if ((sfControlBlocksTreeView.SelectedItem as Component).Owner is SwitchModel)
                {
                    var node = JsonNode.Parse(Clipboard.GetText());
                    var sw = (sfControlBlocksTreeView.SelectedItem as ControlBlockModel).Owner as SwitchModel;
                    sw.InsertBranchBefore(sfControlBlocksTreeView.SelectedItem as ControlBlockModel, node);
                    component = sw;
                }

                if (component != null)
                {
                    sfControlBlocksTreeView.SelectedItem = component;
                    __show_component_view(component);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteBeforeItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool ret = false;
            var component = Component.COMPONENT_IN_CLIPBOARD();
            var selected = sfControlBlocksTreeView.SelectedItem as Component;
            if (selected != null && selected.Modified == false)
            {

                if (component?.type == (typeof(SimpleStepModel)) && selected is StepModel)
                    ret = true;
                else if(component?.type.IsSubclassOf(typeof(ControlBlockModel)) == true)
                {
                    if (selected.Owner == null)
                        ret = true;
                    else if (selected.Owner is SwitchModel)
                        ret = true;
                    else if (selected.Owner is CompoundModel)
                        ret = true;
                }
            }

            e.CanExecute = ret;
            e.Handled = true;
        }

        private void sfControlBlocksTreeView_ItemContextMenuOpening(object sender, Syncfusion.UI.Xaml.TreeView.ItemContextMenuOpeningEventArgs e)
        {
            if (e.MenuInfo.Node.Content != sfControlBlocksTreeView.SelectedItem)
                e.Cancel = true;
        }

        private void ControlBlockManager_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var ret = sfControlBlocksTreeView.Focus();
        }
    }
}
