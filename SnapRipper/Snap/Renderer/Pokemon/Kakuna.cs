using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Kakuna : Actor
    {
        public Kakuna(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        public override void Reset(LevelGlobals globals)
        {
            base.Reset(globals);
            this.MotionData.StoredValues[0] = this.Translation.y;
            this.MotionData.StoredValues[1] = this.MotionData.GroundHeight + 25;
        }
    }

}
