using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using Syncfusion.Data.Extensions;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls
{
    /// <summary>
    /// CompoundControl.xaml 的交互逻辑
    /// </summary>
    public partial class CompoundControl : UserControl, IControlBlockControl
    {
        public CompoundControl(CompoundModel compound)
        {
            InitializeComponent();
            DataContext = compound;

            sfGridSubControlBlocks.RowDragDropController.Dropped += OnsfGridSubControlBlocks_Dropped;
            sfGridSubControlBlocks.RowDragDropController.Drop += OnsfGridSubControlBlocks_Drop;
        }

        public void ResetDataModel(ControlBlockModel blk)
        {
            DataContext = blk;
        }

        public void UpdateBindingSource()
        {
            var binding = txtBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }

        private void OnsfGridSubControlBlocks_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnsfGridSubControlBlocks_Dropped(object? sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                int dragIndex = (DataContext as CompoundModel).SubIndex(draggingRecords[0] as ControlBlockModel);
                int targetIndex = (int)e.TargetRecord;

                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;

                sfGridSubControlBlocks.View.BeginInit();
                try
                {
                    if (dragIndex != insertionIndex)
                        (DataContext as CompoundModel).Move(dragIndex, insertionIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while adjusting the order of sub-steps:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sfGridSubControlBlocks.View.EndInit();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void AddSubControlBlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as CompoundModel)?.Modified == false;
            e.Handled = true;
        }

        private void AddSubControlBlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as CompoundModel).Add("sequential", typeof(Sequential_S));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while adding sub-control-block:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertSubControlBlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridSubControlBlocks.SelectedItem != null && (DataContext as CompoundModel)?.Modified == false;
            e.Handled = true;
        }

        private void InsertSubControlBlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as CompoundModel).InsertBefore(sfGridSubControlBlocks.SelectedIndex, "sequential", typeof(Sequential_S));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while inserting sub-control-block:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveSubControlBlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridSubControlBlocks.SelectedItem != null && (DataContext as CompoundModel)?.Modified == false;
            e.Handled = true;
        }

        private void RemoveSubControlBlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to remove the following step:\n{(sfGridSubControlBlocks.SelectedItem as ControlBlockModel).Name}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                (DataContext as CompoundModel).RemoveAt(sfGridSubControlBlocks.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while removing sub-control-block:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void SwitchSubControlBlockTypeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridSubControlBlocks.SelectedItem != null && (DataContext as CompoundModel)?.Modified == false;
            e.Handled = true;
        }

        private void SwitchSubControlBlockTypeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((sfGridSubControlBlocks.SelectedItem as ControlBlockModel).ControlBlock.GetType() != (e.Parameter as Type))
            {
                if (MessageBox.Show($"Change the control block type of loop body from current value {(sfGridSubControlBlocks.SelectedItem as ControlBlockModel).ControlBlock.GetType().Name} to {(e.Parameter as Type).Name} and remove all child items? ",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    (DataContext as CompoundModel).ResetControlBlock(sfGridSubControlBlocks.SelectedIndex, (e.Parameter as Type));
            }
            e.Handled = true;
        }
    }
}
