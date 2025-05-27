using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class AnimationUtils
    {
        public static bool StepYawTowards(ref Vector3 euler, float target, float maxTurn, float dt)
        {
            float dist = (float)MathHelper.AngleDist(euler.y, target);
            float maxStep = maxTurn * dt * 30f;
            euler.y += Mathf.Clamp(dist, -maxStep, maxStep);
            return Mathf.Abs(dist) < maxStep;
        }

        public static MotionResult FollowPath(ref Vector3 pos, ref Vector3 euler, MotionData data, FollowPathMotion block, float dt, LevelGlobals globals)
        {
            if ((data.StateFlags & (long)EndCondition.Pause) != 0 || data.Path == null)
                return MotionResult.None;

            float speed = SnapUtils.LookupValue(data, block.Speed);
            data.PathParam += 30f * speed * dt / data.Path.Duration;

            float end = SnapUtils.LookupValue(data, block.End);
            if (block.Start == PathStart.StoredSegment)
                end = (float)data.Path.Times[(int)data.StoredValues[1]];
            else if (block.Start == PathStart.FirstSegment)
                end = (float)data.Path.Times[1];

            if (end > 0 && data.PathParam > end)
                return MotionResult.Done;

            data.PathParam %= 1f;

            float oldY = pos.y;
            GetPathPoint(ref pos, data.Path, data.PathParam);

            if ((block.Flags & (long)MoveFlags.ConstHeight) != 0)
            {
                pos.y = oldY;
            }
            else if ((block.Flags & (long)MoveFlags.Ground) != 0)
            {
                pos.y = (float)SnapUtils.FindGroundHeight(globals.Level.Collision, pos.x, pos.z);
            }

            if ((block.Flags & (long)(MoveFlags.SmoothTurn | MoveFlags.SnapTurn)) != 0)
            {
                Vector3 tangent = Vector3.zero;
                GetPathTangent(ref tangent, data.Path, data.PathParam);
                float yaw = Mathf.Atan2(tangent.x, tangent.z);
                if ((block.Flags & (long)MoveFlags.SnapTurn) != 0)
                    euler.y = yaw;
                else
                    StepYawTowards(ref euler, yaw, block.MaxTurn, dt);
            }

            return MotionResult.Update;
        }

        public static MotionResult Projectile(ref Vector3 pos, MotionData data, ProjectileMotion block, float tMillis, LevelGlobals globals)
        {
            float t = (tMillis - data.Start) / 1000f;

            Vector3 tangent = new Vector3(
                Mathf.Sin(data.MovingYaw) * data.ForwardSpeed,
                block.ySpeed,
                Mathf.Cos(data.MovingYaw) * data.ForwardSpeed
            );

            Vector3 posScratch = data.StartPos + tangent * t;
            posScratch.y += block.G * t * t * 15f;

            if (block.MoveForward)
                SnapUtils.AttemptMove(ref pos, posScratch, data, globals, 0);

            pos.y = posScratch.y;

            if ((30f * t * block.G + block.ySpeed) < 0 && pos.y < data.GroundHeight)
            {
                pos.y = data.GroundHeight;
                return MotionResult.Done;
            }

            return MotionResult.Update;
        }

        public static MotionResult Vertical(ref Vector3 pos, Vector3 euler, MotionData data, VerticalMotion block, float dt)
        {
            if ((data.StateFlags & (long)EndCondition.Pause) != 0)
                return MotionResult.None;

            pos.x += Mathf.Sin(euler.y) * data.ForwardSpeed * dt;
            pos.y += data.YSpeed * dt * (long)block.Direction;
            pos.z += Mathf.Cos(euler.y) * data.ForwardSpeed * dt;

            if (block.G != 0f)
            {
                data.YSpeed += 30f * block.G * dt;
                data.YSpeed = Mathf.Clamp(data.YSpeed, block.MinVel, block.MaxVel);
            }

            float target = SnapUtils.LookupValue(data, block.Target);

            bool reachedTarget = block.AsDelta
                ? Mathf.Abs(data.StartPos.y - pos.y) >= target
                : ((pos.y > target) == (block.Direction > 0));

            return reachedTarget ? MotionResult.Done : MotionResult.Update;
        }

        public static MotionResult RandomCircle(ref Vector3 pos, ref Vector3 euler, MotionData data, RandomCircle block, float dt, LevelGlobals globals)
        {
            data.MovingYaw += dt * data.ForwardSpeed / block.Radius;

            Vector3 posScratch = new Vector3
            {
                x = data.RefPosition.x + block.Radius * Mathf.Sin(data.MovingYaw),
                z = data.RefPosition.z + block.Radius * Mathf.Cos(data.MovingYaw),
                y = pos.y 
            };

            if (SnapUtils.AttemptMove(ref pos, posScratch, data, globals, (long)MoveFlags.Ground))
                return MotionResult.Done;

            StepYawTowards(ref euler, data.MovingYaw + Mathf.PI / 2f, block.MaxTurn, dt);
            return MotionResult.Update;
        }

        public static MotionResult Linear(ref Vector3 pos, ref Vector3 euler, MotionData data, LinearMotion block, Target target, float dt, float currentTime)
        {
            pos += block.Velocity * dt;

            if (block.MatchTarget && target != null)
            {
                float lerpFactor = (currentTime - data.Start) / 1000f / block.Duration;
                Vector3 linearScratch = Vector3.Lerp(data.StartPos, target.Translation, lerpFactor);
                pos.x = linearScratch.x;
                pos.z = linearScratch.z;
            }

            euler.y += block.TurnSpeed * dt;

            if ((currentTime - data.Start) / 1000f >= block.Duration)
                return MotionResult.Done;

            return MotionResult.Update;
        }

        public static MotionResult WalkToTarget(ref Vector3 pos, ref Vector3 euler, MotionData data, WalkToTargetMotion block, Target target, float dt, LevelGlobals globals)
        {
            if ((block.Flags & (long)MoveFlags.Update) != 0)
            {
                if (target == null)
                    return MotionResult.Done;

                data.RefPosition = target.Translation;
            }

            var yawToTarget = SnapUtils.YawTowards(data.RefPosition, pos) + (block.Away ? Mathf.PI : 0);

            Vector3 nextPos = pos;
            nextPos.x += data.ForwardSpeed * dt * Mathf.Sin((float)yawToTarget);
            nextPos.z += data.ForwardSpeed * dt * Mathf.Cos((float)yawToTarget);

            if (SnapUtils.AttemptMove(ref pos, nextPos, data, globals, block.Flags))
                return MotionResult.Done;

            StepYawTowards(ref euler, (float)yawToTarget, block.MaxTurn, dt);

            float dist = Vector3.Distance(data.RefPosition, pos);
            bool shouldFinish = (dist < block.Radius) != block.Away;

            if (shouldFinish)
            {
                data.StateFlags |= (long)EndCondition.Target;
                return MotionResult.Done;
            }

            return MotionResult.Update;
        }

        public static MotionResult FaceTarget(ref Vector3 pos, ref Vector3 euler, MotionData data, FaceTargetMotion block, Target target, float dt, LevelGlobals globals)
        {
            if ((block.Flags & (long)MoveFlags.Update) != 0)
            {
                if ((block.Flags & (long)MoveFlags.FacePlayer) != 0)
                {
                    data.RefPosition = globals.Translation;
                }
                else if (target == null)
                {
                    return MotionResult.Done;
                }
                else
                {
                    data.RefPosition = target.Translation;
                }
            }

            if ((block.Flags & (long)MoveFlags.DuringSong) != 0 && !SnapUtils.CanHearSong(pos, globals))
                return MotionResult.Done;

            float targetYaw = (float)SnapUtils.YawTowards(data.RefPosition, pos);
            if ((block.Flags & (long)MoveFlags.FaceAway) != 0)
                targetYaw += Mathf.PI;

            bool aligned = StepYawTowards(ref euler, targetYaw, block.MaxTurn, dt);

            if (aligned && (block.Flags & (long)MoveFlags.Continuous) == 0)
                return MotionResult.Done;

            return MotionResult.Update;
        }

        public static MotionResult ApproachPoint(ref Vector3 pos, ref Vector3 euler, MotionData data, LevelGlobals globals, ApproachPointMotion block, float dt)
        {
            if (block.Destination == Destination.Player)
                data.Destination = globals.Translation;

            float dist = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(data.Destination.x, data.Destination.z));
            bool atPoint = dist < data.ForwardSpeed * dt;

            if (atPoint)
                data.StateFlags |= (long)EndCondition.Target;

            bool done = false;
            switch (block.Goal)
            {
                case ApproachGoal.AtPoint:
                    done = atPoint;
                    if (done)
                    {
                        pos = data.Destination;
                        if ((block.Flags & (long)MoveFlags.Ground) != 0)
                            pos.y = (float)SnapUtils.FindGroundHeight(globals.Level.Collision, pos.x, pos.z);
                    }
                    break;
                case ApproachGoal.GoodGround:
                    done = data.GroundType != 0x7F6633;
                    break;
                case ApproachGoal.Radius:
                    done = dist < 500;
                    break;
            }

            if (done)
                return MotionResult.Done;

            Vector3 delta = data.Destination - pos;
            delta.y = 0;
            float targetYaw = Mathf.Atan2(delta.x, delta.z);

            delta = delta.normalized * (dt * data.ForwardSpeed);
            Vector3 newPos = pos + delta;

            if (SnapUtils.AttemptMove(ref pos, newPos, data, globals, block.Flags))
                return MotionResult.None;

            StepYawTowards(ref euler, targetYaw, block.MaxTurn, dt);
            return MotionResult.Update;
        }

        public static MotionResult Forward(ref Vector3 pos, ref Vector3 euler, MotionData data, LevelGlobals globals, ForwardMotion block, float dt)
        {
            Vector3 direction = new Vector3(Mathf.Sin(euler.y), 0, Mathf.Cos(euler.y));
            Vector3 targetPos = pos + direction * (dt * data.ForwardSpeed);

            if (block.StopIfBlocked)
            {
                if (SnapUtils.AttemptMove(ref pos, targetPos, data, globals, (long)MoveFlags.Ground))
                    return MotionResult.Done;
            }
            else
            {
                pos = targetPos;
            }

            return MotionResult.Update;
        }

        public static MotionResult MotionBlockInit(MotionData data, Vector3 pos, Vector3 euler, ViewerRenderInput viewerInput, Target target)
        {
            data.Start = viewerInput.Time;
            data.StartPos = pos;

            Motion block = data.CurrMotion[data.CurrBlock];
            Vector3 scratch = Vector3.zero;

            switch (block.Kind)
            {
                case MotionKind.path:
                    var bp = (FollowPathMotion)block;
                    switch (bp.Start)
                    {
                        case PathStart.Resume:
                            break;
                        case PathStart.Begin:
                        case PathStart.FirstSegment:
                            data.PathParam = 0f;
                            break;
                        case PathStart.Random:
                            data.PathParam = UnityEngine.Random.value;
                            break;
                        case PathStart.SkipFirst:
                            data.PathParam = (float)data.Path.Times[1];
                            break;
                        case PathStart.StoredSegment:
                            data.PathParam = (float)data.Path.Times[(int)data.StoredValues[0]];
                            break;
                    }
                    break;

                case MotionKind.projectile:
                    var pj = (ProjectileMotion)block;
                    switch (pj.Direction)
                    {
                        case Direction.Forward:
                            break;
                        case Direction.Backward:
                            data.MovingYaw = euler.y + Mathf.PI;
                            break;
                        case Direction.Constant:
                            data.MovingYaw = pj.Yaw;
                            euler.y = pj.Yaw;
                            break;
                        case Direction.Impact:
                            data.MovingYaw = Mathf.Atan2(data.LastImpact.x, data.LastImpact.z);
                            break;
                        case Direction.PathStart:
                        case Direction.PathEnd:
                             GetPathPoint(ref scratch, data.Path, pj.Direction == Direction.PathStart ? 0 : 1);
                            if (pj.Direction == Direction.PathEnd)
                                data.MovingYaw = (float)SnapUtils.YawTowards(scratch, pos);
                            else
                            {
                                pos = scratch;
                                data.StartPos = scratch;
                            }
                            break;
                    }
                    break;

                case MotionKind.vertical:
                    var v = (VerticalMotion)block;
                    data.YSpeed = v.StartSpeed;
                    break;

                case MotionKind.random:
                    var r = (RandomCircle)block;
                    float centerAngle = euler.y + Mathf.PI * (1 + 2f / 3f * Mathf.Floor(UnityEngine.Random.value * 3));
                    data.RefPosition = new Vector3(
                        pos.x + r.Radius * Mathf.Sin(centerAngle),
                        pos.y,
                        pos.z + r.Radius * Mathf.Cos(centerAngle)
                    );
                    data.MovingYaw = centerAngle - Mathf.PI;
                    break;

                case MotionKind.walkToTarget:
                    if (target == null)
                        return MotionResult.Done;
                    data.RefPosition = target.Translation;
                    break;

                case MotionKind.faceTarget:
                    var ft = (FaceTargetMotion)block;
                    if ((block.Flags & (long)MoveFlags.FacePlayer) != 0)
                        data.RefPosition = viewerInput.Camera.transform.position;
                    else if (target != null)
                        data.RefPosition = target.Translation;
                    else
                        return MotionResult.Done;
                    break;

                case MotionKind.point:
                    var pt = (ApproachPointMotion)block;
                    if (pt.Destination == Destination.Target)
                        data.Destination = target.Translation;
                    else if (pt.Destination == Destination.PathStart)
                       GetPathPoint(ref data.Destination, data.Path, 0);
                    break;
            }

            return MotionResult.None;
        }

        public static void GetPathPoint(ref Vector3 dst, TrackPath path, float t, bool useRaw = false)
        {
            int segment = 0;
            while (segment + 1 < path.Length && t > (float)path.Times[segment + 1])
                segment++;

            float time0 = (float)path.Times[segment];
            float time1 = (float)path.Times[segment + 1];
            float frac = (t - time0) / (time1 - time0);

            int offs = segment * (path.Kind == PathKind.Bezier ? 12 : 3); // 4 points * 3 components = 12

            switch (path.Kind)
            {
                case PathKind.Linear:
                    dst.x = Mathf.Lerp((float)path.Points[offs + 0], (float)path.Points[offs + 3], frac);
                    dst.y = Mathf.Lerp((float)path.Points[offs + 1], (float)path.Points[offs + 4], frac);
                    dst.z = Mathf.Lerp((float)path.Points[offs + 2], (float)path.Points[offs + 5], frac);
                    break;

                case PathKind.Bezier:
                    dst.x = SplineUtils.GetPointBezier((float)path.Points[offs + 0], (float)path.Points[offs + 3], (float)path.Points[offs + 6], (float)path.Points[offs + 9], frac);
                    dst.y = SplineUtils.GetPointBezier((float)path.Points[offs + 1], (float)path.Points[offs + 4], (float)path.Points[offs + 7], (float)path.Points[offs + 10], frac);
                    dst.z = SplineUtils.GetPointBezier((float)path.Points[offs + 2], (float)path.Points[offs + 5], (float)path.Points[offs + 8], (float)path.Points[offs + 11], frac);
                    break;

                case PathKind.BSpline:
                    dst.x = SplineUtils.GetPointBspline((float)path.Points[offs + 0], (float)path.Points[offs + 3], (float)path.Points[offs + 6], (float)path.Points[offs + 9], frac);
                    dst.y = SplineUtils.GetPointBspline((float)path.Points[offs + 1], (float)path.Points[offs + 4], (float)path.Points[offs + 7], (float)path.Points[offs + 10], frac);
                    dst.z = SplineUtils.GetPointBspline((float)path.Points[offs + 2], (float)path.Points[offs + 5], (float)path.Points[offs + 8], (float)path.Points[offs + 11], frac);
                    break;

                case PathKind.Hermite:
                    dst.x = SplineUtils.GetPointHermite((float)path.Points[offs + 3], (float)path.Points[offs + 6],
                        ((float)path.Points[offs + 6] - (float)path.Points[offs + 0]) * path.SegmentRate,
                        ((float)path.Points[offs + 9] - (float)path.Points[offs + 3]) * path.SegmentRate, frac);
                    dst.y = SplineUtils.GetPointHermite((float)path.Points[offs + 4], (float)path.Points[offs + 7],
                        ((float)path.Points[offs + 7] - (float)path.Points[offs + 1]) * path.SegmentRate,
                        ((float)path.Points[offs + 10] - (float)path.Points[offs + 4]) * path.SegmentRate, frac);
                    dst.z = SplineUtils.GetPointHermite((float)path.Points[offs + 5], (float)path.Points[offs + 8],
                        ((float)path.Points[offs + 8] - (float)path.Points[offs + 2]) * path.SegmentRate,
                        ((float)path.Points[offs + 11] - (float)path.Points[offs + 5]) * path.SegmentRate, frac);
                    break;
            }

            if (!useRaw)
                dst *= 100.0f;
        }

        public static void GetPathTangent(ref Vector3 dst, TrackPath path, float t)
        {
            int segment = 0;
            while (segment + 1 < path.Length && t > (float)path.Times[segment + 1])
                segment++;

            float frac = (t - (float)path.Times[segment]) / ((float)path.Times[segment + 1] - (float)path.Times[segment]);

            int offset = segment * (path.Kind == PathKind.Bezier ? 9 : 3);

            switch (path.Kind)
            {
                case PathKind.Linear:
                    dst.x = (float)path.Points[offset + 3] - (float)path.Points[offset];
                    dst.y = (float)path.Points[offset + 4] - (float)path.Points[offset + 1];
                    dst.z = (float)path.Points[offset + 5] - (float)path.Points[offset + 2];
                    break;

                case PathKind.Bezier:
                    dst.x = SplineUtils.GetDerivativeBezier((float)path.Points[offset], (float)path.Points[offset + 3], (float)path.Points[offset + 6], (float)path.Points[offset + 9], frac);
                    dst.y = SplineUtils.GetDerivativeBezier((float)path.Points[offset + 1], (float)path.Points[offset + 4], (float)path.Points[offset + 7], (float)path.Points[offset + 10], frac);
                    dst.z = SplineUtils.GetDerivativeBezier((float)path.Points[offset + 2], (float)path.Points[offset + 5], (float)path.Points[offset + 8], (float)path.Points[offset + 11], frac);
                    break;

                case PathKind.BSpline:
                    dst.x = SplineUtils.GetDerivativeBspline((float)path.Points[offset], (float)path.Points[offset + 3], (float)path.Points[offset + 6], (float)path.Points[offset + 9], frac);
                    dst.y = SplineUtils.GetDerivativeBspline((float)path.Points[offset + 1], (float)path.Points[offset + 4], (float)path.Points[offset + 7], (float)path.Points[offset + 10], frac);
                    dst.z = SplineUtils.GetDerivativeBspline((float)path.Points[offset + 2], (float)path.Points[offset + 5], (float)path.Points[offset + 8], (float)path.Points[offset + 11], frac);
                    break;

                case PathKind.Hermite:
                    dst.x = SplineUtils.GetDerivativeHermite(
                        (float)path.Points[offset + 3],
                        (float)path.Points[offset + 6],
                        ((float)path.Points[offset + 6] - (float)path.Points[offset]) * path.SegmentRate,
                        ((float)path.Points[offset + 9] - (float)path.Points[offset + 3]) * path.SegmentRate,
                        frac);
                    dst.y = SplineUtils.GetDerivativeHermite(
                        (float)path.Points[offset + 4],
                        (float)path.Points[offset + 7],
                        ((float)path.Points[offset + 7] - (float)path.Points[offset + 1]) * path.SegmentRate,
                        ((float)path.Points[offset + 10] - (float)path.Points[offset + 4]) * path.SegmentRate,
                        frac);
                    dst.z = SplineUtils.GetDerivativeHermite(
                        (float)path.Points[offset + 5],
                        (float)path.Points[offset + 8],
                        ((float)path.Points[offset + 8] - (float)path.Points[offset + 2]) * path.SegmentRate,
                        ((float)path.Points[offset + 11] - (float)path.Points[offset + 5]) * path.SegmentRate,
                        frac);
                    break;
            }
        }

    }
}
