using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
  
        [SerializeField] private ObjectDict m_dict;
        [SerializeField] private List<Texture2D> m_textures;
        
        public List<Texture2D> Texturs { get { return m_textures; } }
        public ObjectDict Objects { get { return m_dict; } }

        public void InitRoom(Dictionary<long, List<PKSnap_ObjectData>> dict, List<Texture2D> textures)
        {
            SetDictionary(dict);
            m_textures = new List<Texture2D>(textures);
        }

        public void SetDictionary(Dictionary<long, List<PKSnap_ObjectData>> dict)
        {
            m_dict = new ObjectDict();
            foreach (var l in dict.Keys)
            {
                m_dict.Add(l, new ObjectDictList() { Objects = dict[l] });
            }
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
    }
}