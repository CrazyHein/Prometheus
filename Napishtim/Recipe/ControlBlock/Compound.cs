using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class Compound_S : ControlBlockSource
    {
        private LinkedList<ControlBlockSource> __control_block_sources;

        public Compound_S(string name, IEnumerable<ControlBlockSource> blks) : base(name)
        {
            __control_block_sources = new LinkedList<ControlBlockSource>(blks);
        }


        public override int Height => throw new NotImplementedException();

        public override IEnumerable<uint> ShaderUserVariablesUsage => throw new NotImplementedException();

        public override IEnumerable<uint> GlobalEventReference => throw new NotImplementedException();

        public override int StepFootprint => throw new NotImplementedException();

        public override int UserVariableFootprint => throw new NotImplementedException();

        public override bool ContainsGlobalEventReference(uint index)
        {
            throw new NotImplementedException();
        }

        public override ControlBlockObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping)
        {
            throw new NotImplementedException();
        }

        public override JsonObject SaveAsJson()
        {
            throw new NotImplementedException();
        }
    }
}
