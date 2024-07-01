using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
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
    /// SequentialControl.xaml 的交互逻辑
    /// </summary>
    public partial class SequentialControl : UserControl, IControlBlockControl
    {
        public SequentialControl(SequentialModel seq)
        {
            InitializeComponent();
            DataContext = seq;

            sfGridSubSteps.RowDragDropController.Dropped += OnSfGridSubSteps_Dropped;
            sfGridSubSteps.RowDragDropController.Drop += OnSfGridSubSteps_Drop;
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

        private void OnSfGridSubSteps_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnSfGridSubSteps_Dropped(object? sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                int dragIndex = (draggingRecords[0] as StepModel).SerialNumber;
                int targetIndex = (DataContext as SequentialModel)[(int)e.TargetRecord].SerialNumber;

                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;

                sfGridSubSteps.View.BeginInit();
                try
                {
                    if(dragIndex != insertionIndex)
                        (DataContext as SequentialModel).Move(dragIndex, insertionIndex);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while adjusting the order of sub-steps:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sfGridSubSteps.View.EndInit();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void AddSubStepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as SequentialModel).Add("unnamed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while adding sub-step:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void AddSubStepCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as SequentialModel)?.Modified == false;
            e.Handled = true;
        }

        private void InsertSubStepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as SequentialModel).InsertBefore(sfGridSubSteps.SelectedIndex, "unnamed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while inserting sub-step:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertSubStepCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridSubSteps.SelectedItem != null && (DataContext as SequentialModel)?.Modified == false;
            e.Handled = true;
        }

        private void RemoveSubStepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to remove the following step:\n{(sfGridSubSteps.SelectedItem as StepModel).Name}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                (DataContext as SequentialModel).RemoveAt(sfGridSubSteps.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while removing sub-step:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveSubStepCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridSubSteps.SelectedItem != null && (DataContext as SequentialModel)?.Modified == false;
            e.Handled = true;
        }
    }
}
