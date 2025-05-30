
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using VirtualPhenix.Nintendo64.BanjoKazooie;
using VirtualPhenix.PokemonSnap64;

namespace VirtualPhenix.Nintendo64.PokemonSnap 
{ 
    public class SceneCreator : EditorWindow
    {
        public static float GlobalScale = .01f;

        public class LoadedLevelArchives
        {
            public List<CRGPokemonArchive> Pokemon;
            public List<CRGLevelArchive> Levels;

            public LoadedLevelArchives()
            {
                Pokemon = new List<CRGPokemonArchive>();
                Levels = new List<CRGLevelArchive>();
            }
        }

        [MenuItem("Pokemon Snap/Parse Scene Models")]
        static void ParseSceneModels()
        {

            string defaultID = "10"; 
            var list = GetArcFileListById(defaultID);
            var archives = LoadLevelArchives(list);
            var parsedLevel = ParseLevel(archives);

            // Animation data is based on model renderer instead of used in direct model graph, so lets check this ;)
            var sceneRenderer = InitSnapRenderer(parsedLevel, defaultID); 

            // Instantiate UnityEngine Objects from ParsedLevel and Snap Renderer
            SpawnObjectsFromParsedLevelRooms(parsedLevel, sceneRenderer);
        }

        public static SnapRenderer InitSnapRenderer(Level parsedLevel, string id)
        {
            var device = new DefaultGfxDevice();
            var sceneCtx = new SceneContext(device);
            var sceneRenderer =  new SnapRenderer(sceneCtx, id);
            sceneRenderer.LevelGlobals.Init(sceneRenderer.RenderHelper.RenderCache, parsedLevel);
            var cache = sceneRenderer.RenderHelper.RenderCache;
            SnapUtils.SceneActorInit();

            var level = parsedLevel;
      
            var viewerTextures = new List<RDP.Texture>();

            if (parsedLevel.Skybox != null)
            {
                var skyboxData = new RenderData(device, cache, level.Skybox.Node.Model.SharedOutput);
                var skyboxRenderer = new ModelRenderer(skyboxData, new List<GFXNode>() { level.Skybox.Node }, new List<AnimationData>(), true);

                if (level.Skybox.Animation != null)
                {
                    skyboxRenderer.Animations.Add(level.Skybox.Animation);
                    skyboxRenderer.SetAnimation(0);
                }

                skyboxRenderer.ForceLoop();
                sceneRenderer.SetSkyboxData(skyboxData, skyboxRenderer);

                for (int j = 0; j < skyboxData.SharedOutput.TextureCache.textures.Count; j++)
                {
                    viewerTextures.Add(skyboxData.SharedOutput.TextureCache.textures[j]);
                }
            }

            // Projectiles Data
            var projData = new List<RenderData>();
            for (int i = 0; i < level.Projectiles.Count; i++)
            {
                projData.Add(new RenderData(device, cache, level.Projectiles[i].SharedOutput));
            }
            sceneRenderer.SetProjData(projData);

            // Objects and Eggs Data
            List<RenderData> objectDatas = new List<RenderData>();
            for (int i = 0; i < level.ObjectInfo.Count; i++)
            {
                var data = new RenderData(device, cache, level.ObjectInfo[i].SharedOutput);

                if (level.ObjectInfo[i].ID == 601 || level.ObjectInfo[i].ID == 602) // replace egg vertex buffers
                    EggUtils.EggInputSetup(cache, data, level.EggData);

                objectDatas.Add(data);
                sceneRenderer.SetObjData(objectDatas);

                for (int j = 0; j < data.SharedOutput.TextureCache.textures.Count; j++)
                {
                    data.SharedOutput.TextureCache.textures[j].name = $"{level.ObjectInfo[i].ID}_{j}";
                    viewerTextures.Add((data.SharedOutput.TextureCache.textures[j]));
                }
            }

            // Haunter Data
            RenderData haunterData = null;
            if (level.HaunterData != null)
            {
                haunterData = new RenderData(device, cache, level.HaunterData[1].Model.SharedOutput);
                sceneRenderer.SetHaunterData(haunterData);
                for (int j = 0; j < haunterData.SharedOutput.TextureCache.textures.Count; j++)
                {
                    haunterData.SharedOutput.TextureCache.textures[j].name = $"93_{j + 1}";
                    viewerTextures.Add(haunterData.SharedOutput.TextureCache.textures[j]);
                }
            }

            // Particles
            foreach (var particle in level.LevelParticles.ParticleTextures)
            {
                foreach (var texture in particle)
                {
                    viewerTextures.Add(texture);
                }
            }

            // Build object data
            sceneRenderer.SetObjectRenderers(sceneRenderer.LevelGlobals.BuildTempObjects(level.ObjectInfo, objectDatas, level).ToList());

            // Particle Manager
            sceneRenderer.LevelGlobals.Particles = new ParticleManager(device, cache, level.LevelParticles, level.PesterParticles);

            // Room Animations
            for (int i = 0; i < level.Rooms.Count; i++)
            {
                var renderData = new RenderData(device, cache, level.Rooms[i].Node.Model.SharedOutput);
                var roomRenderer = new ModelRenderer(renderData, new List<GFXNode>() { level.Rooms[i].Node }, new List<AnimationData>() { });

                if (level.Rooms[i].Animation != null)
                {
                    roomRenderer.Animations.Add(level.Rooms[i].Animation);
                    roomRenderer.SetAnimation(0);
                }

                roomRenderer.ForceLoop();
                sceneRenderer.SetRoomData(renderData, roomRenderer);

                var objects = level.Rooms[i].Objects;
                for (int j = 0; j < objects.Count; j++)
                {
                    var objIndex = level.ObjectInfo.FindIndex(def => def.ID == objects[j].ID);
                    if (objIndex == -1)
                    {
                        Debug.LogWarning("missing object: "+ VP_BYMLUtils.HexZero(objects[j].ID, 3));
                        continue;
                    }

                    var def = level.ObjectInfo[objIndex];
                    if (SnapUtils.IsActor(def))
                    {
                        ActorDef actorDef = def as ActorDef;
                        Actor objectRenderer = SnapUtils.CreateActor(objectDatas[objIndex], objects[j], actorDef, sceneRenderer.LevelGlobals);

                        if (def.ID == 133) // eevee actually uses chansey's path
                            objectRenderer.MotionData.Path = objects.Find(obj => obj.ID == 113).Path;

                        if (def.ID == 93)
                        {
                            var fullHaunter = new ModelRenderer(haunterData, level.HaunterData, new List<AnimationData>() { }, false, false, actorDef.ID);
                            Haunter h = (Haunter)objectRenderer;
                            h.FullModel = fullHaunter;
                            fullHaunter.Visible = false;
                            sceneRenderer.SetHaunterRenderer(fullHaunter);
                        }

                        sceneRenderer.LevelGlobals.AllActors.Add(objectRenderer);
                        sceneRenderer.SetActorRenderer(objectRenderer);
                    }
                    else
                    {
                        StaticDef staticDef = def as StaticDef;
                        var objectRenderer = new ModelRenderer(objectDatas[objIndex], new List<GFXNode>() { staticDef.Node }, new List<AnimationData>() { }, false, false, staticDef.ID);
                        RendererUtils.BuildTransform(ref objectRenderer.ModelMatrix, objects[j].Position, objects[j].Euler, objects[j].Scale);
                        sceneRenderer.SetStaticActorRenderer(objectRenderer);
                    }
                }
            }
            sceneRenderer.SetTextures(viewerTextures);
            return sceneRenderer;
        }

        public static void SpawnObjectsFromParsedLevelRooms(Level level, SnapRenderer snapRenderer)
        {
            GameObject go = new GameObject("[Level "+ level.Name+"]");
            var pkLevel = go.AddComponent<PKSnap_Level>();
            pkLevel.SetSnapRenderer(snapRenderer);
            //go.transform.localScale = new Vector3(-1f, 1f, 1f);

            var skyboxRoom = SpawnFromRoomData(level.Skybox, "Skybox", go.transform, -1, snapRenderer);
            PKSnap_Skybox skybox = skyboxRoom.gameObject.AddComponent<PKSnap_Skybox>();
            skybox.InitSkybox(skyboxRoom.Texturs);
            pkLevel.SetSkybox(skybox);

            int idx = 0;
            foreach (var rooms in level.Rooms)
            {
                PKSnap_Room room = SpawnFromRoomData(rooms, "[Room "+ idx + "]", go.transform, idx, snapRenderer);
                pkLevel.AddRoom(room);
                idx++;
            }

            var staticActors = snapRenderer.StaticActorData;
            foreach (var mdls in staticActors.ModelRenderers)
            {
                mdls.SetAnimation(0);

                PKSnap_Actor spawnedActor = SpawnActorFromStaticModelRenderer(mdls, "[Static Actor " + mdls.ID + "]", go.transform, mdls.ID, snapRenderer);
                var instActors = SpawnActorPerSpawnObject(spawnedActor, pkLevel);
                pkLevel.AddStaticActors(instActors);
            }

            var dynamicActors = snapRenderer.ActorData;
            foreach (var mdls in dynamicActors.ModelRenderers)
            {
                PKSnap_Actor spawnedActor = SpawnActorFromDynamicModelRenderer(mdls, "[Dynamic Actor " + mdls.ID + "]", go.transform, mdls.ID, snapRenderer);
                var instActors = SpawnActorPerSpawnObject(spawnedActor, pkLevel);
                foreach (var act in instActors)
                {
                    // TODO
                    //act.transform.localScale *= 0.5f;
                    //act.transform.localPosition = Vector3.zero;
                }

                pkLevel.AddDynamicActors(instActors);
            }

            ZeroOne zeroOne = snapRenderer.LevelGlobals.Level.ZeroOne;
            PKSnap_ZeroOne pkZero = SpawnZeroOneObject(zeroOne, snapRenderer.LevelGlobals.ZeroOne, pkLevel, go.transform);
            pkLevel.SetZeroOne(pkZero);
            pkLevel.ForceUpdate();

            // TODO
            go.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        public static PKSnap_Actor SpawnActorFromDynamicModelRenderer(ModelRenderer data, string customName, Transform parent, long id, SnapRenderer snapRenderer)
        {
            GameObject go = new GameObject(customName);
            var actor = go.AddComponent<PKSnap_Actor>();         
            bool visible = data.Visible;

            Actor parsedActor = (Actor)data;

            var sharedOutput = data.SharedOutput;
            var textures = new List<Texture2D>();
         
            var smr = BuildMeshAndBonesFromData(actor, customName, parsedActor.ID, go.transform, sharedOutput, parsedActor.Renderers, out textures, data.Nodes, data.Animations);
            actor.InitActor(id, textures, visible, smr, data);
            go.transform.parent = parent;

            return actor;
        }

        public static BoneWeight[] ParseBoneWeights(List<NodeRenderer> renderers, RSPSharedOutput sharedOutput, List<Transform> bones, Dictionary<NodeRenderer, int> nodeToBoneIndexMap)
        {
            var vertexToBone = ParseVerticesToBones(renderers, sharedOutput, bones, nodeToBoneIndexMap);

            BoneWeight[] boneWeights = new BoneWeight[sharedOutput.Vertices.Count];

            for (int i = 0; i < boneWeights.Length; i++)
            {
                if (vertexToBone.TryGetValue(i, out int boneIndex))
                {
                   
                    boneWeights[i] = new BoneWeight
                    {
                        boneIndex0 = boneIndex,
                        weight0 = 1.0f,
                        boneIndex1 = 0,
                        weight1 = 0f,
                        boneIndex2 = 0,
                        weight2 = 0f,
                        boneIndex3 = 0,
                        weight3 = 0f
                    };
                }
                else
                {
                    // fallback para v�rtices no usados (si existen)
                    boneWeights[i] = new BoneWeight
                    {
                        boneIndex0 = 0,
                        weight0 = 1.0f,
                        boneIndex1 = 0,
                        weight1 = 0f,
                        boneIndex2 = 0,
                        weight2 = 0f,
                        boneIndex3 = 0,
                        weight3 = 0f
                    };
                }
            }

            return boneWeights;
        }

        public static Dictionary<int, int> ParseVerticesToBones(List<NodeRenderer> renderers, RSPSharedOutput sharedOutput, List<Transform> bones, Dictionary<NodeRenderer, int> nodeToBoneIndexMap)
        {
            Dictionary<int, int> vertexToBone = new Dictionary<int, int>();

            foreach (var node in renderers)
            {
                if (!nodeToBoneIndexMap.TryGetValue(node, out int boneIndex))
                    continue;

                foreach (var drawcall in node.DrawCalls)
                {
                    for (int i = drawcall.DrawCallInfo.FirstIndex; i < drawcall.DrawCallInfo.FirstIndex + drawcall.DrawCallInfo.IndexCount; i++)
                    {
                        var vertexIndex = (int)sharedOutput.Indices[i];

                        if (!vertexToBone.ContainsKey(vertexIndex))
                        {
                            vertexToBone[vertexIndex] = boneIndex;
                        }
                        else if (vertexToBone[vertexIndex] != boneIndex)
                        {
                            Debug.LogWarning($"Vertex {vertexIndex} was used for multiple bones: ({vertexToBone[vertexIndex]} and {boneIndex}).");
                       
                        }
                    }
                }
            }

            return vertexToBone;
        }

        public static SkinnedMeshRenderer BuildMeshAndBonesFromData(PKSnap_Actor actor, string customName, long id, Transform mainObjectTRS, 
            RSPSharedOutput sharedOutput, List<NodeRenderer> renderers, out List<Texture2D> textures, List<GFXNode> nodeList, List<AnimationData> animationList)
        {
            var materials = new List<Material>();

            var mesh = BuildMeshFromNodeRendererList(customName, sharedOutput.Vertices, sharedOutput.Indices, renderers, out materials, out textures);
            SkinnedMeshRenderer smr = null;
            Matrix4x4 localToWorldMatrix = Matrix4x4.identity;

            if (mesh != null)
            {
                GameObject meshObj = new GameObject("[Actor " + (id == -1 ? customName : id) + " Mesh]");
                meshObj.transform.parent = mainObjectTRS;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localRotation = Quaternion.identity;
                meshObj.transform.localScale = Vector3.one;

                var needToMirror = id != 1004;
   
                smr = meshObj.AddComponent<SkinnedMeshRenderer>();
                List<Transform> bones = new List<Transform>();
                List<Matrix4x4> bindPoseMatrixArray = new List<Matrix4x4>();
                List<Matrix4x4> modelMatricesArray = new List<Matrix4x4>();
                Dictionary<NodeRenderer, int> nodeToBoneIndexMap = new Dictionary<NodeRenderer, int>();
                var topBone = new GameObject("TopJoint");
                topBone.transform.parent = mainObjectTRS;
                topBone.transform.localScale = Vector3.one;
                //localToWorldMatrix = topBone.transform.localToWorldMatrix;



                // Bones
                int i = 0;
                int rendererIdx = 0;
                foreach (var renderer in renderers)
                {
                    if (rendererIdx == 0)
                    {
                        localToWorldMatrix = renderer.ModelMatrix.transpose;
                        // Only the first NodeRenderer has the bone hierarchy, the rest are duplicates to access bones by index
                        ParseActorBones(actor, renderer, topBone.transform, ref i, ref bones, ref bindPoseMatrixArray, ref localToWorldMatrix, ref nodeToBoneIndexMap, nodeList.Count, needToMirror, ref modelMatricesArray);
                    }
                    
                    rendererIdx++;
                }

                var boneWeights = ParseBoneWeights(renderers, sharedOutput, bones, nodeToBoneIndexMap);

                smr.sharedMesh = mesh;
                smr.rootBone = topBone.transform;
                smr.bones = bones.ToArray();
                mesh.bindposes = bindPoseMatrixArray.ToArray();
                mesh.boneWeights = boneWeights;

                smr.materials = materials.ToArray();
            }

            return smr;
        }

        public static Vector3[] RevertTransformedVertices(Vector3[] vertices, BoneWeight[] boneWeights, Matrix4x4[] boneModelMatricesN64)
        {
            Vector3[] correctedVertices = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i > boneWeights.Length)
                {
                    Debug.LogError("BW: " + boneWeights.Length + "  - i: " + i);
                }

                int boneIndex = boneWeights[i].boneIndex0;

                if (boneIndex < 0 || boneIndex >= boneModelMatricesN64.Length)
                {
                    Debug.LogWarning($"Vértice {i} tiene boneIndex fuera de rango: {boneIndex}");
                    correctedVertices[i] = vertices[i];
                    continue;
                }

                // Convertimos la matriz a formato Unity (fila mayor)
                Matrix4x4 n64Matrix = boneModelMatricesN64[boneIndex];
                Matrix4x4 unityMatrix = n64Matrix.transpose;

                // Revertimos la transformación aplicada
                correctedVertices[i] = unityMatrix.inverse.MultiplyPoint3x4(vertices[i]);
            }

            return correctedVertices;
        }

        public static SkinnedMeshRenderer BuildSkinnedMeshAndBonesFromData(PKSnap_Actor actor, string customName, long id, Transform mainObjectTRS,
           RSPSharedOutput sharedOutput, List<NodeRenderer> renderers, out List<Texture2D> textures, List<GFXNode> nodeList, List<AnimationData> animationList)
        {
            var materials = new List<Material>();

            var mesh = BuildMeshFromNodeRendererList(customName, sharedOutput.Vertices, sharedOutput.Indices, renderers, out materials, out textures);
            SkinnedMeshRenderer smr = null;
            Matrix4x4 localToWorldMatrix = Matrix4x4.identity;

            if (mesh != null)
            {
                GameObject meshObj = new GameObject("[Actor " + (id == -1 ? customName : id) + " Mesh]");
                meshObj.transform.parent = mainObjectTRS;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localRotation = Quaternion.identity;
                meshObj.transform.localScale = Vector3.one;

                var needToMirror = id != 1004;

                smr = meshObj.AddComponent<SkinnedMeshRenderer>();
                List<Transform> bones = new List<Transform>();
                List<Matrix4x4> bindPoseMatrixArray = new List<Matrix4x4>();
                List<Matrix4x4> modelMatrixArray = new List<Matrix4x4>();
                Dictionary<NodeRenderer, int> nodeToBoneIndexMap = new Dictionary<NodeRenderer, int>();
                var topBone = new GameObject("TopJoint");
                topBone.transform.parent = mainObjectTRS;
                topBone.transform.localScale = new Vector3(1, 1, 1) * (needToMirror ? 0.25f : 0.75f);
                localToWorldMatrix = topBone.transform.localToWorldMatrix;

                // Bones
                int i = 0;
                int rendererIdx = 0;
                foreach (var renderer in renderers)
                {
                    if (rendererIdx == 0)
                    {
                        // Only the first NodeRenderer has the bone hierarchy, the rest are duplicates to access bones by index
                        ParseActorBones(actor, renderer, topBone.transform, ref i, ref bones, ref bindPoseMatrixArray, ref localToWorldMatrix, ref nodeToBoneIndexMap, nodeList.Count, needToMirror, ref modelMatrixArray);
                    }

                    rendererIdx++;
                }

                var boneWeights = ParseBoneWeights(renderers, sharedOutput, bones, nodeToBoneIndexMap);

                smr.sharedMesh = mesh;
                smr.rootBone = topBone.transform;
                smr.bones = bones.ToArray();
                mesh.bindposes = bindPoseMatrixArray.ToArray();
                mesh.boneWeights = boneWeights;

                smr.materials = materials.ToArray();
            }

            return smr;
        }


        public static PKSnap_Actor SpawnActorFromStaticModelRenderer(ModelRenderer data, string customName, Transform parent, long id, SnapRenderer snapRenderer)
        {
            GameObject go = new GameObject(customName);
            go.transform.parent = parent;
            bool visible = data.Visible;

            var sharedOutput = data.SharedOutput;
            List<Texture2D> textures = new List<Texture2D>();
            List<Material> materials = new List<Material>();
            var mesh = BuildMeshFromNodeRendererList(customName, sharedOutput.Vertices, sharedOutput.Indices, data.Renderers, out materials, out textures);
  
            if (mesh != null)
            {
                var mr = go.AddComponent<MeshRenderer>();
                mr.materials = materials.ToArray();
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
            }

            var actor = go.AddComponent<PKSnap_Actor>();
            actor.InitActor(id, textures, visible, null, data);
            return actor;
        }

        public static PKSnap_ZeroOne SpawnZeroOneObject(ZeroOne data, ModelRenderer zeroOneModelRenderer, PKSnap_Level pkLevel, Transform parent)
        {
            GameObject go = new GameObject("Zero One");
            PKSnap_ZeroOne zeroOne = go.AddComponent<PKSnap_ZeroOne>();
            go.transform.parent = parent;
            go.transform.localScale = new Vector3(-1, 1, 1);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;

            bool visible = zeroOneModelRenderer.Visible;

            var sharedOutput = data.SharedOutput;
            var textures = new List<Texture2D>();
            var smr = BuildMeshAndBonesFromData(zeroOne, "ZeroOne", -1, go.transform, sharedOutput, zeroOneModelRenderer.Renderers, out textures, data.Nodes, data.Animations);
            zeroOne.InitActor(-1, textures, visible, smr, zeroOneModelRenderer);
            go.SetActive(visible);

            return zeroOne;
        }

        public static List<PKSnap_Actor> SpawnActorPerSpawnObject(PKSnap_Actor actor, PKSnap_Level pkLevel)
        {
            List<PKSnap_Actor> instActors = new List<PKSnap_Actor>();
            bool found = false;
            foreach (var room in pkLevel.Rooms)
            {
                if (room.HasActor(actor.ID, out List<PKSnap_ObjectData> spawnedObjects))
                {
                    //Debug.LogWarning("Cloning actor"+ actor.name + " in: "+room.name+" room - times:" + spawnedObjects.Count);
                    foreach (var obj in spawnedObjects)
                    {
                        if (!found)
                            found = true;

                        if (obj.transform.childCount == 0)
                        {
                            PKSnap_Actor ac = Instantiate(actor, obj.transform);
                            ac.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                            //ac.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
                            instActors.Add(ac);
                            room.AddActor(ac);
                            break;
                        }
                    }
                }
            }

            if (found)
            {
                DestroyImmediate(actor.gameObject);
            }

            return instActors;
        }
       
        public static Sprite ToUnitySprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 
                100.0f            
            );
        }

        public static void ApplyOpenGLModelMatrixToTransform(Matrix4x4 openGLMatrix, Transform targetTransform)
        {
            Matrix4x4 unityMatrix = openGLMatrix.transpose;

            Vector3 position = unityMatrix.GetColumn(3);

            Quaternion rotation = Quaternion.LookRotation(
                unityMatrix.GetColumn(2), // Forward
                unityMatrix.GetColumn(1)  // Up
            );
   
            Vector3 scale = new Vector3(
                unityMatrix.GetColumn(0).magnitude,
                unityMatrix.GetColumn(1).magnitude,
                unityMatrix.GetColumn(2).magnitude
            );

            targetTransform.localPosition = position * .0100f;
            targetTransform.localRotation = rotation;
            targetTransform.localScale = scale;
        }

        public static void ParseActorBones(
            PKSnap_Actor actor,
            NodeRenderer renderer, Transform trs, ref int count, 
            ref List<Transform> bones, ref List<Matrix4x4> bindPoseMatrixArray, 
            ref Matrix4x4 rootLocalToWorldMatrix, ref Dictionary<NodeRenderer, int> nodeToBoneIndexMap,
            int nodeListCount,
            bool needToMirror, ref List<Matrix4x4> modelMatricesArray)
        {
            GameObject currGo = new GameObject(count.ToString());
          //  ApplyOpenGLModelMatrixToTransform(renderer.ModelMatrix, currGo.transform);

            var pkBone = currGo.AddComponent<PKSnap_Bone>();
            if (pkBone.InitBone(actor, renderer, trs, GlobalScale, count, needToMirror))
            {
                modelMatricesArray.Add(renderer.ModelMatrix);

                bones.Add(currGo.transform);
                var offset = new Vector3(0, 1.23f, 0);
                Matrix4x4 bindPose = Matrix4x4.identity;//renderer.ModelMatrix.transpose.inverse * rootLocalToWorldMatrix;  //Matrix4x4.identity;//currGo.transform.worldToLocalMatrix * rootLocalToWorldMatrix;
                bindPoseMatrixArray.Add(bindPose);
                nodeToBoneIndexMap[renderer] = count;   
            }

            count++;

            foreach (var child in renderer.Children)
            {
                ParseActorBones(actor, child, currGo.transform, ref count, ref bones, ref bindPoseMatrixArray, ref rootLocalToWorldMatrix, ref nodeToBoneIndexMap, nodeListCount, needToMirror, ref modelMatricesArray);
            }
        }


        public static List<Texture2D> BuildTexturesFromTextureCache(RDP.TextureCache textureCache, string customName)
        {
            return BuildTexturesFromTextureList(textureCache.textures, customName);
        }

        public static List<Texture2D> BuildTexturesFromTextureList(List<RDP.Texture> textures, string customName)
        {
            var list = new List<Texture2D>();

            foreach (RDP.Texture rdpTex in textures)
            {
                if (rdpTex == null || rdpTex.pixels == null || rdpTex.pixels.Length == 0)
                {
                    Debug.LogError("Error parsing TEXTURE in cache for " + customName);
                    continue;
                }

                var texture = new Texture2D((int)rdpTex.width, (int)rdpTex.height, TextureFormat.RGBA32, mipChain: false);
                texture.name = rdpTex.name;

                // RGBA8 = 4 bytes per pixel
                int expectedLength = (int)(rdpTex.width * rdpTex.height * 4);
                if (rdpTex.pixels.Length < expectedLength)
                {
                    Debug.LogError($"Pixel buffer too small: expected {expectedLength}, got {rdpTex.pixels.Length}");
                    continue;
                }

                texture.LoadRawTextureData(rdpTex.pixels);
                texture.Apply();

                list.Add(texture);
            }

            return list;
        }

        public static List<Material> BuildMaterialsFromData(string customName, List<MaterialData> materialData, List<Texture2D> loadedTextures)
        {
            Debug.Log("MaterialData Count: " + materialData.Count);
            Debug.Log("Loaded Texture Count: " + loadedTextures.Count);

            List<Material> materials = new List<Material>();
            var defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = Color.white;
        
            if (materialData == null || materialData.Count == 0 || loadedTextures == null ||  materialData.Count == 0)
            {
                materials.Add(defaultMat);
                return materials;
            }

            materials.Add(defaultMat);

            foreach (var md in materialData)
            {
                var parsedMat = new Material(Shader.Find("Standard"));
                parsedMat.color = Color.white;

                foreach (var ut in md.UsedTextures)
                {
                    if (ut.Index < loadedTextures.Count && loadedTextures.Count > 0)
                    {
                        Debug.Log("Adding " + loadedTextures.ElementAt((int)ut.Index)  +" to "+ customName);
                        parsedMat.SetTexture("_MainTex",loadedTextures.ElementAt((int)ut.Index));
                    }
                }
                materials.Add(parsedMat);
            }

            return materials;
        }
        
        public static PKSnap_Room SpawnFromRoomData(Room room, string customName, Transform parent, int id, SnapRenderer snapRenderer)
        {
            Debug.Log("Parsing Room: " + customName);
            Dictionary<long, List<PKSnap_ObjectData>> roomObjectDict = new Dictionary<long, List<PKSnap_ObjectData>>();

            GameObject go = new GameObject(customName);
            go.transform.parent = parent;

            Vector3 pos = room.Node.Translation * GlobalScale;
            Vector3 euler = room.Node.Euler * Mathf.Rad2Deg;
            Vector3 scale = room.Node.Scale;

            /*
             * pos.x *= -1;
            euler.y *= -1;
            euler.z *= -1;*/

            go.transform.localScale = scale;
            go.transform.localEulerAngles = euler;
            go.transform.localPosition = pos;

            PKSnap_Room pkRoom = go.AddComponent<PKSnap_Room>();
            var data = id == -1 ? snapRenderer.SkyboxData.ModelRenderers[0] : snapRenderer.RoomData.ModelRenderers[id];

            var sharedOutput = data.SharedOutput;
            List<Texture2D> textures = new List<Texture2D>();
            List<Material> materials = new List<Material>();
            var mesh = BuildMeshFromNodeRendererList(customName, sharedOutput.Vertices, sharedOutput.Indices, data.Renderers, out materials, out textures);

            if (mesh != null)
            {
                if (materials.Count > 0 && materials.Count > mesh.subMeshCount)
                {
                    Debug.LogWarning("Materials were trimmed due to the lack of Sub Meshes in object: " + go.name);
                    materials.RemoveRange(mesh.subMeshCount, materials.Count - mesh.subMeshCount);
                }
                var mr = go.AddComponent<MeshRenderer>();
                mr.materials = materials.ToArray();
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
            }

            var model = room.Node.Model;
            foreach (var o in room.Objects)
            {
                GameObject ob = new GameObject("[Room Object: " + o.ID + "]");
                ob.transform.parent = go.transform;

                Vector3 posObj = o.Position * GlobalScale;
                Vector3 eulerObj = o.Euler * Mathf.Rad2Deg;
                Vector3 scaleObj = o.Scale;

                /*posObj.x *= -1;
                eulerObj.y *= -1;
                scaleObj.z *= -1;*/

                ob.transform.localScale = scaleObj;
                ob.transform.eulerAngles = eulerObj;
                ob.transform.position = posObj;

                PKSnap_ObjectData odata = ob.AddComponent<PKSnap_ObjectData>();
                odata.Behaviour = o.Behaviour;
                odata.ID = o.ID;
                odata.Path = new PKSnap_TrackPath();
                if (o.Path != null)
                {
                    odata.Path.Duration = o.Path.Duration;
                    odata.Path.Kind = o.Path.Kind;
                    odata.Path.Length = o.Path.Length;
                    odata.Path.SegmentRate = o.Path.SegmentRate;
                    odata.Path.Times = (float[])o.Path.Times.ToArray();
                    odata.Path.Points = (float[])o.Path.Points.ToArray();
                    odata.Path.Quartics = (float[])o.Path.Quartics.ToArray();
                }
                if (!roomObjectDict.ContainsKey(o.ID))
                {
                    roomObjectDict.Add(o.ID, new List<PKSnap_ObjectData>() { odata });
                }
                else
                {
                    roomObjectDict[o.ID].Add(odata);
                }
            }

            pkRoom.InitRoom(roomObjectDict, textures);
            return pkRoom;
        }

 
        public static Mesh BuildMeshFromNodeRendererList(string customName, List<RSPVertex> vertices, List<long> indices, List<NodeRenderer> nodeRenderers, out List<Material> materialsOut, out List<Texture2D> textures, Vector2? manualUVs = null, bool debugUVs = false)
        {
            var finalVertices = new List<Vector3>();
            var finalColors = new List<Color32>();
            var finalUVs = new List<Vector2>();
            var submeshTriangles = new List<int[]>();
            var materials = new List<Material>();

            var vertexMap = new Dictionary<int, int>(); // map global index -> new vertex index
            int nextVertexIndex = 0;
            textures = new List<Texture2D>();

            foreach (var nodeRenderer in nodeRenderers)
            {
                var drawCallInstances = nodeRenderer.DrawCalls;

                foreach (var (drawCall, drawCallIndex) in drawCallInstances.Select((dc, i) => (dc, i)))
                {
                    var usedTextures = drawCall.TextureEntry;
                    var drawCallMaterials = new List<Material>();
                    var submeshIndices = new List<int>();

                    var scaleX = 32f;
                    var scaleY = 64f;

                    var texList = BuildTexturesFromTextureList(drawCall.TextureEntry, customName);
                    if (texList.Count > 0)
                    {
                        textures.AddRange(texList);

                        if (manualUVs == null)
                        {
                            scaleX = texList[0].width;
                            scaleY = texList[0].height;
                        }
                        else
                        {
                            scaleX = manualUVs.Value.x;
                            scaleY = manualUVs.Value.y;
                        }
                    }
                    
                    for (int i = 0; i < drawCall.DrawCallInfo.IndexCount; i++)
                    {
                        int globalIndex = (int)indices[drawCall.DrawCallInfo.FirstIndex + i];

                        if (!vertexMap.ContainsKey(globalIndex))
                        {
                            var v = vertices[globalIndex];
                            var vval = new Vector3((float)v.x, (float)v.y, (float)v.z) * GlobalScale;
                            finalVertices.Add(vval);
                            finalColors.Add(new Color((float)v.c0, (float)v.c1, (float)v.c2, (float)v.a));

                            var vtx = v.tx / scaleX;
                            var vty = v.ty / scaleY;

                            float tx = (float)vtx;// Mathf.Clamp((float)vtx, 0, 1);
                            float ty = (float)vty;//Mathf.Clamp((float)vty, 0, 1);
                            finalUVs.Add(new Vector2(tx, ty));
                           
                            if (debugUVs)
                                Debug.LogWarning($"Added V{globalIndex} as new V{nextVertexIndex} -  UV: {tx}, {ty}");

                            vertexMap[globalIndex] = nextVertexIndex++;
                        }

                        submeshIndices.Add(vertexMap[globalIndex]);
                    }
                    submeshTriangles.Add(submeshIndices.ToArray());
                    foreach (var t in texList)
                    {
                        var mat = new Material(Shader.Find("Standard"));
                        mat.name = t.name;
                        mat.mainTexture = t;
                        materials.Add(mat); 
                    }
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(finalVertices);
            mesh.SetColors(finalColors);
            mesh.SetUVs(0, finalUVs);
            mesh.subMeshCount = submeshTriangles.Count;

            for (int i = 0; i < submeshTriangles.Count; i++)
            {
                mesh.SetTriangles(submeshTriangles[i], i);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            materialsOut = materials;
            return mesh;
        }

        public static LoadedLevelArchives LoadLevelArchives(List<string> files)
        {
            var archives = new LoadedLevelArchives();

            foreach (var fileName in files)
            {
                var isPokemon = IsPokemon(fileName);
                var bytes = System.IO.File.ReadAllBytes(Application.dataPath + "//CRG1//" + fileName+ "_arc.crg1");
                if (isPokemon)
                {
                    var pkmn = (CRGPokemonArchive)VP_BYML.Parse<CRGPokemonArchive>(bytes, FileType.CRG1);

                    if (pkmn != null)
                    {
                        archives.Pokemon.Add(pkmn);
                      //  pkmn.Log();
                    }
                }
                else
                {
                    var level = (CRGLevelArchive)VP_BYML.Parse<CRGLevelArchive>(bytes, FileType.CRG1);

                    if (level != null)
                    {
                        archives.Levels.Add(level);
                       // level.Log();
                    }
                }
                
            }

            return archives;
        }

        public static List<string> GetPokemonList()
        {
           return  new List<string> { "magikarp", "pikachu", "zubat", "magikarp", "bulbasaur" };
        }

        public static bool IsPokemon(string id)
        {
            List<string> fileList = GetPokemonList();
            return fileList.Contains(id);   
        }

        public static List<string> GetFilesNotInList()
        {
            var files = GetFiles();
            var excludedNames = GetPokemonList();

            var filtered = files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(name => !excludedNames.Contains(name))
                .ToList();

            return filtered;
        }

        public static List<string> GetFiles()
        {
            string fullPath = Application.dataPath + "/CRG1";

            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"La carpeta no existe: {fullPath}");
                return new List<string>();
            }

            return Directory.GetFiles(fullPath, $"*.crg1", SearchOption.TopDirectoryOnly).ToList();
        }

        public static List<string> GetArcFileListById(string id)
        {
            // Base files
            List<string> fileList = new List<string> { id, "0E", "magikarp" };

            // Condicionales
            switch (id)
            {
                case "10": // beach
                    fileList.Add("pikachu");
                    break;
                case "12": // tunnel
                    fileList.AddRange(new[] { "pikachu", "zubat" });
                    break;
                case "16": // river
                    fileList.AddRange(new[] { "pikachu", "bulbasaur" });
                    break;
                case "14": // cave
                    fileList.AddRange(new[] { "pikachu", "bulbasaur", "zubat" });
                    break;
            }

            return fileList;
        }

        public static Level ParseLevel(LoadedLevelArchives loaded)
        {
            var archives = loaded.Levels;
            var pokemonArchives = loaded.Pokemon;

            var level = archives[0];
            //level.Log();

            var dataMap = new CRGDataMap(new List<CRGDataRange>
            {
                new CRGDataRange { Data = level.Data, Start = level.StartAddress },
                new CRGDataRange { Data = level.Code, Start = level.CodeStartAddress },
                new CRGDataRange { Data = level.Photo, Start = level.PhotoStartAddress },
            });

            for (int i = 1; i < archives.Count; i++)
            {
                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = archives[i].Data,
                    Start = archives[i].StartAddress,
                    Overlay = 1
                });
                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = archives[i].Code,
                    Start = archives[i].CodeStartAddress
                    // overlay omitido
                });

                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = archives[i].Photo,
                    Start = archives[i].PhotoStartAddress,
                    Overlay = 2
                });
            }
    
            var pokemon = loaded.Pokemon;
            for (int i = 0; i < pokemon.Count; i++)
            {
                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = pokemon[i].Data,
                    Start = pokemon[i].StartAddress,
                    Overlay = 0
                });

                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = pokemon[i].Code,
                    Start = pokemon[i].CodeStartAddress
                    // overlay omitido
                });

                dataMap.ranges.Add(new CRGDataRange
                {
                    Data = pokemon[i].Photo,
                    Start = pokemon[i].PhotoStartAddress,
                    Overlay = 2
                });
            }

            List<Room> rooms = new List<Room>();
            var roomHeader = dataMap.Deref(level.Header);

            var view = dataMap.GetView(roomHeader);
            long pathRooms = view.GetUint32(0x00, false);
            long nonPathRooms = view.GetUint32(0x04, false);
            long skyboxDescriptor = view.GetUint32(0x08, false);

            var sharedCache = new RDP.TextureCache();
            Room skybox = null;

            if (skyboxDescriptor > 0)
            {
                var skyboxView = dataMap.GetView(skyboxDescriptor);
                long skyboxDL = skyboxView.GetUint32(0x00);
                long skyboxRenderer = skyboxView.GetUint32(0x04);
                long skyboxMats = skyboxView.GetUint32(0x08);
                long animData = skyboxView.GetUint32(0x0C);
                var Materials = skyboxMats != 0 ? ParseMaterialData(dataMap, dataMap.Deref(skyboxMats)) : new List<MaterialData>();
                var SkyboxState = new RSPState(new RSPSharedOutput(), dataMap);
                InitDL(SkyboxState, true, skyboxRenderer != 0x800E1CA4);
                var SkyboxModel = RunRoomDL(dataMap, skyboxDL, new List<RSPState>() { SkyboxState }, Materials);

                GFXNode Node = new GFXNode
                {
                    Billboard = 0,
                    Model = SkyboxModel,
                    Translation = Vector3.zero,
                    Euler = Vector3.zero,
                    Scale = Vector3.one,
                    Parent = -1,
                    Materials = Materials,
                };

                AnimationData Animation = null;
                if (animData != 0)
                {
                    long animStart = dataMap.Deref(animData);
                    var Mats = new List<AnimationTrack>();
                    for (int i = 0; i < Materials.Count; i++)
                    {
                        var Track = ParseAnimationTrack(dataMap, dataMap.Deref(animStart + 4 * i))!;
                        FindNewTextures(dataMap, Track, Node, i);
                        Mats.Add(Track);
                    }
                    Animation = new AnimationData { FPS = 30, Frames = 0, Tracks = new List<AnimationTrack> { null }, MaterialTracks = new List<List<AnimationTrack>>() { Mats } };
                }

                skybox = new Room
                {
                    Node = Node,
                    Objects = new List<ObjectSpawn>(),
                    Animation = Animation,
                };
            }

            var pathView = dataMap.GetView(pathRooms);
            long offs = 0;
            while (pathView.GetUint32(offs) != 0)
            {
                var o = pathView.GetUint32(offs);
                rooms.Add(ParseRoom(dataMap, pathView.GetUint32(offs, false), sharedCache));
                offs += 4;
            }

            offs = 0;
            // also different material handling?
            var nonPathView = dataMap.GetView(nonPathRooms);
            while (nonPathView.GetUint32(offs) != 0)
            {
                var o = nonPathView.GetUint32(offs);

                rooms.Add(ParseRoom(dataMap, o, sharedCache));
                offs += 4;
            }

            if (level.Name == 0x1C)
            {
                // rainbow cloud spawns things dynamically
                rooms[0].Objects.AddRange(new[]
                {
                    new ObjectSpawn
                    {
                        ID = 0x97,
                        Behaviour = 1,
                        Position = new Vector3(0, 100, 500),
                        Euler = Vector3.zero,
                        Scale = Vector3.one,
                    },
                    new ObjectSpawn
                    {
                        ID = 0x3E9,
                        Behaviour = 0,
                        Position = new Vector3(0, 0, 10000),
                        Euler = new Vector3(0, Mathf.PI, 0),
                        Scale = Vector3.one,
                    }
                });
            }
        
            SpawnParser spawnParser = new SpawnParser();

            spawnParser.DataMap = dataMap;
            var fishTable = ParseFishTable(dataMap, level.Name, spawnParser);

             var objectInfo = new List<ObjectDef>();

            if (level.Objects != 0)
            {
                var objFunctionView = dataMap.GetView(level.Objects);
                long offs2 = 0;

                while (objFunctionView.GetInt32(offs2 + 0x00, false) != 0)
                {
                    var id = objFunctionView.GetInt32(offs2 + 0x00, false);
                    var initFunc = objFunctionView.GetUint32(offs2 + 0x04, false);
                    offs2 += 0x10;

                    var o = ParseObject(dataMap, id, initFunc);
                    if (o != null)
                    {
                        objectInfo.Add(o);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (level.Name == 16)
            {
                var o = ParseObject(dataMap, 1006, 0x802CBDA0);
                if (o != null)
                {
                    objectInfo.Add(o);
                }
            }

                Debug.Log("Parsing Haunter");

            List<GFXNode> haunterData = null;
            if (level.Name == 18)
            {
                var sharedOutput = new RSPSharedOutput();
                haunterData = ParseGraph(dataMap, 0x801A5CC0, 0, 0x800A1650, sharedOutput);
            }

            try
            {
                long statics = dataMap.Deref(level.Header + 0x04);
                if (statics != 0)
                {
                    var staticView = dataMap.GetView(statics);
                    offs = 0;

                    while (true)
                    {
                        int id = staticView.GetInt32(offs + 0x00);
                        if (id == -1)
                            break;

                        long func = staticView.GetUint32(offs + 0x04);
                        long dlStart = staticView.GetUint32(offs + 0x08);
                        Debug.Assert(func == 0x800E30B0, VP_BYMLUtils.HexZero(func, 8));

                        var sharedOutput = new RSPSharedOutput();
                        var states = new List<RSPState>() { new RSPState(sharedOutput, dataMap) };
                        InitDL(states[0], true);

                        var model = RunRoomDL(dataMap, dlStart, states);

                        GFXNode node = new GFXNode
                        {
                            Model = model,
                            Billboard = 0,
                            Parent = -1,
                            Translation = Vector3.zero,
                            Euler = Vector3.zero,
                            Scale = Vector3.one,
                            Materials = new List<MaterialData>(),
                        };

                        objectInfo.Add(new StaticDef { ID = id, Node = node, SharedOutput = sharedOutput });

                        offs += 0x0C;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing STATISTICS");
                Debug.LogError(e.Message + " - " + e.StackTrace);
            }
        
            var zeroOne = BuildZeroOne(dataMap, level.Name);
            var projectiles = BuildProjectiles(dataMap);

            CollisionTree collision = null;
            if (level.Collision != 0)
                collision = SnapUtils.ParseCollisionTree(dataMap, level.Collision);

            var levelParticles = new CustomParticleSystem();//ParticleUtils.ParseParticles(archives[0].ParticleData, false);
            var pesterParticles = new CustomParticleSystem();//archives.Count > 1 ? ParticleUtils.ParseParticles(archives[1].ParticleData, true) : new CustomParticleSystem();

            var eggData = EggUtils.BuildEggData(dataMap, level.Name);

            return new Level (archives[0].Name, rooms, skybox, sharedCache, objectInfo, collision, zeroOne, projectiles, fishTable, levelParticles, pesterParticles, eggData, haunterData);
        }

        

        

        public static List<ProjectileData> BuildProjectiles(CRGDataMap dataMap)
        {
            try
            {
                var output = new List<ProjectileData>();

                // Apple
                {
                    var sharedOutput = new RSPSharedOutput();
                    var nodes = ParseGraph(dataMap, 0x800EAED0, 0x800EAC58, 0x800A15D8, sharedOutput);
                    var animations = new List<AnimationData>()
                    {
                        new AnimationData
                        {
                            FPS = 12,
                            Frames = 0,
                            Tracks = new List<AnimationTrack> { null, null },
                            MaterialTracks = ParseMaterialAnimation(dataMap, 0x800EAF60, nodes)
                        }
                    };
                    output.Add(new ProjectileData { Nodes = nodes, SharedOutput = sharedOutput, Animations = animations });
                }

                // Pester Ball
                {
                    var sharedOutput = new RSPSharedOutput();
                    var nodes = ParseGraph(dataMap, 0x800E9138, 0x800E8EB8, 0x800A15D8, sharedOutput);
                    var animations = new List<AnimationData>
                    {
                        new AnimationData
                        {
                            FPS = 12,
                            Frames = 0,
                            Tracks = new List<AnimationTrack> { null, null },
                            MaterialTracks = ParseMaterialAnimation(dataMap, 0x800E91C0, nodes)
                        }
                    };
                    output.Add(new ProjectileData { Nodes = nodes, SharedOutput = sharedOutput, Animations = animations });
                }

                // Water Splash
                {
                    var sharedOutput = new RSPSharedOutput();
                    var nodes = ParseGraph(dataMap, 0x800EB430, 0x800EB510, 0x800A1608, sharedOutput);
                    var tracks = new List<AnimationTrack>();
                    for (int i = 0; i < nodes.Count; i++)
                        tracks.Add(ParseAnimationTrack(dataMap, dataMap.Deref(0x800EAFB0 + 4 * i)));

                    var animations = new List<AnimationData>
                    {
                        new AnimationData
                        {
                            FPS = 27,
                            Frames = 0,
                            Tracks = tracks,
                            MaterialTracks = ParseMaterialAnimation(dataMap, 0x800EB0C0, nodes)
                        }
                    };
                    output.Add(new ProjectileData { Nodes = nodes, SharedOutput = sharedOutput, Animations = animations });
                }

                // Lava Splash
                {
                    var sharedOutput = new RSPSharedOutput();
                    var nodes = ParseGraph(dataMap, 0x800EDAB0, 0x800EDB90, 0x800A1608, sharedOutput);
                    var tracks = new List<AnimationTrack>();
                    for (int i = 0; i < nodes.Count; i++)
                        tracks.Add(ParseAnimationTrack(dataMap, dataMap.Deref(0x800ED5B0 + 4 * i)));

                    var animations = new List<AnimationData>
                    {
                        new AnimationData
                        {
                            FPS = 27,
                            Frames = 0,
                            Tracks = tracks,
                            MaterialTracks = ParseMaterialAnimation(dataMap, 0x800ED6B0, nodes)
                        }
                    };
                    output.Add(new ProjectileData { Nodes = nodes, SharedOutput = sharedOutput, Animations = animations });
                }

                return output;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing PROJECTILE DATA");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new List<ProjectileData>(); 
            }      
        }

        public static uint[] GetCartAnimationAddresses(long id)
        {
            switch (id)
            {
                case 16: return new[] { 0x8013C580, 0x8013CEA0 };
                case 18: return new[] { 0x8013D920, 0x8013E3D0 };
                case 24: return new[] { 0x801174E0, 0x801182F0 };
                case 22: return new[] { 0x8014A660, 0x8014B450 };
                case 20: return new[] { 0x80147540, 0x80148420 };
                case 26: return new[] { 0x80120520, 0x801212A0 };
                case 28: return new[] { 0x80119AE0, 0x8011A970 };
            }
            throw new Exception("Bad level ID");
        }

        public static ZeroOne BuildZeroOne(CRGDataMap dataMap, long id)
        {
            try
            {
                long graphStart = 0x803AAA30;
                long materials = 0x8039D938;
                var sharedOutput = new RSPSharedOutput();
                var nodes = ParseGraph(dataMap, graphStart, materials, 0x800A16B0, sharedOutput);

                var animAddrs = GetCartAnimationAddresses(id);
                var tracks = new List<AnimationTrack>();

                for (int i = 0; i < nodes.Count; i++)
                {
                    long trackStart = dataMap.Deref(animAddrs[0] + 4 * i);
                    tracks.Add(ParseAnimationTrack(dataMap, trackStart));
                }

                var animations = new List<AnimationData>
                {
                    new AnimationData
                    {
                        FPS = 15,
                        Frames = 0,
                        Tracks = tracks.Cast<AnimationTrack>().ToList(),
                        MaterialTracks = ParseMaterialAnimation(dataMap, animAddrs[1], nodes)
                    }
                };

                return new ZeroOne
                {
                    Nodes = nodes,
                    SharedOutput = sharedOutput,
                    Animations = animations
                };
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing ZERO ONE");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new ZeroOne
                {
                    Nodes = new List<GFXNode>(),
                    SharedOutput = new RSPSharedOutput(),
                    Animations = new List<AnimationData>()
                };
            }
        }

        public static List<FishEntry> ParseFishTable(CRGDataMap dataMap, long id, SpawnParser spawnParser = null)
        {
            long fishStart = 0;
            switch (id)
            {
                case 16: fishStart = 0x802CC004; break;
                case 18: fishStart = 0x802EE120; break;
                case 20: fishStart = 0x802C6368; break;
                case 22: fishStart = 0x802E2908; break;
                case 24: fishStart = 0x802E0EA4; break;
                case 26: fishStart = 0x802D29B4; break;
            }

            if (fishStart == 0)
                return new List<FishEntry>();

            var fish = new List<FishEntry>();
            var view = dataMap.GetView(fishStart);
            long offs = 0;
            long total = 0;
        
            if (spawnParser == null)
                spawnParser = new SpawnParser();

            while (true)
            {
                sbyte probability = view.GetInt8(offs + 0x00);
                long spawner = view.GetUint32(offs + 0x04);
                offs += 8;

                if (probability < 0)
                    break;

                total += probability;

                if (spawner == 0)
                {
                    fish.Add(new FishEntry { Probability = probability, ID = 0 });
                    break;
                }

                spawnParser.ParseFromView(dataMap.GetView(spawner));
                //Debug.Assert(spawnParser.Data.ID != 0);

                fish.Add(new FishEntry { Probability = probability, ID = spawnParser.Data.ID });
            }

            if (id == 22)
                total = 100;

            for (int i = 0; i < fish.Count; i++)
                fish[i].Probability = fish[i].Probability / total;

            return fish;
        }

        public const long photoDataStart = 0x800ADBEC;
        public static Vector3 Div(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 MulScalar(Vector3 a, float scalar)
        {
            return new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        }

        static List<GFXNode> ParseGraph(CRGDataMap dataMap, long graphStart, long materialList, long renderFunc, RSPSharedOutput output, long overlay = 0)
        {
            var view = dataMap.GetView(graphStart);
            var nodes = new List<GFXNode>();

            var parentIndices = new List<int>();

            var states = new RSPState[] 
            {
                new RSPState(output, dataMap),
                new RSPState(output, dataMap)
            };

            var renderer = SelectRenderer(renderFunc);
            InitDL(states[0], true);
            InitDL(states[1], false);

            int currIndex = 0;
            int offs = 0;
            while (true)
            {
                var billboard = view.GetUint8(offs + 0x02) >> 4;
                var depth = view.GetUint16(offs + 0x02, false) & 0xFFF;
                if (depth == 0x12)
                    break;

                var dl = view.GetUint32(offs + 0x04, false);
                Vector3 translation = GetVec3(view, offs + 0x08);
                Vector3 euler = GetVec3(view, offs + 0x14);
                Vector3 scale = GetVec3(view, offs + 0x20);

                var parent = depth == 0 ? -1 : parentIndices[depth - 1];
                if (parentIndices.Count <= depth)
                    parentIndices.Add(currIndex);
                else
                    parentIndices[depth] = currIndex;

                var node = new GFXNode
                {
                    Billboard = billboard,
                    Translation = translation,
                    Euler = euler,
                    Scale = scale,
                    Parent = parent,
                    Materials = new List<MaterialData>()
                };

                if (dl > 0)
                {
                    node.Materials = materialList == 0 ? new List<MaterialData>() : ParseMaterialData(dataMap, dataMap.Deref(materialList + currIndex * 4));
                    node.Model = renderer(dataMap, dl, states, node.Materials);
                    states[0].Clear();
                    states[1].Clear();
                }

                nodes.Add(node);
                offs += 0x2c;
                currIndex++;
            }

            return nodes;
        }

        public delegate Model graphRenderer(CRGDataMap dataMap, long displayList, RSPState[] states, List<MaterialData> materials = null);

        public static graphRenderer SelectRenderer(long addr)
        {
            long ad = (long)addr;
            switch (ad)
            {
                case 0x80014F98:
                case 0x800A15D8:
                case 0x803594DC: // object
                    return runRoomDL;

                case 0x800A1650: // fog
                case 0x800A1680:
                case 0x8035942C: // object, fog
                case 0x8035958C: // object
                    return runSplitDL;

                case 0x802DE26C: // moltres: set 2 cycle and no Z update
                    return moltresDL;

                case 0x80359534: // object
                case 0x800A1608:
                case 0x802DFAE4: // volcano smoke: disable TLUT, set xlu
                    return runMultiDL;

                case 0x80359484: // object
                case 0x800A16B0: // just the zero-one?
                    return runMultiSplitDL;

                default:
                    throw new System.Exception($"unknown renderfunc {VP_BYMLUtils.HexZero(addr, 8)}");
            }
        }

        private static Model runSplitDL(CRGDataMap dataMap, long dlPair, RSPState[] states, List<MaterialData> materials = null)
        {
            var view = dataMap.GetView(dlPair);
            long firstDL = view.GetUint32(0x00, false);
            long secondDL = view.GetUint32(0x04, false);
            var rspState = states[0];
            rspState.GSPResetMatrixStackDepth(1);
            if (firstDL != 0)
                F3DEXUtils.RunDL_F3DEX2(rspState, firstDL, MaterialDLHandler(materials ?? new List<MaterialData>()));
            rspState.GSPResetMatrixStackDepth(0);
            if (secondDL != 0)
                F3DEXUtils.RunDL_F3DEX2(rspState, secondDL, MaterialDLHandler(materials ?? new List<MaterialData>()));
            var rspOutput = rspState.Finish();
            return new Model { SharedOutput = rspState.sharedOutput, RSPState = rspState, RSPOutput = rspOutput };
        }

        private static Model moltresDL(CRGDataMap dataMap, long dlPair, RSPState[] states, List<MaterialData> materials = null)
        {
            return runSplitDL(dataMap, dlPair, states, materials);
        }

        private static Model runMultiDL(CRGDataMap dataMap, long dlList, RSPState[] states, List<MaterialData> materials = null)
        {
            var view = dataMap.GetView(dlList);
            var handler = MaterialDLHandler(materials ?? new List<MaterialData>());
            int offs = 0;
            while (true)
            {
                long Index = view.GetUint32(offs, false);
                long dlStart = view.GetUint32(offs + 4, false);
                if (Index == 4) break;
                F3DEXUtils.RunDL_F3DEX2(states[Index], dlStart, handler);
                offs += 8;
            }
            var rspOutput = states[0].Finish();
            var xluOutput = states[1].Finish();
            if (rspOutput == null) rspOutput = xluOutput;
            else if (xluOutput != null) rspOutput.DrawCalls.AddRange(xluOutput.DrawCalls);
            return new Model { SharedOutput = states[0].sharedOutput, RSPState = states[0], RSPOutput = rspOutput };
        }

        private static Model runMultiSplitDL(CRGDataMap dataMap, long dlList, RSPState[] states, List<MaterialData> materials = null)
        {
            var view = dataMap.GetView(dlList);
            int offs = 0;
            while (true)
            {
                long Index = view.GetUint32(offs, false);
                if (Index == 4) break;
                runSplitDL(dataMap, dlList + (offs + 4), new RSPState[] { states[Index] }, materials);
                offs += 0xC;
            }
            var rspOutput = states[0].Finish();
            var xluOutput = states[1].Finish();
            if (rspOutput == null) rspOutput = xluOutput;
            else if (xluOutput != null) rspOutput.DrawCalls.AddRange(xluOutput.DrawCalls);
            return new Model { SharedOutput = states[0].sharedOutput, RSPState = states[0], RSPOutput = rspOutput };
        }

        private static Model runRoomDL(CRGDataMap dataMap, long displayList, RSPState[] states, List<MaterialData> materials = null)
        {
            var rspState = states[0];
            F3DEXUtils.RunDL_F3DEX2(rspState, displayList, MaterialDLHandler(materials ?? new List<MaterialData>()));
            var rspOutput = rspState.Finish();
            return new Model { SharedOutput = rspState.sharedOutput, RSPState = rspState, RSPOutput = rspOutput };
        }

        public static ActorDef ParseObject(CRGDataMap dataMap, long id, long initFunc)
        {
            var dataFinder = new MIPS.ObjectDataFinder();

            var initView = dataMap.GetView(initFunc);
            if (!dataFinder.ParseFromView(initView) && initFunc != 0x802EAF18)
                throw new System.Exception($"bad parse for init function {initFunc:X8}");

            var objectView = dataMap.GetView(dataFinder.DataAddress);

            long graphStart = objectView.GetUint32(0x00, false);
            long materials = objectView.GetUint32(0x04, false);
            long renderer = objectView.GetUint32(0x08, false);

            bool usePhoto = false;

            if (id != 93 && id != 101)
            {
                var photoView = dataMap.GetView(photoDataStart);
                long offs = 0;
                while (offs < photoView.ByteLength)
                {
                    long photoID = photoView.GetUint32(offs, false);
                    if (photoID != id)
                    {
                        offs += 0x14;
                        continue;
                    }

                    graphStart = photoView.GetUint32(offs + 0x08, false);
                    materials = photoView.GetUint32(offs + 0x0C, false);
                    renderer = photoView.GetUint32(offs + 0x10, false);
                    usePhoto = true;
                    break;
                }
            }

            long animationStart = objectView.GetUint32(0x0C, false);
            var scale = GetVec3(objectView, 0x10);
            var center = GetVec3(objectView, 0x1C);
            float radius = objectView.GetFloat32(0x28, false) * scale.y;
            ushort flags = objectView.GetUint16(0x2C, false);
            long extraTransforms = objectView.GetUint32(0x2E, false) >> 8;

            center = Div(center, scale);
            scale = MulScalar(scale, 0.1f);

            var sharedOutput = new RSPSharedOutput();

            try
            {
                if (usePhoto && id != 1003)
                    dataMap.overlay = 2;

                if (id == 1007)
                {
                    Debug.Log("Parsing palm tree!");
                }

                var nodes = ParseGraph(dataMap, graphStart, materials, renderer, sharedOutput);
                var stateGraph = ParseStateGraph(dataMap, animationStart, nodes);

                dataMap.overlay = 0;

                return new ActorDef
                {
                    ID = id,
                    Flags = flags,
                    Nodes = nodes,
                    Scale = scale,
                    Center = center,
                    Radius = radius,
                    SharedOutput = sharedOutput,
                    Spawn = GetSpawnType(dataFinder.SpawnFunc),
                    StateGraph = stateGraph,
                    GlobalPointer = dataFinder.GlobalRef
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        public static SpawnType GetSpawnType(long addr)
        {
            switch (addr)
            {
                case 0x362EE0: // normal spawn
                case 0x362E10: // spawn plus some other gfx function?
                    return SpawnType.Ground;
                case 0x362E5C: // normal flying
                case 0x362DC4: // flying plus other gfx function
                    return SpawnType.Flying;
                default:
                    return SpawnType.Other;
            }
        }

        private static Vector3 GetVec3(VP_DataView view, long offset)
        {
            return SnapUtils.GetVec3(view, offset);
        }

        public static DlRunner MaterialDLHandler(List<MaterialData> scrollData)
        {
            return (RSPState state, long addr, DlRunner subDLHandler) =>
            {
                Debug.Assert((addr >> 24) == 0x0E, $"bad dl jump address {addr:X8}");
                state.materialIndex = (addr >> 3) & 0xFF;
                var scroll = scrollData[((int)addr >> 3) & 0xFF];

                if ((scroll.Flags & MaterialFlags.Palette) != 0)
                {
                    state.GDPSetTextureImage((long)ImageFormat.RGBA, (long)ImageSize.SIZE_16B, 0, state.dataMap.Deref(scroll.PaletteStart));
                    if ((scroll.Flags & (MaterialFlags.Tex1 | MaterialFlags.Tex2)) != 0)
                    {
                        state.GDPSetTile((long)ImageFormat.RGBA, (long)ImageSize.SIZE_4B, 0, 0x100, 5, 0, 0, 0, 0, 0, 0, 0);
                        state.GDPLoadTLUT(5, scroll.Textures[0].Size == ImageSize.SIZE_8B ? 256 : 16);
                    }
                }

                if ((scroll.Flags & (MaterialFlags.Prim | MaterialFlags.Special | MaterialFlags.PrimLOD)) != 0)
                {
                    state.GSPSetPrimColor(
                        scroll.PrimaryLOD,
                        (scroll.PrimaryColor[0] * 255),
                        (scroll.PrimaryColor[1] * 255),
                        (scroll.PrimaryColor[2] * 255),
                        (scroll.PrimaryColor[3] * 255)
                    );
                }

                if ((scroll.Flags & (MaterialFlags.Tex2 | MaterialFlags.Special)) != 0)
                {
                    var siz2 = scroll.Textures[1].Size == ImageSize.SIZE_32B ? ImageSize.SIZE_32B : ImageSize.SIZE_16B;
                    var texOffset = (scroll.Flags & (MaterialFlags.Tex1 | MaterialFlags.Special)) == 0 ? 0 : 4;

                    state.GDPSetTextureImage((long)scroll.Textures[1].Format, (long)siz2, 0, state.dataMap.Deref(scroll.TextureStart + texOffset));

                    if ((scroll.Flags & (MaterialFlags.Tex1 | MaterialFlags.Special)) != 0)
                    {
                        int texels = (int)(scroll.Textures[1].Width * scroll.Textures[1].Height);
                        switch (scroll.Textures[1].Size)
                        {
                            case ImageSize.SIZE_4B:
                                texels = (texels + 4) >> 2;
                                break;
                            case ImageSize.SIZE_8B:
                                texels = (texels + 1) >> 1;
                                break;
                        }
                        int dxt = (1 << (14 - (int)scroll.Textures[1].Size)) / (int)scroll.Textures[1].Width;
                        state.GDPLoadBlock(6, 0, 0, texels, dxt);
                    }
                }

                if ((scroll.Flags & (MaterialFlags.Tex1 | MaterialFlags.Special)) != 0)
                {
                    state.GDPSetTextureImage((long)scroll.Textures[0].Format, (long)scroll.Textures[0].Size, 0, state.dataMap.Deref(scroll.TextureStart));
                }

                float adjXScale = scroll.Halve == 0 ? scroll.xScale : scroll.xScale / 2;

                if ((scroll.Flags & MaterialFlags.Tile0) != 0)
                {
                    float uls = (scroll.Tiles[0].Width * scroll.Tiles[0].xShift + scroll.Shift) / adjXScale;
                    float ult = (scroll.Tiles[0].Height * (1 - scroll.Tiles[0].yShift) + scroll.Shift) / scroll.yScale - scroll.Tiles[0].Height;
                    state.GDPSetTileSize(0, (long)(uls * 4), (long)(ult * 4), (long)((scroll.Tiles[0].Width + uls - 1) * 4), (long)((scroll.Tiles[0].Height + ult - 1) * 4));
                }

                if ((scroll.Flags & MaterialFlags.Tile1) != 0)
                {
                    float uls = (scroll.Tiles[1].Width * scroll.Tiles[1].xShift + scroll.Shift) / adjXScale;
                    float ult = (scroll.Tiles[1].Height * (1 - scroll.Tiles[1].yShift) + scroll.Shift) / scroll.yScale - scroll.Tiles[1].Height;
                    state.GDPSetTileSize(1, (long)(uls * 4), (long)(ult * 4), (long)((scroll.Tiles[1].Width + uls - 1) * 4), (long)((scroll.Tiles[1].Height + ult - 1) * 4));
                }

                if ((scroll.Flags & MaterialFlags.Scale) != 0)
                {
                    float s = (1 << 21) / scroll.Scale / adjXScale;
                    float t = (1 << 21) / scroll.Scale / scroll.yScale;
                    state.GSPTexture(true, 0, 0, (long)s, (long)t);
                }
            };
        }

        public static Model RunRoomDL(CRGDataMap dataMap, long displayList, List<RSPState> states, List<MaterialData> materials = null)
        {
            var rspState = states[0];
            F3DEXUtils.RunDL_F3DEX2(rspState, displayList, MaterialDLHandler(materials ?? new List<MaterialData>()));
            var rspOutput = rspState.Finish();
            return new Model { SharedOutput = rspState.sharedOutput, RSPState = rspState, RSPOutput = rspOutput };
        }

        public static void InitDL(RSPState rspState, bool opaque, bool twoCycle = true)
        {
            rspState.GSPSetGeometryMode((long)RSP_Geometry.G_SHADE);

            if (opaque)
            {
                // Opaque surfaces
                rspState.GDPSetOtherModeL(0, 29, 0x0C192078);
                rspState.GSPSetGeometryMode((long)RSP_Geometry.G_LIGHTING);
            }
            else
            {
                // Translucent surfaces
                rspState.GDPSetOtherModeL(0, 29, 0x005049D8);
            }

            rspState.GDPSetOtherModeH(
                (long)OtherModeH_Layout.G_MDSFT_TEXTFILT,
                2,
                (int)TextFilt.G_TF_BILERP << (int)OtherModeH_Layout.G_MDSFT_TEXTFILT
            );

            if (twoCycle)
            {
                rspState.GDPSetOtherModeH(
                    (long)OtherModeH_Layout.G_MDSFT_CYCLETYPE,
                    2,
                    (int)OtherModeH_CycleType.G_CYC_2CYCLE << (int)OtherModeH_Layout.G_MDSFT_CYCLETYPE
                );
            }

            // Assume this gets set, some objects rely on it
            rspState.GDPSetTile((long)ImageFormat.RGBA, (long)ImageSize.SIZE_16B, 0, 0x100, 5, 0, 0, 0, 0, 0, 0, 0);
        }


        public static Room ParseRoom(CRGDataMap dataMap, long roomStart, RDP.TextureCache sharedCache)
        {
            var view = dataMap.GetView(roomStart);

            var roomGeoStart = view.GetUint32(0x00);
            Vector3 pos = GetVec3(view, 0x04);
            float yaw = view.GetFloat32(0x10);
            if (yaw != 0)
                throw new System.Exception("Yaw must be 0");

            var staticSpawns = view.GetUint32(0x18);
            var objectSpawns = view.GetUint32(0x1C);

            pos *= 100;

            var roomView = dataMap.GetView(roomGeoStart);
            var dlStart = roomView.GetUint32(0x00);
            var materialData = roomView.GetUint32(0x04);
            var animData = roomView.GetUint32(0x08);
            var renderer = roomView.GetUint32(0x0C);
            var graphStart = roomView.GetUint32(0x10);
            var moreAnimData = roomView.GetUint32(0x18);
            var animTimeScale = roomView.GetUint32(0x1C);

            var sharedOutput = new RSPSharedOutput();
            sharedOutput.TextureCache = sharedCache;
            var rspState = new RSPState(sharedOutput, dataMap);
            InitDL(rspState, true);

            var materials = materialData != 0 ? ParseMaterialData(dataMap, dataMap.Deref(materialData)) : new List<MaterialData>();
            var model = RunRoomDL(dataMap, dlStart, new List<RSPState> { rspState }, materials);

            var node = new GFXNode
            {
                Model = model,
                Billboard = 0,
                Parent = -1,
                Translation = pos,
                Euler = Vector3.zero,
                Scale = Vector3.one,
                Materials = materials,
            };

            AnimationData animation = null;
            if (animData != 0)
            {
                animation = new AnimationData
                {
                    FPS = 30,
                    Frames = 0,
                    Tracks = new List<AnimationTrack>(),
                    MaterialTracks = ParseMaterialAnimation(dataMap, animData, new List<GFXNode>() { node })
                };
            }

            var objects = new List<ObjectSpawn>();
            if (staticSpawns > 0)
            {
                var objView = dataMap.GetView(staticSpawns);
                int offs = 0;
                while (true)
                {
                    int id = objView.GetInt32(offs + 0x00, false);
                    if (id == -1)
                        break;
                    Vector3 objPos = GetVec3(objView, offs + 0x04) * 100 + pos;
                    Vector3 euler = GetVec3(objView, offs + 0x10);
                    Vector3 scale = GetVec3(objView, offs + 0x1C);

                    objects.Add(new ObjectSpawn { ID = id, Behaviour = 0, Position = objPos, Euler = euler, Scale = scale });
                    offs += 0x28;
                }
            }

            if (objectSpawns > 0)
            {
                var objView = dataMap.GetView(objectSpawns);
                int offs = 0;
                while (true)
                {
                    int id = objView.GetInt32(offs + 0x00, false);
                    if (id == -1)
                        break;

                    int behavior = objView.GetInt32(offs + 0x04, false);
                    Vector3 objPos = GetVec3(objView, offs + 0x08) * 100 + pos;
                    Vector3 euler = GetVec3(objView, offs + 0x14);
                    Vector3 scale = GetVec3(objView, offs + 0x20);
                    long pathAddr = objView.GetUint32(offs + 0x2C, false);

                    var newSpawn = new ObjectSpawn { ID = id, Behaviour = behavior, Position = objPos, Euler = euler, Scale = scale };
                    if (pathAddr != 0)
                        newSpawn.Path = ParsePath(dataMap, pathAddr);

                    objects.Add(newSpawn);
                    offs += 0x30;
                }
            }

            return new Room { Node = node, Objects = objects, Animation = animation };
        }

        public static TrackPath ParsePath(CRGDataMap dataMap, long addr)
        {
            return MIPSUtils.ParsePath(dataMap, addr);
        }

        public static long EntryDataSize(EntryKind kind, long count)
        {
            return MIPSUtils.EntryDataSize(kind, count);
        }

        public static bool EntryShouldBlock(EntryKind kind)
        {
            return MIPSUtils.EntryShouldBlock(kind);
        }

        public static List<MaterialData> ParseMaterialData(CRGDataMap dataMap, long listStart)
        {
            return MIPSUtils.ParseMaterialData(dataMap, listStart);
        }

        public static void FindNewTextures(CRGDataMap dataMap, AnimationTrack track, GFXNode node, int index)
        {
            MIPSUtils.FindNewTextures(dataMap, track, node, index);
        }

        public static List<List<AnimationTrack>> ParseMaterialAnimation(CRGDataMap dataMap, long addr, List<GFXNode> nodes)
        {
            return MIPSUtils.ParseMaterialAnimation(dataMap, addr, nodes);
        }

        public static AnimationTrack ParseAnimationTrack(CRGDataMap dataMap, long addr)
        {
            return MIPSUtils.ParseAnimationTrack(dataMap, addr);
        }

        public static Vector4 GetColor(VP_DataView view, long offset)
        {
            return MIPSUtils.GetColor(view, offset);
        }

        public static StateGraph ParseStateGraph(CRGDataMap dataMap, long addr, List<GFXNode> nodes)
        {
            return MIPSUtils.ParseStateGraph(dataMap, addr, nodes); 
        }
    }
}