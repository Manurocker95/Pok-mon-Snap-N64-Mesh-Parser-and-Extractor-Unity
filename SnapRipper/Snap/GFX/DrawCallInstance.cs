using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class DrawCallInstance
    {
        public Matrix4x4 viewMatrixScratch;
        public Matrix4x4 modelViewScratch;
        public Matrix4x4 texMatrixScratch;
        public Vector4 colorScratch;

        public bool Visible = true;

        private List<RDP.Texture> textureEntry = new List<RDP.Texture>();
        private bool vertexColorsEnabled = true;
        private bool texturesEnabled = true;
        private bool monochromeVertexColorsEnabled = false;
        private bool alphaVisualizerEnabled = false;
        private VP_Partial<GfxMegaStateDescriptor> MegaStateFlags;
        private DeviceProgram Program;
        private GfxProgram GfxProgram = null;
        // private List<TextureMapping> textureMappings = GfxPlatformUtils.NArray(2, () => new TextureMapping()); 
        private List<TextureMapping> textureMappings = new List<TextureMapping>() 
        {
            new TextureMapping(),
            new TextureMapping()
        };
        private CRGMaterial Material = null;

        private DrawCall drawCall;
        private List<Matrix4x4> drawMatrices;
        private long billboard;

        public CRGMaterial MaterialInfo
        {
            get { return Material; }
        }

        public DrawCall DrawCallInfo
        {
            get { return drawCall; }
        }

        public List<Matrix4x4> DrawMatrices
        {
            get { return drawMatrices; }
        }

        public List<RDP.Texture> TextureEntry
        {
            get { return textureEntry; }
        }

        public DrawCallInstance(RenderData geometryData, BanjoKazooie.DrawCall drawCall, List<Matrix4x4> drawMatrices, long billboard, List<CRGMaterial> materials = null)
        {
            this.drawCall = (PokemonSnap.DrawCall)drawCall;
            this.drawMatrices = drawMatrices;
            this.billboard = billboard;

            for (int i = 0; i < this.textureMappings.Count; i++)
            {
                if (i < this.drawCall.TextureIndices.Count)
                {
                    var idx = this.drawCall.TextureIndices[i];
                    this.textureEntry.Add(geometryData.SharedOutput.TextureCache.textures[idx]);
                    this.textureMappings[i].GfxTexture = geometryData.Textures[idx];
                    this.textureMappings[i].GfxSampler = geometryData.Samplers[idx];
                }
            }

            if ((int)this.drawCall.materialIndex >= 0)
                this.Material = GfxPlatformUtils.AssertExists(materials[(int)this.drawCall.materialIndex]);

            this.MegaStateFlags = new VP_Partial<GfxMegaStateDescriptor>(F3DEXUtils.TranslateBlendMode(this.drawCall.SP_GeometryMode, this.drawCall.DP_OtherModeL));
            this.CreateProgram();
        }

        protected virtual void CreateProgram()
        {
            var tiles = new List<RDP.TileState>();
            for (int i = 0; i < this.textureEntry.Count; i++)
                tiles.Add(this.textureEntry[i].tile);

            var program = this.ProgramConstructor(
                drawCall.DP_OtherModeH,
                drawCall.DP_OtherModeL,
                drawCall.DP_Combine,
                8.0 / 255.0,
                tiles
            );

            program.SetDefine("BONE_MATRIX_COUNT", this.drawMatrices.Count.ToString());

            if (this.texturesEnabled && this.drawCall.TextureIndices.Count > 0)
                program.SetDefine("USE_TEXTURE", "1");

            var shade = (this.drawCall.SP_GeometryMode & (long)RSP_Geometry.G_SHADE) != 0;
            if (this.vertexColorsEnabled && shade)
                program.SetDefine("USE_VERTEX_COLOR", "1");

            if ((this.drawCall.SP_GeometryMode & (long)RSP_Geometry.G_LIGHTING) != 0)
                program.SetDefine("LIGHTING", "1");

            if ((this.drawCall.SP_GeometryMode & (long)RSP_Geometry.G_TEXTURE_GEN) != 0)
                program.SetDefine("TEXTURE_GEN", "1");

            if ((this.drawCall.SP_GeometryMode & (long)RSP_Geometry.G_TEXTURE_GEN_LINEAR) != 0)
                program.SetDefine("TEXTURE_GEN_LINEAR", "1");

            if (this.monochromeVertexColorsEnabled)
                program.SetDefine("USE_MONOCHROME_VERTEX_COLOR", "1");

            if (this.alphaVisualizerEnabled)
                program.SetDefine("USE_ALPHA_VISUALIZER", "1");

            program.SetDefine("EXTRA_COMBINE", "1");

            this.Program = program;
            this.GfxProgram = null;
        }


        public virtual void setBackfaceCullingEnabled(bool v)
        {
            var cullMode = v ? GfxUtils.TranslateCullMode(this.drawCall.SP_GeometryMode) : GfxCullMode.None;
            this.MegaStateFlags.Value.CullMode = cullMode;
        }

        public virtual void setVertexColorsEnabled(bool v)
        {
            this.vertexColorsEnabled = v;
            this.CreateProgram();
        }

        public virtual void setTexturesEnabled(bool v)
        {
            this.texturesEnabled = v;
            this.CreateProgram();
        }

        public virtual void setMonochromeVertexColorsEnabled(bool v)
        {
            this.monochromeVertexColorsEnabled = v;
            this.CreateProgram();
        }

        public virtual void setAlphaVisualizerEnabled(bool v)
        {
            this.alphaVisualizerEnabled = v;
            this.CreateProgram();
        }

        protected virtual void ComputeTextureMatrix(ref Matrix4x4 m, int textureEntryIndex)
        {
            if (this.textureEntry[textureEntryIndex] != null)
            {
                var entry = this.textureEntry[textureEntryIndex];

                // Pass in 1 for texture scale, since we've already rescaled the vertex coordinates
                RSP.CalculateTextureMatrixFromRSPState(ref m, 1, 1, entry.width, entry.height, entry.tile.shifts, entry.tile.shiftt);

                if (this.Material != null && (this.Material.Data.Flags & MaterialFlags.Scale) != 0)
                {
                    m[0] *= (float)this.Material.XScale();
                    m[5] *= (float)this.Material.YScale();
                }

                // shift by 10.2 UL coords, rescaled by texture size
                float sOffset = -entry.tile.uls / 4.0f;
                float tOffset = -entry.tile.ult / 4.0f;

                var tileFlag = textureEntryIndex == 0 ? MaterialFlags.Tile0 : MaterialFlags.Tile1;
                if (this.Material != null && (this.Material.Data.Flags & tileFlag) != 0)
                {
                    sOffset = -(float)this.Material.GetXShift(textureEntryIndex);
                    tOffset = -(float)this.Material.GetYShift(textureEntryIndex);
                }

                m[12] += sOffset / entry.width;
                m[13] += tOffset / entry.height;
            }
            else
            {
                m = Matrix4x4.identity; 
            }
        }

        public virtual List<GfxSamplerBinding> GetSamplerBindingsFromTextureMappings(List<TextureMapping> tm)
        {
            List<GfxSamplerBinding> sb = new List<GfxSamplerBinding>();
            foreach (TextureMapping mapping in tm)
            {
                sb.Add(mapping);
            }

            return sb;
        }

        public virtual void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput, bool isSkybox)
        {
            if (!this.Visible)
                return;

            if (this.GfxProgram == null)
                this.GfxProgram = renderInstManager.GfxRenderCache.CreateProgram(this.Program);

            var renderInst = renderInstManager.NewRenderInst();
            renderInst.SetGfxProgram(this.GfxProgram);

            // TODO: figure out layers
            if ((this.drawCall.DP_OtherModeL & (1 << (int)OtherModeL_Layout.Z_UPD)) == 0)
                renderInst.SortKey = GfxRenderInstUtils.MakeSortKey(GfxRendererLayer.TRANSLUCENT);

            this.Material?.FillTextureMappings(this.textureMappings);
            renderInst.SetSamplerBindingsFromTextureMappings(GetSamplerBindingsFromTextureMappings(this.textureMappings));
            renderInst.SetMegaStateFlags(MegaStateFlags);
            renderInst.SetDrawCount(this.drawCall.IndexCount, this.drawCall.FirstIndex);

            long offs = renderInst.AllocateUniformBuffer((int)F3DEX_Program.Ub_DrawParams, 12 * this.drawMatrices.Count + 8 * 2);
            var mappedF32 = renderInst.MapUniformBufferF32((int)F3DEX_Program.Ub_DrawParams);

            if (isSkybox)
                CameraHelpers.ComputeViewMatrixSkybox(ref viewMatrixScratch, viewerInput.Camera);
            else
                CameraHelpers.ComputeViewMatrix(ref viewMatrixScratch, viewerInput.Camera);

            for (int i = 0; i < this.drawMatrices.Count; i++)
            {
                modelViewScratch = viewMatrixScratch * drawMatrices[i];
                if ((this.billboard & 8) != 0)
                    MathHelper.CalcBillboardMatrix(ref modelViewScratch, ref modelViewScratch, CalcBillboardFlags.UseRollLocal | CalcBillboardFlags.PriorityZ | CalcBillboardFlags.UseZPlane);
                else if ((this.billboard & 2) != 0)
                    MathHelper.CalcBillboardMatrix(ref modelViewScratch, ref modelViewScratch, CalcBillboardFlags.UseRollLocal | CalcBillboardFlags.PriorityY | CalcBillboardFlags.UseZPlane);
                offs += GfxBufferHelpers.FillMatrix4x3(mappedF32, offs, modelViewScratch);
            }

            this.ComputeTextureMatrix(ref texMatrixScratch, 0);
            offs += GfxBufferHelpers.FillMatrix4x2(mappedF32, offs, texMatrixScratch);

            this.ComputeTextureMatrix(ref texMatrixScratch, 1);
            offs += GfxBufferHelpers.FillMatrix4x2(mappedF32, offs, texMatrixScratch);

            offs = renderInst.AllocateUniformBuffer((int)F3DEX_Program.Ub_CombineParams, 3 * 4);
            var comb = renderInst.MapUniformBufferF32((int)F3DEX_Program.Ub_CombineParams);

            colorScratch = new Vector4(this.drawCall.DP_PrimColor.x, this.drawCall.DP_PrimColor.y, this.drawCall.DP_PrimColor.z, this.drawCall.DP_PrimColor.w);
            this.Material?.GetColor(ref colorScratch, ColorField.Prim);
            offs += GfxBufferHelpers.FillVec4v(comb, offs, colorScratch);

            colorScratch = new Vector4(this.drawCall.DP_EnvColor.x, this.drawCall.DP_EnvColor.y, this.drawCall.DP_EnvColor.z, this.drawCall.DP_EnvColor.w);
            this.Material?.GetColor(ref colorScratch, ColorField.Env);
            offs += GfxBufferHelpers.FillVec4v(comb, offs, colorScratch);

            this.FillExtraCombine(offs, comb);

            renderInstManager.SubmitRenderInst(renderInst);
        }

        protected virtual int FillExtraCombine(long offs, VP_Float32Array<VP_ArrayBuffer> comb)
        {
            float primLOD = this.drawCall.DP_PrimLOD;

            if (this.Material != null && (this.Material.Data.Flags & (MaterialFlags.PrimLOD | MaterialFlags.Special)) != 0)
                primLOD = (float)this.Material.GetPrimLOD();

            comb[offs] = primLOD;
            return 1;
        }

        protected virtual F3DEX_Program ProgramConstructor(long otherH, long otherL, RDP.CombineParams combine, double alpha, List<RDP.TileState> tiles)
        {
            return new F3DEX_Program(otherH, otherL, combine, alpha, tiles);
        }

    }

}
