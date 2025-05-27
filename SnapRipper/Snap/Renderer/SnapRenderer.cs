using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class SnapRenderer 
    {
        [System.Serializable]
        public class SnapRendererPair
        {
            public List<BanjoKazooie.RenderData> RenderData = new List<BanjoKazooie.RenderData>();
            public List<ModelRenderer> ModelRenderers = new List<ModelRenderer>();


        }

        public List<BanjoKazooie.RenderData> RenderData = new List<BanjoKazooie.RenderData>();
        public List<ModelRenderer> ModelRenderers = new List<ModelRenderer>();
        public LevelGlobals LevelGlobals;
        public GfxRenderHelper RenderHelper;

        public SnapRendererPair SkyboxData;
        public SnapRendererPair ProjData;
        public SnapRendererPair ObjData;
        public SnapRendererPair ActorData;
        public SnapRendererPair StaticActorData;
        public SnapRendererPair HaunterData;
        public SnapRendererPair ParticleData;
        public SnapRendererPair RoomData;

        public List<RDP.Texture> Textures = new List<RDP.Texture>();

        public SnapRenderer(SceneContext ctx, string id)
        {
            LevelGlobals = new LevelGlobals(ctx, id);
            RenderHelper = new GfxRenderHelper(ctx.GFXDevice);

            SkyboxData = new SnapRendererPair();
            ProjData = new SnapRendererPair();
            ActorData = new SnapRendererPair();
            StaticActorData = new SnapRendererPair();
            ObjData = new SnapRendererPair();
            HaunterData = new SnapRendererPair();
            ParticleData = new SnapRendererPair();
            RoomData = new SnapRendererPair();

            Textures = new List<RDP.Texture>();
        }

        public void SetTextures(List<RDP.Texture> textures)
        {
            Textures.AddRange(textures);
        }

        public void SetSkyboxData(RenderData skyboxData, ModelRenderer skyboxRenderer)
        {
            SkyboxData.RenderData.Add(skyboxData);
            SkyboxData.ModelRenderers.Add(skyboxRenderer);

            RenderData.Add(skyboxData);
            ModelRenderers.Add(skyboxRenderer);
        }
        public void SetProjData(List<RenderData> data)
        {
            ProjData.RenderData.AddRange(data);

            RenderData.AddRange(data);
        }
        public void SetObjData(List<RenderData> data)
        {
            ObjData.RenderData.AddRange(data);

            RenderData.AddRange(data);
        }
        
        public void SetHaunterData(RenderData data)
        {
            HaunterData.RenderData.Add(data);

            RenderData.Add(data);
        }

        public void SetHaunterRenderer(ModelRenderer r)
        {
            HaunterData.ModelRenderers.Add(r);

            ModelRenderers.Add(r);
        }

        public void SetObjectRenderers(List<ModelRenderer> r)
        {
            ObjData.ModelRenderers.AddRange(r);

            ModelRenderers.AddRange(r);
        }

        public void SetRoomData(RenderData d, ModelRenderer r)
        {
            RoomData.RenderData.Add(d);
            RoomData.ModelRenderers.Add(r);

            RenderData.Add(d);
            ModelRenderers.Add(r);
        }

        public void SetActorRenderer(ModelRenderer r)
        {
            ActorData.ModelRenderers.Add(r);

            ModelRenderers.Add(r);
        }

        public void SetStaticActorRenderer(ModelRenderer r)
        {
            StaticActorData.ModelRenderers.Add(r);

            ModelRenderers.Add(r);
        }
    }
}
