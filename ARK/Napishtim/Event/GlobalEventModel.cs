using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using System;
using System.Text.Json.Nodes;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class GlobalEventModel: Component
    {
        public override string Header
        {
            get { return $"GEVENT{Index} -- {Name}"; }
        }

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/tag.png"));

        public EventModel Event { get; private set; }
        //private bool __apply_from_owner_request;
        public GlobalEventModelCollection Manager { get; }

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

        private string __name = "unnamed_global_event";
        public string Name 
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if(__name != value)
                {
                    __name = value;
                    _notify_property_changed();
                    _reload_property("Header");
                    _reload_property("Summary");
                }
            }
        }

        public GlobalEventModel(GlobalEventModelCollection collection, uint idx, string name)
        {
            __index = idx;
            __applied_index = idx;
            __name = name;
            __applied_name = name;

            Event = new EventModel() { Owner = this };
            Manager = collection;
        }

        public GlobalEventModel(GlobalEventModelCollection collection, uint idx, string name, Event evt)
        {
            __index = idx;
            __applied_index = idx;
            __name = name;
            __applied_name = name;

            Event = new EventModel(evt) { Owner = this };
            Manager = collection;
        }

        public GlobalEventModel(GlobalEventModelCollection collection, uint idx, string name, (string pname, Expression pvalue)[] parameters)
        {
            __index = idx;
            __applied_index = idx;
            __name = name;
            __applied_name = name;

            Event = new EventModel(name, parameters) { Owner = this };
            Manager = collection;
        }

        protected bool _unused = true;
        public bool Unused
        {
            get { return _unused; }
            set
            {
                if (value != _unused)
                {
                    _unused = value;
                    _notify_property_changed();
                }
            }
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                try
                {
                    if (Event.Modified)
                    {
                        //__apply_from_owner_request = true;
                        Event.ApplyChanges();
                    }
                    else
                        Manager.RecipeDocument.ReplaceGlobalEvent(__applied_index, Index, Name, Event.Event);

                    __applied_index = Index;
                    __applied_name = Name;

                    Manager.IsDirty = true;

                    base.ApplyChanges();
                }
                finally
                {
                    //__apply_from_owner_request = false;
                }
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

        public GlobalEventModel(GlobalEventModelCollection collection, uint idx, string name, JsonNode evtNode)
        {
            __index = idx;
            __applied_index = idx;
            __name = name;
            __applied_name = name;

            Event evt = AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.Event.MAKE(evtNode);

            Event = new EventModel(evt) { Owner = this };
            Manager = collection;
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

        public override void SubComponentChangesApplied(Component sub)
        {
            //if(__apply_from_owner_request)
            Manager.RecipeDocument.ReplaceGlobalEvent(__applied_index, Index, Name, (sub as EventModel).Event);
            _notify_property_changed("Summary");
            //ApplyChanges();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            _notify_property_changed("Summary");
        }

        protected override void RollbackChanges()
        {
            throw new NotSupportedException();
        }
    }
}
