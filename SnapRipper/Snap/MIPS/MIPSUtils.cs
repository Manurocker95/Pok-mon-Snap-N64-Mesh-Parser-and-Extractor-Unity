using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class MIPSUtils 
    {
        public const int ColorFlagStart = 9;
        public static ApproachPointMotion StaryuApproach;

        public static long BitCount(long bits)
        {
            long count = 0;
            long uBits = bits;
            while (uBits > 0)
            {
                if ((uBits & 1) != 0)
                    count++;
                uBits >>= 1;
            }
            return count;
        }

        public static long EntryDataSize(EntryKind kind, long count)
        {
            switch (kind)
            {
                case EntryKind.Exit:
                case EntryKind.InitFunc:
                case EntryKind.Block:
                case EntryKind.Skip:
                case EntryKind.Path:
                case EntryKind.Loop:
                case EntryKind.SetFlags:
                case EntryKind.Func:
                    return 0;
                case EntryKind.SplineVel:
                case EntryKind.SplineVelBlock:
                    return 2 * count;
                default:
                    return count;
            }
        }

        public static Vector4 GetColor(VP_DataView view, long offset)
        {
            float r = view.GetUint8(offset + 0) / 255f;
            float g = view.GetUint8(offset + 1) / 255f;
            float b = view.GetUint8(offset + 2) / 255f;
            float a = view.GetUint8(offset + 3) / 255f;
            return new Vector4(r, g, b, a);
        }

        public static bool EntryShouldBlock(EntryKind kind)
        {
            switch (kind)
            {
                case EntryKind.Block:
                case EntryKind.LerpBlock:
                case EntryKind.SplineVelBlock:
                case EntryKind.SplineBlock:
                case EntryKind.StepBlock:
                case EntryKind.SetFlags:
                case EntryKind.Func:
                case EntryKind.MultiFunc:
                case EntryKind.ColorStepBlock:
                case EntryKind.ColorLerpBlock:
                    return true;
                default:
                    return false;
            }
        }

        public static AnimationTrack ParseAnimationTrack(CRGDataMap dataMap, long addr)
        {
            if (addr == 0)
                return null;

            var range = dataMap.GetRange(addr);
            var view = range.Data.CreateDefaultDataView();

            List<long> entryStarts = new List<long>();
            List<TrackEntry> entries = new List<TrackEntry>();

            var offs = addr - range.Start;
            while (true)
            {
                entryStarts.Add(offs);
                var o = offs + 0x00;
                EntryKind kind = (EntryKind)(view.GetUint8(o) >> 1);
                uint flags = ((view.GetUint32(offs + 0x00, false) >> 15) & 0x3FF);
                int increment = view.GetUint16(offs + 0x02, false) & 0x7FFF;

                offs += 4;
                if (kind == EntryKind.Loop)
                {
                    long loopAddr = view.GetUint32(offs, false);
                    int loopStart = entryStarts.FindIndex(start => start + range.Start == loopAddr);
                    if (loopStart < 0)
                        throw new System.Exception($"Bad loop start address {loopAddr:X8}");
                    return new AnimationTrack { Entries = entries, LoopStart = loopStart };
                }
                if (kind == EntryKind.Exit)
                {
                    return new AnimationTrack { Entries = entries, LoopStart = -1 };
                }

                bool block = EntryShouldBlock(kind);

                long count = EntryDataSize(kind, BitCount(flags));
                VP_Float32Array<VP_ArrayBuffer> data;
                List<Vector4> colors = new List<Vector4>();

                if ((long)kind >= (long)EntryKind.ColorStepBlock)
                {
                    for (long i = 0; i < count; i++)
                    {
                        colors.Add(GetColor(view, offs));
                        offs += 4;
                    }
                    data = new VP_Float32Array();
                }
                else
                {
                    data = range.Data.CreateTypedArray<VP_Float32Array<VP_ArrayBuffer>>(TypedArrayKind.Float32, offs, count, Endianness.BigEndian);
                    offs += count * 4;
                }

                var entry = new TrackEntry
                {
                    Kind = kind,
                    Flags = flags,
                    Increment = increment,
                    Block = block,
                    Data = data,
                    Path = null,
                    Colors = colors
                };

                if (kind == EntryKind.Path)
                {
                    entry.Path = ParsePath(dataMap, view.GetUint32(offs, false));
                    offs += 4;
                }

                entries.Add(entry);
            }
        }

        public static TrackPath ParsePath(CRGDataMap dataMap, long addr)
        {
            var view = dataMap.GetView(addr);

            PathKind kind = (PathKind)view.GetUint8(0x00);
            int length = view.GetUint16(0x02, false);
            float segmentRate = view.GetFloat32(0x04, false);
            long pointList = view.GetUint32(0x08, false);
            float duration = view.GetFloat32(0x0C, false);
            long timeList = view.GetUint32(0x10, false);
            long quarticList = view.GetUint32(0x14, false);

            var timeRange = dataMap.GetRange(timeList);
            VP_Float32Array<VP_ArrayBuffer> times = timeRange.Data.CreateTypedArray<VP_Float32Array<VP_ArrayBuffer>>(TypedArrayKind.Float32, (timeList - timeRange.Start), length, Endianness.BigEndian);

            int pointCount = (length + 2) * 3;
            if (kind == PathKind.Bezier)
                pointCount = length * 9;
            else if (kind == PathKind.Linear)
                pointCount = length * 3;

            var pointRange = dataMap.GetRange(pointList);
            VP_Float32Array<VP_ArrayBuffer> points = pointRange.Data.CreateTypedArray<VP_Float32Array<VP_ArrayBuffer>>(TypedArrayKind.Float32, (pointList - pointRange.Start), pointCount, Endianness.BigEndian);

            VP_Float32Array<VP_ArrayBuffer> quartics;
            if (quarticList != 0)
            {
                var quarticRange = dataMap.GetRange(quarticList);
                quartics = quarticRange.Data.CreateTypedArray<VP_Float32Array<VP_ArrayBuffer>>(TypedArrayKind.Float32, (quarticList - quarticRange.Start), (length - 1) * 5, Endianness.BigEndian);
            }
            else
            {
                quartics = new VP_Float32Array();
            }

            return new TrackPath
            {
                Kind = kind,
                Length = length,
                SegmentRate = segmentRate,
                Duration = duration,
                Times = times,
                Points = points,
                Quartics = quartics
            };
        }

        public static bool EmptyStateBlock(StateBlock block)
        {
            return block.Animation == -1 &&
                   block.Motion == null &&
                   block.Signals.Count == 0 &&
                   block.FlagClear == 0 &&
                   block.AuxAddress == -1 &&
                   block.Spawn == null &&
                   block.FlagSet == 0 &&
                   block.IgnoreGround == null &&
                   block.EatApple == null &&
                   block.ForwardSpeed == null &&
                   block.Tangible == null &&
                   block.Splash == null;
        }

        public static List<AnimationData> ParseAnimations(CRGDataMap dataMap, List<long> addresses, List<GFXNode> nodes)
        {
            var anims = new List<AnimationData>();
            foreach (var addr in addresses)
            {
                var view = dataMap.GetView(addr);
                float fps = 30 * view.GetFloat32(0x00, false);
                if (fps == 0)
                    fps = 30;

                float frames = view.GetFloat32(0x04, false);
                long trackList = view.GetUint32(0x08, false);
                long materialData = view.GetUint32(0x0C, false);
                long someIDs = view.GetUint32(0x10, false);

                var tracks = new List<AnimationTrack>();
                var trackView = dataMap.GetView(trackList);
                for (long i = 0; i < nodes.Count; i++)
                {
                    long trackStart = trackView.GetUint32(4 * i, false);
                    tracks.Add(ParseAnimationTrack(dataMap, trackStart));
                }

                anims.Add(new AnimationData
                {
                    FPS = (long)fps,
                    Frames = (long)frames,
                    Tracks = tracks,
                    MaterialTracks = ParseMaterialAnimation(dataMap, materialData, nodes)
                });
            }

            if (anims.Count == 0)
            {
                // make sure materials load default textures
                ParseMaterialAnimation(dataMap, 0, nodes);
            }

            return anims;
        }

        public static List<List<AnimationTrack>> ParseMaterialAnimation(CRGDataMap dataMap, long addr, List<GFXNode> nodes)
        {
            var materialTracks = new List<List<AnimationTrack>>();

            if (addr != 0)
            {
                var materialView = dataMap.GetView(addr);

                for (int i = 0; i < nodes.Count; i++)
                {
                    long matListStart = materialView.GetUint32(i * 4, false);
                    var nodeMats = new List<AnimationTrack>();

                    for (int j = 0; j < nodes[i].Materials.Count; j++)
                    {
                        var o = (matListStart + j * 4);
                        AnimationTrack newTrack = matListStart == 0 ? null : ParseAnimationTrack(dataMap, dataMap.Deref(o));
                        FindNewTextures(dataMap, newTrack, nodes[i], j);
                        nodeMats.Add(newTrack);
                    }

                    materialTracks.Add(nodeMats);
                }
            }
            else
            {
                // Default texture application when animation address is 0
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = 0; j < nodes[i].Materials.Count; j++)
                    {
                        FindNewTextures(dataMap, null, nodes[i], j);
                    }
                }
            }

            return materialTracks;
        }

        public static void FindNewTextures(CRGDataMap dataMap, AnimationTrack track, GFXNode node, int index)
        {
            MaterialData matData = node.Materials[index];
            if ((matData.Flags & (MaterialFlags.Special | MaterialFlags.Tex1 | MaterialFlags.Tex2 | MaterialFlags.Palette)) == 0)
                return;

            var model = node.Model;
            var textureCache = model.SharedOutput.TextureCache;
            var dc = model.RSPOutput?.DrawCalls.Find(d => (d as DrawCall).materialIndex == index);
            if (dc == null)
            {
                //Debug.LogError("No corresponding draw call for material");
                return;
            }

            if (track == null)
            {
                if (matData.Optional)
                    return;
                matData.Optional = true;
            }

            var dummyAnimator = new CRGAnimator(true);
            var dummyTiles = new List<TileState>();
            dummyAnimator.SetTrack(track);
            var buffer = dataMap.GetRange(dataMap.Deref(matData.TextureStart));

            void MaybeAppend(long tex, long pal, TileState tile, long extraAddrParam = 0)
            {
                if (matData.UsedTextures.Exists(entry => entry.TextureID == tex && entry.PAL == pal))
                    return;

                long paletteAddr = pal == -1 ? extraAddrParam : (long)(dataMap.Deref(matData.PaletteStart + 4 * pal) - buffer.Start);
                long texAddr = tex == -1 ? extraAddrParam : (long)(dataMap.Deref(matData.TextureStart + 4 * tex) - buffer.Start);

                int idx = 0;
                if (0 <= texAddr && texAddr < buffer.Data.ByteLength)
                {
                    var buff2 = new[] { buffer.Data };
                    idx = (int)textureCache.TranslateTileTexture(buff2, texAddr, paletteAddr, tile);
                }
                else
                {
                    long globalTexAddr = dataMap.Deref(matData.TextureStart + 4 * tex);
                    var newRange = dataMap.GetRange(globalTexAddr);
                    var buff = new[] { buffer.Data, newRange.Data };
                    idx = (int)textureCache.TranslateTileTexture(buff, (globalTexAddr - newRange.Start) | (1 << 24), paletteAddr, tile);
                }

                matData.UsedTextures.Add(new TextureEntry { TextureID = tex, PAL = pal, Index = idx });
            }

            for (int i = 0; i < dc.TextureIndices.Count; i++)
            {
                dummyTiles.Add(new TileState());
                dummyTiles[i].Copy(textureCache.textures[dc.TextureIndices[i]].tile);
            }

            long extraAddr = 0;
            bool onlyPalette = (matData.Flags & (MaterialFlags.Special | MaterialFlags.Tex1 | MaterialFlags.Tex2)) == 0;

            if (onlyPalette)
            {
                extraAddr = textureCache.textures[dc.TextureIndices[0]].dramAddr;
            }
            else if ((matData.Flags & MaterialFlags.Palette) == 0)
            {
                extraAddr = textureCache.textures[dc.TextureIndices[0]].dramPalAddr;
            }

            var palAnim = dummyAnimator.Interpolators[(int)MaterialField.PalIndex];

            while (true)
            {
                bool done = dummyAnimator.RunUntilUpdate();
                int palStart = -1, palEnd = -1;

                if ((matData.Flags & MaterialFlags.Palette) != 0)
                {
                    palStart = (int)palAnim.p0;
                    palEnd = (int)palAnim.p1;
                    Debug.Assert(palAnim.op != AObjOP.Spline && (palAnim.op != AObjOP.Lerp || System.Math.Abs(palStart - palEnd) <= 1));
                }

                if (onlyPalette)
                {
                    for (long i = 0; i < dc.TextureIndices.Count; i++)
                    {
                        MaybeAppend(-1, palStart, dummyTiles[0], extraAddr);
                        MaybeAppend(-1, palEnd, dummyTiles[0], extraAddr);
                    }
                }
                else if ((matData.Flags & MaterialFlags.Special) != 0)
                {
                    var lodAnim = dummyAnimator.Interpolators[(int)MaterialField.PrimLOD];
                    int lodStart = (int)System.Math.Min(lodAnim.p0, lodAnim.p1);
                    int lodEnd = (int)System.Math.Max(lodAnim.p0, lodAnim.p1);

                    if (lodAnim.op == AObjOP.Lerp || lodAnim.op == AObjOP.Spline)
                    {
                        for (long i = lodStart; i <= lodEnd + 1; i++)
                        {
                            MaybeAppend(i, palStart, dummyTiles[0], extraAddr);
                            MaybeAppend(i, palEnd, dummyTiles[0], extraAddr);
                        }
                    }
                    else
                    {
                        for (long i = 0; i < 2; i++)
                        {
                            MaybeAppend(lodStart + i, palStart, dummyTiles[0], extraAddr);
                            MaybeAppend(lodStart + i, palEnd, dummyTiles[0], extraAddr);
                            MaybeAppend(lodEnd + i, palStart, dummyTiles[0], extraAddr);
                            MaybeAppend(lodEnd + i, palEnd, dummyTiles[0], extraAddr);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dc.TextureIndices.Count; i++)
                    {
                        var texFlag = i == 0 ? MaterialFlags.Tex1 : MaterialFlags.Tex2;
                        if ((matData.Flags & texFlag) == 0)
                            continue;

                        var texField = i == 0 ? MaterialField.TexIndex1 : MaterialField.TexIndex2;
                        var texAnim = dummyAnimator.Interpolators[(int)texField];
                        int start = (int)texAnim.p0;
                        int end = (int)texAnim.p1;

                        Debug.Assert(texAnim.op != AObjOP.Spline && (texAnim.op != AObjOP.Lerp || System.Math.Abs(start - end) <= 1));
                        MaybeAppend(start, palStart, dummyTiles[i], extraAddr);
                        MaybeAppend(start, palEnd, dummyTiles[i], extraAddr);
                        MaybeAppend(end, palStart, dummyTiles[i], extraAddr);
                        MaybeAppend(end, palEnd, dummyTiles[i], extraAddr);
                    }
                }

                if (done)
                    break;
            }
        }

        public static List<MaterialData> ParseMaterialData(CRGDataMap dataMap, long listStart)
        {
            var materialList = new List<MaterialData>();
            if (listStart == 0)
                return materialList;

            var range = dataMap.GetRange(listStart);
            var listView = range.Data.CreateDefaultDataView();
            var offs = listStart - range.Start;

            while (true)
            {
                long scrollEntry = listView.GetUint32(offs, false);
                if (scrollEntry == 0)
                    break;

                offs += 4;
                var scrollView = dataMap.GetView(scrollEntry);

                ushort flags = scrollView.GetUint16(0x30, false);
                long textureStart = scrollView.GetUint32(0x04, false);
                long paletteStart = scrollView.GetUint32(0x2C, false);
                ushort scale = scrollView.GetUint16(0x08, false);
                ushort shift = scrollView.GetUint16(0x0A, false);
                long halve = scrollView.GetUint32(0x10, false);
                float xScale = scrollView.GetFloat32(0x1C, false);
                float yScale = scrollView.GetFloat32(0x20, false);

                Vector4 primColor = GetColor(scrollView, 0x50);
                Vector4 envColor = GetColor(scrollView, 0x58);
                Vector4 blendColor = GetColor(scrollView, 0x5C);
                Vector4 diffuse = GetColor(scrollView, 0x60);
                Vector4 ambient = GetColor(scrollView, 0x64);

                float primLOD = scrollView.GetUint8(0x54) / 255f;

                var tiles = new List<TileParams>
                {
                    new TileParams
                    {
                        Width = scrollView.GetUint16(0x0C, false),
                        Height = scrollView.GetUint16(0x0E, false),
                        xShift = scrollView.GetFloat32(0x14, false),
                        yShift = scrollView.GetFloat32(0x18, false)
                    },
                    new TileParams
                    {
                        Width = scrollView.GetUint16(0x38, false),
                        Height = scrollView.GetUint16(0x3A, false),
                        xShift = scrollView.GetFloat32(0x3C, false),
                        yShift = scrollView.GetFloat32(0x40, false)
                    }
                };

                var textures = new List<TextureParams>
                {
                    new TextureParams
                    {
                        Format = (ImageFormat)scrollView.GetUint8(0x02),
                        Size = (ImageSize)scrollView.GetUint8(0x03),
                        Width = 0,
                        Height = 0
                    },
                    new TextureParams
                    {
                        Format = (ImageFormat)scrollView.GetUint8(0x32),
                        Size = (ImageSize)scrollView.GetUint8(0x33),
                        Width = scrollView.GetUint16(0x34, false),
                        Height = scrollView.GetUint16(0x36, false)
                    }
                };

                materialList.Add(new MaterialData
                {
                    Flags = flags == 0 ? (MaterialFlags)0xA1 : (MaterialFlags)flags,
                    TextureStart = textureStart,
                    PaletteStart = paletteStart,
                    Tiles = tiles,
                    Textures = textures,
                    PrimaryLOD = primLOD,
                    Halve = halve,
                    UsedTextures = new List<TextureEntry>(),
                    Shift = shift,
                    Scale = scale,
                    xScale = xScale,
                    yScale = yScale,
                    PrimaryColor = primColor,
                    EnviromentalColor = envColor,
                    BlendColor = blendColor,
                    Diffuse = diffuse,
                    Ambient = ambient,
                    Optional = false
                });
            }

            return materialList;
        }

        public static StateGraph ParseStateGraph(CRGDataMap dataMap, long addr, List<GFXNode> nodes)
        {
            var defaultAnimation = dataMap.Deref(addr);
            var initFunc = dataMap.Deref(addr + 0x04); // used to set initial state

            List<State> states = new List<State>();
            List<long> animationAddresses = new List<long>();

            if (defaultAnimation != 0)
                animationAddresses.Add(defaultAnimation);

            ParseStateSubgraph(dataMap, initFunc, states, animationAddresses);
            var animations = ParseAnimations(dataMap, animationAddresses, nodes);

            return new StateGraph { States = states, Animations = animations };
        }

        public const long fakeAux = 0x123456;

        public static int ParseStateSubgraph(CRGDataMap dataMap, long addr, List<State> states, List<long> animationAddresses)
        {
            int existingState = states.FindIndex(s => s.StartAddress == addr);
            if (existingState >= 0)
                return existingState;

            var parser = new StateParser(dataMap, addr, states, animationAddresses);
            parser.Parse();

            if (!parser.Trivial)
                FixupState(states[parser.StateIndex], animationAddresses);

            return parser.StateIndex;
        }

        public static void FixupMotion(long addr, List<Motion> blocks)
        {
            switch (addr)
            {
                // for some reason follows the ground in an aux process
                // do it the easy way instead
                case 0x802CC1E0:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.path);
                    blocks[0].Flags = 0x3;
                    break;
                // these use doubles for the increment, easier to just fix by hand
                case 0x802CBBDC:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.linear);
                    var parsed = (LinearMotion)blocks[0];
                    parsed.Velocity = new Vector3(parsed.Velocity.x, -60f, parsed.Velocity.z);
                    break;
                case 0x802CBCDC:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.linear);
                    var parsed2 = (LinearMotion)blocks[0];
                    parsed2.Velocity = new Vector3(parsed2.Velocity.x, -15f, parsed2.Velocity.z);
                    break;
                case 0x802DFC38:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.linear);
                    var parsed3 = (LinearMotion)blocks[0];
                    parsed3.Velocity = new Vector3(parsed3.Velocity.x, -150f, parsed3.Velocity.z);
                    break;
                // special goals aren't worth parsing -> approach point
                case 0x802D9A80:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.point);
                    var parsed4 = (ApproachPointMotion)blocks[0];
                    parsed4.Goal = ApproachGoal.GoodGround;
                    parsed4.Destination = Destination.Target;
                    break;
                case 0x802E9288:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.point);
                    var parsed5 = (ApproachPointMotion)blocks[0];
                    parsed5.Destination = Destination.PathStart;
                    break;
                // actually in a loop-> Face target
                case 0x802E7DDC:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.faceTarget);
                    var parsed6 = (FaceTargetMotion)blocks[0];
                    parsed6.Flags |= (long)MoveFlags.Continuous;
                    break;
                case 0x802DD1C0:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.faceTarget);
                    var parsed7 = (FaceTargetMotion)blocks[0];
                    parsed7.Flags |= (long)MoveFlags.Continuous;
                    break;
                case 0x802D7C30:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.faceTarget);
                    var parsed8 = (FaceTargetMotion)blocks[0];
                    parsed8.Flags |= (long)MoveFlags.Continuous;
                    break;
                // set splash params
                case 0x802BFF74:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.splash);
                    var parsed9 = (SplashMotion)blocks[1];
                    parsed9.Scale = new Vector3(2f, 2f, 2f);
                    break;
                case 0x802CA434:
                case 0x802D2428:
                case 0x802DB270:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.splash);
                    var parsed10 = (SplashMotion)blocks[1];
                    parsed10.OnImpact = true;
                    parsed10.Index = 13;
                    break;
                case 0x802DBDB0:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.splash);
                    var parsed11 = (SplashMotion)blocks[1];
                    parsed11.OnImpact = true;
                    parsed11.Index = 4;
                    break;
                // make linear motion match target position
                case 0x802DCA28:
                case 0x802BFC84:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.linear);
                    var parsed12 = (LinearMotion)blocks[0];
                    parsed12.MatchTarget = true;
                    break;
                // special projectile directions
                case 0x802D1D4C:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.projectile);
                    var parsed13 = (ProjectileMotion)blocks[1];
                    parsed13.Direction = Direction.PathStart;
                    break;
                case 0x802D1FC0:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.projectile);
                    var parsed14 = (ProjectileMotion)blocks[1];
                    parsed14.Direction = Direction.PathEnd;

                    blocks.Add(new BasicMotion
                    {
                        Subtype = BasicMotionKind.Loop,
                        Param = 0
                    });
                    break;
                // custom motion
                case 0x802DDA0C:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.basic);
                    var parsed15 = (BasicMotion)blocks[0];
                    parsed15.Subtype = BasicMotionKind.Custom;
                    break;
                // staryu player tracking
                case 0x802CCE70:
                    VP_BYMLUtils.Assert(blocks[1].Kind == MotionKind.point);
                    var parsed16 = (ApproachPointMotion)blocks[1];
                    parsed16.Destination = Destination.Player;
                    parsed16.Goal = ApproachGoal.Radius;

                    blocks.Insert(2, new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 1
                    });
                    var parsed17 = (BasicMotion)blocks[3];
                    VP_BYMLUtils.Assert(blocks[3].Kind == MotionKind.basic && parsed17.Subtype == BasicMotionKind.SetSpeed);
                    parsed17.Param = 8000; // speed up to accommodate increased radius

                    var parsed18 = (ApproachPointMotion)blocks[4];
                    VP_BYMLUtils.Assert(blocks[4].Kind == MotionKind.point);
                    StaryuApproach = parsed18;

                    blocks[4] = new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 2
                    };
                    break;
                case 0x802CD1AC:
                    blocks[0] = new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 3
                    };
                    break;
                case 0x802CD2FC:
                    blocks.Insert(0, new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 4
                    });
                    break;
                // charmeleon motion
                case 0x802DC280:
                    blocks.Add(new FollowPathMotion
                    {
                        Speed = new ObjParam { Stored = new StoredValue() { Index = 5 } },
                        Start = PathStart.Resume,
                        End = new ObjParam() { Stored = new StoredValue() { Index = 4 } },
                        MaxTurn = 0,
                        Flags = (long)(MoveFlags.Ground | MoveFlags.SnapTurn)
                    });
                    break;
                // bulbasaur
                case 0x802E1604:
                    var parsed19 = (BasicMotion)blocks[0];
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.basic && parsed19.Subtype == BasicMotionKind.Custom);
                    parsed19.Param = 1;
                    break;
                // zapdos egg
                case 0x802EC294:
                    blocks[0] = new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 1
                    };
                    break;
                // poliwag face player in state
                case 0x802DCA7C:
                    blocks.Add(new BasicMotion
                    {
                        Subtype = BasicMotionKind.Custom,
                        Param = 1
                    });

                    blocks.Add(new FaceTargetMotion
                    {
                        MaxTurn = 0.1f,
                        Flags = (long)MoveFlags.FacePlayer
                    });
                    break;
                // keep psyduck path from modifying y
                // the game doesn't need this, because it treats the path as a relative offset,
                // but I can't follow part of the logic, so we're treating it as absolute position
                case 0x802DB5C0:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.path);
                    var parsed20 = (FollowPathMotion)blocks[0];
                    parsed20.Flags |= (long)MoveFlags.ConstHeight;
                    break;
                // articuno egg
                case 0x802C4C70:
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.basic);
                    var parsed21 = (BasicMotion)blocks[0];
                    parsed21.Subtype = BasicMotionKind.Custom;
                    parsed21.Param = 0;
                    break;
                // this motion also has a useless horizontal component
                case 0x802C4D60:
                    blocks[0] = new VerticalMotion
                    {
                        Target = new ObjParam() { Stored = new StoredValue() { Index = 0 } },
                        AsDelta = false,
                        StartSpeed = 300,
                        G = 0,
                        MinVel = 0,
                        MaxVel = 0,
                        Direction = (Direction)1
                    };
                    break;
                // face target in state
                case 0x802C4820:
                    blocks.Insert(0, new FaceTargetMotion
                    {
                        MaxTurn = 1f,
                        Flags = (long)MoveFlags.FacePlayer
                    });
                    break;

                case 0x802C502C:
                    var parsed22 = (BasicMotion)blocks[0];
                    VP_BYMLUtils.Assert(blocks[0].Kind == MotionKind.basic && parsed22.Subtype == BasicMotionKind.Custom);
                    parsed22.Param = 1;
                    break;

            }
        }

        public static void FixupState(State state, List<long> animationAddresses)
        {
            long d = (long)state.StartAddress;
            switch (d)
            {
                case 0x802DBBA0:
                    state.Blocks[0].Edges[0].Type = InteractionType.Behavior;
                    state.Blocks[0].Edges[0].Param = 1;
                    state.Blocks[0].Edges[1].Type = InteractionType.Behavior;
                    state.Blocks[0].Edges[1].Param = 2;
                    state.Blocks[0].Edges[2].Type = InteractionType.Behavior;
                    state.Blocks[0].Edges[2].Param = 3;
                    state.Blocks[0].Edges[3].Type = InteractionType.NonzeroBehavior;
                    break;
                case 0x802D8BB8:
                    state.Blocks[0].Edges[0].Type = InteractionType.Behavior;
                    state.Blocks[0].Edges[0].Param = 3;
                    state.Blocks[0].Edges[1].Type = InteractionType.Behavior;
                    state.Blocks[0].Edges[1].Param = 4;
                    break;
                case 0x802DDB60:
                    state.Blocks[0].Edges.Add(state.Blocks[1].Edges[1]);
                    state.Blocks.RemoveRange(1, state.Blocks.Count - 1);
                    break;
                case 0x802DD398:
                    state.Blocks[0].Edges.Clear();
                    break;
                case 0x802CB9BC:
                case 0x802CBC74:
                case 0x802CBD74:
                    state.Blocks[0].AuxAddress = 0;
                    break;
                case 0x802C7F74:
                    state.Blocks[0].Wait.AllowInteraction = true;
                    state.Blocks[0].Wait.Interactions.Add(new StateEdge { Type = InteractionType.PhotoSubject, Param = 0, Index = -1, AuxFunc = fakeAux });
                    state.Blocks[1].Wait.AllowInteraction = true;
                    state.Blocks[1].Wait.Interactions.Add(new StateEdge { Type = InteractionType.PhotoSubject, Param = 0, Index = -1, AuxFunc = fakeAux });
                    break;
                case 0x802D97B8:
                    state.Blocks[1].Signals[0].Condition = InteractionType.Behavior;
                    state.Blocks[1].Signals[0].ConditionParam = 1;
                    state.Blocks[1].Signals[1].Condition = InteractionType.Behavior;
                    state.Blocks[1].Signals[1].ConditionParam = 2;
                    state.Blocks[1].Signals[1].Value = 0x28;
                    break;
                case 0x802BF894:
                    state.Blocks[2].Signals[0].Condition = InteractionType.OverSurface;
                    break;
                case 0x802BFE34:
                    Debug.Assert(state.Blocks.Count == 3);
                    state.Blocks.RemoveAt(0);
                    break;
                case 0x802DC4F0:
                case 0x802DC590:
                    state.Blocks[1].Signals.Add(new Signal { Target = 0, Value = 0x1C, Condition = InteractionType.OverSurface, ConditionParam = 0 });
                    break;
                case 0x802CA020:
                    Debug.Assert(state.Blocks[0].Splash != null);
                    state.Blocks[0].Splash.Scale = new Vector3(15, 15, 10);
                    break;
                case 0x802BEB24:
                    Debug.Assert(state.DoCleanup);
                    state.Blocks[0].Edges.Clear();
                    break;
                case 0x802CD0B8:
                    int animIdx = animationAddresses.IndexOf(0x802D3808);
                    Debug.Assert(animIdx >= 0);
                    state.Blocks.Add(new StateBlock
                    {
                        Animation = animIdx,
                        Force = false,
                        Edges = new List<StateEdge>(state.Blocks[0].Edges),
                        Motion = null,
                        AuxAddress = -1,
                        Wait = null,
                        FlagClear = 0,
                        FlagSet = 0,
                        Signals = new List<Signal>()
                    });
                    break;
                case 0x802CD4F4:
                    state.Blocks[1].Signals[0].Condition = InteractionType.Unknown;
                    state.Blocks[1].Signals[1].Condition = InteractionType.Unknown;
                    state.Blocks[1].Signals[2].Condition = InteractionType.Unknown;
                    break;
                case 0x802D9074:
                    state.Blocks[0].Edges[0].Type = InteractionType.Unknown;
                    break;
                case 0x802E4434:
                    state.Blocks[1].Signals[0].Condition = InteractionType.Unknown;
                    break;
                case 0x802D9B8C:
                    Debug.Assert(state.Blocks[0].Spawn != null);
                    state.Blocks[0].Spawn.ID = 1002;
                    break;
                case 0x802D9C84:
                    Debug.Assert(state.Blocks[0].Spawn != null);
                    state.Blocks[0].Spawn.ID = 5;
                    break;
                case 0x802E7D04:
                    Debug.Assert(state.Blocks[0].Wait != null);
                    state.Blocks[0].Wait.Duration = 6;
                    break;
                case 0x802DB0A0:
                    var firstEdge = state.Blocks[0].Edges[0];
                    state.Blocks[0].Edges.Insert(1, new StateEdge
                    {
                        Param = 2,
                        Type = firstEdge.Type,
                        Index = firstEdge.Index,
                        AuxFunc = firstEdge.AuxFunc
                    });
                    break;
                case 0x802DB78C:
                    state.Blocks[2].Signals[0].Condition = InteractionType.Behavior;
                    state.Blocks[2].Signals[0].ConditionParam = 2;
                    break;
                case 0x802DC6BC:
                    state.Blocks[1].Signals[0].Condition = InteractionType.Behavior;
                    state.Blocks[1].Signals[0].ConditionParam = 1;
                    state.Blocks[1].Signals[1].Condition = InteractionType.Behavior;
                    state.Blocks[1].Signals[1].ConditionParam = 2;
                    state.Blocks[1].Signals[2].Condition = InteractionType.Behavior;
                    state.Blocks[1].Signals[2].ConditionParam = 3;
                    break;
                case 0x802DCBB8:
                    var interaction = state.Blocks[0].Wait.Interactions[0];
                    var fs = state.Blocks[0].FlagSet;
                    var fc = state.Blocks[0].FlagClear;
                    state.Blocks[1].Wait.Interactions = new List<StateEdge>
        {
            new StateEdge { Type = interaction.Type + 1, Index = interaction.Index, Param = 0, AuxFunc = 0 }
        };
                    state.Blocks[1].FlagSet = fs;
                    state.Blocks[1].FlagClear = fc;
                    state.Blocks[2].Wait.Interactions = new List<StateEdge>
        {
            new StateEdge { Type = interaction.Type + 2, Index = interaction.Index, Param = 0, AuxFunc = 0 }
        };
                    state.Blocks[2].FlagSet = fs;
                    state.Blocks[2].FlagClear = fc;
                    break;
                case 0x802EA79C:
                case 0x802EA970:
                    state.Blocks[0].Animation = 1;
                    state.Blocks[1].Animation = 2;
                    state.Blocks[1].Signals.Clear();
                    Debug.Assert(state.Blocks[0].Wait != null);
                    state.Blocks[0].Wait.Duration = 10;
                    state.Blocks[0].Wait.DurationRange = 2;
                    state.Blocks[0].Wait.EndCondition = (long)EndCondition.Timer;
                    state.Blocks[1].Wait = new WaitParams
                    {
                        AllowInteraction = false,
                        Interactions = new List<StateEdge>(),
                        Duration = 6,
                        DurationRange = 0.5f,
                        LoopTarget = 1,
                        EndCondition = (long)EndCondition.Timer
                    };
                    state.Blocks[1].Edges.Add(new StateEdge { Type = InteractionType.Basic, Index = 1, Param = 0, AuxFunc = 0 });
                    state.DoCleanup = false;
                    break;
            }
        }
    }
}
