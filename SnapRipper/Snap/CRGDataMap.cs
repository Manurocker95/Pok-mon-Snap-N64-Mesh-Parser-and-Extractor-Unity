using System;
using System.Collections.Generic;
using VirtualPhenix.Nintendo64;

public class CRGDataMap
{
    public long overlay = 0;
    public List<CRGDataRange> ranges;

    public CRGDataMap(List<CRGDataRange> ranges)
    {
        this.ranges = ranges;
    }

    public VP_DataView GetView(long addr)
    {
        CRGDataRange range = GetRange(addr);
        var offset = addr - range.Start;
        return range.Data.CreateDefaultDataView(offset);
    }

    public CRGDataRange GetRange(long addr, long overlay = 0)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            if ((ranges[i].Overlay != null && ranges[i].Overlay == 1) && (this.overlay == 1) && ranges[i].Overlay != this.overlay)
                continue;

            var offset = addr - ranges[i].Start;
            if (0 <= offset && offset < ranges[i].Data.ByteLength)
                return ranges[i];
        }

        throw new Exception("no matching range for " + VP_BYMLUtils.HexZero(addr, 8));
    }
   
    public long Deref(long addr)
    {
        return GetView(addr).GetUint32(0, false);
    }
}
