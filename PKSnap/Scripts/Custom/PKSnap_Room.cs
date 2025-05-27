using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VirtualPhenix.PokemonSnap64
{
    [System.Serializable]
    public class PKSnap_Room : MonoBehaviour
    {
        [System.Serializable]
        public class ObjectDictList
        {
            public List<PKSnap_ObjectData> Objects;
        }

        [System.Serializable]
        public class ObjectDict : VP_SerializableDictionary<long, ObjectDictList>
        {

        }
  
        [SerializeField] protected ObjectDict m_dict;
        [SerializeField] protected List<Texture2D> m_textures;
        [SerializeField] protected List<PKSnap_Actor> m_instantiatedActors;
        
        public List<Texture2D> Texturs { get { return m_textures; } }
        public ObjectDict Objects { get { return m_dict; } }

        public void InitRoom(Dictionary<long, List<PKSnap_ObjectData>> dict, List<Texture2D> textures)
        {
            SetDictionary(dict);
            m_textures = new List<Texture2D>(textures);
            m_instantiatedActors = new List<PKSnap_Actor>();
        }

        public void SetDictionary(Dictionary<long, List<PKSnap_ObjectData>> dict)
        {
            m_dict = new ObjectDict();
            foreach (var l in dict.Keys)
            {
                m_dict.Add(l, new ObjectDictList() { Objects = dict[l] });
            }
        }

        public void AddActor(PKSnap_Actor actor)
        {
            if (m_instantiatedActors == null)
                m_instantiatedActors = new List<PKSnap_Actor>();

            m_instantiatedActors.Add(actor);
        }

        public bool HasActor(long id, out List<PKSnap_ObjectData> data)
        {
            if (m_dict.ContainsKey(id))
            {
                data = m_dict[id].Objects;
                return true;
            }
            data = new List<PKSnap_ObjectData>();
            return false;
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/PKSnap_Room/Export Mesh")]
        private static void ExportMesh(MenuCommand command)
        {
            PKSnap_Room actor = (PKSnap_Room)command.context;
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
        
        [MenuItem("CONTEXT/PKSnap_Room/Export Textures")]
        private static void ExportTextures(MenuCommand command)
        {
            PKSnap_Room actor = (PKSnap_Room)command.context;

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