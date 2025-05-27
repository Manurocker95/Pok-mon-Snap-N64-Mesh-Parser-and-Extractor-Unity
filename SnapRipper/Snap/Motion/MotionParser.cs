using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.MIPS;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class MotionParser : MIPS.NaiveInterpreter
    {
        public List<Motion> Blocks = new();
        private CRGDataMap DataMap;
        private List<long> Animations;
        private long StartAddress = 0;

        private float Timer = 0;
        private float YSpeed = 0;
        private Direction YawDirection = Direction.Forward;
        private float MovingYaw = 0;
        private Vector3 PositionOffset = Vector3.one;
        private float YawOffset = 0;

        public override void Reset()
        {
            base.Reset();
            Blocks = new List<Motion>();
            Timer = 0;
            YSpeed = 0;
            MovingYaw = 0;
            PositionOffset = Vector3.zero;
            YawOffset = 0;
        }

        public bool Parse(CRGDataMap dataMap, long startAddress, List<long> animations)
        {
            DataMap = dataMap;
            Animations = animations;
            StartAddress = startAddress;
            return base.ParseFromView(dataMap.GetView(startAddress));
        }

        private ObjParam GetFloatValue(MIPS.Register reg)
        {
            float value;

            if (reg.Value > 0x80000000 && reg.Value < 0x80400000)
                value = MathHelper.BitsAsFloat32(this.DataMap.Deref(reg.Value));
            else
                value = MathHelper.BitsAsFloat32(reg.Value);

            //VP_BYMLUtils.Assert((value == 0 || Math.Abs(value) > 1e-6f) && Math.Abs(value) < 1e6f, $"bad float value {VP_BYMLUtils.HexZero(reg.Value, 8)}}");

            return new ObjParam() { Constant = value };
        }

        private ObjParam GetFloatParam(MIPS.Register reg)
        {
            if (reg.Value < 0x100)
            {
                long index = (reg.Value - (long)ObjectField.StoredValues) >> 2;

                if (index >= 3)
                {
                    Debug.LogError("bad stored value index" +  index);
                    return new ObjParam() { Stored = new StoredValue() { Index = 0 } };
                }

                return new ObjParam() { Stored = new StoredValue() { Index = index } };
            }
            else
            {
                return GetFloatValue(reg);
            }
        }

        protected override long HandleFunction(long func, Register a0, Register a1, Register a2, Register a3, Register[] stackArgs, BranchInfo branch = null)
        {
            switch (func)
            {
                case (long)MotionFuncs.ResetPos:
                    break;
                case (long)StateFuncs.ForceAnimation:
                case (long)StateFuncs.SetAnimation:
                    bool force = func == (long)StateFuncs.ForceAnimation;

                    if (a1.LastOp != MIPS.Opcode.ADDIU || a1.Value < 0x80000000)
                    {
                        this.Valid = false;
                        Debug.LogError("bad animation address in motion " +  VP_BYMLUtils.HexZero(a1.Value, 8));
                        break;
                    }

                    long index = this.Animations.FindIndex(a => a == a1.Value);

                    if (index == -1)
                    {
                        index = this.Animations.Count;
                        this.Animations.Add(a1.Value);
                    }

                    this.Blocks.Add(new AnimationMotion
                    {
                        Index = index,
                        Force = force
                    });
                    break;
                case (long)MotionFuncs.Path:
                    var start = PathStart.Begin;

                    if (a1.LastOp == MIPS.Opcode.LW)
                    {
                        if (a1.Value == (long)ObjectField.PathParam)
                            start = PathStart.Resume;
                        else if (a1.Value == 4)
                            start = PathStart.SkipFirst;
                        else if (a1.Value == 0)
                            start = PathStart.StoredSegment;
                        else
                            Debug.LogError("unknown path start " +  a1.Value);
                    }
                    else if (a1.LastOp == MIPS.Opcode.JAL)
                    {
                        start = PathStart.Random;
                    }
                    else if ((a1.LastOp != MIPS.Opcode.MFC1 &&
                              a1.LastOp != MIPS.Opcode.ADDIU &&
                              a1.LastOp != MIPS.Opcode.NOP) || a1.Value != 0)
                    {
                        Debug.LogError("unknown path start "+ a1.LastOp+" "+ a1.Value);
                    }

                    ObjParam end = new ObjParam();

                    if (a2.LastOp == MIPS.Opcode.LW && a2.Value == 4)
                    {
                        start = PathStart.FirstSegment;
                    }
                    else if (start != PathStart.StoredSegment)
                    {
                        end = this.GetFloatValue(a2);
                    }

                    this.Blocks.Add(new FollowPathMotion
                    {
                        Start = start,
                        End = end,
                        Speed = this.GetFloatParam(a3),
                        MaxTurn = this.GetFloatValue(stackArgs[0]).Constant,
                        Flags = stackArgs[1].Value
                    });
                    break;
                case (long)MotionFuncs.Projectile:
                    this.Blocks.Add(new ProjectileMotion
                    {
                        G = this.GetFloatValue(a1).Constant,
                        MoveForward = a2.Value == 1,
                        ySpeed = this.YSpeed,
                        Direction = this.YawDirection,
                        Yaw = this.MovingYaw
                    });
                    break;
                case (long)MotionFuncs.RiseBy:
                case (long)MotionFuncs.RiseTo:
                case (long)MotionFuncs.FallBy:
                case (long)MotionFuncs.FallTo:
                    bool asDelta = func == (long)MotionFuncs.RiseBy || func == (long)MotionFuncs.FallBy;
                    int direction = (func == (long)MotionFuncs.FallBy || func == (long)MotionFuncs.FallTo) ? -1 : 1;

                    this.Blocks.Add(new VerticalMotion
                    {
                        AsDelta = asDelta,
                        StartSpeed = this.YSpeed,
                        Target = GetFloatParam(a1),
                        G = GetFloatValue(a2).Constant,
                        MinVel = GetFloatValue(a3).Constant,
                        MaxVel = GetFloatValue(stackArgs[0]).Constant,
                        Direction = (Direction)direction
                    });
                    break;
                case (long)MotionFuncs.RandomCircle:
                    this.Blocks.Add(new RandomCircle
                    {
                        Radius = GetFloatValue(a1).Constant,
                        MaxTurn = GetFloatValue(a2).Constant
                    });
                    break;
                case (long)MotionFuncs.WalkToTarget:
                case (long)MotionFuncs.WalkFromTarget:
                    this.Blocks.Add(new WalkToTargetMotion
                    {
                        Radius = GetFloatValue(a1).Constant,
                        MaxTurn = GetFloatValue(a2).Constant,
                        Flags = a3.Value | (long)MoveFlags.Ground,
                        Away = func == (long)MotionFuncs.WalkFromTarget
                    });
                    break;
                case (long)MotionFuncs.WalkFromTarget2:
                    this.Blocks.Add(new WalkToTargetMotion
                    {
                        Radius = GetFloatValue(a1).Constant,
                        MaxTurn = 0.1f,
                        Flags = (long)MoveFlags.Ground,
                        Away = true
                    });
                    break;
                case (long)MotionFuncs.FaceTarget:
                    this.Blocks.Add(new FaceTargetMotion
                    {
                        MaxTurn = GetFloatValue(a1).Constant,
                        Flags = a2.Value
                    });
                    break;
                case (long)MotionFuncs.GetSong:
                    this.Blocks.Add(new BasicMotion
                    {
                        Subtype = BasicMotionKind.Song,
                        Param = 0
                    });
                    break;
                case (long)MotionFuncs.StepToPoint:
                    // these often appear in pairs, one before a loop, one inside
                    if (this.Blocks.Count > 0 && this.Blocks[this.Blocks.Count - 1].Kind == MotionKind.point)
                        break;

                    this.Blocks.Add(new ApproachPointMotion
                    {
                        Goal = ApproachGoal.AtPoint,
                        MaxTurn = GetFloatValue(a1).Constant,
                        Destination = Destination.Custom,
                        Flags = a2.Value
                    });
                    break;
                case (long)MotionFuncs.ApproachPoint:
                    this.Blocks.Add(new ApproachPointMotion
                    {
                        Goal = ApproachGoal.AtPoint,
                        MaxTurn = GetFloatValue(a1).Constant,
                        Destination = Destination.Custom,
                        Flags = (long)MoveFlags.Ground
                    });
                    break;
                case (long)MotionFuncs.MoveForward:
                case (long)MotionFuncs.VolcanoForward:
                    this.Blocks.Add(new ForwardMotion
                    {
                        StopIfBlocked = false
                    });
                    break;
                case (long)MotionFuncs.DynamicVerts:
                    this.Blocks.Add(new BasicMotion
                    {
                        Subtype = BasicMotionKind.Dynamic,
                        Param = GetFloatValue(a2).Constant,
                    });
                    break;
                case (long)StateFuncs.SplashAt:
                case (long)StateFuncs.SplashBelow:
                    this.Blocks.Add(new SplashMotion
                    {
                        OnImpact = false,
                        Index = -1,
                        Scale = Vector3.one
                    });
                    break;
                case (long)StateFuncs.SplashOnImpact:
                    this.Blocks.Add(new SplashMotion
                    {
                        OnImpact = true,
                        Index = 8,
                        Scale = Vector3.one
                    });
                    break;
                case (long)StateFuncs.InteractWait:
                    {
                        VP_BYMLUtils.Assert(a1.Value == (long)EndCondition.Timer && this.Timer > 0);

                        this.Blocks.Add(new BasicMotion
                        {
                            Subtype = BasicMotionKind.Wait,
                            Param = this.Timer
                        });

                        this.Timer = 0;
                    }
                    break;
                case (long)GeneralFuncs.Yield:
                    {
                        // same idea as in StateParser
                        if (a0.Value > 1)
                        {
                            this.Blocks.Add(new BasicMotion
                            {
                                Subtype = BasicMotionKind.Wait,
                                Param = a0.Value / 30f
                            });
                        }
                    }
                    break;
            }

            return 0;
        }

        public override void HandleStore(Opcode op, Register value, Register target, long offset)
        {
            if (op == MIPS.Opcode.SW || op == MIPS.Opcode.SWC1)
            {
                // same condition as StateParser, looks like loading a struct field
                switch (offset)
                {
                    case 0x4:
                    case 0x8:
                    case 0xC:
                        if ((value.LastOp != MIPS.Opcode.ADDS && value.LastOp != MIPS.Opcode.SUBS) || value.Value < 0x100)
                            break;

                        float delta = GetFloatValue(value).Constant;
                        if (value.LastOp == MIPS.Opcode.SUBS)
                            delta *= -1;

                        if (target.LastOp == MIPS.Opcode.ADDIU && target.Value == 4)
                        {
                            this.PositionOffset[(int)((offset - 4) >> 2)] = delta;
                        }
                        else if (target.LastOp == MIPS.Opcode.ADDIU && target.Value == 0x14)
                        {
                            VP_BYMLUtils.Assert(offset == 0xC);
                            this.YawOffset = delta;
                        }
                        break;
                    case (long)ObjectField.TranslationX:
                    case (long)ObjectField.TranslationY:
                    case (long)ObjectField.TranslationZ:
                        if (op != MIPS.Opcode.SWC1 || target.LastOp != MIPS.Opcode.LW || target.Value != 0x48)
                            break;

                        if (!(value.LastOp == MIPS.Opcode.ADDS || value.LastOp == MIPS.Opcode.SUBS) || value.Value < 0x100)
                            break;

                        float delta2 = GetFloatValue(value).Constant;
                        if (value.LastOp == MIPS.Opcode.SUBS)
                            delta2 *= -1;

                        var componentIndex = ((offset - (long)ObjectField.TranslationX) >> 2);

                        switch (componentIndex)
                        {
                            case 0: this.PositionOffset.x = delta2; break;
                            case 1: this.PositionOffset.y = delta2; break;
                            case 2: this.PositionOffset.z = delta2; break;
                        }
                        break;
                    case (long)ObjectField.Timer:
                        VP_BYMLUtils.Assert(op == MIPS.Opcode.SW && value.LastOp == MIPS.Opcode.ADDIU);
                        this.Timer = value.Value / 30f;
                        break;
                    case (long)ObjectField.ForwardSpeed:
                        VP_BYMLUtils.Assert(op == MIPS.Opcode.SWC1);

                        this.Blocks.Add(new BasicMotion
                        {
                            Subtype = BasicMotionKind.SetSpeed,
                            Param = GetFloatValue(value).Constant
                        });
                        break;
                    case (long)ObjectField.VerticalSpeed:
                        VP_BYMLUtils.Assert(op == MIPS.Opcode.SWC1);
                        this.YSpeed = GetFloatValue(value).Constant;
                        break;

                    case (long)ObjectField.MovingYaw:
                        if (value.LastOp == MIPS.Opcode.JAL)
                        {
                            this.YawDirection = Direction.Impact; // assume this is from an atan2
                        }
                        else if (value.LastOp == MIPS.Opcode.ADDS)
                        {
                            this.YawDirection = Direction.Backward;
                        }
                        else if ((value.LastOp == MIPS.Opcode.LWC1 && value.Value > 0x80000000) || value.LastOp == MIPS.Opcode.NOP)
                        {
                            this.YawDirection = Direction.Constant;
                            this.MovingYaw = GetFloatValue(value).Constant;
                        }
                        break;

                }
            }
        }

        protected override void HandleLoop(Opcode op, Register left, Register right, long offset)
        {
            long frames = 0;

            if (!(op == MIPS.Opcode.BNE || op == MIPS.Opcode.BNEL) ||
                !(left.LastOp == MIPS.Opcode.ADDIU || left.LastOp == MIPS.Opcode.NOP) ||
                !(right.LastOp == MIPS.Opcode.ADDIU || right.LastOp == MIPS.Opcode.NOP))
                return; // doesn't look like a for loop

            if (left.Value > 15 && right.Value == 0)
                frames = left.Value + 1;
            else if (left.Value == 1 && right.Value > 15)
                frames = right.Value;

            var velocity = this.PositionOffset;
            velocity *= 30f;

            this.Blocks.Add(new LinearMotion
            {
                Duration = frames / 30f,
                Velocity = velocity,
                TurnSpeed = this.YawOffset * 30f,
                MatchTarget = false
            });
        }

        protected override void Finish()
        {
            // if we couldn't understand the function, add a placeholder 5 second wait
            if (this.Blocks.Count == 0)
            {
                this.Blocks.Add(new BasicMotion
                {
                    Subtype = BasicMotionKind.Custom,
                    Param = 0
                });
            }

            MIPSUtils.FixupMotion(this.StartAddress, this.Blocks);
        }
    }
}
