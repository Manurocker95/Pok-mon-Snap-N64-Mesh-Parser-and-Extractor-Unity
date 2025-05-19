using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class ParticleUtils
    {
        public static CustomParticleSystem ParseParticles(VP_ArrayBufferSlice data, bool isCommon)
        {
            try
            {
                var emitters = new List<EmitterData>();
                var view = data.CreateDefaultDataView();

                long particleStart = 0;
                var textureFlags = new List<List<long>>();

                int emitterCount = view.GetInt32(0);
                for (int i = 0; i < emitterCount; i++)
                {
                    int offs = view.GetInt32(4 * (i + 1));
                    Debug.Assert(view.GetInt16(offs + 0x00) == 0);

                    int particleIndex = view.GetInt16(offs + 0x02);
                    int lifetime = view.GetInt16(offs + 0x04);
                    int particleLifetime = view.GetInt16(offs + 0x06);
                    long flags = view.GetInt32(offs + 0x08);
                    float g = view.GetFloat32(offs + 0x0C);
                    float drag = view.GetFloat32(offs + 0x10);
                    Vector3 velocity = CRGUtils.GetVec3(view, offs + 0x14);
                    float radius = view.GetFloat32(offs + 0x20);
                    float sprayAngle = view.GetFloat32(offs + 0x24);
                    float increment = view.GetFloat32(offs + 0x28);
                    float size = view.GetFloat32(offs + 0x2C);

                    var texFlags = particleIndex < textureFlags.Count && textureFlags[particleIndex] != null
                        ? textureFlags[particleIndex]
                        : new long[256].ToList();
                    long currFlags = flags;

                    void SetFlags(int tex)
                    {
                        long old = texFlags[tex];
                        if (old != 0)
                            Debug.Assert((old & 0x70) == (currFlags & 0x70), "flag mismatch");
                        texFlags[tex] = currFlags;
                    }

                    var program = new List<ParticleCommand>();
                    offs += 0x30;


                    while (true)
                    {
                        var command = view.GetUint8(offs++);
                        if (command < 0x80)
                        {
                            long frames = command & 0x1F;
                            if ((command & 0x20) != 0)
                                frames = (frames << 8) + view.GetUint8(offs++);
                            long texIndex = (command & 0x40) != 0 ? view.GetUint8(offs++) : -1;
                            program.Add(new WaitCommand()
                            {
                                Kind = CommandKind.Wait,
                                Frames = frames,
                                TexIndex = texIndex,
                            });
                            if (texIndex >= 0)
                                SetFlags((int)texIndex);
                        }
                        else if (command < 0xA0)
                        {
                            var values = Vector3.zero;
                            flags = command & 0x1F;
                            for (int j = 0; j < 3; j++)
                                if ((flags & (1 << j)) != 0)
                                {
                                    values[j] = view.GetFloat32(offs);
                                    offs += 4;
                                }
                            program.Add(new PhysicsCommand()
                            {
                                Kind = CommandKind.Physics,
                                Flags = flags,
                                Values = values,
                            });
                        }
                        else if (command < 0xC0)
                        {
                            long subtype = command - 0xA0;
                            var values = new List<long>();
                            var misc = new MiscCommand
                            {
                                Kind = CommandKind.Misc,
                                Subtype = subtype,
                                Values = values,
                            };
                            switch (subtype)
                            {
                                case 0x00:
                                case 0x0C:
                                    {
                                        long frames = view.GetUint8(offs++);
                                        if ((frames & 0x80) != 0)
                                            frames = ((frames & 0x7F) << 8) + view.GetUint8(offs++);
                                        values.Add(frames + 1);
                                        values.Add(BitConverter.ToInt64(BitConverter.GetBytes(view.GetFloat32(offs)), 0));
                                        offs += 4;
                                        if (subtype == 0x0C)
                                        {
                                            values.Add((long)view.GetFloat32(offs));
                                            offs += 4;
                                        }
                                    }
                                    break;
                                case 0x1C:
                                    values.Add(view.GetUint8(offs++));
                                    goto case 0x01;
                                case 0x01:
                                case 0x07:
                                case 0x17:
                                case 0x18:
                                case 0x1F:
                                    values.Add(view.GetUint8(offs++)); break;
                                case 0x02:
                                case 0x03:
                                case 0x09:
                                case 0x0B:
                                case 0x1D:
                                    {
                                        values.Add((long)view.GetFloat32(offs));
                                        offs += 4;
                                        if (subtype == 0x1D)
                                        {
                                            values.Add((long)view.GetFloat32(offs));
                                            offs += 4;
                                        }
                                    }
                                    break;
                                case 0x06:
                                case 0x0A:
                                    values.Add(view.GetUint16(offs));
                                    offs += 2;
                                    goto case 0x04;
                                case 0x04:
                                case 0x05:
                                case 0x19:
                                    {
                                        values.Add(view.GetUint16(offs));
                                        offs += 2;
                                    }
                                    break;
                                case 0x08:
                                case 0x1E:
                                    {
                                        misc.Vector = CRGUtils.GetVec3(view, offs);
                                        offs += 0xC;
                                    }
                                    break;
                                case 0x1A:
                                case 0x1B:
                                    {
                                        misc.Color = CRGUtils.GetColor(view, offs);
                                        offs += 0x4;
                                    }
                                    break;
                            }
                            program.Add(misc);
                            if (subtype == 1)
                                currFlags = values[0];
                            else if (subtype == 0x0E)
                            {
                                var p = (ParticleFlags)currFlags;
                                p &= ~(ParticleFlags.MirrorS | ParticleFlags.MirrorT);
                                currFlags = (long)p;
                            }
                            else if (subtype == 0x0F)
                            {
                                var p = (ParticleFlags)currFlags;
                                p |= ParticleFlags.MirrorS;
                                p &= ~ParticleFlags.MirrorT;
                                currFlags = (long)p;
                            }
                            else if (subtype == 0x10)
                            {
                                var p = (ParticleFlags)currFlags;
                                p |= ParticleFlags.MirrorT;
                                p &= ~ParticleFlags.MirrorS;
                                currFlags = (long)p;
                            }
                            else if (subtype == 0x11)
                            {
                                var p = (ParticleFlags)currFlags;
                                p |= ParticleFlags.MirrorS | ParticleFlags.MirrorT;
                                currFlags = (long)p;
                            }
                            else if (subtype == 0x1C)
                                for (long tex = values[0]; tex < values[0] + values[1]; tex++)
                                    SetFlags((int)tex);
                        }
                        else if (command < 0xE0)
                        {
                            long frames = view.GetUint8(offs++);
                            if ((frames & 0x80) != 0)
                                frames = ((frames & 0x7F) << 8) + view.GetUint8(offs++);
                            frames++;
                            var color = Vector4.zero;
                            for (int j = 0; j < 4; j++)
                                if ((command & (1 << (int)j)) != 0)
                                    color[j] = view.GetUint8(offs++) / 0xFF;
                            program.Add(new ColorCommand()
                            {
                                Kind = CommandKind.Color,
                                Flags = command & 0x1F,
                                Frames = frames,
                                Color = color,
                            });
                        }
                        else
                        {
                            //Assert(command >= 0xFA, $"bad command {HexZero(command, 2)}");
                            if (command > 0xFD)
                                break;
                            long count = -1;
                            if (command == 0xFA)
                                count = view.GetUint8(offs++);
                            else if (command == 0xFB)
                                count = 0;
                            program.Add(new LoopCommand()
                            {
                                Kind = CommandKind.Loop,
                                IsEnd = (command & 1) != 0,
                                Count = count,
                            });
                        }
                    }

                    // Fin del while

                    var em = new EmitterData()
                    {
                        IsCommon = isCommon,
                        Index = i,

                        ParticleIndex = particleIndex,
                        Lifetime = lifetime,
                        ParticleLifetime = particleLifetime,
                        Flags = flags,
                        G = g,
                        Drag = drag,
                        Velocity = velocity,
                        Radius = radius,
                        Size = size,
                        SprayAngle = sprayAngle,
                        Increment = increment,
                        Program = program,
                    };
                    emitters.Add(em);

                    particleStart = VP_BYML.Align(offs, 16);
                    textureFlags[particleIndex] = texFlags;
                }

                var tile = new RDP.TileState();
                var particleTextures = new List<List<RDP.Texture>>();
                long particleCount = view.GetInt32(particleStart);
                for (int i = 0; i < particleCount; i++)
                {
                    long offs = view.GetInt32(particleStart + 4 * (i + 1)) + particleStart;
                    long count = view.GetInt32(offs + 0x00);
                    long fmt = view.GetInt32(offs + 0x04);
                    long siz = view.GetInt32(offs + 0x08);
                    long width = view.GetInt32(offs + 0x0C);
                    long height = view.GetInt32(offs + 0x10);
                    bool sharedPalette = view.GetInt32(offs + 0x14) != 0;
                    var flagList = textureFlags[i] ?? new List<long>();

                    var textures = new List<RDP.Texture>();

                    tile.fmt = fmt;
                    tile.siz = siz;
                    tile.lrs = 4 * (width - 1);
                    tile.lrt = 4 * (height - 1);
                    tile.cms = (long)TexCM.CLAMP;
                    tile.cmt = (long)TexCM.CLAMP;

                    offs += 0x18;
                    for (int j = 0; j < count; j++)
                    {
                        if (j >= flagList.Count)
                        {
                            UnityEngine.Debug.LogWarning("unused particle: " + i + "," + j);
                        }
                        else
                        {
                            var p = ((ParticleFlags)flagList[j] & (ParticleFlags.MirrorS | ParticleFlags.MirrorT));
                            VP_BYMLUtils.Assert((long)p == 0);
                        }


                        long palette = particleStart + view.GetInt32(offs + 4 * (sharedPalette ? count : j + count));
                        textures.Add(RDP.TextureCacheUtils.TranslateTileTexture(
                            new[] { data },
                            particleStart + view.GetInt32(offs + 4 * j),
                            palette,
                            tile
                        ));
                        textures[textures.Count - 1].name = $"particle_{i}_{j}";
                    }

                    particleTextures.Add(textures);
                }
                return new CustomParticleSystem { Emitters = emitters, ParticleTextures = particleTextures };
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing PARTICLE DATA (" + (isCommon ? "PESTER" :"OTHER")+")");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new CustomParticleSystem();
            }            
        }
    }
}
