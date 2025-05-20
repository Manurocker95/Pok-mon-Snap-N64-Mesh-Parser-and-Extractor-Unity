#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VirtualPhenix.PokemonSnap64
{
    public static class PKSnap_QuarticSplineDrawer
    {
        public static void DrawQuarticSpline(float[] points, float[] quartics, float[] times = null, int resolution = 20)
        {
            if (points == null || points.Length < 3)
                return;

            int pointCount = points.Length / 3;
            Vector3[] vecPoints = new Vector3[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                vecPoints[i] = new Vector3(points[i * 3], points[i * 3 + 1], points[i * 3 + 2]);
            }

            int quarticSets = (quartics?.Length ?? 0) / 5; // 5 coef por segmento
            int segments = Mathf.Min(quarticSets, vecPoints.Length - 4); // 5 puntos por tramo

            if (segments <= 0)
                return;

            Handles.color = Color.green;

            for (int seg = 0; seg < segments; seg++)
            {
                List<Vector3> segmentCurve = new List<Vector3>();

                for (int i = 0; i <= resolution; i++)
                {
                    float t = i / (float)resolution;

                    Vector3 result = Vector3.zero;
                    for (int j = 0; j < 5; j++)
                    {
                        int pointIndex = seg + j;
                        float coeff = quartics[seg * 5 + j];
                        result += vecPoints[pointIndex] * coeff;
                    }

                    segmentCurve.Add(result);
                }

                Handles.DrawAAPolyLine(2f, segmentCurve.ToArray());
            }
        }
    }

}
#endif