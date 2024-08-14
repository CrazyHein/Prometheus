using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class Loop_S : ControlBlockSource
    {
        private ControlBlockSource? __loop_body;
        public ControlBlockSource? LoopBody 
        {
            get {  return __loop_body; }
            set
            {
                int delta = (value != null ? value.Height : 0) - (__loop_body != null ? __loop_body.Height : 0);
                if (this.Nesting + delta > MAX_NESTING_DEPTH)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
                if (__loop_body != null)
                {
                    __loop_body.Owner = null;
                    //StepFootprint -= __loop_body.StepFootprint;
                    //UserVariableFootprint -= __loop_body.UserVariableFootprint;
                }

                if (value != null)
                {
                    value.Owner = this;
                    //StepFootprint += value.StepFootprint;
                    //UserVariableFootprint += value.UserVariableFootprint;
                }
                    
                __loop_body = value;
            } 
        }

        public override int StepFootprint => 2 + (__loop_body == null ? 0 : __loop_body.StepFootprint);

        public override int UserVariableFootprint => 1 + (__loop_body == null ? 0 : __loop_body.UserVariableFootprint);

        private int __loop_count = 1;
        public int LoopCount 
        {
            get { return __loop_count; }
            set
            {
                if(value <= 0)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, "The number of cycles must be an integer greater than zero.");
                __loop_count = value;
            }
        }

        public Loop_S(string name, ControlBlockSource body, int count): base(name)
        {
            if(this.Nesting + body.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            body.Owner = this;
            __loop_body = body;
            LoopCount = count;

            //StepFootprint = 2 + body.StepFootprint;
            //UserVariableFootprint = 1 + body.UserVariableFootprint;
        }

        public Loop_S(string name, int count) : base(name)
        {
            __loop_body = null;
            LoopCount = count;

            //StepFootprint = 2;
            //UserVariableFootprint = 1;
        }

        private Loop_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            if (node.TryGetPropertyValue("BODY", out _))
            {
                __loop_body = ControlBlockSource.MAKE_BLK(node["BODY"].AsObject(), this);
                if (this.Nesting + __loop_body.Height > MAX_NESTING_DEPTH)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
                LoopCount = node["COUNT"].GetValue<int>();

                //StepFootprint = 2 + __loop_body.StepFootprint;
                //UserVariableFootprint = 1 + __loop_body.UserVariableFootprint;
            }
            else
            {
                __loop_body = null;
                LoopCount = node["COUNT"].GetValue<int>();

                //StepFootprint = 2;
                //UserVariableFootprint = 1;
            }
        }

        public override IEnumerable<uint> ShaderUserVariablesUsage => __loop_body != null ? __loop_body.ShaderUserVariablesUsage : Enumerable.Empty<uint>();

        static public ControlBlockSource MAKE(JsonObject node, ControlBlockSource? owner)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(Loop_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(Loop_S).FullName}.");

                return new Loop_S(node) { Owner = owner};
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore Loop_S object from node:\n{node.ToString()}", ex);
            }
        }



        public override ControlBlockObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping)
        {
            if (LoopBody == null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, "The loop body of 'Loop' Control Block is empty.");
            ProcessShaders postShaders = new ReservedProcessShaders([("#Loop Control Variable Initialization#", $"&USER{userVariableMapping.Span[0]}", "0")]);
            ProcessStepSource step = new SimpleStep_S("#InitialStep#", null, null, null, postShaders);
            var initializationStatement = new Sequential_S("#InitialControlBlock#", [step]) { Owner = this };

            postShaders = new ReservedProcessShaders([("#Increase Loop Control Variable#", $"&USER{userVariableMapping.Span[0]}", $"&USER{userVariableMapping.Span[0]}+1")]);
            step = new BranchStep_S("#JudgementStep#", [("#Number of loops reached?#", "LES", 
                                    Expression.USER_VARIABLE_EXPR(userVariableMapping.Span[0]),
                                    Expression.NUMBER_EXPR(LoopCount), postShaders, stepLinkMapping.Span[2])]);
            var conditionalStatement = new Sequential_S("#JudgementControlBlock#", [step]) { Owner = this };

            //initializationStatement.Next = conditionalStatement;
            //conditionalStatement.Next = Next;
            //LoopBody.Next = conditionalStatement;

            var c = conditionalStatement.ResolveTarget(next, abort, context, globals, stepLinkMapping.Slice(1), userVariableMapping, stepNameMapping);
            var i = initializationStatement.ResolveTarget(stepLinkMapping.Span[1], abort, context, globals, stepLinkMapping, userVariableMapping, stepNameMapping);
            var b = LoopBody.ResolveTarget(stepLinkMapping.Span[1], abort, context, globals, stepLinkMapping.Slice(2), userVariableMapping.Slice(1), stepNameMapping);

            return new Loop_O(Name, i, c, b, StepFootprint, UserVariableFootprint);
        }



        public override JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            if(LoopBody != null)
                node["BODY"] = LoopBody.SaveAsJson();
            node["COUNT"] = LoopCount;

            return node;
        }

        public override bool ContainsGlobalEventReference(uint index)
        {
            return LoopBody != null ? LoopBody.ContainsGlobalEventReference(index) : false;
        }

        public override IEnumerable<uint> GlobalEventReference => __loop_body != null ? __loop_body.GlobalEventReference : Enumerable.Empty<uint>();

        public override int Height
        {
            get
            {
                if (__loop_body == null)
                    return 1;
                else
                    return __loop_body.Height + 1;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}({typeof(Loop_S).FullName})");
            sb.Append($"\n<Loop NumberOfCycles = \"{LoopCount}\">");
            if (LoopBody != null)
            {
                var lines = LoopBody.ToString().Split('\n').Skip(1);
                if (lines.Count() > 0)
                {
                    sb.Append($"\n\x20\u2191\t{lines.First()}");
                    foreach (var line in lines.Skip(1))
                        sb.Append($"\n\x20\u2503\t{line}");
                }
            }
            sb.Append($"\n</Loop>\x20\u2199");
            sb.Append($"\n\x20\u2193");
            return sb.ToString();
        }
    }

    public class Loop_O: ControlBlockObject
    {
        private ControlBlockObject __initialization_statement;
        private ControlBlockObject __conditional_statement;
        private ControlBlockObject __control_block_body;

        internal Loop_O(string name, ControlBlockObject init, ControlBlockObject condition, ControlBlockObject body, int stepFootprint, int userVariableFootprint): base(name)
        {
            __initialization_statement = init;
            __conditional_statement = condition;
            __control_block_body = body;
            //StepFootprint = stepFootprint;
            //UserVariableFootprint = userVariableFootprint;
            //Level = level;
            ID = __initialization_statement.ID;
        }

        public override int StepFootprint => 2 + __control_block_body.StepFootprint;

        public override int UserVariableFootprint => 1 + __control_block_body.UserVariableFootprint;

        public override IEnumerable<Step> Build(Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            if (__initialization_statement == null || __conditional_statement == null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Please invoke 'Loop.ResolveTarget' first.");

            var x = __initialization_statement.Build(context, globals);
            var y = __conditional_statement.Build(context, globals);
            var z = __control_block_body.Build(context, globals);

            return x.Concat(y).Concat(z);
        }

        public override IEnumerable<ProcessStepObject> ProcessSteps(IEnumerable<Type> owners)
        {
            if (owners.Contains(typeof(Loop_O)))
                return __control_block_body.ProcessSteps(owners);
            else
                return Enumerable.Empty<ProcessStepObject>();
        }
    }
}
