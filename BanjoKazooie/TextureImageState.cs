using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class TextureImageState
    {
        public long fmt = 0;
        public long siz = 0;
        public long w = 0;
        public long addr = 0;

        public void Set(long fmt, long siz, long w, long addr)
        {
            this.fmt = fmt;
            this.siz = siz;
            this.w = w;
            this.addr = addr;
        }
    }
}