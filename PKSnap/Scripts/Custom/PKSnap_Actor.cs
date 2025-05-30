using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using VirtualPhenix.Nintendo64;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Actor : MonoBehaviour
    {
        [SerializeField] private long m_ID;
        [SerializeField] private List<Texture2D> m_textures;
        [SerializeField] private SkinnedMeshRenderer m_skinnedMeshRenderer;
        [SerializeField] private ModelRenderer m_modelRenderer;

        public long ID { get { return m_ID; } }
        public List<Texture2D> Texturs { get { return m_textures; } }
        public SkinnedMeshRenderer SkinnedMeshRenderer { get { return m_skinnedMeshRenderer; } }

        public void InitActor(long id, List<Texture2D> textures, bool _visible, SkinnedMeshRenderer _skinnedMeshRenderer = null, ModelRenderer _modelRenderer = null)
        {
            m_ID = id;
            m_textures = new List<Texture2D>(textures);
            gameObject.SetActive(_visible);
            m_skinnedMeshRenderer = _skinnedMeshRenderer;
            m_modelRenderer = _modelRenderer;
        }
    

#if UNITY_EDITOR
        [MenuItem("CONTEXT/PKSnap_Actor/Export Textures")]
        private static void ExportTextures(MenuCommand command)
        {
            PKSnap_Actor actor = (PKSnap_Actor)command.context;

            string path = EditorUtility.OpenFolderPanel("Select Export Folder", "Assets", "");

            // Ensure path is within Assets and not null/empty
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);

                for (int i = 0; i < actor.m_textures.Count; i++)
                {
                    Texture2D texture = actor.m_textures[i];
                    if (texture == null) continue;

                    byte[] pngData = texture.EncodeToPNG();
                    if (pngData != null)
                    {
                        string fileName = $"Texture_{i}_{texture.name}.png";
                        string fullPath = Path.Combine(relativePath, fileName);
                        File.WriteAllBytes(fullPath, pngData);
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log($"Exported {actor.m_textures.Count} textures to {relativePath}");
            }
            else
            {
                Debug.LogWarning("Export cancelled or folder not inside Assets.");
            }
        }

        [MenuItem("CONTEXT/PKSnap_Actor/Export Mesh")]
        private static void ExportMesh(MenuCommand command)
        {
            Mesh m = null;
            PKSnap_Actor actor = (PKSnap_Actor)command.context;
            if (!actor.SkinnedMeshRenderer)
            {
                MeshFilter meshFilter = actor.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    Debug.LogWarning("No MeshFilter or Mesh found to export.");
                    return;
                }
                m = meshFilter.sharedMesh;
            }
            else
            {
                m = actor.SkinnedMeshRenderer.sharedMesh;
            }


                string folderPath = EditorUtility.OpenFolderPanel("Select Export Folder", "Assets", "");
            if (string.IsNullOrEmpty(folderPath) || !folderPath.StartsWith(Application.dataPath))
            {
                Debug.LogWarning("Export cancelled or folder not inside Assets.");
                return;
            }

            // Convert absolute path to relative Unity project path
            string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
            string assetName = actor.name + "_Mesh.asset";
            string fullAssetPath = Path.Combine(relativePath, assetName).Replace("\\", "/");

            // Duplicate mesh to avoid modifying the original sharedMesh
            Mesh meshCopy = Object.Instantiate(m);
            AssetDatabase.CreateAsset(meshCopy, fullAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Mesh exported to {fullAssetPath}");
        }
#endif
    }
}