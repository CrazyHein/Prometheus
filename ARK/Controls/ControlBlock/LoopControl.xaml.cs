using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// LoopControl.xaml 的交互逻辑
    /// </summary>
    public partial class LoopControl : UserControl, IControlBlockControl
    {
        public LoopControl(LoopModel loop)
        {
            InitializeComponent();
            DataContext = loop;
        }

        public void ResetDataModel(ControlBlockModel blk)
        {
            DataContext = blk;
        }

        public void UpdateBindingSource()
        {
            var binding = txtBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = txtLoopCount.GetBindingExpression(IntegerTextBox.ValueProperty);
            binding.UpdateSource();
        }

        private void SwitchControlBlockTypeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((sfGridLoopBody.SelectedItem as ControlBlockModel).ControlBlock.GetType() != (e.Parameter as Type))
            {
                if (MessageBox.Show($"Change the control block type of loop body from current value {(sfGridLoopBody.SelectedItem as ControlBlockModel).ControlBlock.GetType().Name} to {(e.Parameter as Type).Name} and remove all child items? ",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    (DataContext as LoopModel).ResetLoopBody((e.Parameter as Type));
            }
            e.Handled = true;
        }

        private void SwitchControlBlockTypeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridLoopBody.SelectedItem != null && (DataContext as LoopModel)?.Modified == false;
            e.Handled = true;
        }

        private void AddControlBlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControlBlockTypeSelector selector = new ControlBlockTypeSelector();
            if (selector.ShowDialog() == true)
                (DataContext as LoopModel).ResetLoopBody(selector.ControlBlockType);
        }

        private void AddControlBlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as LoopModel)?.LoopBody.Count() == 0;
        }

        private void RemoveControlBlockCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as LoopModel).ClearLoopBody();
        }

        private void RemoveControlBlockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridLoopBody.SelectedItem != null && (DataContext as LoopModel)?.Modified == false;
            e.Handled = true;
        }
    }
}
