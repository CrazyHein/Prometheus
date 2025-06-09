using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim.Initialization;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Globals;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Initialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class InitializationModel : Component
    {
        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if (Modified)
                        summary = Export().ToString();
                    else
                        summary = Configuration.ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        public override string Header => "Initialization";

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/initial_values.png"));

        public override void SubComponentChangesApplied(Component sub)
        {
            _notify_property_changed("Summary");
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            _notify_property_changed("Summary");
        }

        public override JsonNode ToJson()
        {
            throw new NotImplementedException();
        }

        public ContextModel Context { get; }

        protected InitializationConfiguration __configuration;
        public InitializationConfiguration Configuration
        {
            get { return __configuration; }
            protected set
            {
                if (__configuration != value)
                {
                    Context.RecipeDocument.InitializationConfiguration = value;
                    __configuration = value;
                }
            }
        }

        private ObservableCollection<UserVariableInitialValueModel> __user_variable_initial_values;
        public IEnumerable<UserVariableInitialValueModel> UserVariableInitialValues { get { return __user_variable_initial_values; } }
        protected InitializationConfiguration? _initialization_configuration_for_rollback;

        public InitializationModel(InitializationConfiguration configuration, ContextModel context)
        {
            __configuration = configuration;
            Context = context;

            __user_variable_initial_values = 
                new ObservableCollection<UserVariableInitialValueModel>(configuration.UserVariableInitialValues.Select(x => new UserVariableInitialValueModel(x) { Owner = this}));

        }

        public InitializationConfiguration Export()
        {
            return new InitializationConfiguration(__user_variable_initial_values.Select(v => (v.Comment, v.Name, v.Value)));
        }

        public void MoveUserVariableInitialValue(int oldIndex, int newIndex)
        {
            var temp = __user_variable_initial_values[oldIndex];
            __user_variable_initial_values.RemoveAt(oldIndex);
            __user_variable_initial_values.Insert(newIndex, temp);

            _notify_property_changed("Summary");
        }

        public void AddUserVariableInitialValue()
        {
            __user_variable_initial_values.Add(new UserVariableInitialValueModel() { Owner = this });
            _notify_property_changed("Summary");
        }

        public void InsertUserVariableInitialValue(int pos)
        {
            __user_variable_initial_values.Insert(pos, new UserVariableInitialValueModel() { Owner = this });
            _notify_property_changed("Summary");
        }

        public void RemoveUserVariableInitialValueAt(int pos)
        {
            __user_variable_initial_values.RemoveAt(pos);
            _notify_property_changed("Summary");
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                _initialization_configuration_for_rollback = Configuration;
                Configuration = Export();
                Context.IsDirty = true;
                base.ApplyChanges();
            }
        }

        protected override void RollbackChanges()
        {
            if (Validated)
            {
                Configuration = _initialization_configuration_for_rollback;
            }
        }

        public override void DiscardChanges()
        {
            if (Modified)
            {
                __user_variable_initial_values.Clear();
                foreach (var v in Configuration.UserVariableInitialValues.Select(x => new UserVariableInitialValueModel(x) { Owner = this }))
                    __user_variable_initial_values.Add(v);

                _notify_property_changed("Summary");
                base.DiscardChanges();
            }
        }
    }
}
