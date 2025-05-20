using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Actor : MonoBehaviour
    {
        [SerializeField] private long m_ID;
        [SerializeField] private List<Texture2D> m_textures;

        public long ID { get { return m_ID; } }
        public List<Texture2D> Texturs { get { return m_textures; } }

        public void InitActor(long id, List<Texture2D> textures)
        {
            m_ID = id;
            m_textures = new List<Texture2D>(textures);
        }
    }
}