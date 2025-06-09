using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Initialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim.Initialization
{
    public class UserVariableInitialValueModel : Component
    {
        public override void SubComponentChangesApplied(Component sub) => throw new NotSupportedException();
        public override void SubComponentChangesHappened(Component sub) => throw new NotSupportedException();
        protected override void RollbackChanges() => throw new NotSupportedException();
        public override BitmapImage ImageIcon => throw new NotSupportedException();
        public override string Summary => throw new NotSupportedException();
        public override JsonNode ToJson() => throw new NotSupportedException();
        public override string Header { get { return Comment; } }

        private string __comment = "unnamed_initial_value";
        public string Comment
        {
            get { return __comment; }
            set
            {
                value = value.Trim();
                if (value != __comment)
                {
                    __comment = value;
                    _notify_property_changed();
                    _reload_property("Header");
                }
            }
        }

        private string __name = "&USER0";
        public string Name
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if (value != __name)
                {
                    __name = value;
                    _notify_property_changed();
                    _notify_property_changed("IsValid");
                }
            }
        }

        private double __value = 0.0;

        public string ValueString
        {
            get { return __value.ToString(); }
            set
            {
                if (double.TryParse(value, out double v))
                {
                    if (v != __value)
                    {
                        __value = v;
                        _notify_property_changed();
                        _notify_property_changed("Value");
                    }
                }
                else
                {
                    _notify_property_changed();
                    _notify_property_changed("Value");
                }
            }
        }

        public double Value => __value;
        public bool IsValid
        {
            get
            {
                if (Modified)
                {
                    try
                    {
                        __initial_value = new UserVariableInitialValue(__comment, __name, __value);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
        }

        private UserVariableInitialValue __initial_value;

        public UserVariableInitialValueModel()
        {
            __initial_value = new UserVariableInitialValue();
            __comment = __initial_value.Comment;
            __name = __initial_value.Name;
            __value = __initial_value.Value;
        }
        public UserVariableInitialValueModel(UserVariableInitialValue value)
        {
            __initial_value = value;
            __comment = value.Comment;
            __name = value.Name;
            __value = value.Value;
        }

        public UserVariableInitialValueModel(string comment, string name, double value)
        {
            __initial_value = new UserVariableInitialValue(comment, name, value);
            __comment = __initial_value.Comment;
            __name = __initial_value.Name;
            __value = __initial_value.Value;
        }

        public override string ToString()
        {
            return $"{Comment}: {Name} = {ValueString}";
        }
    }
}
