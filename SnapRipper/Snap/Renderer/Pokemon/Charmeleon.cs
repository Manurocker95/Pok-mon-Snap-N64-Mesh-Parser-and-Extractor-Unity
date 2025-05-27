using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Charmeleon : Actor
    {
        public Charmeleon(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];

            switch (state.StartAddress)
            {
                case 0x802DC170:
                    this.MotionData.StoredValues[5] = 0.04;
                    break;

                case 0x802DC1F8:
                    this.MotionData.StoredValues[5] = 0.08;
                    break;

                case 0x802DC758:
                case 0x802DC7A8:
                    this.MotionData.Destination = this.Translation;
                    break;
            }

            base.StartBlock(globals);
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            this.MotionData.PathParam = this.MotionData.PathParam % 1;
            this.MotionData.StoredValues[4] = System.Math.Min(1, this.MotionData.PathParam + 0.3 * (1 + UnityEngine.Random.value));
            return MotionResult.Done;
        }
    }

}
