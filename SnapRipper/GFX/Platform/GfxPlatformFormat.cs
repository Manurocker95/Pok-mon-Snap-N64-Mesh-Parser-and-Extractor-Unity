using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class GfxFormat 
    {
        public long Value;

        public GfxFormat(GfxFormatOrder order)
        {
            Value = GfxUtils.GetGfxFormat(order);
        }

        public GfxFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            Value = MakeFormat(type, comp, flags);
        }

        public uint MakeFormat(FormatTypeFlags type, FormatCompFlags comp, FormatFlags flags)
        {
            return GfxUtils.MakeFormat(type, comp, flags);
        }

        public FormatCompFlags GetFormatCompFlags()
        {
            return (FormatCompFlags)((Value >> 8) & 0xFF);
        }

        public FormatTypeFlags GetFormatTypeFlags()
        {
            return (FormatTypeFlags)((Value >> 16) & 0xFF);
        }

        public FormatFlags GetFormatFlags()
        {
            return (FormatFlags)(Value & 0xFF);
        }
    }
}
