using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Lapras : Actor
    {
        private static int Flags = 0;

        public Lapras(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult StateOverride(long addr, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            int mask = 0;
            switch (addr)
            {
                case 0x802C816C: mask = 1; break;
                case 0x802C81C4: mask = 2; break;
                case 0x802C821C: mask = 4; break;
            }

            if ((Lapras.Flags & mask) != 0 || mask == 0)
                return MotionResult.None;
            else
                return MotionResult.Done;
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == SnapUtils.FakeAux)
            {
                Lapras.Flags |= 1 << (int)this.Spawn.Behaviour;
                if (this.Spawn.Behaviour == 2)
                    Lapras.Flags |= 2;
                return MotionResult.Done;
            }

            return MotionResult.None;
        }
    }

}
