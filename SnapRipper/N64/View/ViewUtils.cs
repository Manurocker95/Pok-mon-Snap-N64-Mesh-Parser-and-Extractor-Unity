using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public static class ViewUtils 
    {
        public static bool ContainsSphereInFrustum(Camera camera, Vector3 center, float radius)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            foreach (Plane plane in planes)
            {
                float distance = plane.GetDistanceToPoint(center);

                if (distance < -radius)
                    return false;
            }

            return true;
        }
    }
}
