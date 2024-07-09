using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public abstract class StepModel : Component
    {
        public ProcessStepSource Step { get; protected set; }
        protected ProcessStepSource? _step_for_rollback;
        public Sequential_S Sequential { get; protected set; }

        public StepModel(ProcessStepSource step, Sequential_S seq, SequentialModel seqm)
        {
            Step = step;
            Sequential = seq;
            //Owner = seqm;

            __name = step.Name;
        }


        public override string Header => $"{SerialNumber}: {Name}";

        public abstract int SerialNumber { get; }

        private string __name;
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

        public abstract ProcessStepSource ExportToProcessStepSource(Sequential_S? seq = null);

        public abstract void SequenceChanged();
        public abstract void EvaluateShaderValidity();
        //public abstract void StepChanged();

        //public string? ExceptionMessage { get; protected set; }

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/step.png"));

        public override JsonNode ToJson()
        {
            throw new NotImplementedException();
        }

        public static StepModel MAKE_STEP(ProcessStepSource step, Sequential_S seq, SequentialModel seqm)
        {
            if (step is SimpleStep_S)
                return new SimpleStepModel(step as SimpleStep_S, seq, seqm) { Owner = seqm };
            else if (step is SimpleStepWithTimeout_S)
                return new SimpleStepModel(step as SimpleStepWithTimeout_S, seq, seqm) { Owner = seqm };
            else
                throw new ArgumentException($"Unsupported step object was given:\n{step.ToString()}");
        }
    }
}
