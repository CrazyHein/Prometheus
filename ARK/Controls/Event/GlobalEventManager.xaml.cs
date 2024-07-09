using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    /// GlobalEventManager.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalEventManager : UserControl
    {
        private GlobalEventModelCollection __globals;
        private ContentControl __global_event_control;
        private GlobalEventControl? __global_event_control_content;
        public GlobalEventManager(GlobalEventModelCollection globals, ContentControl content)
        {
            __globals = globals;
            InitializeComponent();
            sfGlobalEventsTreeView.ItemsSource = globals.Events;
            __global_event_control = content;
            DataContext = globals;
        }

        private void sfGlobalEventsTreeView_SelectionChanged(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
                __show_global_event_view(e.AddedItems[0] as GlobalEventModel);
            else
                __close_global_event_view();
        }

        private void sfGlobalEventsTreeView_ItemDropped(object sender, Syncfusion.UI.Xaml.TreeView.TreeViewItemDroppedEventArgs e)
        {
            var source = (e.DraggingNodes[0].Content as GlobalEventModel);
            var target = (e.TargetNode.Content as GlobalEventModel);
            if (source != target)
            {
                if (e.DropPosition == Syncfusion.UI.Xaml.TreeView.DropPosition.DropBelow)
                    __globals.MoveAfter(source, target);
                else
                    __globals.MoveBefore(source, target);
            }
        }

        private void sfGlobalEventsTreeView_ItemDropping(object sender, Syncfusion.UI.Xaml.TreeView.TreeViewItemDroppingEventArgs e)
        {
            if (e.DragSource != sfGlobalEventsTreeView)
                e.Handled = true;
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var ret = __globals.Add();
                sfGlobalEventsTreeView.SelectedItem = ret;

                __show_global_event_view(ret);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }

        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __globals.Events.All(x => x.Modified == false);
            e.Handled = true;
        }

        private void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var ret = __globals.InsertBefore(__globals.Events.IndexOf(sfGlobalEventsTreeView.SelectedItem));
                sfGlobalEventsTreeView.SelectedItem = ret;

                __show_global_event_view(ret);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }

        private void InsertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGlobalEventsTreeView?.SelectedItem != null && __globals.Events.All(x => x.Modified == false);
            e.Handled = true;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to remove the Global Event:\n{(sfGlobalEventsTreeView.SelectedItem as GlobalEventModel).Summary}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                __globals.Remove(sfGlobalEventsTreeView.SelectedItem as GlobalEventModel);
                __close_global_event_view();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGlobalEventsTreeView?.SelectedItem != null && __globals.Events.All(x => x.Modified == false);
            e.Handled = true;
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject((sfGlobalEventsTreeView.SelectedItem as GlobalEventModel).ToJson().ToJsonString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGlobalEventsTreeView?.SelectedItem != null && __globals.Events.All(x => x.Modified == false);
            e.Handled = true;
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                GlobalEventModel ret = null;
                //if (sfGlobalEventsTreeView.SelectedItem == null)
                {
                    ret = __globals.Add(JsonNode.Parse(Clipboard.GetText()));
                    sfGlobalEventsTreeView.SelectedItem = ret;
                    __show_global_event_view(ret);
                }
                //else
                //{
                    //ret = __globals.InsertBefore(__globals.Events.IndexOf(sfGlobalEventsTreeView.SelectedItem), JsonNode.Parse(Clipboard.GetText()));
                    //sfGlobalEventsTreeView.SelectedItem = ret;
                    //__show_global_event_view(ret);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __globals.Events.All(x => x.Modified == false) && Component.COMPONENT_IN_CLIPBOARD()?.type == typeof(GlobalEventModel);
            e.Handled = true;
        }

        private void PasteBeforeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                GlobalEventModel ret = null;
                ret = __globals.InsertBefore(__globals.Events.IndexOf(sfGlobalEventsTreeView.SelectedItem), JsonNode.Parse(Clipboard.GetText()));
                sfGlobalEventsTreeView.SelectedItem = ret;
                __show_global_event_view(ret);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during the operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteBeforeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __globals.Events.All(x => x.Modified == false) && sfGlobalEventsTreeView.SelectedItem != null && Component.COMPONENT_IN_CLIPBOARD()?.type == typeof(GlobalEventModel);
            e.Handled = true;
        }


        private void __show_global_event_view(GlobalEventModel evt)
        {
            if (__global_event_control_content == null)
                __global_event_control_content = new GlobalEventControl(evt);
            else
                __global_event_control_content.ResetDataModel(evt);

            __global_event_control.Content = __global_event_control_content;
        }

        private void __close_global_event_view()
        {
            __global_event_control.Content = null;
        }

        private void sfGlobalEventsTreeView_SelectionChanging(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangingEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                var evt = e.RemovedItems[0] as GlobalEventModel;
                if (evt.Modified == true)
                {
                    var ret = MessageBox.Show("Changes have been detected.\nClick 'Yes' to apply the changes or 'No' to discard the changes.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (ret == MessageBoxResult.No)
                        evt.DiscardChanges();
                    else if (ret == MessageBoxResult.Yes)
                    {
                        try
                        {
                            evt.ApplyChanges();
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

        private void sfGlobalEventsTreeView_ItemContextMenuOpening(object sender, Syncfusion.UI.Xaml.TreeView.ItemContextMenuOpeningEventArgs e)
        {
            if (e.MenuInfo.Node.Content != sfGlobalEventsTreeView.SelectedItem)
                e.Cancel = true;
        }

        private void GlobalEventManager_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            sfGlobalEventsTreeView.Focus();
        }
    }
}
