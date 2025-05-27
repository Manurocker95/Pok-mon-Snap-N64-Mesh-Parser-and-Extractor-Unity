using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.RDP
{
    public class TileState
    {
        public long cacheKey = 0;
        public long fmt = 0;
        public long siz = 0;
        public long line = 0;
        public long tmem = 0;
        public long palette = 0;
        public long cmt = 0;
        public long maskt = 0;
        public long shiftt = 0;
        public long cms = 0;
        public long masks = 0;
        public long shifts = 0;
        public long uls = 0;
        public long ult = 0;
        public long lrs = 0;
        public long lrt = 0;

        public void Set(long fmt, long siz, long line, long tmem, long palette, long cmt, long maskt, long shiftt, long cms, long masks, long shifts)
        {
            this.fmt = fmt;
            this.siz = siz;
            this.line = line;
            this.tmem = tmem;
            this.palette = palette;
            this.cmt = cmt;
            this.maskt = maskt;
            this.shiftt = shiftt;
            this.cms = cms;
            this.masks = masks;
            this.shifts = shifts;
        }

        public void SetSize(long uls, long ult, long lrs, long lrt)
        {
            this.uls = uls;
            this.ult = ult;
            this.lrs = lrs;
            this.lrt = lrt;
        }

        public void Copy(TileState o)
        {
            Set(o.fmt, o.siz, o.line, o.tmem, o.palette, o.cmt, o.maskt, o.shiftt, o.cms, o.masks, o.shifts);
            SetSize(o.uls, o.ult, o.lrs, o.lrt);
            this.cacheKey = o.cacheKey;
        }
    }
}
