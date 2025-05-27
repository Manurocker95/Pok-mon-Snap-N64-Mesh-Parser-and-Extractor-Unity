using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_ObjectData : MonoBehaviour
    {
        public long ID;
        public long Behaviour;
        public PKSnap_TrackPath Path;

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Path.Kind == PathKind.BSpline)
            {
                /*PKSnap_QuarticSplineDrawer.DrawQuarticSpline(Path.Points, Path.Quartics, Path.Times);

                for (int i = 0; i < Path.Points.Length; i += 3)
                {
                    Vector3 p = new Vector3(Path.Points[i], Path.Points[i + 1], Path.Points[i + 2]);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(p, 0.05f);
                }*/
            }
#endif
        }
    }
}