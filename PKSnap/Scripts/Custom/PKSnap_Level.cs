using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64;
using VirtualPhenix.Nintendo64.PokemonSnap;

namespace VirtualPhenix.PokemonSnap64
{
    public class PKSnap_Level : MonoBehaviour
    {
        [SerializeField] private PKSnap_Skybox m_skybox;
        [SerializeField] private List<PKSnap_Room> m_rooms;
        [SerializeField] private SnapRenderer m_snapRenderer;
        [SerializeField] private ViewerRenderInput m_viewerInput;
        [SerializeField] private bool m_updateLevel = false;
        [SerializeField] private List<PKSnap_Actor> m_staticActors;
        [SerializeField] private List<PKSnap_Actor> m_dynamicActors;
        [SerializeField] private PKSnap_ZeroOne m_zeroOne;

        public List<PKSnap_Room> Rooms { get { return m_rooms; } }
        public PKSnap_Skybox Skybox { get { return m_skybox; } }

        public void AddStaticActors(List<PKSnap_Actor> actors)
        {
            if (m_staticActors == null)
                m_staticActors = new List<PKSnap_Actor>();

            m_staticActors.AddRange(actors);
        }

        public void AddDynamicActors(List<PKSnap_Actor> actors)
        {
            if (m_dynamicActors == null)
                m_dynamicActors = new List<PKSnap_Actor>();

            m_dynamicActors.AddRange(actors);
        }

        public void SetZeroOne(PKSnap_ZeroOne zeroOne)
        {
            m_zeroOne = zeroOne;
        }

        public void SetSnapRenderer(SnapRenderer snapRenderer)
        {
            m_snapRenderer = snapRenderer;
        }

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

        private void Start()
        {
            m_viewerInput = new ViewerRenderInput();
            m_viewerInput.Camera = Camera.main;
            m_viewerInput.Time = Time.time;
        }

        public void ForceUpdate()
        {
            if (m_viewerInput == null)
            {
                m_viewerInput = new ViewerRenderInput();
                m_viewerInput.Camera = Camera.main;
                m_viewerInput.Time = Time.time;
                m_viewerInput.DeltaTime = Time.deltaTime;
            }
            if (m_snapRenderer == null)
            {
                return;
            }
            for (var i = 0; i < m_snapRenderer.ModelRenderers.Count; i++)
            {
                m_snapRenderer.ModelRenderers[i].PrepareToRender(null, m_snapRenderer.RenderHelper.RenderInstManager, m_viewerInput, m_snapRenderer.LevelGlobals);
            }

            m_snapRenderer.LevelGlobals.PrepareToRender(null, m_snapRenderer.RenderHelper.RenderInstManager, m_viewerInput);
        }

        
    }
}