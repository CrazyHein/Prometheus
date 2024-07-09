using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public abstract class ExceptionResponseModel : Component
    {
        protected ExceptionResponseSource? __exception_response_source;
        public ExceptionResponseSource? ExceptionResponse
        {
            get { return __exception_response_source; }
            protected set
            {
                if (__exception_response_source != value)
                {
                    Context.RecipeDocument.ExceptionResponseSource = value;
                    __exception_response_source = value;
                }
            }
        }
        protected ExceptionResponseSource? _exception_response_for_rollback;
        public ContextModel Context { get; }

        public string Name { get; }

        public override string Header => $"{Name}";
        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/alarm.png"));

        private bool __enabled = false;
        public bool Enabled
        {
            get { return __enabled; }
            set 
            { 
                __enabled = value;
                _notify_property_changed();
                _reload_property("Summary");
            }
        }

        public ExceptionResponseModel(ExceptionResponseSource? exception, ContextModel context)
        {
            if (exception != null)
            {
                __enabled = true;
                __exception_response_source = exception;
                Name = exception.Name;
            }
            else
            {
                __enabled = false;
                __exception_response_source = null;
                Name = "#DEFAULT_EXCEPTION_RESPONSE#";
            }
            Context = context;
        }

        public abstract ExceptionResponseSource? ExportToExceptionResponseSource();

        public override void ApplyChanges()
        {
            if(Modified)
                Context.IsDirty = true;
            base.ApplyChanges();
        }
    }
}
