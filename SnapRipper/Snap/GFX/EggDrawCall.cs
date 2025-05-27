using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class EggDrawCall : DrawCallInstance
    {
        public float Separation = 0;

        public EggDrawCall(RenderData geometryData, BanjoKazooie.DrawCall drawCall, List<Matrix4x4> drawMatrices, long billboard, List<CRGMaterial> materials = null) : base(geometryData, drawCall, drawMatrices, billboard, materials)
        {

        }



        protected override int FillExtraCombine(long offs, VP_Float32Array<VP_ArrayBuffer> comb)
        {
            offs = base.FillExtraCombine(offs, comb);
            comb[offs] = Separation;
            return 1;
        }

        protected override F3DEX_Program ProgramConstructor(long otherH, long otherL, RDP.CombineParams combine, double alpha, List<RDP.TileState> tiles)
        {
            return new EggProgram(otherH, otherL, combine, alpha, tiles);
        }
    }
}
