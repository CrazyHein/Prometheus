using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// SwitchControl.xaml 的交互逻辑
    /// </summary>
    public partial class SwitchControl : UserControl, IControlBlockControl
    {
        public SwitchControl(SwitchModel sw)
        {
            InitializeComponent();
            DataContext = sw;

            sfGridBranches.RowDragDropController.Dropped += OnSfGridBranches_Dropped;
            sfGridBranches.RowDragDropController.Drop += OnSfGridBranches_Drop;

            LocalEventCollection.DataSource = sw.LocalEvents;
        }

        public void ResetDataModel(ControlBlockModel blk)
        {
            DataContext = blk;
            LocalEventCollection.DataSource = (blk as SwitchModel).LocalEvents;
        }

        public void UpdateBindingSource()
        {
            var binding = txtBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }

        private void OnSfGridBranches_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnSfGridBranches_Dropped(object? sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                int dragIndex = (DataContext as SwitchModel).IndexOf(draggingRecords[0] as SwitchBranchModel);
                int targetIndex = (int)e.TargetRecord;

                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;

                sfGridBranches.View.BeginInit();
                try
                {
                    if (dragIndex != insertionIndex)
                        (DataContext as SwitchModel).MoveBranch(dragIndex, insertionIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while adjusting the priorities of branches:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sfGridBranches.View.EndInit();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void AddBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as SwitchModel).AddBranch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while adding branch:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void AddBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void RemoveBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to remove the following branch:\n{(sfGridBranches.SelectedItem as SwitchBranchModel).Summary}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                int pos = sfGridBranches.SelectedIndex;
                (DataContext as SwitchModel).RemoveBranchAt(pos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while removing branch:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void RemoveBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItem != null;
            e.Handled = true;
        }

        private void EditBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void EditBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItem != null;
            e.Handled = true;
        }

        private void InsertBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                int pos = sfGridBranches.SelectedIndex;
                (DataContext as SwitchModel).InsertBranch(pos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while inserting branch:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        private void InsertBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItem != null;
            e.Handled = true;
        }

        private void SwitchControlBlockTypeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string currentType = (sfGridBranches.SelectedItem as SwitchBranchModel).Type;
            if (currentType != (e.Parameter as Type).Name)
            {
                if (MessageBox.Show($"Change the control block type of loop body from current value {currentType} to {(e.Parameter as Type).Name} and remove all child items? ",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    int pos = sfGridBranches.SelectedIndex;
                    (DataContext as SwitchModel).ResetBranch(pos, e.Parameter as Type);
                }
            }
            e.Handled = true;
        }

        private void SwitchControlBlockTypeCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItem != null && (DataContext as SwitchModel)?.Modified == false;
            e.Handled = true;
        }
    }
}
