using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    /// <summary>
    /// IInterpreter for N64 F3DEX microcode.
    /// </summary>
    public class TextureState
    {
        public bool on = false;
        public long tile = 0;
        public long level = 0;
        public double s = 1.0f;
        public double t = 1.0f;
        public double ss = 1.0f;
        public double tt = 1.0f;

        public void Set(bool on, long tile, long level, double s, double t)
        {
            this.on = on;
            this.tile = tile;
            this.level = level;
            this.s = s;
            this.t = t;
        }

        public void Set(bool on, long tile, long level, double s, double t, double ss, double tt)
        {
            this.on = on;
            this.tile = tile;
            this.level = level;
            this.s = s;
            this.t = t;
            this.ss = ss;
            this.tt = tt;
        }

        public void Copy(TextureState other)
        {
            Set(other.on, other.tile, other.level, other.s, other.t, other.ss, other.tt);

        }
    }
}