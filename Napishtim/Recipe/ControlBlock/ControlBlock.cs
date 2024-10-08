﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Globals;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public abstract class ControlBlock
    {
        private string __name = "ControlBlock";
        public string Name 
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if (value.Length == 0)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Invaild name for ControlBlock(name string length should be greater than zero).");
                __name = value;
            }
        }
        public abstract int StepFootprint { get; }
        public abstract int UserVariableFootprint { get; }

        //private int __level;
        //public abstract int Level { get; }
        /*
        {
            get { return __level; }
            protected set
            {
                if (value >= ControlBlockSource.MAX_NESTING_DEPTH)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
                __level = value;
            }
        }
        */

        protected ControlBlock(string name) => Name = name;
    }
    public abstract class ControlBlockSource: ControlBlock
    {
        public abstract ControlBlockObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping);

        private ControlBlockSource? __owner = null;
        public ControlBlockSource? Owner 
        {
            get { return __owner; }
            internal set
            {
                if (value != null)
                {
                    if (__owner != null)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"The Control Block already has an owner.");
                }
                __owner = value;
            }
        }

        public string FullName
        {
            get
            {
                string name = Name;
                ControlBlockSource owner = Owner;
                while(owner != null)
                {
                    name = String.Join('/', owner.Name, name);
                    owner = owner.Owner;
                }
                return name;
            }
        }
        public abstract int Height { get; }
        public const int MAX_NESTING_DEPTH = 32;


        public virtual int Nesting
        {
            get
            {
                int n = 1;
                ControlBlockSource? owner = Owner;
                while (owner != null) {
                    owner = owner.Owner;
                    n++;
                }
                return n;
            }
        }

        /*
        private ControlBlockSource? __next;
        public virtual ControlBlockSource? Next 
        {
            get { return __next; }
            set
            {
                var v = value;
                while(v != null)
                {
                    if(v.Next == this)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "'ControlBlock' circular reference.");
                    v = v.Next;
                }
                __next = value;
            }
        }
        
        protected uint NextStepID(Context context)
        {
            if (Next == null)
                return context.StepCapacity - 1;
            else if(Next.ID == null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The next step ID is unresolved.");
            else
                return Next.ID.Value;
        }
        */
        public abstract IEnumerable<uint> ShaderUserVariablesUsage { get; }
        public abstract IEnumerable<uint> GlobalEventReference { get; }
        public abstract IEnumerable<(Sequential_S container, ProcessStepSource step)> ProcessSteps { get; }
        public abstract bool ContainsGlobalEventReference(uint index);

        public abstract JsonObject SaveAsJson();
        private delegate ControlBlockSource MakeControlBlock(JsonObject node, ControlBlockSource? owner);
        private static Dictionary<string, MakeControlBlock> __BUIILD_CONTROL_BLOCK = new Dictionary<string, MakeControlBlock>();
        public static ControlBlockSource MAKE_BLK(JsonObject node, ControlBlockSource? owner)
        {
            string name = node["ASSEMBLY"].GetValue<string>();
            if (__BUIILD_CONTROL_BLOCK.ContainsKey(name) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Unknown ControlBlock: {name}.");
            return __BUIILD_CONTROL_BLOCK[name](node, owner);
        }
        static ControlBlockSource()
        {
            __BUIILD_CONTROL_BLOCK[typeof(Sequential_S).FullName] = Sequential_S.MAKE;
            __BUIILD_CONTROL_BLOCK[typeof(Loop_S).FullName] = Loop_S.MAKE;
            __BUIILD_CONTROL_BLOCK[typeof(Switch_S).FullName] = Switch_S.MAKE;
            __BUIILD_CONTROL_BLOCK[typeof(Compound_S).FullName] = Compound_S.MAKE;
        }

        protected ControlBlockSource(string name) : base(name)
        {
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ControlBlockObject : ControlBlock
    {
        internal ControlBlockObject(string name) : base(name)
        {
        }

        public abstract IEnumerable<Step> Build(Context context, IReadOnlyDictionary<uint, Event> globals);
        public uint? ID { get; protected init; }

        public abstract IEnumerable<ProcessStepObject> ProcessSteps { get; }
    }
}
