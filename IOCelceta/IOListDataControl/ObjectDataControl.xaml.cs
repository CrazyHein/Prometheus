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
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// ObjectDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectDataControl : Window
    {
        private bool __new_object_data_item;
        private int __insert_pos;
        private uint __original_object_index;
        public ObjectDataControl(ObjectItemDataModel dataModel, bool newObjectDataItem, int insertPos = -1)
        {
            InitializeComponent();
            DataContext = dataModel;
            __new_object_data_item = newObjectDataItem;
            __insert_pos = insertPos;
            __original_object_index = dataModel.Index;

            //__cmb_basic_data_type_selection.ItemsSource = dataModel.Host.DataHelper.DataTypeCatalogue.DataTypes.Values;
            //__cmb_coverter_data_type_selection.ItemsSource = dataModel.Host.DataHelper.DataTypeCatalogue.DataTypes.Values;

            __cmb_binding_module_selection.ItemsSource = dataModel.Host.DataHelper.ControllerModuleCollection;
        }

        private void __on_cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void __on_ok(object sender, RoutedEventArgs e)
        {
            if (__data_binding_error_counter != 0)
                MessageBox.Show("Invalid User Input ...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {

                try
                {
                    var data = DataContext as ObjectItemDataModel;
                    if (__new_object_data_item)
                        data.Host.AddDataModel(data, __insert_pos);
                    else
                        data.Host.ModifyDataModel(__original_object_index, data);
                    DialogResult = true;
                }
                catch (IOListParseExcepetion exception)
                {
                    string message;
                    if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while updating controller objects . \n{0}", exception.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while updating controller objects . \n{0}", exception.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private int __data_binding_error_counter = 0;

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __data_binding_error_counter++;
            else
                __data_binding_error_counter--;
        }
    }
}
