using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Actor : ModelRenderer
    {
        public MotionData MotionData = new MotionData();
        protected long CurrState = -1;
        protected long CurrBlock = 0;
        protected long CurrAux = 0;
        protected Target Target = null;
        protected Actor LastSpawn = null;

        private double BlockEnd = 0;
        private long LoopTarget = 1;
        private double PhotoTimer = 0;

        public Vector3 Euler = Vector3.zero;
        public Vector3 Scale = Vector3.one;
        public Vector3 Center = Vector3.zero;

        public bool Tangible = true;
        public long GlobalPointer = 0;

        public ObjectSpawn Spawn;
        public ActorDef Def;

        public Actor(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false)
            : base(renderData, def.Nodes, def.StateGraph.Animations, false, isEgg)
        {
            ID = def.ID;
            Spawn = spawn;
            Def = def;
            this.MotionData.Path = spawn.Path;
            this.GlobalPointer = def.GlobalPointer;
            this.Reset(globals);
        }

        public bool GetImpact(out Vector3 dst, Vector3 pos)
        {
            dst = Vector3.zero;

            if (!this.Visible || this.Hidden || !this.Tangible || this.Def.Radius == 0)
                return false;

            dst = this.Center - pos;
            return true;
        }

        public virtual void Reset(LevelGlobals globals)
        {
            // Set transform components
            this.Translation = Spawn.Position;

            if (Def.Spawn == SpawnType.Ground)
                this.Translation.y = (float)SnapUtils.GroundHeightAt(globals, Spawn.Position);
            this.Euler = Spawn.Euler;

            this.Scale = Vector3.Scale(Def.Scale, Spawn.Scale);
            UpdatePositions();

            MotionData.Reset();

            Visible = true;
            Hidden = false;
            Tangible = true;

            var ground = SnapUtils.FindGroundPlane(globals.Level.Collision, Translation.x, Translation.z);
            MotionData.GroundHeight = (float)SnapUtils.ComputePlaneHeight(ground, Translation.x, Translation.z);
            MotionData.GroundType = (int)ground.Type;

            CurrAux = 0;

            if (Animations.Count > 0)
                SetAnimation(0);

            if (Def.StateGraph.States.Count > 0)
                ChangeState(0, globals);
        }

        protected void UpdatePositions()
        {
            RendererUtils.BuildTransform(ref this.ModelMatrix, this.Translation, this.Euler, this.Scale);

            if ((this.Def.Flags & 1) != 0)
            {
                // the raw center shouldn't be multiplied by the scale in the model matrix, so divide out
                this.Center = new Vector3(
                    this.Def.Center.x / this.Scale.x,
                    this.Def.Center.y / this.Scale.y,
                    this.Def.Center.z / this.Scale.z
                );

                this.Center += this.Renderers[0].Translation;
            }
            else
            {
                this.Center = this.Def.Center;
            }

            this.Center = this.ModelMatrix.MultiplyPoint3x4(this.Center);
        }

        protected override void Motion(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            this.MotionStep(viewerInput, globals);
            while (this.CurrState >= 0)
            {
                var state = this.Def.StateGraph.States[(int)this.CurrState];
                var result = this.StateOverride(state.StartAddress, viewerInput, globals);
                if (result == MotionResult.Update)
                    continue;
                else if (result == MotionResult.Done)
                    break;

                var block = state.Blocks[(int)this.CurrBlock];
                if (block.Wait != null)
                {
                    if (block.Wait.AllowInteraction && this.BasicInteractions(block.Wait, viewerInput, globals))
                        continue;
                    if (!this.MetEndCondition(block.Wait.EndCondition, globals))
                        break;
                }

                if (this.EndBlock(state.StartAddress, globals))
                    break;

                if (!this.ChooseEdge(block.Edges, globals))
                {
                    this.CurrBlock++;
                    this.StartBlock(globals);
                }
            }
        }

        protected bool ChangeState(long newIndex, LevelGlobals globals)
        {
            if (newIndex == -1)
                return false;

            this.CurrState = newIndex;
            this.CurrBlock = 0;
            this.StartBlock(globals);
            return true;
        }


        protected virtual MotionResult StateOverride(long stateAddr, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            return MotionResult.None;
        }

        protected virtual bool EndBlock(long address, LevelGlobals globals)
        {
            return false;
        }

        protected virtual MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (viewerInput.Time - this.MotionData.Start > 5000)
                return MotionResult.Done;
            else
                return MotionResult.None;
        }

        protected override void Animate(LevelGlobals globals)
        {
            base.Animate(globals);
            if (this.Renderers[0].Animator.Track != null)
                this.UpdatePositions(); // collision center depends on root node position
        }

        protected virtual void StartBlock(LevelGlobals globals)
        {
            var state = this.Def.StateGraph.States[(int)this.CurrState];
            if (this.CurrBlock >= state.Blocks.Count)
            {
                if (state.DoCleanup)
                    this.Visible = false;
                this.CurrState = -1;
                return;
            }

            var block = state.Blocks[(int)this.CurrBlock];
            if (block.Animation >= 0)
            {
                if (block.Animation != this.CurrAnimation || block.Force)
                    this.SetAnimation((int)block.Animation);

                var currLoops = GfxPlatformUtils.AssertExists(this.Renderers[this.HeadAnimationIndex]).Animator.LoopCount;
                this.LoopTarget = currLoops + ((block.Wait != null && block.Wait.LoopTarget > 0) ? block.Wait.LoopTarget : 1);
            }

            for (int i = 0; i < block.Signals.Count; i++)
            {
                bool skip = true; // skip signals we don't understand
                switch (block.Signals[i].Condition)
                {
                    case InteractionType.Behavior:
                        skip = this.Spawn.Behaviour != block.Signals[i].ConditionParam;
                        break;
                    case InteractionType.OverSurface:
                        skip = (this.MotionData.GroundType != 0x337FB2 && this.MotionData.GroundType != 0x7F66 && this.MotionData.GroundType != 0xFF4C19);
                        break;
                    case InteractionType.Basic:
                        skip = false;
                        break;
                }
                if (skip)
                    continue;

                if (block.Signals[i].Target == 0)
                    globals.SendGlobalSignal(this, block.Signals[i].Value);
                else if (block.Signals[i].Target == (long)ObjectField.Target && this.Target is Actor)
                    ((Actor)this.Target).ReceiveSignal(this, block.Signals[i].Value, globals);
                else
                    globals.SendTargetedSignal(this, block.Signals[i].Value, block.Signals[i].Target);
            }

            if (block.AuxAddress >= 0)
            {
                this.CurrAux = block.AuxAddress;
                if (block.AuxAddress > 0)
                {
                    this.MotionData.StateFlags &= ~(long)EndCondition.Aux;
                    this.MotionData.AuxStart = -1;
                }
                else
                {
                    this.MotionData.StateFlags |= (long)EndCondition.Aux;
                }
            }

            if (block.FlagSet != 0)
                this.MotionData.StateFlags |= block.FlagSet;
            if (block.FlagClear != 0)
                this.MotionData.StateFlags &= ~block.FlagClear;

            if ((block.FlagSet & (long)EndCondition.Hidden) != 0)
                this.Hidden = true;
            if ((block.FlagClear & (long)EndCondition.Hidden) != 0)
                this.Hidden = false;

            if ((block.FlagSet & (long)EndCondition.PauseAnim) != 0)
                this.AnimationPaused = true;
            if ((block.FlagClear & (long)EndCondition.PauseAnim) != 0)
                this.AnimationPaused = false;

            if (block.IgnoreGround.HasValue)
                this.MotionData.IgnoreGround = block.IgnoreGround.Value;

            if (block.EatApple.HasValue && block.EatApple.Value && this.Target != null)
            {
                if (this.Target is Projectile)
                    ((Projectile)this.Target).Remove(globals);
                else
                    Debug.LogWarning("eating non apple");

                this.Target = null;
            }

            if (block.ForwardSpeed.HasValue)
                this.MotionData.ForwardSpeed = block.ForwardSpeed.Value;

            if (block.Tangible.HasValue)
                this.Tangible = block.Tangible.Value;

            if (block.Splash != null)
            {
                Vector3 splashScratch;
                if (block.Splash.Index == -1)
                {
                    splashScratch = this.Translation;
                }
                else
                {
                    splashScratch = this.Renderers[(int)block.Splash.Index].ModelMatrix.GetColumn(3); // extracting translation from matrix
                }

                globals.CreateSplash(SplashType.Water, splashScratch, block.Splash.Scale);
            }

            if (block.Spawn != null)
            {
                double yaw = 0;
                if (block.Spawn.Yaw == Direction.Forward)
                    yaw = this.Euler[1];
                else if (block.Spawn.Yaw == Direction.Backward)
                    yaw = this.Euler[1] + Math.PI;

                this.LastSpawn = globals.ActivateObject(
                    block.Spawn.ID,
                    this.Translation,
                    yaw,
                    block.Spawn.Behavior >= 0 ? block.Spawn.Behavior : this.Spawn.Behaviour,
                    block.Spawn.Scale
                );

                if (this.LastSpawn != null)
                    this.LastSpawn.MotionData.Path = this.MotionData.Path;
            }

            if (block.Motion != null)
            {
                this.MotionData.CurrMotion = block.Motion;
                this.MotionData.CurrBlock = 0;
                this.MotionData.Start = -1;
                this.MotionData.StateFlags &= ~((long)EndCondition.Pause | (long)EndCondition.Motion | (long)EndCondition.Target);
            }

            if (block.Wait != null)
            {
                this.BlockEnd = this.AnimationController.GetTimeInSeconds() + block.Wait.Duration + block.Wait.DurationRange * UnityEngine.Random.value;
            }
        }

        public virtual bool ReceiveSignal(Target source, long signal, LevelGlobals globals)
        {
            if (this.CurrState == -1)
                return false;

            var block = this.Def.StateGraph.States[(int)this.CurrState].Blocks[(int)this.CurrBlock];
            if (block.Wait == null)
                return false;

            // these are handled even if there's no corresponding edge
            if (signal == (long)InteractionType.AppleRemoved || signal == (long)InteractionType.TargetRemoved)
            {
                if (source != this.Target)
                    return false;
                this.Target = null;
            }

            for (int i = 0; i < block.Wait.Interactions.Count; i++)
            {
                if ((long)block.Wait.Interactions[i].Type != signal)
                    continue;

                double distToSource = source != null ? Vector3.Distance(this.Translation, source.Translation) : double.PositiveInfinity;

                switch (signal)
                {
                    case (long)InteractionType.PesterLanded:
                        if (distToSource >= 150)
                            return false;
                        this.Target = source;
                        break;

                    case (long)InteractionType.AppleLanded:
                    case (long)InteractionType.GravelerLanded:
                        if (distToSource >= 600)
                            return false;
                        this.Target = source;
                        break;

                    case (long)InteractionType.PhotoTaken:
                        if (Vector3.Distance(this.Translation, globals.Translation) >= 400)
                            return false;
                        this.Target = source;
                        break;

                    case (long)InteractionType.AppleRemoved:
                    case (long)InteractionType.TargetRemoved:
                        break; // just avoid setting the target

                    default:
                        if (source != null)
                            this.Target = source;
                        break;
                }

                return this.FollowEdge(block.Wait.Interactions[i], globals);
            }

            return false;
        }

        protected virtual bool FollowEdge(StateEdge edge, LevelGlobals globals)
        {
            if (edge.AuxFunc != 0)
            {
                this.CurrAux = edge.AuxFunc;
                this.MotionData.StateFlags &= ~(long)EndCondition.Aux;
                this.MotionData.AuxStart = -1;
            }
            return this.ChangeState(edge.Index, globals);
        }

        protected virtual bool ChooseEdge(List<StateEdge> edges, LevelGlobals globals)
        {
            double random = UnityEngine.Random.value;
            bool follow = false;

            for (int i = 0; i < edges.Count; i++)
            {
                switch (edges[i].Type)
                {
                    case InteractionType.Basic:
                        follow = true;
                        break;

                    case InteractionType.Random:
                        random -= edges[i].Param;
                        follow = random < 0;
                        break;

                    case InteractionType.Behavior:
                        follow = this.Spawn.Behaviour == edges[i].Param;
                        break;

                    case InteractionType.NonzeroBehavior:
                        follow = this.Spawn.Behaviour != 0;
                        break;

                    case InteractionType.Flag:
                    case InteractionType.NotFlag:
                        bool metCondition = this.MetEndCondition((long)edges[i].Param, globals);
                        follow = metCondition == (edges[i].Type == InteractionType.Flag);
                        break;

                    case InteractionType.HasTarget:
                        follow = this.Target != null;
                        break;

                    case InteractionType.OverSurface:
                        follow = this.MotionData.GroundType == 0x337FB2 ||
                                 this.MotionData.GroundType == 0x7F66 ||
                                 this.MotionData.GroundType == 0xFF4C19;
                        break;
                }

                if (follow)
                    return this.FollowEdge(edges[i], globals);
            }

            return false;
        }

        protected virtual bool MetEndCondition(long flags, LevelGlobals globals)
        {
            return ((flags & (long)EndCondition.Dance) != 0 && !SnapUtils.CanHearSong(this.Translation, globals)) ||
                   ((flags & (long)EndCondition.Timer) != 0 && this.AnimationController.GetTimeInSeconds() >= this.BlockEnd) ||
                   ((flags & (long)EndCondition.Animation) != 0 && this.Renderers[this.HeadAnimationIndex].Animator.LoopCount >= this.LoopTarget) ||
                   ((flags & this.MotionData.StateFlags) != 0);
        }

        protected virtual bool BasicInteractions(WaitParams block, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            Vector3 cameraScratch = viewerInput.Camera.transform.position;//viewerInput.Camera.WorldMatrix.GetColumn(3); // equivalent to mat4.getTranslation
            double playerDist = Vector3.Distance(this.Translation, cameraScratch);
            bool onScreen = viewerInput.ContainsSphereInFrustum(this.Translation, 100) && !this.Hidden;

            for (int i = 0; i < block.Interactions.Count; i++)
            {
                switch (block.Interactions[i].Type)
                {
                    case InteractionType.PokefluteA:
                    case InteractionType.PokefluteB:
                    case InteractionType.PokefluteC:
                        if (SnapUtils.CanHearSong(this.Translation, globals) && (int)block.Interactions[i].Type == globals.CurrentSong)
                        {
                            this.Target = globals;
                            return this.FollowEdge(block.Interactions[i], globals);
                        }
                        break;

                    case InteractionType.NearPlayer:
                        if (playerDist < block.Interactions[i].Param)
                        {
                            this.Target = globals;
                            return this.FollowEdge(block.Interactions[i], globals);
                        }
                        break;

                    case InteractionType.PhotoFocus:
                    case InteractionType.PhotoSubject:
                        if (onScreen && playerDist < 1500)
                            this.PhotoTimer += viewerInput.DeltaTime;
                        else
                            this.PhotoTimer = 0;

                        double limit = block.Interactions[i].Type == InteractionType.PhotoSubject
                            ? 3000
                            : (block.Interactions[i].Param / 30.0) * 1000;

                        if (this.PhotoTimer > limit)
                        {
                            this.PhotoTimer = 0;
                            return this.FollowEdge(block.Interactions[i], globals);
                        }
                        break;

                    case InteractionType.FindApple:
                        Projectile nearest = null;
                        double dist = 600;

                        for (int j = 0; j < globals.Projectiles.Count; j++)
                        {
                            var proj = globals.Projectiles[j];
                            if (proj.IsPester || proj.LandedAt == 0 || proj.InWater)
                                continue;

                            double newDist = proj.DistFrom(this.Translation);
                            if (newDist < dist)
                            {
                                dist = newDist;
                                nearest = proj;
                            }
                        }

                        if (nearest != null)
                        {
                            this.Target = nearest;
                            return this.FollowEdge(block.Interactions[i], globals);
                        }
                        break;
                }
            }

            return false;
        }

        protected virtual void MotionStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            double dt = viewerInput.DeltaTime / 1000.0;
            MotionResult result = MotionResult.None;
            bool updated = false;

            while (this.MotionData.CurrBlock < this.MotionData.CurrMotion.Count)
            {
                if (this.MotionData.Start < 0)
                {
                    result = AnimationUtils.MotionBlockInit(this.MotionData, this.Translation, this.Euler, viewerInput, this.Target);
                    if (result == MotionResult.Done)
                    {
                        this.MotionData.CurrBlock++;
                        this.MotionData.Start = -1;
                        continue;
                    }
                }

                Motion block = this.MotionData.CurrMotion[this.MotionData.CurrBlock];

                switch (block.Kind)
                {
                    case MotionKind.animation:
                        AnimationMotion a = (AnimationMotion)block;
                        if (a.Index != this.CurrAnimation || a.Force)
                            this.SetAnimation((int)a.Index);
                        result = MotionResult.Done;
                        break;

                    case MotionKind.path:
                        result = AnimationUtils.FollowPath(ref this.Translation, ref this.Euler, this.MotionData, block as FollowPathMotion, (float)dt, globals);
                        break;

                    case MotionKind.projectile:
                        result = AnimationUtils.Projectile(ref this.Translation, this.MotionData, block as ProjectileMotion, viewerInput.Time, globals);
                        break;

                    case MotionKind.vertical:
                        result = AnimationUtils.Vertical(ref this.Translation, this.Euler, this.MotionData, block as VerticalMotion, (float)dt);
                        break;

                    case MotionKind.random:
                        result = AnimationUtils.RandomCircle(ref this.Translation, ref this.Euler, this.MotionData, block as RandomCircle, (float)dt, globals);
                        break;

                    case MotionKind.linear:
                        result = AnimationUtils.Linear(ref this.Translation, ref this.Euler, this.MotionData, block as LinearMotion, this.Target, (float)dt, viewerInput.Time);
                        break;

                    case MotionKind.walkToTarget:
                        result = AnimationUtils.WalkToTarget(ref this.Translation, ref this.Euler, this.MotionData, block as WalkToTargetMotion, this.Target, (float)dt, globals);
                        break;

                    case MotionKind.faceTarget:
                        result = AnimationUtils.FaceTarget(ref this.Translation, ref this.Euler, this.MotionData, block as FaceTargetMotion, this.Target, (float)dt, globals);
                        break;

                    case MotionKind.point:
                        result = AnimationUtils.ApproachPoint(ref this.Translation, ref this.Euler, this.MotionData, globals, block as ApproachPointMotion, (float)dt);
                        break;

                    case MotionKind.forward:
                        result = AnimationUtils.Forward(ref this.Translation, ref this.Euler, this.MotionData, globals, block as ForwardMotion, (float)dt);
                        break;

                    case MotionKind.splash:
                        var sm = block as SplashMotion;
                        Vector3 splashScratch;
                        if (sm.Index == -1)
                            splashScratch = this.Translation;
                        else
                            splashScratch = this.Renderers[(int)sm.Index].ModelMatrix.GetColumn(3);

                        if (sm.OnImpact)
                        {
                            double height = SnapUtils.GroundHeightAt(globals, splashScratch);
                            if (splashScratch.y > height)
                            {
                                result = MotionResult.None;
                                break;
                            }
                        }

                        globals.CreateSplash(SplashType.Water, splashScratch, sm.Scale);
                        result = MotionResult.Done;
                        break;

                    case MotionKind.basic:
                        var bm = block as BasicMotion;
                        switch (bm.Subtype)
                        {
                            case BasicMotionKind.Wait:
                                if ((viewerInput.Time - this.MotionData.Start) / 1000.0 > bm.Param)
                                    result = MotionResult.Done;
                                else
                                    result = MotionResult.None;
                                break;

                            case BasicMotionKind.Song:
                                if (SnapUtils.CanHearSong(this.Translation, globals))
                                    result = MotionResult.None;
                                else
                                    result = MotionResult.Done;
                                break;

                            case BasicMotionKind.SetSpeed:
                                this.MotionData.ForwardSpeed = (float)bm.Param;
                                result = MotionResult.Done;
                                break;

                            case BasicMotionKind.Custom:
                                result = this.CustomMotion((long)bm.Param, viewerInput, globals);
                                break;

                            case BasicMotionKind.Loop:
                                this.MotionData.CurrBlock = -1;
                                result = MotionResult.Done;
                                break;

                            case BasicMotionKind.Dynamic:
                                EggDrawCall eggDC = (EggDrawCall)this.Renderers[1].DrawCalls[0];
                                eggDC.Separation += (float)(bm.Param * dt * 30);
                                if (eggDC.Separation >= 1)
                                {
                                    eggDC.Separation = 1;
                                    result = MotionResult.Done;
                                }
                                else
                                {
                                    result = MotionResult.None;
                                }
                                break;
                        }
                        break;
                }

                if (result != MotionResult.None)
                    updated = true;

                if (result == MotionResult.Done)
                {
                    this.MotionData.CurrBlock++;
                    this.MotionData.Start = -1;
                }
                else
                {
                    break;
                }
            }

            if (this.MotionData.CurrBlock >= this.MotionData.CurrMotion.Count && this.MotionData.CurrMotion.Count > 0)
                this.MotionData.StateFlags |= (long)EndCondition.Motion;

            result = this.AuxStep(viewerInput, globals);
            if (result != MotionResult.None)
                updated = true;

            if (result == MotionResult.Done)
            {
                this.CurrAux = 0;
                this.MotionData.StateFlags |= (long)EndCondition.Aux;
            }

            if (updated)
                this.UpdatePositions();

            if ((this.MotionData.StateFlags & (long)EndCondition.Collide) != 0)
                this.Collide(globals);
        }

        protected virtual MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            return MotionResult.None;
        }

        // TODO: figure out the proper order of operations, respect collision flags
        // collision is run twice per frame in game, for all objects simultaneously
        // this seems to work well enough for now
        protected void Collide(LevelGlobals globals)
        {
            for (int i = 0; i < globals.AllActors.Count; i++)
            {
                var other = globals.AllActors[i];

                // collide with all the previous actors, which have finished their motion
                if ((other.MotionData.StateFlags & (long)EndCondition.Collide) == 0)
                    continue;

                if (this == other)
                    break;

                Vector3 delta = other.Center - this.Center;
                float separation = delta.magnitude - other.Def.Radius - this.Def.Radius;

                if (separation > 0)
                    continue;

                other.ReceiveSignal(this, (long)InteractionType.Collided, globals);
                this.ReceiveSignal(other, (long)InteractionType.Collided, globals); // can this break anything?

                // move them apart, though it won't affect the other one until next frame
                delta.y = 0;
                delta.Normalize();

                bool moveBoth = (this.MotionData.StateFlags & (long)EndCondition.AllowBump) != 0 &&
                                (other.MotionData.StateFlags & (long)EndCondition.AllowBump) != 0;

                float magnitude = separation * (moveBoth ? 0.5f : 1f);

                if ((this.MotionData.StateFlags & (long)EndCondition.AllowBump) != 0)
                {
                    Vector3 target = this.Translation + delta * magnitude;
                    if (!SnapUtils.AttemptMove(ref this.Translation, target, this.MotionData, globals, (long)MoveFlags.Ground))
                        this.UpdatePositions();
                }

                if ((other.MotionData.StateFlags & (long)EndCondition.AllowBump) != 0)
                {
                    Vector3 target = other.Translation - delta * magnitude;
                    if (!SnapUtils.AttemptMove(ref other.Translation, target, other.MotionData, globals, (long)MoveFlags.Ground))
                        other.UpdatePositions();
                }
            }
        }

    }

}
