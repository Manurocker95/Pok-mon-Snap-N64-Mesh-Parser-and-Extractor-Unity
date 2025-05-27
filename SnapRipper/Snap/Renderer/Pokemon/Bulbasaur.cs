using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Bulbasaur : Actor
    {
        public Bulbasaur(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            switch (param)
            {
                case 1:
                    this.Translation = new Vector3(
                        this.Translation.x,
                        (float)(SnapUtils.GroundHeightAt(globals, this.Translation) - 80),
                        this.Translation.z
                    );
                    return MotionResult.Update;

                default:
                    return base.CustomMotion(param, viewerInput, globals);
            }
        }
    }
}
