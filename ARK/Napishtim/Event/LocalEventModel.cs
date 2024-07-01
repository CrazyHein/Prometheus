using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class LocalEventModel : Component
    {
        private uint __applied_index;
        private uint __index = 0;
        public uint Index
        {
            get { return __index; }
            set
            {
                if (__index != value)
                {
                    __index = value;
                    _notify_property_changed();
                    _reload_property("Header");
                    _reload_property("Summary");
                }
            }
        }

        private string __applied_name;
        private string __name = "unnamed";
        public string Name
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if (__name != value)
                {
                    __name = value;
                    _notify_property_changed();
                    _reload_property("Header");
                    _reload_property("Summary");
                }
            }
        }
        public EventModel Event { get; private set; }


        public override string Header
        {
            get { return $"EVENT{Index} -- {Name}"; }
        }

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/tag.png"));

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
            //if (Modified)
                //throw new InvalidOperationException("Unapplied changes detected.");
            var json = new JsonObject();
            json["VERSION"] = Settings.ArkVersion;
            json["ASSEMBLY"] = this.GetType().FullName;
            json["ID"] = Index;
            json["NAME"] = Name;
            json["EVENT"] = Event.ToEvent().ToJson();
            return json;
        }

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    summary = ToJson().ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        protected override void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                Event.ApplyChanges();
                __applied_index = Index;
                __applied_name = Name;
                base.ApplyChanges();
            }
        }

        public override void DiscardChanges()
        {
            if (Modified)
            {
                Event.DiscardChanges();

                Index = __applied_index;
                Name = __applied_name;
                _notify_property_changed("Summary");
                base.DiscardChanges();
            }
        }


        public LocalEventModel(uint index, string name, Event evt)
        {
            __index = index;
            __applied_index = index;
            __name = name;
            __applied_name = name;
            Event = new EventModel(evt) { Owner = this};
        }

        public LocalEventModel(uint index, string name)
        {
            __index = index;
            __applied_index = index;
            __name = name;
            __applied_name = name;
            Event = new EventModel() { Owner = this};
        }

        public LocalEventModel(JsonObject node)
        {
            __index = node["ID"].GetValue<uint>();
            __applied_index = __index;
            __name = node["NAME"].GetValue<string>(); 
            __applied_name = __name;
            Event = new EventModel(Prometheus.Napishtim.Engine.EventMechansim.Event.MAKE(node["EVENT"])) { Owner = this };
        }
    }
}
