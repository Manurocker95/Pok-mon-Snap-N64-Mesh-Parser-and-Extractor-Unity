using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class CRGAnimator
    {
        public AnimationTrack Track = null;
        public AObj[] Interpolators = new AObj[10];
        public List<ColorAObj> Colors = new List<ColorAObj>();
        public long StateFlags = 0;
        public long LoopCount = 0;
        public bool ForceLoop = false;
        public long LastFunction = -1;

        private long TrackIndex = 0;
        private long NextUpdate = 0;

        public CRGAnimator(bool useColor = false)
        {
            for (long i = 0; i < Interpolators.Length; i++)
                Interpolators[i] = new AObj();

            if (useColor)
            {
                Colors = new List<ColorAObj>();
                for (long i = 0; i < 5; i++)
                    Colors.Add(new ColorAObj());
            }
        }

        public void SetTrack(AnimationTrack newTrack)
        {
            Track = newTrack;
            LoopCount = 0;
            Reset();
        }

        public void Reset(long time = 0)
        {
            TrackIndex = 0;
            NextUpdate = time;
            foreach (var interp in Interpolators)
                interp.Reset();
            foreach (var color in Colors)
                color.Reset();
        }

        public bool RunUntilUpdate()
        {
            long oldIndex = TrackIndex;
            Update(NextUpdate);
            return Track == null || TrackIndex <= oldIndex;
        }

        public float Compute(long field, double time)
        {
            return Interpolators[field].Compute((float)time);
        }

        public bool Update(double time)
        {
            if (Track == null)
                return false;

            var entries = Track.Entries;
            while (NextUpdate <= time)
            {
                if (TrackIndex == entries.Count)
                {
                    LoopCount++;
                    if (Track.LoopStart >= 0)
                        TrackIndex = Track.LoopStart;
                    else
                    {
                        if (!ForceLoop)
                            return LoopCount == 1;
                        TrackIndex = 0;
                    }
                }

                var entry = entries[(int)TrackIndex++];
                long offs = 0;

                switch (entry.Kind)
                {
                    case EntryKind.Lerp:
                    case EntryKind.LerpBlock:
                        for (int i = 0; i < 10; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var interp = Interpolators[i];
                                interp.op = AObjOP.Lerp;
                                interp.p0 = interp.p1;
                                interp.p1 = (float)entry.Data[(int)offs++];
                                interp.v1 = 0;
                                if (entry.Increment != 0)
                                    interp.v0 = (interp.p1 - interp.p0) / entry.Increment;
                                interp.start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.SplineVel:
                    case EntryKind.SplineVelBlock:
                        for (int i = 0; i < 10; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var interp = Interpolators[i];
                                interp.op = AObjOP.Spline;
                                interp.p0 = interp.p1;
                                interp.p1 = (float)entry.Data[(int)offs++];
                                interp.v0 = interp.v1;
                                interp.v1 = (float)entry.Data[(int)offs++];
                                if (entry.Increment != 0)
                                    interp.len = 1f / entry.Increment;
                                interp.start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.SplineEnd:
                        for (int i = 0; i < 10; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                                Interpolators[i].v1 = (float)entry.Data[(int)offs++];
                        }
                        break;

                    case EntryKind.Spline:
                    case EntryKind.SplineBlock:
                        for (int i = 0; i < 10; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var interp = Interpolators[i];
                                interp.op = AObjOP.Spline;
                                interp.p0 = interp.p1;
                                interp.p1 = (float)entry.Data[(int)offs++];
                                interp.v0 = interp.v1;
                                interp.v1 = 0;
                                if (entry.Increment != 0)
                                    interp.len = 1f / entry.Increment;
                                interp.start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.Step:
                    case EntryKind.StepBlock:
                        for (int i = 0; i < 10; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var interp = Interpolators[i];
                                interp.op = AObjOP.Step;
                                interp.p0 = interp.p1;
                                interp.p1 = (float)entry.Data[(int)offs++];
                                interp.v1 = 0;
                                interp.len = entry.Increment;
                                interp.start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.Skip:
                        for (int i = 0; i < 10; i++)
                            if ((entry.Flags & (1 << i)) != 0)
                                Interpolators[i].start -= entry.Increment;
                        break;

                    case EntryKind.SetFlags:
                        StateFlags = entry.Flags;
                        break;

                    case EntryKind.Path:
                        Interpolators[(long)ModelField.Path].path = entry.Path;
                        break;

                    case EntryKind.ColorStep:
                    case EntryKind.ColorStepBlock:
                        for (int i = 0; i < 5; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var c = Colors[i];
                                c.op = AObjOP.Step;
                                c.c0 = c.c1;
                                c.c1 = entry.Colors[(int)offs++];
                                Interpolators[i].len = entry.Increment;
                                Interpolators[i].start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.ColorLerp:
                    case EntryKind.ColorLerpBlock:
                        for (int i = 0; i < 5; i++)
                        {
                            if ((entry.Flags & (1 << i)) != 0)
                            {
                                var c = Colors[i];
                                c.op = AObjOP.Lerp;
                                c.c0 = c.c1;
                                c.c1 = entry.Colors[(int)offs++];
                                if (entry.Increment != 0)
                                    Interpolators[i].len = 1f / entry.Increment;
                                Interpolators[i].start = NextUpdate;
                            }
                        }
                        break;

                    case EntryKind.Func:
                        LastFunction = entry.Flags;
                        break;
                }

                if (entry.Block)
                    NextUpdate += entry.Increment;
            }

            return true;
        }

        public class AObj
        {
            public AObjOP op = AObjOP.NOP;
            public long start = 0;
            public float len = 1;
            public float p0 = 0;
            public float p1 = 0;
            public float v0 = 0;
            public float v1 = 0;
            public TrackPath path = null;

            public float Compute(float t)
            {
                switch (op)
                {
                    case AObjOP.NOP:
                        return 0;
                    case AObjOP.Step:
                        return (t - start) >= len ? p1 : p0;
                    case AObjOP.Lerp:
                        return p0 + (t - start) * v0;
                    case AObjOP.Spline:
                        return GetPointHermite(p0, p1, v0 / len, v1 / len, (t - start) * len);
                    default:
                        return 0;
                }
            }

            public void Reset()
            {
                op = AObjOP.NOP;
                start = 0;
                len = 1;
                p0 = 0;
                p1 = 0;
                v0 = 0;
                v1 = 0;
            }

            private float GetPointHermite(float p0, float p1, float m0, float m1, float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                return (2 * t3 - 3 * t2 + 1) * p0 +
                       (t3 - 2 * t2 + t) * m0 +
                       (-2 * t3 + 3 * t2) * p1 +
                       (t3 - t2) * m1;
            }
        }

        public class ColorAObj
        {
            public AObjOP op = AObjOP.NOP;
            public long start = 0;
            public float len = 1;
            public Vector4 c0 = Vector4.zero;
            public Vector4 c1 = Vector4.zero;

            public void Compute(float t, ref Vector4 dst)
            {
                switch (op)
                {
                    case AObjOP.Step:
                        dst = (t - start) >= len ? c1 : c0;
                        break;
                    case AObjOP.Lerp:
                        float alpha = Math.Clamp((t - start) * len, 0, 1);
                        dst = Vector4.Lerp(c0, c1, alpha);
                        break;
                }
            }

            public void Reset()
            {
                op = AObjOP.NOP;
                start = 0;
                len = 1;
                c0 = Vector4.zero;
                c1 = Vector4.zero;
            }
        }

    }
}
