using System;
using System.Collections.Generic;

namespace VirtualPhenix.Nintendo64.MIPS
{
    public enum Opcode
    {
        NOP = 0x00,
        BRANCH = 0x01,
        JAL = 0x03,
        BEQ = 0x04,
        BNE = 0x05,
        ADDIU = 0x09,
        SLTI = 0x0A,
        SLTIU = 0x0B,
        ANDI = 0x0C,
        ORI = 0x0D,
        XORI = 0x0E,
        LUI = 0x0F,

        COP0 = 0x10,
        COP1 = 0x11,

        BEQL = 0x14,
        BNEL = 0x15,

        LB = 0x20,
        LH = 0x21,
        LW = 0x23,
        LBU = 0x24,
        LHU = 0x25,
        SB = 0x28,
        SH = 0x29,
        SW = 0x2B,
        LWC1 = 0x31,
        SWC1 = 0x39,

        // Register op block
        REGOP = 0x100,
        SLL = 0x100,
        SRL = 0x102,
        SRA = 0x103,
        SLLV = 0x104,
        SRLV = 0x106,
        SRAV = 0x107,

        JR = 0x108,
        JALR = 0x109,
        MFHI = 0x110,
        MFLO = 0x112,
        MULT = 0x118,
        DIV = 0x11A,
        ADD = 0x120,
        ADDU = 0x121,
        SUB = 0x122,
        SUBU = 0x123,
        AND = 0x124,
        OR = 0x125,
        XOR = 0x126,
        NOR = 0x127,
        SLT = 0x12A,

        // Coprocessor 1 op block
        COPOP = 0x200,
        MFC1 = 0x200,
        MTC1 = 0x204,

        // Float (single) op block
        FLOATOP = 0x300,
        ADDS = 0x300,
        SUBS = 0x301,
        MULS = 0x302,
        MOVS = 0x306,

        // Extra branch op block
        BLTZ = 0x400,
        BGEZ = 0x401
    }

    public enum RegName
    {
        R0 = 0x00,
        AT = 0x01,
        V0 = 0x02,
        V1 = 0x03,
        A0 = 0x04,
        A1 = 0x05,
        A2 = 0x06,
        A3 = 0x07,

        S0 = 0x10,
        S1 = 0x11,
        S2 = 0x12,
        S3 = 0x13,
        S4 = 0x14,
        S5 = 0x15,
        S6 = 0x16,
        S7 = 0x17,

        SP = 0x1D,
        FP = 0x1E,
        RA = 0x1F,
    }

    public static class OpcodeHelper
    {
        public static Opcode ParseMIPSOpcode(long instr)
        {
            Opcode op = (Opcode)(instr >> 26);
            long rs = (instr >> 21) & 0x1F;
            long rt = (instr >> 16) & 0x1F;
            if (op == Opcode.NOP && instr != 0)
                op = (Opcode)((instr & 0x3F) | (long)Opcode.REGOP);
            else if (op == Opcode.COP1)
            {
                if (rs == (long)Opcode.COP0 || rs == (long)Opcode.COP1)
                    op = (Opcode)((long)Opcode.FLOATOP | (long)(instr & 0x3F));
                else
                    op = (Opcode)((long)Opcode.COPOP | (long)rs);
            }
            else if (op == Opcode.BRANCH)
            {
                op = (Opcode)((long)Opcode.BLTZ | (long)rt);
            }
            return op;
        }
    }

    public class Register
    {
        public long Value;
        public Opcode LastOp;
    }

    public class BranchInfo
    {
        public long Start;
        public long End;
        public Register Comparator;
        public Opcode Op;
    }
}
