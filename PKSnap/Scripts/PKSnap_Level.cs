using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Level : MonoBehaviour
    {
        [SerializeField] private PKSnap_Skybox m_skybox;
        [SerializeField] private List<PKSnap_Room> m_rooms;

        public List<PKSnap_Room> Rooms { get { return m_rooms; } }
        public PKSnap_Skybox Skybox { get { return m_skybox; } }

        public void AddRoom(PKSnap_Room room)
        {
            if (m_rooms == null)
            {
                m_rooms = new List<PKSnap_Room>();
            }

            m_rooms.Add(room);
        }

        public void SetSkybox(PKSnap_Skybox skybox)
        {
            m_skybox = skybox;
        }
    }
}