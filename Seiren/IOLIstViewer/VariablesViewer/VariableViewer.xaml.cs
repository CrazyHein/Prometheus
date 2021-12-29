using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System.Windows;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// VariableViewer.xaml 的交互逻辑
    /// </summary>
    public partial class VariableViewer : Window
    {
        private VariablesModel __variable_collection;
        private InputDialogDisplayMode __input_mode;
        private int __insert_index;
        private VariableModel __result_variable_model;
        private VariableModel __original_variable_model;
        public VariableModel Result { get; private set; } = null;
        public VariableViewer(VariablesModel models, VariableModel model, InputDialogDisplayMode mode, int insertIndex = 0)
        {
            InitializeComponent();
            __variable_collection = models;
            __result_variable_model = new VariableModel() { Name = model.Name, DataType = model.DataType, Unit = model.Unit, Comment = model.Comment };
            __original_variable_model = model;
            __input_mode = mode;
            __insert_index = insertIndex;
            AvailableDataTypes.ItemsSource = models.DataTypeCatalogue.DataTypes.Values;
            AvailableDataTypes.SelectedItem = model.DataType;
            DataContext = __result_variable_model;
            switch (mode)
            {
                case InputDialogDisplayMode.Add:
                    this.Title = "Add a new variable";
                    break;
                case InputDialogDisplayMode.Insert:
                    this.Title = "Insert a new variable";
                    break;
                case InputDialogDisplayMode.Edit:
                    this.Title = "Edit the variable";
                    break;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            __result_variable_model.Name = __result_variable_model.Name.Trim();
            __result_variable_model.Unit = __result_variable_model.Unit.Trim();
            if (__result_variable_model.Name.Length == 0 || __result_variable_model.Unit.Length == 0)
                MessageBox.Show("The variable name or unit name is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                try
                {
                    switch (__input_mode)
                    {
                        case InputDialogDisplayMode.Add:
                            __variable_collection.Add(__result_variable_model);
                            break;
                        case InputDialogDisplayMode.Insert:
                            __variable_collection.Insert(__insert_index, __result_variable_model);
                            break;
                        case InputDialogDisplayMode.Edit:
                            if(__original_variable_model.Equals(__result_variable_model) == false)
                                __variable_collection.Replace(__original_variable_model, __result_variable_model);
                            break;
                    }
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Result = __result_variable_model;
                DialogResult = true;
            }
        }
    }
}
