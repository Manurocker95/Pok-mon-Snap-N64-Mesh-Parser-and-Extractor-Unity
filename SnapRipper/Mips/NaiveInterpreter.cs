using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using VirtualPhenix.Nintendo64;

namespace VirtualPhenix.Nintendo64.MIPS
{
    public class NaiveInterpreter
    {
        public Register[] Regs = new Register[32];
        public Register[] FRegs = new Register[32];
        public Register[] StackArgs = new Register[10];

        protected bool Done = false;
        protected bool Valid = true;
        public long LastInstr = 0;

        public NaiveInterpreter()
        {
            Reset();
        }

        public virtual void Reset()
        {
            for (long i = 0; i < Regs.Length; i++)
                Regs[i] = new Register { Value = 0, LastOp = Opcode.NOP };
            for (long i = 0; i < FRegs.Length; i++)
                FRegs[i] = new Register { Value = 0, LastOp = Opcode.NOP };
            for (long i = 0; i < StackArgs.Length; i++)
                StackArgs[i] = new Register { Value = 0, LastOp = Opcode.NOP };
            Done = false;
            Valid = true;
            LastInstr = 0;
        }

        protected virtual long HandleFunction(long func, Register a0, Register a1, Register a2, Register a3, Register[] stackArgs, MIPS.BranchInfo branch = null)
        {
            return 0;
        }

        public virtual void HandleStore(Opcode op, Register value, Register target, long offset) { }

        protected virtual void HandleUnknown(Opcode op) => Valid = false;
        protected virtual void HandleLoop(Opcode op, Register left, Register right, long offset) => Valid = false;
        protected virtual void Finish() => Done = true;

        protected bool SeemsLikeLiteral(Register r)
        {
            switch (r.LastOp)
            {
                case Opcode.JAL:
                    return r.Value != 0;
                case Opcode.AND:
                case Opcode.OR:
                case Opcode.ADDIU:
                case Opcode.ANDI:
                case Opcode.ORI:
                case Opcode.LUI:
                    return true;
                default:
                    return false;
            }
        }

        protected long GuessValue(Register r1, Register r2)
        {
            if (SeemsLikeLiteral(r1)) return r1.Value;
            if (SeemsLikeLiteral(r2)) return r2.Value;
            return 0;
        }

        protected long GuessValue(Register r1)
        {
            Register r2 = Regs[(long)RegName.R0];
            return GuessValue(r1, r2);
        }

        protected long GuessValue(RegName r1, RegName r2 = RegName.R0)
        {
            if (SeemsLikeLiteral(r1)) return Regs[(long)r1].Value;
            if (SeemsLikeLiteral(r2)) return Regs[(long)r2].Value;
            return 0;
        }

        protected bool SeemsLikeLiteral(RegName r)
        {
            switch (Regs[(long)r].LastOp)
            {
                case Opcode.JAL:
                    return Regs[(long)r].Value != 0; // We provided this, trust it
                case Opcode.AND:
                case Opcode.OR:
                case Opcode.ADDIU:
                case Opcode.ANDI:
                case Opcode.ORI:
                case Opcode.LUI:
                    return true;
                default:
                    return false;
            }
        }
       
        public virtual string GetOpcodeDescription(Opcode opcode)
        {
            int value = (int)opcode;
            return $"{opcode} (0x{value:X}) - {value}";
        }

        public virtual bool ParseFromView(VP_DataView view, long offset = 0)
        {
            Reset();

            long func = 0;
            long nextMeet = 0;
            BranchInfo currBranch = null;
            List<BranchInfo> branches = new List<BranchInfo>();

            while (!Done && offset + 4 <= view.ByteLength)
            {
                long instr = view.GetUint32(offset, false);
                LastInstr = instr;
                Opcode op = OpcodeHelper.ParseMIPSOpcode(instr);
                long rs = (long)((instr >> 21) & 0x1F);
                long rt = (long)((instr >> 16) & 0x1F);
                long rd = (long)((instr >> 11) & 0x1F);
                long frd = (long)((instr >> 6) & 0x1F);
                short imm = (short)(instr & 0xFFFF);
                ushort u_imm = (ushort)(instr & 0xFFFF);
                //UnityEngine.Debug.Log(GetOpcodeDescription(op));
                switch (op)
                {
                    case Opcode.NOP:
                        break;
                    case Opcode.BEQ:
                        if (rs == 0 && rt == 0 && imm > 0)
                        {
                            nextMeet = Math.Max(nextMeet, offset + 4 * (imm + 1));
                            if (currBranch != null)
                            {
                                if (Valid && currBranch.End != -1 && currBranch.End != offset + 8)
                                    throw new Exception("unconditional branch in the middle of if block");
                                currBranch.End = offset + 8;
                            }
                            break;
                        }
                        // Don't try to track loops or nested conditionals
                        if (imm <= 0)
                        {
                            HandleLoop(op, Regs[rs], Regs[rt], imm);
                            break;
                        }
                        else if (currBranch != null)
                        {
                            HandleUnknown(op);
                            break;
                        }

                        if (rs == 0 && rt == 0)
                            throw new Exception("bad trivial branch");

                        Register compReg2 = Regs[rt];
                        if (rt == 0 || (rs != 0 && SeemsLikeLiteral(Regs[rs])))
                            compReg2 = Regs[rs];

                        Register comparator2 = new Register { LastOp = compReg2.LastOp, Value = compReg2.Value };

                        // If the body starts right away, the condition is effectively inverted
                        long start2 = offset + 8;
                        long end2 = offset + 4 * (imm + 1);
                        nextMeet = (long)Math.Max(nextMeet, end2);

                        if (rs != 0 && rt != 0 && (op == Opcode.BEQ || op == Opcode.BEQL))
                        {
                            // if not comparing to zero, assume we are looking at
                            //      if (x == y)
                            // meaning "positive" branches jump to the start of the body
                            start2 = end2;
                            end2 = -1;
                        }

                        branches.Add(new BranchInfo
                        {
                            Op = op,
                            Start = (long)start2,
                            End = (long)end2,
                            Comparator = comparator2
                        });
                        break;
                    case Opcode.BNE:
                    case Opcode.BNEL:
                    case Opcode.BEQL:
                        // Don't try to track loops or nested conditionals
                        if (imm <= 0)
                        {
                            HandleLoop(op, Regs[rs], Regs[rt], imm);
                            break;
                        }
                        else if (currBranch != null)
                        {
                            HandleUnknown(op);
                            break;
                        }

                        if (rs == 0 && rt == 0)
                            throw new Exception("bad trivial branch");

                        Register compReg = Regs[rt];
                        if (rt == 0 || (rs != 0 && SeemsLikeLiteral(Regs[rs])))
                            compReg = Regs[rs];

                        Register comparator = new Register { LastOp = compReg.LastOp, Value = compReg.Value };

                        // If the body starts right away, the condition is effectively inverted
                        long start = offset + 8;
                        long end = offset + 4 * (imm + 1);
                        nextMeet = (long)Math.Max(nextMeet, end);

                        if (rs != 0 && rt != 0 && (op == Opcode.BEQ || op == Opcode.BEQL))
                        {
                            // if not comparing to zero, assume we are looking at
                            //      if (x == y)
                            // meaning "positive" branches jump to the start of the body
                            start = end;
                            end = -1;
                        }

                        branches.Add(new BranchInfo
                        {
                            Op = op,
                            Start = (long)start,
                            End = (long)end,
                            Comparator = comparator
                        });
                        break;
                    case Opcode.SB:
                    case Opcode.SH:
                    case Opcode.SW:
                        if (rs != (long)RegName.SP)
                        {
                            HandleStore(op, Regs[rt], Regs[rs], imm);
                        }
                        else if (rt != (long)RegName.RA && op == Opcode.SW)
                        {
                            long stackOffset = (u_imm >> 2) - 4;
                            if (stackOffset >= 0 && stackOffset < StackArgs.Length)
                            {
                                StackArgs[stackOffset].LastOp = Regs[rt].LastOp;
                                StackArgs[stackOffset].Value = Regs[rt].Value;
                            }
                        }
                        break;
                    case Opcode.SWC1:
                        if (rs != (long)RegName.SP)
                        {
                            HandleStore(op, FRegs[rt], Regs[rs], imm);
                        }
                        else
                        {
                            long stackOffset = (u_imm >> 2) - 4;
                            if (stackOffset >= 0 && stackOffset < StackArgs.Length)
                            {
                                StackArgs[stackOffset].LastOp = FRegs[rt].LastOp;
                                StackArgs[stackOffset].Value = FRegs[rt].Value;
                            }
                        }
                        break;
                    case Opcode.LW:
                        long stackIndex = (imm >> 2) - 4;
                        if (rs == (long)RegName.SP && stackIndex >= 0 && stackIndex < StackArgs.Length)
                        {
                            var stored = StackArgs[stackIndex];
                            Regs[rt].Value = stored.Value;
                            Regs[rt].LastOp = stored.LastOp;
                            break;
                        }
                        Register targetLW = (op == Opcode.LWC1) ? FRegs[rt] : Regs[rt];
                        if (imm == 0)
                            targetLW.Value = GuessValue(Regs[rs]);
                        else if ((Regs[rs].Value & 0xFFFF) == 0)
                            targetLW.Value = GuessValue(Regs[rs]) + imm;
                        else
                            targetLW.Value = imm;

                        targetLW.LastOp = op;
                        break;
                    case Opcode.LB:
                    case Opcode.LBU:
                    case Opcode.LH:
                    case Opcode.LHU:
                    case Opcode.LWC1:
                        Register target = (op == Opcode.LWC1) ? FRegs[rt] : Regs[rt];
                        if (imm == 0)
                            target.Value = GuessValue(Regs[rs]);
                        else if ((Regs[rs].Value & 0xFFFF) == 0)
                            target.Value = GuessValue(Regs[rs]) + imm;
                        else
                            target.Value = imm;

                        target.LastOp = op;
                        break;
                    case Opcode.ADDU:
                        Regs[rd].Value = GuessValue(Regs[rs]) + GuessValue(Regs[rt]);
                        Regs[rd].LastOp = op;
                        break;
                    case Opcode.AND:
                        Regs[rd].Value = GuessValue(Regs[rs], Regs[rt]);
                        // if multiple flag operations happened, try to get the one used for AND
                        if (Regs[rs].LastOp == Opcode.OR || Regs[rs].LastOp == Opcode.ORI)
                            Regs[rd].Value = GuessValue(Regs[rt]);
                        Regs[rd].LastOp = op;
                        break;
                    case Opcode.OR:
                        if (rt == (long)RegName.R0)
                        {
                            // actually a MOV
                            Regs[rd].Value = Regs[rs].Value;
                            Regs[rd].LastOp = Regs[rs].LastOp;
                        }
                        else
                        {
                            Regs[rd].Value = GuessValue(Regs[rs], Regs[rt]);
                            Regs[rd].LastOp = op;
                        }
                        break;
                    case Opcode.ANDI:
                        Regs[rt].Value = u_imm;
                        Regs[rt].LastOp = op;
                        break;
                    case Opcode.LUI:
                        Regs[rt].Value = (long)((long)u_imm << 16);
                        Regs[rt].LastOp = op;
                        break;
                    case Opcode.ORI:
                        Regs[rt].Value = GuessValue(Regs[rs], Regs[rt]) | u_imm;
                        Regs[rt].LastOp = op;
                        break;
                    case Opcode.ADDIU:
                        if (rt != (long)RegName.SP) // ignore stack changes
                        {
                            Regs[rt].Value = GuessValue(Regs[rs]) + imm;
                            Regs[rt].LastOp = op;
                        }
                        break;
                    case Opcode.JAL:
                        func = (long)((instr & 0xFFFFFF) << 2);
                        break;
                    case Opcode.JR:
                        if (rs == (long)RegName.RA)
                        {
                            Finish();
                            return Valid;
                        }
                        // a switch statement, beyond the scope of this interpreter
                        HandleUnknown(op);
                        break;
                    case Opcode.MFC1:
                        Regs[rt].LastOp = FRegs[rd].LastOp;
                        Regs[rt].Value = FRegs[rd].Value;
                        break;
                    case Opcode.MTC1:
                        FRegs[rd].LastOp = Regs[rt].LastOp;
                        FRegs[rd].Value = Regs[rt].Value;
                        break;
                    case Opcode.SUBS:
                    case Opcode.ADDS:
                    case Opcode.MULS:
                        FRegs[frd].LastOp = op;
                        FRegs[frd].Value = FRegs[rd].Value;
                        if (FRegs[rd].Value == 0 || (FRegs[rd].LastOp == Opcode.LWC1 && FRegs[rd].Value < 0x80000000))
                            FRegs[frd].Value = FRegs[rt].Value;
                        break;
                    case Opcode.MOVS:
                        this.FRegs[frd].LastOp = this.FRegs[rd].LastOp;
                        break;
                    default:
                        HandleUnknown(op);
                        break;
                }

                if (op == Opcode.BEQL || op == Opcode.BNEL)
                    offset += 4; // skip the delay slot entirely

                if (func != 0 && op != Opcode.JAL)
                {
                    long v0 = HandleFunction(func, Regs[(long)RegName.A0], Regs[(long)RegName.A1], Regs[(long)RegName.A2], Regs[(long)RegName.A3], StackArgs);
                    Regs[(long)RegName.V0].LastOp = Opcode.JAL;
                    Regs[(long)RegName.V0].Value = v0;
                    FRegs[0].LastOp = Opcode.JAL;
                    FRegs[0].Value = v0;
                    func = 0;
                }

                offset += 4;

                if (currBranch != null &&
                    ((currBranch.End >= 0 && offset >= currBranch.End) || (offset >= nextMeet)))
                {
                    currBranch = null;
                }

                // check if we started a new branch
                for (int i = 0; i < branches.Count; i++)
                {
                    if (branches[i].Start == offset)
                    {
                        currBranch = branches[i];
                        break;
                    }
                }

            }
            Finish();
            return Valid;
        }

        protected long GuessValue(long r, long s = (long)RegName.R0)
        {
            if (SeemsLikeLiteral(Regs[r]))
                return Regs[r].Value;
            if (SeemsLikeLiteral(Regs[s]))
                return Regs[s].Value;
            return 0;
        }
    }
}
