using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.PokemonSnap64
{
    using UnityEngine;
    using UnityEditor;

    [ExecuteInEditMode]
    public class PKSnap_BoneVertexDebugger : MonoBehaviour
    {
        public SkinnedMeshRenderer smr;
        public Transform targetBone;
        public bool debugVertices;
        public float gizmoSize = 0.01f;
        public Color gizmoColor = Color.red;

        private void Reset()
        {
            targetBone = this.transform;
            if (transform.TryGetComponentInParent(out PKSnap_Actor actor))
            {
                smr = actor.SkinnedMeshRenderer;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!debugVertices || smr == null || targetBone == null || smr.sharedMesh == null)
                return;

            Mesh mesh = smr.sharedMesh;
            BoneWeight[] boneWeights = mesh.boneWeights;
            Vector3[] vertices = mesh.vertices;

            int boneIndex = System.Array.IndexOf(smr.bones, targetBone);
            if (boneIndex < 0)
            {
                Debug.LogWarning("Bone not found on SkinnedMeshRenderer.");
                return;
            }

            Matrix4x4 localToWorld = smr.transform.localToWorldMatrix;
            Matrix4x4[] bindposes = mesh.bindposes;

            int count = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                BoneWeight bw = boneWeights[i];

                if (bw.boneIndex0 == boneIndex && bw.weight0 > 0f)
                {
                    // Transformar el vértice desde bind pose
                    Vector3 localVertex = vertices[i];
                    Matrix4x4 boneMatrix = targetBone.localToWorldMatrix * bindposes[boneIndex];
                    Vector3 worldPos = boneMatrix.MultiplyPoint3x4(localVertex);

                    Gizmos.color = gizmoColor;
                    Gizmos.DrawSphere(worldPos, gizmoSize);
                    count++;
                }
            }

            Handles.Label(targetBone.position + Vector3.up * 0.05f, $"Influenced vertices: {count}");
        }
    }

}
