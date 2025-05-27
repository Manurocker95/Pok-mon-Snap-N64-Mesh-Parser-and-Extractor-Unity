using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class RSPOutput : BanjoKazooie.RSPOutput
    {
        public RSPOutput() 
        {
            CurrentDrawCall = new PokemonSnap.DrawCall();

            if (DrawCalls == null)
                DrawCalls = new List<BanjoKazooie.DrawCall>();
        }

        public override BanjoKazooie.DrawCall NewDrawCall(int firstIndex)
        {
            if (DrawCalls == null)
                DrawCalls = new List<BanjoKazooie.DrawCall>();

            CurrentDrawCall = new PokemonSnap.DrawCall() { FirstIndex = firstIndex };
            DrawCalls.Add(CurrentDrawCall);
            return CurrentDrawCall;
        }
    }
}
