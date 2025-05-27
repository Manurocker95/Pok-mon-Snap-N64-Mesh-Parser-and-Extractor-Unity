using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Projectile : ModelRenderer
    {
      
        private Vector3 prevPos = Vector3.zero;
        public Vector3 Velocity = Vector3.zero;
        public float LandedAt = 0;
        public bool InWater = false;

        public static double MaxSlope = System.Math.Sqrt(3) / 2;
        public static long MinSpeed = 390;

        private static Vector3 ProjectileScale = new Vector3(0.1f, 0.1f, 0.1f);
        private static Vector3[] ImpactScratch = { Vector3.zero, Vector3.zero };
        private static Vector3 GroundScratch = Vector3.zero;

        public Projectile(RenderData renderData, ProjectileData def, bool isPester)
            : base(renderData, def.Nodes, def.Animations)
        {
            this.Def = def;
            this.IsPester = isPester;
            this.Visible = false;
        }

        public ProjectileData Def { get; }
        public bool IsPester { get; }

        public double DistFrom(Vector3 pos)
        {
            if (!this.Visible || this.Hidden || this.LandedAt == 0)
                return double.PositiveInfinity;

            return Vector3.Distance(pos, this.Translation);
        }

        public bool TryThrow(Vector3 pos, Vector3 dir, Vector3 cameraVel)
        {
            if (this.Visible)
                return false;

            this.Visible = true;
            this.LandedAt = 0;
            this.InWater = false;
            this.AnimationPaused = false;
            this.SetAnimation(0);

            this.prevPos = pos;
            this.Velocity = dir.normalized * 1500f;
            this.Translation = pos + this.Velocity * (1.0f / 10);
            this.Velocity += cameraVel;

            this.ModelMatrix = Matrix4x4.Scale(ProjectileScale);
            return true;
        }

        public void Remove(LevelGlobals globals)
        {
            this.Visible = false;
            globals.SendGlobalSignal(this, (long)InteractionType.AppleRemoved);

            if (this.IsPester)
            {
                var smoke = globals.Particles.CreateEmitter(true, 0, null);
                if (smoke != null)
                    smoke.Position = this.Translation;
            }
        }

        protected override void Motion(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            double dt = viewerInput.DeltaTime / 1000.0;

            if (this.LandedAt > 0)
            {
                double frames = 30 * (viewerInput.Time - this.LandedAt) / 1000.0;

                if (this.InWater)
                {
                    if (frames > 60)
                        this.Remove(globals);
                    else
                        this.Translation.y -= (float)(60 * dt);
                }
                else
                {
                    if (frames > 170)
                        this.Remove(globals);
                    else if (frames > 140)
                    {
                        float scale = (float)(0.1 * System.Math.Pow(0.9, 2 * (frames - 140)));
                        this.ModelMatrix[0] = scale;
                        this.ModelMatrix[5] = scale;
                        this.ModelMatrix[10] = scale;
                        this.Translation.y -= scale * 360 * (float)dt;
                    }
                }
            }
            else if (!this.HitGround(viewerInput, globals))
            {
                this.CheckCollision(this.Velocity.magnitude, dt, globals);
                this.Velocity.y -= (float)(1080 * dt);
            }

            this.prevPos = this.Translation;

            if (this.LandedAt == 0)
                this.Translation += this.Velocity * (float)dt;

            this.ModelMatrix[12] = this.Translation.x;
            this.ModelMatrix[13] = this.Translation.y;
            this.ModelMatrix[14] = this.Translation.z;
        }

        private bool HitGround(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            var ground = SnapUtils.FindGroundPlane(globals.Level.Collision, this.Translation.x, this.Translation.z);
            double height = SnapUtils.ComputePlaneHeight(ground, this.Translation.x, this.Translation.z);

            if (this.Translation.y > height)
                return false;

            double lo = 0, hi = 1;
            double delta = this.Translation.y - height;
            int stepCount = 15;

            GroundScratch = this.Translation;

            while (System.Math.Abs(delta) > 0.375 && stepCount-- > 0)
            {
                double t = (lo + hi) / 2.0;
                GroundScratch = Vector3.Lerp(this.prevPos, this.Translation, (float)t);
                double midHeight = SnapUtils.ComputePlaneHeight(ground, GroundScratch.x, GroundScratch.z);
                delta = GroundScratch.y - midHeight;
                if (delta > 0)
                    lo = t;
                else
                    hi = t;
            }

            this.Translation = GroundScratch + Vector3.up * 12;

            if (ground.Type == 0x337FB2)
                globals.SpawnFish(this.Translation);

            if (this.IsPester)
            {
                globals.SendGlobalSignal(this, (long)InteractionType.PesterLanded);

                switch (ground.Type)
                {
                    case 0x00FF00: globals.SendGlobalSignal(this, 0x26); break;
                    case 0xFF0000: globals.SendGlobalSignal(this, 0x2A); break;
                    case 0xFF7FB2: globals.SendGlobalSignal(this, 0x1D); break;
                    case 0x0019FF: globals.SendTargetedSignal(this, 0x2B, 0x802D3B34); break;
                }

                this.Remove(globals);
                return true;
            }

            globals.SendGlobalSignal(this, (long)InteractionType.AppleLanded);

            double slowdownFactor = 1;

            switch (ground.Type)
            {
                case 0x0019FF:
                case 0x007F66:
                case 0x337FB2:
                case 0x4CCCCC:
                    globals.CreateSplash(SplashType.AppleWater, this.Translation, null);
                    this.LandedAt = viewerInput.Time;
                    this.InWater = true;
                    break;
                case 0x00FF00:
                case 0xFF4C19:
                    globals.CreateSplash(SplashType.AppleLava, this.Translation, null);
                    this.LandedAt = viewerInput.Time;
                    this.InWater = true;
                    break;
                case 0xFF0000:
                    this.Remove(globals);
                    break;
                case 0x193333:
                case 0x331919:
                case 0x4C1900:
                case 0x4C4C33:
                case 0x7F4C00:
                case 0x7F6633:
                case 0x7F667F:
                case 0x7F7F7F:
                case 0xFF7FB2:
                    slowdownFactor = 0.3;
                    break;
                case 0x4C7F00:
                case 0x996666:
                case 0xB2997F:
                case 0xFF9919:
                    slowdownFactor = 0.2;
                    break;
                default:
                    slowdownFactor = 0;
                    break;
            }

            if (!this.Visible || this.LandedAt != 0)
                return true;

            Vector3 normal = ground.Normal.normalized;
            MathHelper.ReflectVec3(ref this.Velocity, this.Velocity, normal);

            double startSpeed = this.Velocity.magnitude;

            if (startSpeed * slowdownFactor < MinSpeed)
            {
                if (normal.y >= MaxSlope)
                {
                    this.LandedAt = viewerInput.Time;
                    this.AnimationPaused = true;
                }
                else
                {
                    this.Velocity = this.Velocity.normalized * (float)(MinSpeed);
                }
            }
            else
            {
                this.Velocity *= (float)slowdownFactor;
            }

            return true;
        }
        private void CheckCollision(double currSpeed, double dt, LevelGlobals globals)
        {
            ImpactScratch[0] = this.Velocity.normalized;
            double furthestIncursion = currSpeed * dt;
            Actor chosenCollider = null;

            foreach (var collider in globals.AllActors)
            {
                if (!collider.GetImpact(out ImpactScratch[1], this.Translation))
                    continue;

                double distUntilClosest = Vector3.Dot(ImpactScratch[0], ImpactScratch[1]);
                if (distUntilClosest < 0)
                    continue;

                double sqrLen = ImpactScratch[1].sqrMagnitude;
                double minDist = collider.Def.Radius - System.Math.Sqrt(sqrLen - distUntilClosest * distUntilClosest);
                if (minDist < 0)
                    continue;

                if ((collider.Def.Flags & 2) != 0)
                {
                    if (this.IsPester)
                        collider.ReceiveSignal(this, (long)InteractionType.PesterAlmost, globals);
                    else
                        collider.ReceiveSignal(this, (long)InteractionType.AppleAlmost, globals);
                }

                double insideDistance = System.Math.Sqrt(collider.Def.Radius * collider.Def.Radius - minDist * minDist) - distUntilClosest;

                if (insideDistance > furthestIncursion)
                {
                    furthestIncursion = insideDistance;
                    chosenCollider = collider;
                }
            }

            if (chosenCollider != null)
            {
                chosenCollider.MotionData.LastImpact = ImpactScratch[0];

                if (this.IsPester)
                {
                    chosenCollider.ReceiveSignal(this, (long)InteractionType.PesterHit, globals);
                    this.Remove(globals);
                }
                else
                {
                    chosenCollider.ReceiveSignal(this, (long)InteractionType.AppleHit, globals);

                    this.Translation += ImpactScratch[0] * (float)(-furthestIncursion);

                    ImpactScratch[1] = (this.Translation - chosenCollider.Center).normalized;

                    MathHelper.ReflectVec3(ref this.Velocity, this.Velocity, ImpactScratch[1]);
                    MathHelper.NormToLength(ref this.Velocity, System.Math.Max(currSpeed / 2, 300));
                }
            }
        }

    }

}
