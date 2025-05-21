using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using VirtualPhenix.Nintendo64.BanjoKazooie;
using System.IO;
using System;
using VirtualPhenix.PokemonSnap64;


namespace VirtualPhenix.Nintendo64.PokemonSnap { 

    public class SceneCreator : EditorWindow
    {
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
            var snapExtractor = FindObjectOfType<SnapExtractor>();
            string defaultID = snapExtractor != null && !string.IsNullOrEmpty(snapExtractor.CRGFile) ? snapExtractor.CRGFile : "10";
            var list = GetArcFileListById(defaultID);
            var archives = LoadLevelArchives(list);
            var parsedLevel = ParseLevel(archives);

            UnityEngine.Debug.Log("Parsed level has room count: " + parsedLevel.Rooms.Count);

            SpawnObjectsFromParsedLevelRooms(parsedLevel);
        }

        public static void SpawnObjectsFromParsedLevelRooms(Level level)
        {
            GameObject go = new GameObject("[Level "+ level.Name+"]");
            var pkLevel = go.AddComponent<PKSnap_Level>();

            var skyboxRoom = SpawnFromRoomData(level.Skybox, "Skybox", go.transform);
            PKSnap_Skybox skybox = skyboxRoom.gameObject.AddComponent<PKSnap_Skybox>();
            pkLevel.SetSkybox(skybox);

            int idx = 0;
            foreach (var rooms in level.Rooms)
            {
                var room = SpawnFromRoomData(rooms, "[Room "+ idx + "]", go.transform);
                pkLevel.AddRoom(room);
                idx++;
            }
        
            foreach (var objs in level.ObjectInfo)
            {
                PKSnap_Actor spawnedActor = SpawnActorFromLevelData(objs, "[Actor " + objs.ID + "]", go.transform, objs.ID);
                SpawnActorPerSpawnObject(spawnedActor, pkLevel);
            }

            go.transform.localScale = new Vector3(-0.01f, 0.01f, 0.01f);
        }

        public static void SpawnActorPerSpawnObject(PKSnap_Actor actor, PKSnap_Level pkLevel)
        {
                bool found = false;
            foreach (var room in pkLevel.Rooms)
            {
                if (room.HasActor(actor.ID, out List<PKSnap_ObjectData> spawnedObjects))
                {
                    foreach (var obj in spawnedObjects)
                    {
                        if (!found)
                            found = true;

                        PKSnap_Actor ac = Instantiate(actor, obj.transform);
                        ac.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                }
            }

            if (found)
            {
                DestroyImmediate(actor.gameObject);
            }           
        }
        public static Sprite ToUnitySprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), // pivot en el centro
                100.0f                   // pixelsPerUnit (ajústalo según necesidad)
            );
        }

        public static PKSnap_Actor SpawnActorFromLevelData(ObjectDef data, string customName, Transform parent, long id)
        {
            Debug.Log("Parsing Actor with id: " + data.ID);
            GameObject go = new GameObject(customName);
            go.transform.parent = parent;

            var textures = BuildTexturesFromTextureCache(data.SharedOutput.TextureCache, customName);
    
            if (data is StaticDef)
            {
                StaticDef staticData = (StaticDef)data;

                var materials = BuildMaterialsFromData(staticData.Node.Materials, textures);
                var mesh = BuildMeshFromRSPVertices(staticData.SharedOutput.Vertices, staticData.SharedOutput.Indices);
                if (mesh != null)
                {
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.materials = materials.ToArray();
                    var mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;
                }
            }
            else
            {

            }


            var actor = go.AddComponent<PKSnap_Actor>();
            actor.InitActor(id, textures);
            return actor;
        }

        public static List<Texture2D> BuildTexturesFromTextureCache(RDP.TextureCache textureCache, string customName)
        {
            var list = new List<Texture2D>();

            Debug.Log("Texture Cache Count: " + textureCache.textures.Count);
            foreach (RDP.Texture rdpTex in textureCache.textures)
            {
                if (rdpTex == null || rdpTex.pixels == null || rdpTex.pixels.Length == 0)
                {
                    Debug.LogError("Error parsing TEXTURE in cache for " + customName);
                    continue;
                }

                var texture = new Texture2D((int)rdpTex.width, (int)rdpTex.height, TextureFormat.RGBA32, mipChain: false);
                texture.name = rdpTex.name;

                // RGBA8 = 4 bytes por píxel
                int expectedLength = (int)(rdpTex.width * rdpTex.height * 4);
                if (rdpTex.pixels.Length < expectedLength)
                {
                    Debug.LogError($"Pixel buffer too small: expected {expectedLength}, got {rdpTex.pixels.Length}");
                    continue;
                }

                // Cargar directamente los bytes al texture
                texture.LoadRawTextureData(rdpTex.pixels);
                texture.Apply();

                list.Add(texture);
            }

            return list;
        }

        public static List<Material> BuildMaterialsFromData(List<MaterialData> materialData, List<Texture2D> loadedTextures)
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
                foreach (var ut in md.UsedTextures)
                {
                    Debug.Log("===========" );
                    Debug.Log("TEXTURE ID: " + ut.TextureID);
                    Debug.Log("TEXTURE PAL: " + ut.PAL);
                    Debug.Log("TEXTURE Index: " + ut.Index);
                    Debug.Log("===========" );
                }
            }

            return materials;
        }
        public static PKSnap_Room SpawnFromRoomData(Room data, string customName, Transform parent)
        {
            Debug.Log("Parsing Room: " + customName);
            Dictionary<long, List<PKSnap_ObjectData>> roomObjectDict = new Dictionary<long, List<PKSnap_ObjectData>>();
            List<Texture2D> textures = BuildTexturesFromTextureCache(data.Node.Model.SharedOutput.TextureCache, customName);
            List<Material> materials = BuildMaterialsFromData(data.Node.Materials, textures);
            GameObject go = new GameObject(customName);
            go.transform.parent = parent;
            go.transform.position = data.Node.Translation;
            go.transform.rotation = Quaternion.Euler(data.Node.Euler);
            go.transform.localScale = data.Node.Scale;
            PKSnap_Room pkRoom = go.AddComponent<PKSnap_Room>();
            var model = data.Node.Model;
            var mesh = BuildMeshFromRSPVertices(model.SharedOutput.Vertices, model.SharedOutput.Indices);
            if (mesh != null)
            {
                var mr = go.AddComponent<MeshRenderer>();

                mr.materials = materials.ToArray();
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
            }
        

            foreach (var o in data.Objects)
            {
                GameObject ob = new GameObject("[Room Object: " + o.ID + "]");
                ob.transform.parent = go.transform;
                ob.transform.transform.position = o.Position;
                ob.transform.rotation = Quaternion.Euler(o.Euler);
                ob.transform.localScale = o.Scale;
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

        public static Mesh BuildMeshFromRSPVertices(List<RSPVertex> vertices, List<long> indices)
        {
            Mesh mesh = new Mesh();

            Vector3[] unityVerts = new Vector3[vertices.Count];
            Color32[] unityColors = new Color32[vertices.Count];
            Vector2[] unityUVs = new Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                unityVerts[i] = new Vector3((float)v.x, (float)v.y, (float)v.z);
                unityColors[i] = new Color((float)v.c0, (float)v.c1, (float)v.c2, (float)v.a);
                unityUVs[i] = new Vector2((float)v.tx, (float)v.ty /32); // Puede que necesites normalizar
            }

            // Suponemos que los indices ya están bien formados en tríos
            int[] triangles = indices.Select(idx => (int)idx).ToArray();

            mesh.vertices = unityVerts;
            mesh.colors32 = unityColors;
            mesh.uv = unityUVs;
            mesh.triangles = triangles;

            mesh.RecalculateNormals(); // opcional
            mesh.RecalculateBounds();

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

                Debug.Log("PkCount: " + pokemonArchives.Count);
                Debug.Log("LvCount: " + archives.Count);

            var level = archives[0];
            level.Log();

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
                    offs += 0x10;

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
                collision = ParseCollisionTree(dataMap, level.Collision);

            var levelParticles = ParticleUtils.ParseParticles(archives[0].ParticleData, false);
            var pesterParticles = archives.Count > 1 ? ParticleUtils.ParseParticles(archives[1].ParticleData, true) : new CustomParticleSystem();

            var eggData = BuildEggData(dataMap, level.Name);

            return new Level (archives[0].Name, rooms, skybox, sharedCache, objectInfo, collision, zeroOne, projectiles, fishTable, levelParticles, pesterParticles, eggData, haunterData);
        }

        public static VP_Float32Array BuildEggData(CRGDataMap dataMap, long id)
        {
            try
            {
                long start = 0;
                long count = 0;

                if (id == 18)
                {
                    start = 0x8018A6F0;
                    count = 0x154;
                }
                else if (id == 20)
                {
                    start = 0x8017C090;
                    count = 0x148;
                }
                else
                {
                    return null;
                }

                var data = new VP_Float32Array(count * 6);
                var view = dataMap.GetView(start);
                var dummyVertex = new StagingVertex();
                long j = 0;

                for (long i = 0; i < count; i++)
                {
                    dummyVertex.SetFromView(view, i << 4);
                    data[j++] = dummyVertex.x;
                    data[j++] = dummyVertex.y;
                    data[j++] = dummyVertex.z;
                    data[j++] = dummyVertex.c0;
                    data[j++] = dummyVertex.c1;
                    data[j++] = dummyVertex.c2;
                }

                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing EGG DATA");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new VP_Float32Array();
            }
            
        }

        public static CollisionTree ParseCollisionTree(CRGDataMap dataMap, long addr)
        {
            try
            {
                var view = dataMap.GetView(addr);
                long planeData = view.GetUint32(0x00);
                long treeData = view.GetUint32(0x04);
                // can be followed by another pair for ceilings

                var planeList = new List<GroundPlane>();
                var planeView = dataMap.GetView(planeData);
                var treeView = dataMap.GetView(treeData);

                return ParseCollisionSubtree(treeView, planeView, planeList, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing COLLISION TREE DATA");
                Debug.LogError(e.Message + " - " + e.StackTrace);
                return new CollisionTree();
            }
        }

        public static CollisionTree ParseCollisionSubtree(VP_DataView treeView, VP_DataView planeView, List<GroundPlane> planeList, long index)
        {
            long offs = index * 0x1C;
            Vector3 line = GetVec3(treeView, offs + 0x00);
            int posTreeIdx = treeView.GetInt32(offs + 0x0C);
            int negTreeIdx = treeView.GetInt32(offs + 0x10);
            int posPlaneIdx = treeView.GetInt32(offs + 0x14);
            int negPlaneIdx = treeView.GetInt32(offs + 0x18);

            GroundPlane GetPlane(int idx)
            {
                if (idx == -1)
                    return null;

                while (planeList.Count < idx + 1)
                {
                    long start = planeList.Count * 0x14;
                    float x = planeView.GetFloat32(start + 0x00);
                    float y = planeView.GetFloat32(start + 0x04);
                    float z = planeView.GetFloat32(start + 0x08);

                    planeList.Add(new GroundPlane
                    {
                        Normal = new Vector3(x, z, y), // the plane equation uses z up
                        Offset = planeView.GetFloat32(start + 0x0C),
                        Type = planeView.GetUint32(start + 0x10) >> 8,
                    });
                }

                return planeList[idx];
            }

            var posSubtree = posTreeIdx > -1 ? ParseCollisionSubtree(treeView, planeView, planeList, posTreeIdx) : null;
            var negSubtree = negTreeIdx > -1 ? ParseCollisionSubtree(treeView, planeView, planeList, negTreeIdx) : null;
            var posPlane = GetPlane(posPlaneIdx);
            var negPlane = GetPlane(negPlaneIdx);

            return new CollisionTree
            {
                Line = line,
                PosSubtree = posSubtree,
                NegSubtree = negSubtree,
                PosPlane = posPlane,
                NegPlane = negPlane
            };
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
                            Tracks = new List<AnimationTrack?> { null, null },
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
                            Tracks = new List<AnimationTrack?> { null, null },
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
                int billboard = view.GetUint8(offs + 0x02) >> 4;
                int depth = view.GetUint16(offs + 0x02, false) & 0xFFF;
                if (depth == 0x12)
                    break;

                long dl = view.GetUint32(offs + 0x04, false);
                Vector3 translation = GetVec3(view, offs + 0x08);
                Vector3 euler = GetVec3(view, offs + 0x14);
                Vector3 scale = GetVec3(view, offs + 0x20);

                int parent = depth == 0 ? -1 : parentIndices[depth - 1];
                if (parentIndices.Count <= depth)
                    parentIndices.Add(parent);
                else
                    parentIndices[depth] = parent;

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
            try
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

                if (usePhoto && id != 1003)
                    dataMap.overlay = 2;

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

                UnityEngine.Debug.LogError("Error parsing ACTORDEF");
                UnityEngine.Debug.LogError($"Error parsing Object {id}: {ex.Message} - {ex.StackTrace}");
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

        public static bool IsActor(ObjectDef def)
        {
            return def is ActorDef;
        }

        private static Vector3 GetVec3(VP_DataView view, long offset)
        {
            float x = view.GetFloat32(offset + 0x00, false);
            float y = view.GetFloat32(offset + 0x04, false);
            float z = view.GetFloat32(offset + 0x08, false);
            return new Vector3(x, y, z);
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