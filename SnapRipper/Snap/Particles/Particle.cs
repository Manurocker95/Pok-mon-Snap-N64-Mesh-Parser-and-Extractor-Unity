using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Particle
    {
        public List<TextureMapping> MappingScratch = new List<TextureMapping>() { new TextureMapping() };
        public EmitterData Data;
        public Matrix4x4 ModelMatrix = Matrix4x4.identity;
        public Matrix4x4 ParticleMtx = Matrix4x4.identity;
        public List<TextureData> Textures;
        public double Timer = -1;
        public long Flags = 0;

        private DeviceProgram Program;
        private GfxProgram GfxProgram = null;
        private bool TexturesEnabled = true;
        private bool AlphaVisualizerEnabled = false;

        private Vector3 Position = Vector3.zero;
        private Vector3? Velocity = Vector3.zero;
        private double G = 0;
        private double Drag = 1;

        private int TexIndex = 0;
        private double WaitTimer = 0;
        private double Size = 1;
        private double SizeGoal = 1;
        private double SizeTimer = 1;

        private Vector4 Prim = Vector4.one;
        private Vector4 PrimGoal = Vector4.one;
        private double PrimTimer = 0;

        private Vector4 Env = Vector4.zero;
        private Vector4 EnvGoal = Vector4.zero;
        private double EnvTimer = 0;

        private int InstrIndex = 0;
        private int LoopStart = 0;
        private int LoopCount = 0;
        private int MarkIndex = 0;

        public void Activate(EmitterData data, List<TextureData> textures, Vector3 pos, Vector3? vel)
        {
            this.Data = data;
            this.Textures = textures;
            this.Timer = data.ParticleLifetime;
            this.Flags = data.Flags;

            this.Position = pos;

            this.Velocity = vel;

            this.G = data.G;
            this.Drag = data.Drag;

            this.TexIndex = 0;
            this.WaitTimer = data.Program != null ? 0 : -1;
            this.Size = data.Size;
            this.SizeTimer = 0;

            this.Prim = new Vector4(1, 1, 1, 1);
            this.PrimTimer = 0;

            this.Env = new Vector4(0, 0, 0, 0);
            this.EnvTimer = 0;

            this.InstrIndex = 0;
            this.MarkIndex = 0;
            this.LoopCount = 0;

            this.CreateProgram();
        }
        public void Update(double dt, ParticleManager manager)
        {
            if ((this.Flags & (long)ParticleFlags.NoUpdate) != 0)
                return;

            long oldFlags = this.Flags;

            if (this.WaitTimer >= 0)
            {
                this.WaitTimer -= dt;

                while (this.WaitTimer < 0 && this.InstrIndex < this.Data.Program.Count)
                {
                    var instr = this.Data.Program[this.InstrIndex++];

                    switch (instr.Kind)
                    {
                        case CommandKind.Wait:
                        WaitCommand wc = (WaitCommand)instr;
                            this.WaitTimer = wc.Frames;
                            if (wc.TexIndex >= 0)
                                this.TexIndex = (int)wc.TexIndex;
                            break;

                        case CommandKind.Loop:
                        LoopCommand lc = (LoopCommand)instr;
                            if (lc.Count < 0)
                            {
                                if (lc.IsEnd)
                                    this.InstrIndex = this.MarkIndex;
                                else
                                    this.MarkIndex = this.InstrIndex;
                            }
                            else
                            {
                                if (lc.IsEnd)
                                {
                                    if (this.LoopCount-- > 0)
                                        this.InstrIndex = this.LoopStart;
                                }
                                else
                                {
                                    this.LoopCount = (int)lc.Count;
                                    this.LoopStart = this.InstrIndex;
                                }
                            }
                            break;

                        case CommandKind.Physics:
                            PhysicsCommand pc = (PhysicsCommand)instr;
                            var vec = (pc.Flags & (long)InstrFlags.UseVel) != 0 ? this.Velocity : this.Position;
                            for (int i = 0; i < 3; i++)
                            {
                                if ((pc.Flags & (1 << i)) != 0)
                                {
                                    vec = SnapUtils.SetValueInVector3(vec.Value, i, (float)(pc.Values[i] + ((pc.Flags & (long)InstrFlags.IncVec) != 0 ? vec.Value[i] : 0)));
                                }
                          
                            }
                            break;

                        case CommandKind.Color:
                        ColorCommand pcc = (ColorCommand)instr;
                            var color = (pcc.Flags & (long)InstrFlags.SetEnv) != 0 ? this.Env : this.Prim;
                            var goal = (pcc.Flags & (long)InstrFlags.SetEnv) != 0 ? this.EnvGoal : this.PrimGoal;
                            goal = color;
                            for (int i = 0; i < 4; i++)
                                if ((pcc.Flags & (1 << i)) != 0)
                                    goal[i] = pcc.Color[i];

                            double frames = pcc.Frames;
                            if (pcc.Frames == 1)
                            {
                                frames = 0;
                                color = goal;
                            }

                            if ((pcc.Flags & (long)InstrFlags.SetEnv) != 0)
                                this.EnvTimer = frames;
                            else
                                this.PrimTimer = frames;
                            break;

                        case CommandKind.Misc:
                            MiscCommand pm = (MiscCommand)instr;
                            switch (pm.Subtype)
                            {
                                case 0x00:
                                    this.SizeTimer = pm.Values[0];
                                    this.SizeGoal = pm.Values[1];
                                    if (this.SizeTimer == 1)
                                    {
                                        this.SizeTimer = 0;
                                        this.Size = this.SizeGoal;
                                    }
                                    break;

                                case 0x01:
                                    this.Flags = (long)pm.Values[0];
                                    break;

                                case 0x02:
                                    this.G = pm.Values[0];
                                    if ((this.G != 0) != ((this.Flags & (long)ParticleFlags.Gravity) != 0))
                                        this.Flags ^= (long)ParticleFlags.Gravity;
                                    break;

                                case 0x03:
                                    this.Drag = pm.Values[0];
                                    if ((this.Drag != 1) != ((this.Flags & (long)ParticleFlags.Drag) != 0))
                                        this.Flags ^= (long)ParticleFlags.Drag;
                                    break;

                                case 0x04:
                                    {
                                        var p = manager.CreateParticle(this.Data.IsCommon, (int)pm.Values[0], this.Position);
                                        p?.Update(0, manager);
                                    }
                                    break;

                                case 0x05:
                                    {
                                        var e = manager.CreateEmitter(this.Data.IsCommon, (int)pm.Values[0], null);
                                        if (e != null)
                                            e.Position = this.Position;
                                    }
                                    break;

                                case 0x06:
                                    this.Timer = pm.Values[0] + UnityEngine.Random.value * pm.Values[1];
                                    break;

                                case 0x07:
                                    if (UnityEngine.Random.value < pm.Values[0] / 100.0)
                                        this.Timer = 0;
                                    break;

                                case 0x08:
                                    this.Position.x += UnityEngine.Random.value * pm.Vector[0];
                                    this.Position.y += UnityEngine.Random.value * pm.Vector[1];
                                    this.Position.z += UnityEngine.Random.value * pm.Vector[2];
                                    break;

                                case 0x09:
                                    {
                                        Matrix4x4 mtx = Matrix4x4.identity;
                                        MathHelper.TargetTo(ref mtx, Vector3.zero, this.Velocity.Value, Vector3.right);
                                        float speed = this.Velocity.Value.magnitude;
                                        float angle = (float)(MathConstants.Tau * UnityEngine.Random.value);
                                        this.Velocity = new Vector3(
                                            Mathf.Sin((float)pm.Values[0]) * Mathf.Cos(angle),
                                            Mathf.Sin((float)pm.Values[0]) * Mathf.Sin(angle),
                                            -Mathf.Cos((float)pm.Values[0])
                                        );
                                        MathHelper.TransformVec3Mat4w0(ref this.Velocity, mtx, this.Velocity);
                                        this.Velocity *= speed;
                                    }
                                    break;

                                case 0x0A:
                                    {
                                        int index = (int)(pm.Values[0] + UnityEngine.Random.Range(0, (int)pm.Values[1]));
                                        var p = manager.CreateParticle(this.Data.IsCommon, index, this.Position);
                                        p?.Update(0, manager);
                                    }
                                    break;

                                case 0x0B:
                                    this.Velocity *= pm.Values[0];
                                    break;

                                case 0x0C:
                                    this.SizeTimer = pm.Values[0];
                                    this.SizeGoal = pm.Values[1] + UnityEngine.Random.value * pm.Values[2];
                                    if (this.SizeTimer == 1)
                                    {
                                        this.SizeTimer = 0;
                                        this.Size = this.SizeGoal;
                                    }
                                    break;

                                case 0x0D:
                                    this.Flags |= (long)ParticleFlags.TexAsLerp;
                                    break;

                                case 0x0E:
                                case 0x0F:
                                case 0x10:
                                case 0x11:
                                    break;

                                case 0x12:
                                    this.Flags |= (long)ParticleFlags.CustomAlphaMask;
                                    break;

                                case 0x13:
                                    this.Flags &= ~(long)ParticleFlags.DitherAlpha;
                                    break;

                                case 0x14:
                                    this.Flags |= (long)ParticleFlags.DitherAlpha;
                                    break;

                                case 0x15:
                                    this.Flags |= (long)ParticleFlags.UseRawTex;
                                    break;

                                case 0x16:
                                    this.Flags &= ~(long)ParticleFlags.UseRawTex;
                                    break;

                                case 0x17:
                                case 0x18:
                                    {
                                        Vector3 delta = manager.RefPositions[(int)pm.Values[0] - 1] - this.Position;
                                        float norm = 0;
                                        if (pm.Subtype == 0x17)
                                        {
                                            norm = this.Velocity.Value.magnitude;
                                            this.Velocity = Vector3.zero;
                                        }
                                        else
                                        {
                                            norm = pm.Values[1];
                                        }
                                        MathHelper.NormToLengthAndAdd(ref this.Velocity, delta, norm);
                                    }
                                    break;

                                case 0x19:
                                    {
                                        var p = manager.CreateParticle(this.Data.IsCommon, (int)pm.Values[0], this.Position, this.Velocity);
                                        p?.Update(0, manager);
                                    }
                                    break;

                                case 0x1A:
                                case 0x1B:
                                    {
                                        Vector4 c = pm.Subtype == 0x1A ? this.PrimGoal : this.EnvGoal;
                                        for (int i = 0; i < 4; i++)
                                            c[i] = (c[i] + pm.Color[i] * UnityEngine.Random.value) % 1.0f;
                                        if (pm.Subtype == 0x1A)
                                        {
                                            if (this.PrimTimer <= 0)
                                                this.Prim = this.PrimGoal;
                                        }
                                        else
                                        {
                                            if (this.EnvTimer <= 0)
                                                this.Env = this.EnvGoal;
                                        }
                                    }
                                    break;

                                case 0x1C:
                                    this.TexIndex = (int)(pm.Values[0] + UnityEngine.Random.value * pm.Values[1]);
                                    break;

                                case 0x1D:
                                    MathHelper.NormToLength(ref this.Velocity, pm.Values[0] + UnityEngine.Random.value * pm.Values[1]);
                                    break;

                                case 0x1E:
                                    this.Velocity = Vector3.Scale(this.Velocity.Value, pm.Vector);
                                    break;

                                case 0x1F:
                                    this.Flags &= ~(long)ParticleFlags.PosIndex;
                                    this.Flags |= (long)ParticleFlags.StorePosition | ((long)(pm.Values[0] - 1) << 12);
                                    break;
                            }
                            break;
                    }
                }
            }

            if (this.SizeTimer > 0)
            {
                this.Size = MathHelper.Lerp(this.Size, this.SizeGoal, MathHelper.Clamp(dt / this.SizeTimer, 0, 1));
                this.SizeTimer -= dt;
            }

            if (this.PrimTimer > 0)
            {
                this.Prim = Vector4.Lerp(this.Prim, this.PrimGoal, (float)MathHelper.Clamp(dt / this.PrimTimer, 0, 1));
                this.PrimTimer -= dt;
            }

            if (this.EnvTimer > 0)
            {
                this.Env = Vector4.Lerp(this.Env, this.EnvGoal, (float)MathHelper.Clamp(dt / this.EnvTimer, 0, 1));
                this.EnvTimer -= dt;
            }

            this.Timer -= dt;

            if ((this.Flags & (long)ParticleFlags.Orbit) != 0)
            {
                Debug.LogWarning("orbit motion unimplemented");
            }
            else
            {
                if ((this.Flags & (long)ParticleFlags.Gravity) != 0)
                {
                    var vy = this.Velocity.Value;
                    vy.y -= (float)(dt * this.G);
                    this.Velocity = vy;
                }

                if ((this.Flags & (long)ParticleFlags.Drag) != 0)
                {
                    this.Velocity *= (float)System.Math.Pow(this.Drag, dt);
                }

                this.Position += this.Velocity.Value * (float)dt;
            }

            if ((this.Flags & (long)ParticleFlags.StorePosition) != 0)
            {
                int index = (int)((this.Flags >> 12) & 7);
                manager.RefPositions[index] = this.Position;
            }

            if (this.Flags != oldFlags)
                this.CreateProgram();

            this.ModelMatrix[0] = (float)this.Size;
            this.ModelMatrix[5] = (float)this.Size;
            this.ModelMatrix[12] = this.Position.x;
            this.ModelMatrix[13] = this.Position.y;
            this.ModelMatrix[14] = this.Position.z;
        }

        public void SetTexturesEnabled(bool v)
        {
            this.TexturesEnabled = v;
            this.CreateProgram();
        }

        public void SetAlphaVisualizerEnabled(bool v)
        {
            this.AlphaVisualizerEnabled = v;
            this.CreateProgram();
        }

        private void CreateProgram()
        {
            var program = new ParticleProgram();

            if (this.TexturesEnabled)
                program.Defines["USE_TEXTURE"] = "1";
            if (this.AlphaVisualizerEnabled)
                program.Defines["USE_ALPHA_VISUALIZER"] = "1";

            if ((this.Flags & (long)ParticleFlags.TexAsLerp) != 0)
                program.Defines["TEX_LERP"] = "1";
            if ((this.Flags & (long)ParticleFlags.UseRawTex) != 0)
                program.Defines["RAW_TEX"] = "1";
            if ((this.Flags & (long)ParticleFlags.CustomAlphaMask) != 0)
                program.Defines["CUSTOM_MASK"] = "1";

            this.Program = program;
            this.GfxProgram = null;
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
        public void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput)
        {
            if (this.GfxProgram == null)
                this.GfxProgram = renderInstManager.GfxRenderCache.CreateProgram(this.Program);

            var renderInst = renderInstManager.NewRenderInst();
            renderInst.SetGfxProgram(this.GfxProgram);
            renderInst.SortKey = GfxRenderInstUtils.MakeSortKey(GfxRendererLayer.TRANSLUCENT);

            MappingScratch[0].GfxSampler = this.Textures[this.TexIndex].Sampler;
            MappingScratch[0].GfxTexture = this.Textures[this.TexIndex].Texture;
            renderInst.SetSamplerBindingsFromTextureMappings(GetSamplerBindingsFromTextureMappings(MappingScratch));
            renderInst.SetDrawCount(6);

            long offs = renderInst.AllocateUniformBuffer((int)ParticleProgram.UbDrawParams,(long) (12 + 4 * 2));
            var draw = renderInst.MapUniformBufferF32(ParticleProgram.UbDrawParams);

            ParticleMtx = (viewerInput.Camera.worldToCameraMatrix * this.ModelMatrix);
            MathHelper.CalcBillboardMatrix(ref ParticleMtx, ref ParticleMtx, CalcBillboardFlags.UseRollLocal | CalcBillboardFlags.PriorityZ | CalcBillboardFlags.UseZPlane);
            offs += GfxBufferHelpers.FillMatrix4x3(draw, offs, ParticleMtx);

            offs += GfxBufferHelpers.FillVec4v(draw, offs, this.Prim);
            offs += GfxBufferHelpers.FillVec4v(draw, offs, this.Env);
            renderInstManager.SubmitRenderInst(renderInst);
        }

    }

}
