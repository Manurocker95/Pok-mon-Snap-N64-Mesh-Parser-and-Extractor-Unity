using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie 
{
    public class RSPOutput
    {
        public List<DrawCall> DrawCalls = new List<DrawCall>();
        public DrawCall CurrentDrawCall = new DrawCall();

        public virtual DrawCall NewDrawCall(int firstIndex)
        {
            DrawCalls = new List<BanjoKazooie.DrawCall>();
            CurrentDrawCall = new BanjoKazooie.DrawCall() { FirstIndex = firstIndex };
            DrawCalls.Add(CurrentDrawCall);
            return CurrentDrawCall;
        }
  
        public RSPOutput()
        {
            if (CurrentDrawCall == null)
                CurrentDrawCall = new DrawCall();

            if (DrawCalls == null)
                DrawCalls = new List<DrawCall>();
        }
    }

}