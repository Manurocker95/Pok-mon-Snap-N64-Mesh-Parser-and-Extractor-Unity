using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.RDP;

namespace VirtualPhenix.Nintendo64
{
    public class F3DEX_Program : DeviceProgram
    {
        public static long A_Position = 0;
        public static long A_Color = 1;
        public static long A_TexCoord = 2;

        public static long Ub_SceneParams = 0;
        public static long Ub_DrawParams = 1;
        public static long Ub_CombineParams = 2;

        private long DP_OtherModeH;
        private long DP_OtherModeL;
        private double BlendAlpha;
        private List<RDP.TileState> Tiles;
        private long G_MW_NUMLIGHT;

        public F3DEX_Program(long dp_OtherModeH, long dp_OtherModeL, CombineParams combParams, double blendAlpha = 0.5, List<RDP.TileState> tiles = null, long g_MW_NUMLIGHT = 0)
        {
            this.DP_OtherModeH = dp_OtherModeH;
            this.DP_OtherModeL = dp_OtherModeL;
            this.BlendAlpha = blendAlpha;
            this.Tiles = tiles != null ? tiles : new List<RDP.TileState>();
            this.G_MW_NUMLIGHT = g_MW_NUMLIGHT;

            if (RDPUtils.GetCycleTypeFromOtherModeH(dp_OtherModeH) == OtherModeH_CycleType.G_CYC_2CYCLE)
                SetDefine("TWO_CYCLE", "1");

            this.Frag = this.GenerateFrag(combParams);
        }

        public void SetDefine(string key, string value)
        {
            if (this.Defines == null)
                this.Defines = new Dictionary<string, string>();

            if (this.Defines.ContainsKey(key))
            {
                this.Defines[key] = value;
            }
            else
            {
                this.Defines.Add(key, value);
            }
        }

        private string GenerateLightingExpression()
        {
            string output = "vec4(";
            for (long i = 0; i < this.G_MW_NUMLIGHT; i++)
            {
                output += $"max(dot(t_Normal.xyz, u_DiffuseDirection[{i}].xyz), 0.0) * u_DiffuseColor[{i}].rgb + ";
            }
            output += "u_AmbientColor.rgb, a_Color.a)";
            return output;
        }

        private string GenerateClamp()
        {
            string output = "";
            for (int i = 0; i < this.Tiles.Count; i++)
            {
                var tile = this.Tiles[i];

                if ((tile.cms & 0x2) != 0)
                {
                    double coordRatio = (((tile.lrs - tile.uls) >> 2) + 1) / (double)TextureCacheUtils.GetTileWidth(tile);
                    string comp = i == 0 ? "x" : "z";
                    if (coordRatio > 1.0)
                        output += $"v_TexCoord.{comp} = clamp(v_TexCoord.{comp}, 0.0, {coordRatio:F1});\n";
                }

                if ((tile.cmt & 0x2) != 0)
                {
                    double coordRatio = (((tile.lrt - tile.ult) >> 2) + 1) / (double)TextureCacheUtils.GetTileHeight(tile);
                    string comp = i == 0 ? "y" : "w";
                    if (coordRatio > 1.0)
                        output += $"v_TexCoord.{comp} = clamp(v_TexCoord.{comp}, 0.0, {coordRatio:F1});\n";
                }
            }
            return output;
        }

        private string GenerateAlphaTest()
        {
            long alphaCompare = ((uint)this.DP_OtherModeL) & 0x03;
            long cvgXAlpha = (this.DP_OtherModeL >> (int)OtherModeL_Layout.CVG_X_ALPHA) & 0x01;
            double alphaThreshold = 0;

            if (alphaCompare == 0x01)
            {
                alphaThreshold = this.BlendAlpha;
            }
            else if (alphaCompare != 0x00)
            {
                alphaThreshold = 0.0125;
            }
            else if (cvgXAlpha != 0x00)
            {
                alphaThreshold = 0.125;
            }

            if (alphaThreshold > 0)
            {
                return $@"
    if (t_Color.a < {alphaThreshold})
        discard;
";
            }
            else
            {
                return string.Empty;
            }
        }

        private string GenerateFrag(CombineParams combParams)
        {
            var textFilt = RDPUtils.GetTextFiltFromOtherModeH(this.DP_OtherModeH);
            string texFiltStr = "";

            if (textFilt == TextFilt.G_TF_POINT)
                texFiltStr = "Point";
            else if (textFilt == TextFilt.G_TF_AVERAGE)
                texFiltStr = "Average";
            else if (textFilt == TextFilt.G_TF_BILERP)
                texFiltStr = "Bilerp";
            else
                texFiltStr ="whoops";

            return texFiltStr;
        }

    }
}