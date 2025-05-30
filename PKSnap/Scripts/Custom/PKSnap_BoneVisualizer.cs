using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VirtualPhenix.PokemonSnap64
{
    [ExecuteAlways]
    public class PKSnap_BoneVisualizer : MonoBehaviour
    {
        public Transform[] bones;
        public Transform rootBone;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer referenceMehs;
        public bool drawMeshVertices = true;
        public float gizmoSize = 0.01f;
        public float lineSize = 0.01f;
        public Vector3 DebugMatrixVal = new Vector3(1, 1, 1);
        public Matrix4x4 a;

        private void Reset()
        {
            var skr = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skr.Length == 1)
            {
                skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                rootBone = transform.childCount > 1 ? transform.GetChild(1) : transform;
                AutoFillBones();
            }
          
        }
        
        public static GameObject CombineSkinnedMeshes(List<SkinnedMeshRenderer> sources, string name = "CombinedMesh")
        {
            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<BoneWeight> boneWeights = new();
            List<Matrix4x4> bindposes = new();
            List<Transform> bones = new();
            List<int> triangles = new();

            int vertexOffset = 0;
            int boneOffset = 0;

            foreach (var smr in sources)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null) continue;

                // Copy vertex data
                vertices.AddRange(mesh.vertices);
                normals.AddRange(mesh.normals);
                uvs.AddRange(mesh.uv);

                // Copy triangles with offset
                var tris = mesh.GetTriangles(0);
                for (int i = 0; i < tris.Length; i++)
                    triangles.Add(tris[i] + vertexOffset);

                // Reindex bone weights
                foreach (var bw in mesh.boneWeights)
                {
                    BoneWeight b = new BoneWeight
                    {
                        boneIndex0 = bw.boneIndex0 + boneOffset,
                        boneIndex1 = bw.boneIndex1 + boneOffset,
                        boneIndex2 = bw.boneIndex2 + boneOffset,
                        boneIndex3 = bw.boneIndex3 + boneOffset,
                        weight0 = bw.weight0,
                        weight1 = bw.weight1,
                        weight2 = bw.weight2,
                        weight3 = bw.weight3,
                    };
                    boneWeights.Add(b);
                }

                // Copy bindposes and bones
                foreach (var bp in mesh.bindposes)
                    bindposes.Add(bp);
                bones.AddRange(smr.bones);

                vertexOffset += mesh.vertexCount;
                boneOffset += smr.bones.Length;
            }

            // Create the combined mesh
            Mesh combinedMesh = new Mesh();
            combinedMesh.name = name;
            combinedMesh.SetVertices(vertices);
            combinedMesh.SetNormals(normals);
            combinedMesh.SetUVs(0, uvs);
            combinedMesh.SetTriangles(triangles, 0);
            combinedMesh.boneWeights = boneWeights.ToArray();
            combinedMesh.bindposes = bindposes.ToArray();
            combinedMesh.RecalculateBounds();

            // Create GameObject and SkinnedMeshRenderer
            GameObject go = new GameObject(name);
            var smrCombined = go.AddComponent<SkinnedMeshRenderer>();
            smrCombined.sharedMesh = combinedMesh;
            smrCombined.bones = bones.ToArray();
            smrCombined.rootBone = sources[0].rootBone;
            smrCombined.materials = sources[0].materials;

            return go;
        }

        [ContextMenu("Combine meshes")]
        public void CombineMeshes()
        {
            CombineSkinnedMeshes(GetComponentsInChildren<SkinnedMeshRenderer>().ToList(), "[Actor 16 Mesh]");
        }

        [ContextMenu("Copy Binds")]
        public void CopyBinds()
        {
            skinnedMeshRenderer.sharedMesh.bindposes = referenceMehs.sharedMesh.bindposes;
        }

        [ContextMenu("Debug Bone Weights")]
        public void DebugBoneWeights()
        {
            foreach (var sk in transform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Debug.Log("==============");
                Debug.Log(sk.name);
                foreach (var bw in sk.sharedMesh.boneWeights)
                {
                    Debug.Log("B0:"+bw.boneIndex0);
                    Debug.Log("B1:" + bw.boneIndex1);
                    Debug.Log("B2:" + bw.boneIndex2);
                    Debug.Log("B3:" +bw.boneIndex3);
                    Debug.Log("W0:" + bw.weight0);
                    Debug.Log("W1:" + bw.weight1);
                    Debug.Log("W2:" + bw.weight2);
                    Debug.Log("W3:" + bw.weight3);
                }
                Debug.Log("==============");
            }
        }

        [ContextMenu("Recalculate Bind Poses")]
        public void RecalculateBindPoses()
        {
            var smr = GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null || smr.bones == null || smr.bones.Length == 0)
            {
                Debug.LogWarning("SkinnedMeshRenderer o huesos no definidos.");
                return;
            }

            Transform rootBone = smr.rootBone;
            if (rootBone == null)
            {
                Debug.LogWarning("RootBone no asignado.");
                return;
            }

            Matrix4x4[] newBindPoses = new Matrix4x4[smr.bones.Length];
            for (int i = 0; i < smr.bones.Length; i++)
            {
                a = Matrix4x4.identity;
               // a.m00 *= 0.01f;
               // a.m11 *= 0.01f;
               // a.m22 *= 0.01f;
               // a.m33 *= 0.01f;
                Matrix4x4 scaleFix = Matrix4x4.Scale(DebugMatrixVal);
                Matrix4x4 bindPose = a;
                newBindPoses[i] = bindPose;// Matrix4x4.identity;//smr.bones[i].worldToLocalMatrix * rootBone.localToWorldMatrix;
            }

            var mesh = smr.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("No hay mesh en el SkinnedMeshRenderer.");
                return;
            }

            mesh.bindposes = newBindPoses;
            Debug.Log("Bindposes recalculados correctamente.");
        }

        [ContextMenu("Auto-Fill Bones from RootBone")]
        public void AutoFillBones()
        {
            if (rootBone == null)
            {
                Debug.LogWarning("BoneAutoFiller: RootBone is not assigned.");
                return;
            }

            List<Transform> collectedBones = new List<Transform>();
            CollectBonesRecursive(rootBone, collectedBones);
            bones = collectedBones.ToArray();

            Debug.Log($"BoneAutoFiller: Auto-filled {bones.Length} bones.");
        }

        private void CollectBonesRecursive(Transform current, List<Transform> list)
        {
            list.Add(current);
            foreach (Transform child in current)
            {
                CollectBonesRecursive(child, list);
            }
        }
        private void OnDrawGizmos()
        {
            if (bones == null || bones.Length == 0 || rootBone == null)
                return;

            if (bones == null || bones.Length == 0)
                return;

#if UNITY_EDITOR
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                if (bone == null) continue;


                Handles.color = Color.red;
                if (Handles.Button(bone.position, Quaternion.identity, gizmoSize, gizmoSize, Handles.SphereHandleCap))
                {
                    Selection.activeTransform = bone;
                }

        
                if (bone.parent != null && System.Array.IndexOf(bones, bone.parent) >= 0)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(bone.position, bone.parent.position);
                }
            }
#endif
        }
    }

}