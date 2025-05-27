using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class MiniCrater : Actor
    {
        private Actor lavaSplash;

        public MiniCrater(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            if (state.StartAddress == 0x802DD954)
            {
                if (this.MotionData.StoredValues[0] == 0 &&
                    this.Target != null &&
                    Vector3.Distance(this.Translation, this.Target.Translation) < 200)
                {
                    long spawnID = UnityEngine.Random.value < 0.2f ? 59 : 58;
                    globals.ActivateObject(spawnID, this.Translation, 0);
                    this.MotionData.StoredValues[0] = 1;
                }
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.MotionData.StoredValues[0] != 0)
            {
                this.lavaSplash.ReceiveSignal(this, 0x23, globals);
                return MotionResult.Done;
            }

            if (viewerInput.Time > this.MotionData.Start)
            {
                this.lavaSplash.ReceiveSignal(this, 0x22, globals);
                this.MotionData.Start += MathHelper.RandomRange(4, 10) * 1000;
            }

            return MotionResult.None;
        }

        protected override bool EndBlock(long address, LevelGlobals globals)
        {
            if (address == 0x802DD7F0)
            {
                this.lavaSplash = GfxPlatformUtils.AssertExists(this.LastSpawn);
            }

            return false;
        }
    }

}
