
using System.Collections;
using System.Collections.Generic;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    [System.Serializable]
    public class Level
    {
        public uint Name;
        public RDP.TextureCache SharedCache;
        public Room Skybox;
        public List<Room> Rooms = new List<Room>();
        public List<ObjectDef> ObjectInfo = new List<ObjectDef>();
        public CollisionTree Collision;
        public ZeroOne ZeroOne;
        public List<ProjectileData> Projectiles = new List<ProjectileData>();
        public List<FishEntry> FishTable = new List<FishEntry>();
        public CustomParticleSystem LevelParticles;
        public CustomParticleSystem PesterParticles;
        public VP_Float32Array<VP_ArrayBuffer> EggData;
        public List<GFXNode> HaunterData;
        public Level()
        {
        }

        public Level(uint name, List<Room> rooms, Room skybox, RDP.TextureCache sharedCache, List<ObjectDef> objectInfo,
            CollisionTree collision, ZeroOne zeroOne, List<ProjectileData> projectiles, List<FishEntry> fishTable,
            CustomParticleSystem levelParticles, CustomParticleSystem pesterParticles, VP_Float32Array eggData, List<GFXNode> haunterData)
        {
            Name = name;
            Rooms = rooms;
            Skybox = skybox;
            SharedCache = sharedCache;
            ObjectInfo = objectInfo;
            Collision = collision;
            ZeroOne = zeroOne;
            Projectiles = projectiles;
            FishTable = fishTable;
            LevelParticles = levelParticles;
            PesterParticles = pesterParticles;
            EggData = eggData;
            HaunterData = haunterData;
        }
    }
}
