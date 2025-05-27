using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Magikarp : Actor
    {
        private Actor gyarados = null;

        public Magikarp(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (this.CurrAux == 0x802D2128)
            {
                if (this.gyarados == null)
                {
                    this.gyarados = GfxPlatformUtils.AssertExists(globals.AllActors.FirstOrDefault(a => a.Def.ID == this.Def.ID + 1));
                }

                if (this.Translation.y > this.gyarados.Translation.y + 100)
                    return MotionResult.Done;
            }

            return MotionResult.None;
        }
    }

}
