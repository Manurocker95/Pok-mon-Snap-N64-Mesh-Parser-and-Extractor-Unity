using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public MIPS.Register[] DummyRegs;

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

        private void MarkNontrivial()
        {
            if (this.Trivial)
            {
                //Debug.Assert(this.stateIndex == -1);
                this.Trivial = false;
                this.StateIndex = this.AllStates.Count;
                this.AllStates.Add(this.State);
            }
        }

        protected override void Finish()
        {
            if (!MIPSUtils.EmptyStateBlock(this.CurrBlock))
                State.Blocks.Add(this.CurrBlock);
        }

        protected override long HandleFunction(long func, Register a0, Register a1, Register a2, Register a3, Register[] stackArgs, MIPS.BranchInfo branch = null)
        {
            StateFuncs sf = (StateFuncs)func;

            if (sf != StateFuncs.SetState || branch != null)
                MarkNontrivial();

            DummyRegs = Enumerable.Range(0, 2).Select(_ => new MIPS.Register { Value = 0, LastOp = MIPS.Opcode.NOP }).ToArray();

            switch (func)
            {
                case (long)StateFuncs.SetState:
                    long index = a1.Value == 0 
                        ? -1 
                        : MIPSUtils.ParseStateSubgraph(this.DataMap, a1.Value, this.AllStates, this.AnimationAddresses);

                    if (index == -1)
                    {
                        // no next state, we're done
                        VP_BYMLUtils.Assert(branch == null, $"bad transition to null {VP_BYMLUtils.HexZero(this.State.StartAddress, 8)} {branch?.Comparator}");
                        return 0;
                    }

                    if (this.Trivial && branch == null)
                    {
                        // this state can be ignored, point to the real one
                        this.StateIndex = (int)index;
                        return 0;
                    }

                    var type = InteractionType.Basic;
                    long param = 0;

                    if (branch != null)
                    {
                        param = branch.Comparator.Value;

                        if (branch.Comparator.LastOp == MIPS.Opcode.ANDI || branch.Comparator.LastOp == MIPS.Opcode.AND)
                        {
                            type = InteractionType.Flag;
                            if (branch.Op == MIPS.Opcode.BNE || branch.Op == MIPS.Opcode.BNEL)
                                type = InteractionType.NotFlag;

                            param = branch.Comparator.Value;
                        }
                        else if (branch.Comparator.LastOp == MIPS.Opcode.ADDIU)
                        {
                            type = branch.Comparator.Value < 10 ? InteractionType.Behavior : InteractionType.OverSurface;
                        }
                        else if (branch.Comparator.LastOp == MIPS.Opcode.LW)
                        {
                            ObjectField of = (ObjectField)branch.Comparator.Value;
                            switch (of)
                            {
                                case ObjectField.Behavior:
                                    type = InteractionType.Behavior;
                                    if (branch.Op == MIPS.Opcode.BEQ || branch.Op == MIPS.Opcode.BEQL)
                                        type = InteractionType.NonzeroBehavior;
                                    break;

                                case ObjectField.Target:
                                    type = InteractionType.NoTarget;
                                    if (branch.Op == MIPS.Opcode.BEQ || branch.Op == MIPS.Opcode.BEQL)
                                        type = InteractionType.HasTarget;
                                    break;

                                case ObjectField.Apple:
                                    type = InteractionType.HasApple;
                                    break;

                                default:
                                    this.Valid = false;
                                    type = InteractionType.Unknown;
                                    break;
                            }
                        }
                        else if (branch.Comparator.LastOp == MIPS.Opcode.ORI)
                        {
                            type = InteractionType.OverSurface;
                        }
                        else
                        {
                            this.Valid = false;
                            type = InteractionType.Unknown;
                        }
                    }

                    this.AddEdge(new StateEdge { Type = type, Param = param, Index = index, AuxFunc = 0 });
                    break;
                case (long)StateFuncs.Random:
                    long randomStart = a1.Value;
                    if (a1.Value < 0x80000000)
                        randomStart = this.LoadAddress; // only used by one starmie state

                    var randomView = this.DataMap.GetView(randomStart);
                    long offs = 0;
                    long total = 0;

                    while (true)
                    {
                        long weight = randomView.GetInt32(offs + 0x00);
                        if (weight == 0)
                            break;

                        total += weight;
                        offs += 8;
                    }

                    VP_BYMLUtils.Assert(total > 0, "empty random transition");

                    offs = 0;
                    while (true)
                    {
                        long weight = randomView.GetInt32(offs + 0x00);
                        if (weight == 0)
                            break;

                        long stateAddr = randomView.GetUint32(offs + 0x04);
                        long state = MIPSUtils.ParseStateSubgraph(this.DataMap, stateAddr, this.AllStates, this.AnimationAddresses);

                        this.AddEdge(new StateEdge
                        {
                            Type = InteractionType.Random,
                            Index = state,
                            Param = (double)weight / total,
                            AuxFunc = 0
                        });

                        offs += 8;
                    }

                    this.Done = true;
                    break;
                case (long)StateFuncs.ForceAnimation:
                    this.CurrBlock.Force = true;
                    if (a1.LastOp != MIPS.Opcode.ADDIU || a1.Value < 0x80000000)
                    {
                        this.Valid = false;
                        break;
                    }

                    long index2 = this.AnimationAddresses.FindIndex(a => a == a1.Value);

                    if (index2 == -1)
                    {
                        index2 = this.AnimationAddresses.Count;
                        this.AnimationAddresses.Add(a1.Value);
                    }

                    this.CurrBlock.Animation = index2;
                    break;
                case (long)StateFuncs.SetAnimation:
                    if (a1.LastOp != MIPS.Opcode.ADDIU || a1.Value < 0x80000000)
                    {
                        this.Valid = false;
                        break;
                    }

                    long index3 = this.AnimationAddresses.FindIndex(a => a == a1.Value);

                    if (index3 == -1)
                    {
                        index3 = this.AnimationAddresses.Count;
                        this.AnimationAddresses.Add(a1.Value);
                    }

                    this.CurrBlock.Animation = index3;
                    break;
                case (long)StateFuncs.SetMotion:
                    if (a1.Value == 0x802D6B14 && this.State.StartAddress == 0x802DB4A8)
                        break; // this function chooses motion based on the behavior param, fix later

                    if (a1.Value != 0)
                    {
                        MotionParser motionParser = new MotionParser();
                        motionParser.Parse(this.DataMap, a1.Value, this.AnimationAddresses);
                        this.CurrBlock.Motion = motionParser.Blocks;
                    }
                    else
                    {
                        this.CurrBlock.Motion = new List<Motion>();
                    }
                    break;
                case (long)StateFuncs.Wait:
                    this.CurrWait.AllowInteraction = false;
                    this.CurrWait.EndCondition = a1.Value;
                    this.CurrBlock.Wait = this.CurrWait;
                    this.State.Blocks.Add(this.CurrBlock);

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
                    break;
                case (long)StateFuncs.InteractWait:
                    this.CurrWait.EndCondition = a1.Value;
                    this.CurrBlock.Wait = this.CurrWait;
                    this.State.Blocks.Add(this.CurrBlock);

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
                    break;
                // first waits a second with hard-coded transitions, then the loop like DanceInteract
                case (long)StateFuncs.DanceInteract2:
                    DummyRegs[0].LastOp = MIPS.Opcode.ADDIU;
                    DummyRegs[0].Value = 0x802C6D60;
                    DummyRegs[1].LastOp = MIPS.Opcode.LW;
                    this.HandleStore(MIPS.Opcode.SW, DummyRegs[0], DummyRegs[1], (long)ObjectField.Transitions);

                    DummyRegs[0].LastOp = MIPS.Opcode.ADDIU;
                    DummyRegs[0].Value = 30;
                    DummyRegs[1].LastOp = MIPS.Opcode.LW;
                    this.HandleStore(MIPS.Opcode.SW, DummyRegs[0], DummyRegs[1], (long)ObjectField.Timer);

                    DummyRegs[0].Value = (long)EndCondition.Timer;
                    this.HandleFunction((long)StateFuncs.InteractWait, a0, DummyRegs[0], a2, a3, stackArgs, branch);
                    DummyRegs[0].LastOp = MIPS.Opcode.LW;
                    this.HandleStore(MIPS.Opcode.SW, a1, DummyRegs[0], (long)ObjectField.Transitions);

                    DummyRegs[0].Value = (long)EndCondition.Dance;
                    this.HandleFunction((long)StateFuncs.InteractWait, a0, DummyRegs[0], a2, a3, stackArgs, branch);
                    break;
                // calls interactWait in a loop until there's no song playing
                case (long)StateFuncs.DanceInteract:
                    DummyRegs[0].LastOp = MIPS.Opcode.LW;
                    this.HandleStore(MIPS.Opcode.SW, a1, DummyRegs[0], (long)ObjectField.Transitions);

                    DummyRegs[0].Value = (long)EndCondition.Dance;
                    this.HandleFunction((long)StateFuncs.InteractWait, a0, DummyRegs[0], a2, a3, stackArgs, branch);
                    break;
                case (long)GeneralFuncs.Yield:
                    // this is usually used to yield until the next frame, but can also serve as an interactionless wait
                    // so add a new wait condition to the current block, preserving any data for the next wait
                    if (a0.Value > 1)
                    {
                        this.CurrBlock.Wait = new WaitParams
                        {
                            AllowInteraction = false,
                            Interactions = new List<StateEdge>(),
                            Duration = a0.Value / 30,
                            DurationRange = 0,
                            LoopTarget = 0,
                            EndCondition = (long)EndCondition.Timer
                        };

                        this.State.Blocks.Add(this.CurrBlock);

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
                    break;
                case (long)GeneralFuncs.SignalAll:
                    // only fails because we aren't actually handling conditionals
                    // Assert(a0.Value == 3, $"signal to unknown link {HexZero(this.State.StartAddress, 8)} {a0.Value}");
                    this.CurrBlock.Signals.Add(new Signal()
                    {
                        Value = a1.Value,
                        Target = 0,
                        Condition = InteractionType.Basic,
                        ConditionParam = 0
                    });
                    break;
                case (long)GeneralFuncs.Signal:
                    long target = (a0.Value > 0x80000000 || a0.Value == (long)ObjectField.Target) ? a0.Value : 0;

                    this.CurrBlock.Signals.Add(new Signal
                    {
                        Value = a1.Value,
                        Target = target,
                        Condition = InteractionType.Basic,
                        ConditionParam = 0
                    });
                    break;
                case (long)GeneralFuncs.RunProcess:
                    if (a1.Value == 0x80000000 + (long)GeneralFuncs.AnimateNode)
                        break;

                    SpawnParser spawnParser = new SpawnParser();
                    spawnParser.ParseFromView(this.DataMap.GetView(a1.Value));

                    if (spawnParser.FoundSpawn)
                    {
                        VP_BYMLUtils.Assert(spawnParser.Data.ID != 0 && this.CurrBlock.Spawn == null);
                        this.CurrBlock.Spawn = spawnParser.Data;
                    }
                    break;
                case (long)StateFuncs.RunAux:
                    if (a1.Value == 0x802C7F74) // lapras uses an auxiliary function for its normal state logic
                    {
                        this.HandleFunction((long)StateFuncs.SetState, a0, a1, a2, a3, stackArgs, null);
                        this.Done = true;
                    }
                    else
                    {
                        this.CurrBlock.AuxAddress = a1.Value;
                    }
                    break;

                case (long)StateFuncs.EndAux:
                    VP_BYMLUtils.Assert(this.CurrBlock.AuxAddress == -1);
                    this.CurrBlock.AuxAddress = 0;
                    this.RecentRandom = a0.Value;
                    return a0.Value;
                case (long)GeneralFuncs.RandomInt:
                    this.RecentRandom = a0.Value;
                    return a0.Value;
                case (long)StateFuncs.Cleanup:
                    this.State.DoCleanup = true;
                    break;

                case(long)StateFuncs.EatApple:
                    this.CurrBlock.EatApple = true;
                    break;
                case (long)StateFuncs.SplashAt:
                case (long)StateFuncs.SplashBelow:
                case (long)StateFuncs.DratiniSplash:
                    VP_BYMLUtils.Assert(this.CurrBlock.Splash == null);

                    this.CurrBlock.Splash = new SplashMotion
                    {
                        OnImpact = false,
                        Index = -1,
                        Scale = UnityEngine.Vector3.one
                    };
                    break;
                default:
                    if (func > 0x200000 && func < 0x350200)
                    {
                        SpawnParser spawnParser2 = new SpawnParser();
                        // see if level-specific functions are spawning something
                        spawnParser2.ParseFromView(this.DataMap.GetView(0x80000000 + func));

                        if (spawnParser2.FoundSpawn)
                        {
                            VP_BYMLUtils.Assert(spawnParser2.Data.ID != 0 && this.CurrBlock.Spawn == null);
                            this.CurrBlock.Spawn = spawnParser2.Data;
                        }
                    }

                    this.Valid = false;
                    break;

            }

            return 0;
        }

        public override void HandleStore(Opcode op, Register value, Register target, long offset)
        {
            MarkNontrivial();

            if (op == MIPS.Opcode.SW && (target.LastOp == MIPS.Opcode.LW || target.LastOp == MIPS.Opcode.NOP))
            {
                // this looks like setting a struct field - LW comes from loading the object struct from the parent,
                // while NOP probably means using one of the function arguments
                switch (offset)
                {
                    case (long)ObjectField.Transitions:
                        if (value.Value == 0)
                            return;

                        long transitionStart = value.Value;
                        if (value.LastOp != MIPS.Opcode.ADDIU || value.Value < 0x80000000)
                            transitionStart = this.LoadAddress;

                        var tView = this.DataMap.GetView(transitionStart);
                        long offs = 0;

                        while (true)
                        {
                            InteractionType type = (InteractionType)tView.GetInt32(offs + 0x00);
                            if (type == InteractionType.EndMarker)
                                break;

                            long stateAddr = tView.GetUint32(offs + 0x04);
                            float param = tView.GetFloat32(offs + 0x08);
                            long auxFunc = tView.GetUint32(offs + 0x0C);

                            // special handling for poliwag, which sets some animations in aux
                            if (auxFunc == 0x802DCB0C)
                            {
                                auxFunc = 0x1000 | MIPSUtils.ParseStateSubgraph(this.DataMap, auxFunc, this.AllStates, this.AnimationAddresses);
                            }

                            offs += 0x10;

                            this.CurrWait.Interactions.Add(new StateEdge
                            {
                                Type = type,
                                Param = param,
                                Index = stateAddr > 0
                                    ? MIPSUtils.ParseStateSubgraph(this.DataMap, stateAddr, this.AllStates, this.AnimationAddresses)
                                    : -1,
                                AuxFunc = auxFunc
                            });
                        }
                        break;
                    case (long)ObjectField.Timer:
                        if (value.LastOp != MIPS.Opcode.ADDIU)
                            this.Valid = false; // starmie sets this using a separate variable

                        this.CurrWait.Duration = value.Value / 30f;

                        // hacky way to recover random ranges: when we see calls to randomInt, we set v0 to the maximum
                        // then, if we used random recently, and duration is long enough to have been a sum with v0, assume it was random
                        if (this.RecentRandom > 30 && this.RecentRandom <= value.Value)
                        {
                            this.CurrWait.Duration -= this.RecentRandom / 30f;
                            this.CurrWait.DurationRange = this.RecentRandom / 30f;
                            this.RecentRandom = 0;
                        }
                        break;
                    case (long)ObjectField.LoopTarget:
                        this.CurrWait.LoopTarget = value.Value;
                        break;
                    case (long)ObjectField.StateFlags:
                        if (value.LastOp == MIPS.Opcode.ORI || value.LastOp == MIPS.Opcode.OR)
                        {
                            this.CurrBlock.FlagSet |= value.Value;//(uint)
                        }
                        else if ((value.LastOp == MIPS.Opcode.ANDI || value.LastOp == MIPS.Opcode.AND) && value.Value < 0)
                        {
                            this.CurrBlock.FlagClear |= ~value.Value;//(uint)
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("unknown flag op "+ value.LastOp+ " "+ VP_BYMLUtils.HexZero(value.Value, 8));
                        }
                        break;
                    case (long)ObjectField.ParentFlags:
                        if (value.LastOp == MIPS.Opcode.ORI || value.LastOp == MIPS.Opcode.OR)
                        {
                            this.CurrBlock.FlagSet |= value.Value * (long)EndCondition.Hidden;
                        }
                        else if ((value.LastOp == MIPS.Opcode.ANDI || value.LastOp == MIPS.Opcode.AND) && value.Value < 0)
                        {
                            this.CurrBlock.FlagClear |= (~value.Value) * (long)EndCondition.Hidden;
                        }
                        else if (value.LastOp == MIPS.Opcode.NOP && value.Value == 0)
                        {
                            this.CurrBlock.FlagClear |= (long)EndCondition.Hidden | (long)EndCondition.PauseAnim;
                        }
                        break;
                    case (long)ObjectField.Tangible:
                        this.CurrBlock.Tangible = value.Value == 1;
                        break;

                    case (long)ObjectField.GroundList:
                        this.CurrBlock.IgnoreGround = value.Value != 0;
                        break;
                    case (long)ObjectField.FrameTarget:
                    case (long)ObjectField.Apple:
                    case (long)ObjectField.StoredValues:
                    case (long)ObjectField.Mystery:
                        break;
                    default:
                        this.Valid = false;
                        break;

                }
            }
            else if (op == MIPS.Opcode.SWC1 && (target.LastOp == MIPS.Opcode.LW || target.LastOp == MIPS.Opcode.NOP) && offset == (long)ObjectField.ForwardSpeed)
            {
                this.CurrBlock.ForwardSpeed = MathHelper.BitsAsFloat32(value.Value);
            }
            else if (op == MIPS.Opcode.SH && (target.LastOp == MIPS.Opcode.LW || target.LastOp == MIPS.Opcode.NOP))
            {
                if (offset == (long)ObjectField.ObjectFlags)
                {
                    if (value.LastOp == MIPS.Opcode.ORI || value.LastOp == MIPS.Opcode.OR)
                    {
                        this.CurrBlock.FlagSet |= (value.Value >> 9) * (long)EndCondition.Collide;
                    }
                    else if ((value.LastOp == MIPS.Opcode.ANDI || value.LastOp == MIPS.Opcode.AND) && value.Value > 0x8000)
                    {
                        this.CurrBlock.FlagClear |= ((~value.Value) >> 9) * (long)EndCondition.Collide;
                    }
                }
            }
            else
            {
                if (op == MIPS.Opcode.SW && value.Value > 0x80000000 && LoadAddress == 0)
                    LoadAddress = value.Value;

                Valid = false;
            }
        }

        public void AddEdge(StateEdge edge)
        {
            var isEmpty = MIPSUtils.EmptyStateBlock(this.CurrBlock);
            if (isEmpty && this.State.Blocks.Count > 0)
            {
                // we haven't done anything since the last block, so append it there
                this.State.Blocks[this.State.Blocks.Count - 1].Edges.Add(edge);
            }
            else
            {
                this.CurrBlock.Edges.Add(edge);
                this.State.Blocks.Add(this.CurrBlock);

                // don't worry about any wait data: it will be overwritten by the state change or used later in this state
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
                    Wait = null,
                };
            }
        }

    }

}
