using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using VirtualPhenix.Nintendo64;

public static class CRGUtils 
{
    public static Vector3 GetVec3(VP_DataView view, long offs)
    {
        return new Vector3(
            view.GetFloat32(offs + 0x00),
            view.GetFloat32(offs + 0x04),
            view.GetFloat32(offs + 0x08)
        );
    }

    public static Vector4 GetColor(VP_DataView view, long offs)
    {
        return new Vector4(
            view.GetUint8(offs + 0x00) / 255f,
            view.GetUint8(offs + 0x01) / 255f,
            view.GetUint8(offs + 0x02) / 255f,
            view.GetUint8(offs + 0x03) / 255f
        );
    }

}
