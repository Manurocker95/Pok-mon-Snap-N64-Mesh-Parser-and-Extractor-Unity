using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class RSPVertex
    {
        // Vertex Coordinates
        public double x, y, z;
        // Texture coordinates.
        public double tx, ty;
        //  Color or normals.
        public double c0, c1, c2;
        // Alpha
        public double a = 0;
        // Pretend
        public long matrixIndex;

        public RSPVertex()
        {

        }

        public virtual void Copy(RSPVertex v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.matrixIndex = v.matrixIndex;
            this.tx = v.tx;
            this.ty = v.ty;
            this.c0 = v.c0;
            this.c1 = v.c1;
            this.c2 = v.c2;
            this.a = v.a;
        }
    }

}