using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class BasisVectors
    {
        public static readonly Vector3 Vec3Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 Vec3One = new Vector3(1, 1, 1);
        public static readonly Vector3 Vec3UnitX = new Vector3(1, 0, 0);
        public static readonly Vector3 Vec3UnitY = new Vector3(0, 1, 0);
        public static readonly Vector3 Vec3UnitZ = new Vector3(0, 0, 1);
        public static readonly Vector3 Vec3NegX = new Vector3(-1, 0, 0);
        public static readonly Vector3 Vec3NegY = new Vector3(0, -1, 0);
        public static readonly Vector3 Vec3NegZ = new Vector3(0, 0, -1);

        public static readonly Matrix4x4 Mat4Identity = Matrix4x4.identity;
    }
}
