using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class SpawnParser : MIPS.NaiveInterpreter
    {
        public CRGDataMap DataMap;
        public SpawnData Data;
        public bool FoundSpawn = false;

        public override void Reset()
        {
            base.Reset();
            Data = new SpawnData
            {
                ID = 0,
                Behavior = -1,
                Scale = Vector3.one,
                Yaw = Direction.Constant,
            };
            FoundSpawn = false;
        }

        protected override long HandleFunction(long f, MIPS.Register a0, MIPS.Register a1, MIPS.Register a2, MIPS.Register a3, MIPS.Register[] stackArgs, MIPS.BranchInfo branch = null)
        {
            var func = (StateFuncs)f;

            if (func == StateFuncs.SpawnActor)
            {
                //VP_BYMLUtils.Assert(Data.ID != 0);
                Data.Behavior = 0;
                FoundSpawn = true;
            }
            else if (func == StateFuncs.SpawnActorHere)
            {
                Data.ID = a1.Value;
                FoundSpawn = true;
            }
            return 0;
        }

        public override void HandleStore(MIPS.Opcode op, MIPS.Register value, MIPS.Register target, long offset)
        {
            if (op == MIPS.Opcode.SW && offset == 0 && target.LastOp == MIPS.Opcode.ADDIU && Data.ID == 0)
                Data.ID = value.Value;

            if (!FoundSpawn)
                return; // can't modify parameters until we have a pointer

            if (op == MIPS.Opcode.SW && target.LastOp == MIPS.Opcode.LW && offset == (long)ObjectField.Behavior)
            {
                Data.Behavior = value.Value;
            }
            else if (op == MIPS.Opcode.SWC1 && target.LastOp == MIPS.Opcode.LW && target.Value == (long)ObjectField.Transform && value.LastOp == MIPS.Opcode.MULS)
            {
                switch ((ObjectField)offset)
                {
                    case ObjectField.ScaleX:
                    case ObjectField.ScaleY:
                    case ObjectField.ScaleZ:
                        Data.Scale[(int)((offset - (long)ObjectField.ScaleX) >> 2)] = value.Value;
                        break;
                }
            }
            else if (op == MIPS.Opcode.SWC1 && target.LastOp == MIPS.Opcode.LW && target.Value == (long)ObjectField.Transform && value.LastOp == MIPS.Opcode.ADDS)
            {
                switch ((ObjectField)offset)
                {
                    case ObjectField.ScaleX:
                    case ObjectField.ScaleY:
                    case ObjectField.ScaleZ:
                        Data.Scale[(int)((offset - (long)ObjectField.ScaleX) >> 2)] = 2;
                        break;
                    case ObjectField.Yaw:
                        Data.Yaw = Direction.Backward;
                        break;
                }
            }
            else if (op == MIPS.Opcode.SWC1 && target.LastOp == MIPS.Opcode.LW && target.Value == (long)ObjectField.Transform && value.LastOp == MIPS.Opcode.LWC1)
            {
                if (offset == (long)ObjectField.Yaw && value.Value == (long)ObjectField.Yaw)
                    Data.Yaw = Direction.Forward;
            }
        }

        protected override void Finish()
        {
            if (!FoundSpawn)
                return;

            if (Data.ID > 0x80000000)
                Data.ID = DataMap.Deref(Data.ID);

            for (int i = 0; i < 3; i++)
            {
                float value = Data.Scale[i];
                if (value == 1)
                    continue;
                if (value > 0x80000000)
                    value = DataMap.Deref((long)value);
                Data.Scale[i] = MathHelper.BitsAsFloat32((long)value);
            }
        }
    }
}
