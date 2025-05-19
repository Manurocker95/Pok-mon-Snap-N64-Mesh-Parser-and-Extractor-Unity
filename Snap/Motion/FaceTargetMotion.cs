using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class FaceTargetMotion : Motion
    {
        public override MotionKind Kind => MotionKind.faceTarget;

        public float MaxTurn;
    }
}
