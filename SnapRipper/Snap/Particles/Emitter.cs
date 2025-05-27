using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
   
    public class Emitter
    {
        public Vector3[] EmitScratch = { Vector3.zero, Vector3.zero, Vector3.zero };
        public Matrix4x4 EmitMatrix = Matrix4x4.identity;

        public EmitterData Data;
        public Matrix4x4? SourceMatrix;
        public Vector3 Position = Vector3.zero;
        public double Timer = -1;

        private double Accumulator = 0;

        public void Activate(EmitterData data, Matrix4x4? mat)
        {
            this.Data = data;
            this.SourceMatrix = mat;
            this.Timer = data.Lifetime;
            this.Accumulator = 0;
        }

        private static float Compute(float x, float? random = null)
        {
            if (x < 0)
                return -x;
            if (random.HasValue)
                return x * random.Value;
            return x * UnityEngine.Random.value;
        }

        public void Update(float dt, ParticleManager manager)
        {
            this.Accumulator += Compute((float)this.Data.Increment) * dt;
            if (this.Accumulator >= 1)
            {
                EmitScratch[0] = this.Data.Velocity;
                if (this.SourceMatrix.HasValue)
                {
                    MathHelper.TransformVec3Mat4W0(ref EmitScratch[0], this.SourceMatrix.Value, EmitScratch[0]);
                    this.Position = this.SourceMatrix.Value.GetColumn(3);
                }

                EmitMatrix = MathHelper.TargetTo(Vector3.zero, EmitScratch[0], Vector3.right);

                while (this.Accumulator >= 1)
                {
                    float phi = (float)(UnityEngine.Random.value * MathConstants.Tau);
                    EmitScratch[0] = new Vector3(
                        Mathf.Cos(phi),
                        Mathf.Sin(phi),
                        0
                    );
                    float radiusScale = this.Data.Radius < 0 ? 1 : UnityEngine.Random.value;
                    EmitScratch[0] *= Compute((float)Data.Radius, radiusScale);
                    MathHelper.TransformVec3Mat4W0(ref EmitScratch[0], EmitMatrix, EmitScratch[0]);
                    EmitScratch[0] += this.Position;

                    float spread = Compute((float)Data.SprayAngle, radiusScale);
                    EmitScratch[1] = new Vector3(
                        Mathf.Cos(phi) * Mathf.Sin(spread),
                        Mathf.Sin(phi) * Mathf.Sin(spread),
                        -Mathf.Cos(spread)
                    );
                    EmitScratch[1] *= this.Data.Velocity.magnitude;
                    MathHelper.TransformVec3Mat4W0(ref EmitScratch[1], EmitMatrix, EmitScratch[1]);

                    manager.CreateParticle(this.Data.IsCommon, (int)this.Data.Index, EmitScratch[0], EmitScratch[1]);
                    this.Accumulator -= 1;
                }
            }

            this.Timer -= dt;
        }
    }

}
