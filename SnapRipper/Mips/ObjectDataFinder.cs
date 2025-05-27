

namespace VirtualPhenix.Nintendo64.MIPS
{
    public class ObjectDataFinder : MIPS.NaiveInterpreter
    {
        public long SpawnFunc { get; private set; } = 0;
        public long DataAddress { get; private set; } = 0;
        public long GlobalRef { get; private set; } = 0;

        public override void Reset()
        {
            base.Reset();
            SpawnFunc = 0;
            DataAddress = 0;
            GlobalRef = 0;
        }

        protected override long HandleFunction(long func, MIPS.Register a0, MIPS.Register a1, MIPS.Register a2, MIPS.Register a3, Register[] stackArgs, MIPS.BranchInfo branch = null)
        {
            SpawnFunc = func;
            DataAddress = stackArgs[1].Value; // stack + 0x14
            return 0;
        }

        public override void HandleStore(MIPS.Opcode op, MIPS.Register value, MIPS.Register target, long offset)
        {
            if (op == MIPS.Opcode.SW && value.LastOp == MIPS.Opcode.JAL)
                GlobalRef = target.Value;
        }
    }

}

