using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using System;
using System.Collections.Generic;
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
    /// ContextManager.xaml 的交互逻辑
    /// </summary>
    public partial class ContextManager : UserControl
    {
        ContentControl __context_context_control;
        LabelsControl __labels_control;
        ExceptionResponseControl __exception_response_control;

        public ContextManager(ContextModel context, ContentControl content)
        {
            InitializeComponent();
            DataContext = context;
            __context_context_control = content;

            __labels_control = new LabelsControl();
            __exception_response_control = new ExceptionResponseControl((DataContext as ContextModel).ExceptionResponse);
        }

        private void ContextListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.RemovedItems.Count > 0)
            {
                if ((e.RemovedItems[0] as ListBoxItem).Tag.ToString() == "Exception")
                {
                    if ((__exception_response_control.DataContext as ExceptionResponseModel).Modified)
                    {
                        var ret = MessageBox.Show("Changes have been detected.\nClick 'Yes' to apply the changes or 'No' to discard the changes.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (ret == MessageBoxResult.No)
                            (__exception_response_control.DataContext as ExceptionResponseModel).DiscardChanges();
                        else if (ret == MessageBoxResult.Yes)
                        {
                            try
                            {
                                (__exception_response_control.DataContext as ExceptionResponseModel).ApplyChanges();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An exception has occurred while applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                (sender as ListBox).SelectedItem = e.RemovedItems[0] as ListBoxItem;
                                e.Handled = true;
                                return;
                            }
                        }
                        else
                        {
                            (sender as ListBox).SelectedItem = e.RemovedItems[0] as ListBoxItem;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
            if(e.AddedItems.Count > 0)
            {
                if ((e.AddedItems[0] as ListBoxItem).Tag.ToString() == "Labels")
                    __context_context_control.Content = __labels_control;
                else if ((e.AddedItems[0] as ListBoxItem).Tag.ToString() == "Exception")
                    __context_context_control.Content = __exception_response_control;
            }
            e.Handled = true;
        }
    }
}
