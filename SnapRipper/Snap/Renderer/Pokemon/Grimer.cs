using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Grimer : Actor
    {
        private static int Flags = 0;

        public Grimer(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals)
            : base(renderData, spawn, def, globals)
        {
            this.MaterialController = new AdjustableAnimationController(0);
        }

        protected override MotionResult StateOverride(long addr, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            int mask = 0;
            switch (addr)
            {
                case 0x802C0960: mask = 1; break;
                case 0x802C09C4: mask = 2; break;
                case 0x802C0A28: mask = 4; break;
                case 0x802C0A8C: mask = 8; break;
            }

            // the listed functions wait for the given bit to be set
            // if it is, let the state logic run, making the actor appear
            if ((Grimer.Flags & mask) != 0 || mask == 0)
                return MotionResult.None;
            else
                return MotionResult.Done;
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802C1018)
            {
                Grimer.Flags |= 1 << (int)(this.Spawn.Behaviour - 1);
                return MotionResult.Done;
            }
            else if (this.CurrAux == 0x802C0E28)
            {
                this.MotionData.StoredValues[1] -= viewerInput.DeltaTime;
                if (this.MotionData.StoredValues[1] <= 0)
                {
                    this.MotionData.StoredValues[0] = 0;
                    return MotionResult.Done;
                }
            }

            return MotionResult.None;
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            if (address == 0x802C0D34 && this.CurrBlock == 1)
            {
                this.MotionData.StoredValues[1] = 6000;
                this.MotionData.StoredValues[0]++;

                if (this.MotionData.StoredValues[0] >= 3)
                {
                    var block = this.Def.StateGraph.States[(int)this.CurrState].Blocks[(int)this.CurrBlock];
                    return this.FollowEdge(block.Edges[0], globals);
                }
            }

            return false;
        }

        public override void SetAnimation(int index)
        {
            base.SetAnimation(index);
            this.MaterialController?.Init((float)(this.Def.StateGraph.Animations[index].FPS * this.MotionData.StoredValues[0] / 2.0));
        }
    }

}
