using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Skybox : MonoBehaviour
    {
        [SerializeField] protected List<Texture2D> m_textures;

        public void InitSkybox(List<Texture2D> textures)
        {
            m_textures = new List<Texture2D>(textures);
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/PKSnap_Skybox/Export Mesh")]
        private static void ExportMesh(MenuCommand command)
        {
            PKSnap_Skybox actor = (PKSnap_Skybox)command.context;
            MeshFilter meshFilter = actor.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("No MeshFilter or Mesh found to export.");
                return;
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
            string fullAssetPath = System.IO.Path.Combine(relativePath, assetName).Replace("\\", "/");

            // Duplicate mesh to avoid modifying the original sharedMesh
            Mesh meshCopy = Object.Instantiate(meshFilter.sharedMesh);
            AssetDatabase.CreateAsset(meshCopy, fullAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Mesh exported to {fullAssetPath}");
        }

        [MenuItem("CONTEXT/PKSnap_Skybox/Export Textures")]
        private static void ExportTextures(MenuCommand command)
        {
            PKSnap_Skybox actor = (PKSnap_Skybox)command.context;

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
                        string fullPath = System.IO.Path.Combine(relativePath, fileName);
                        System.IO.File.WriteAllBytes(fullPath, pngData);
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
#endif
    }
}
