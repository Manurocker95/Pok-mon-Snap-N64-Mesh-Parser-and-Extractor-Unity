using System.Collections.Generic;
using VirtualPhenix.Nintendo64.MIPS;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class StateParser : MIPS.NaiveInterpreter
    {
        public State State;
        public WaitParams CurrWait;
        public StateBlock CurrBlock;
        public bool Trivial = true;
        public int StateIndex = -1;
        public long RecentRandom = 0;
        public long LoadAddress = 0;

        public CRGDataMap DataMap;
        public List<State> AllStates;
        public List<long> AnimationAddresses;
        private CRGDataMap DataMap1;
        private long Addr;
        private List<State> States;

        public StateParser(CRGDataMap dataMap, long startAddress, List<State> allStates, List<long> animationAddresses) : base()
        {
            this.DataMap = dataMap;
            this.AllStates = allStates;
            this.AnimationAddresses = animationAddresses;

            this.State = new State
            {
                StartAddress = (long)startAddress,
                Blocks = new List<StateBlock>(),
                DoCleanup = false
            };
            this.Trivial = true;
            this.StateIndex = -1;
        }

        public bool Parse()
        {
            return base.ParseFromView(this.DataMap.GetView(this.State.StartAddress));
        }

        public override void Reset()
        {
            base.Reset();
            this.Trivial = true;
            this.RecentRandom = 0;
            this.LoadAddress = 0;
            this.StateIndex = -1;
            this.CurrWait = new WaitParams
            {
                AllowInteraction = true,
                Interactions = new List<StateEdge>(),
                Duration = 0,
                DurationRange = 0,
                LoopTarget = 0,
                EndCondition = 0
            };
            this.CurrBlock = new StateBlock
            {
                Edges = new List<StateEdge>(),
                Animation = -1,
                Force = false,
                Motion = null,
                AuxAddress = -1,
                Signals = new List<Signal>(),
                FlagSet = 0,
                FlagClear = 0,
                Wait = null
            };
        }

        private void markNontrivial()
        {
            if (this.Trivial)
            {
                //Debug.Assert(this.stateIndex == -1);
                this.Trivial = false;
                this.StateIndex = this.AllStates.Count;
                this.AllStates.Add(this.State);
            }
        }

        protected override long HandleFunction(long func, Register a0, Register a1, Register a2, Register a3, Register[] stackArgs)
        {
            return base.HandleFunction(func, a0, a1, a2, a3, stackArgs);
        }
    }

}
