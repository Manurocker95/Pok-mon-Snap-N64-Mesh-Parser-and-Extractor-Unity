using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
	[System.Serializable]
    public class LevelGlobals : Target
    {
        public bool ThrowBalls = true;
        public bool PlayFlute = false;

        public Level Level;
        public long CurrentSong = 0;
        public float SongStart = 0;
        public List<Actor> AllActors = new List<Actor>();

        public float LastThrow = -1;
        public List<Projectile> Projectiles = new List<Projectile>();
        public bool PesterNext = false;

        public List<FishEntry> FishTable = new List<FishEntry>();
        public long FishTracker = 0;
        public Actor ActiveFish = null;

        public List<Splash> Splashes = new List<Splash>();
        public List<Actor> TempActors = new List<Actor>();
        public ModelRenderer ZeroOne;

        public ParticleManager Particles;

        private RenderData ZeroOneData;
        private List<RenderData> ProjData = new List<RenderData>();

        public GfxRenderInstList RenderInstListSky = new GfxRenderInstList();
        public GfxRenderInstList RenderInstListMain = new GfxRenderInstList();
        private List<Vector3> throwScratch = GfxPlatformUtils.NArray(2, () => Vector3.zero);
        public SceneContext Context;
        public string Id;

        public LevelGlobals(SceneContext context, string id)
        {
            Context = context;
            Id = id;
        }

        public virtual void Init(GfxRenderCache cache, Level level)
        {
            var device = cache.device;

            Level = level;
            ZeroOneData = new RenderData(device, cache, level.ZeroOne.SharedOutput);
            ProjData = new List<RenderData>();
            foreach (var proj in level.Projectiles)
            {
                ProjData.Add(new RenderData(device, cache, proj.SharedOutput));
            }
        }

        private Projectile CreateProjectile(long type)
        {
            var proj = new Projectile(ProjData[(int)type], Level.Projectiles[(int)type], type == 1);
            Projectiles.Add(proj);
            return proj;
        }
		
		public virtual void CreateSplash(SplashType type, Vector3 pos, Vector3? scale)
		{
			if (scale == null)
				scale = Vector3.one;

			Vector3 projectileScale = new Vector3(0.1f, 0.1f, 0.1f);


            if (type == SplashType.AppleWater)
			{
				var projData = ProjData[2];
				var projDef = Level.Projectiles[2];
				var splash = new Splash(projData, projDef.Nodes, projDef.Animations, type, projectileScale);
				splash.TryStart(pos, scale.Value, this);
				Splashes.Add(splash);
			}
			else if (type == SplashType.AppleLava)
			{
				var projData = ProjData[3];
				var projDef = Level.Projectiles[3];
				var splash = new Splash(projData, projDef.Nodes, projDef.Animations, type, projectileScale);
				splash.TryStart(pos, scale.Value, this);
				Splashes.Add(splash);
			}
			else
			{
				for (int i = 0; i < Splashes.Count; i++)
				{
					if (Splashes[i].Type == type && Splashes[i].TryStart(pos, scale.Value, this))
						break;
				}
			}
		}

		public virtual void Update(ViewerRenderInput viewerInput)
		{
			Translation = viewerInput.Camera.transform.position;

            if (PlayFlute)
			{
				if (CurrentSong == 0 || viewerInput.Time > SongStart + 10000)
				{
					CurrentSong = (long)InteractionType.PokefluteA + (long)(Random.value * 3);
					SongStart = viewerInput.Time;
				}
			}
			else
			{
				CurrentSong = 0;
			}

			if (LastThrow < 0)
				LastThrow = viewerInput.Time + 2000; // extra wait before the first throw

			bool shouldThrow = false;
			if (ThrowBalls)
			{
				if (viewerInput.Time > LastThrow + 2500)
					shouldThrow = true;

				if (Input.GetKeyDown(KeyCode.F))
					shouldThrow = true;
			}

			if (shouldThrow)
			{
				// if we're above ground, throw the next type of projectile
				if (Translation.y > SnapUtils.FindGroundHeight(Level.Collision, Translation.x, Translation.z) + 20)
				{
					var ts = throwScratch[0];
					MathHelper.GetMatrixAxisZ(ref ts, viewerInput.Camera.cameraToWorldMatrix);
					throwScratch[0] = ts;
                    throwScratch[0] = throwScratch[0] * -1;

					if (viewerInput.DeltaTime > 0)
					{
                        throwScratch[1] = viewerInput.LinearVelocity * (1000.0f / viewerInput.DeltaTime);
                        var ts2 = throwScratch[1];
						var ts3 = throwScratch[1];
                        MathHelper.TransformVec3Mat4w0(ref ts2, viewerInput.Camera.cameraToWorldMatrix, ts3);
						throwScratch[1] = ts2;
                    }
					else
					{
                        throwScratch[1] = Vector3.zero;
					}

					long projectileType = PesterNext ? 1 : 0;
					var proj = CreateProjectile(projectileType);
					proj.TryThrow(Translation, throwScratch[0], throwScratch[1]);

					LastThrow = viewerInput.Time;
					PesterNext = !PesterNext; // alternate apple and pester ball
				}
			}

			for (int i = 0; i < Projectiles.Count; i++)
			{
				if (!Projectiles[i].Visible)
					Projectiles.RemoveAt(i--);
			}

			for (int i = 0; i < Splashes.Count; i++)
			{
				if ((Splashes[i].Type == SplashType.AppleWater || Splashes[i].Type == SplashType.AppleLava) && !Splashes[i].Visible)
					Splashes.RemoveAt(i--);
			}
		}

		public virtual void SpawnFish(Vector3 pos)
		{
			// check active fish, and clear if it's done
			if (ActiveFish != null)
			{
				if (ActiveFish.Visible)
					return;
				else
					ActiveFish = null;
			}

			long id = 0;
			if (Id == "16") // river has special logic
			{
				var entry = FishTable[(int)FishTracker];
				if (Random.value < entry.Probability)
					id = entry.ID;
			}
			else // make a weighted random choice from the table
			{
				double p = Random.value;
				for (int i = 0; i < FishTable.Count; i++)
				{
					if (p < FishTable[i].Probability)
					{
						id = FishTable[i].ID;
						break;
					}
					else
					{
						p -= FishTable[i].Probability;
					}
				}
			}

			if (id == 0)
				return;

			// random yaw isn't explicit in the fish code, but seems to always be in the state logic
			ActiveFish = ActivateObject(id, pos, MathConstants.Tau * Random.value);
		}

		public virtual Actor ActivateObject(long id, Vector3 pos, double yaw, long behavior = 0, Vector3 scale = default(Vector3))
		{
			if (scale == default(Vector3))
				scale = Vector3.one;

			var chosen = TempActors.Find(a => a.Def.ID == id && !a.Visible);
			if (chosen == null)
				return null;

			// overwrite spawn data
			chosen.Spawn.Behaviour = behavior;
			chosen.Spawn.Position = pos;
			chosen.Spawn.Scale = scale;
			chosen.Spawn.Euler.y = (float)yaw;
			chosen.Reset(this);

			return chosen;
		}

		public virtual void SendGlobalSignal(Target source, long signal)
		{
			for (int i = 0; i < AllActors.Count; i++)
			{
				AllActors[i].ReceiveSignal(source, signal, this);
			}
		}

		public virtual void SendTargetedSignal(Target source, long signal, long targetPointer)
		{
			for (int i = 0; i < AllActors.Count; i++)
			{
				if (AllActors[i].GlobalPointer != targetPointer)
					continue;

				AllActors[i].ReceiveSignal(source, signal, this);
				break;
			}
		}

		public virtual ModelRenderer[] BuildTempObjects(List<ObjectDef> defs, List<RenderData> data, Level level)
		{
			List<ModelRenderer> output = new List<ModelRenderer>();

			ZeroOne = new ModelRenderer(ZeroOneData, level.ZeroOne.Nodes, level.ZeroOne.Animations);
			ZeroOne.SetAnimation(0);

			int splashIndex = defs.FindIndex(d => d.ID == 1003);
			if (splashIndex >= 0)
			{
				ActorDef splashDef = (ActorDef)defs[splashIndex];
				for (int i = 0; i < 5; i++)
				{
					var splash = new Splash(data[splashIndex], splashDef.Nodes, splashDef.StateGraph.Animations, SplashType.Water, splashDef.Scale);
					Splashes.Add(splash);
				}
			}

			FishTable = level.FishTable;
			for (int i = 0; i < level.FishTable.Count; i++)
			{
				if (level.FishTable[i].ID == 0)
					continue;

				int fishIndex = defs.FindIndex(d => d.ID == level.FishTable[i].ID);
				Debug.Assert(fishIndex >= 0);

				ObjectSpawn fakeSpawn = new ObjectSpawn
				{
					ID = level.FishTable[i].ID,
					Behaviour = 0,
					Position = Vector3.zero,
					Euler = Vector3.zero,
					Scale = Vector3.one
				};

				var fish = SnapUtils.CreateActor(data[fishIndex], fakeSpawn, (ActorDef)defs[fishIndex], this);
				fish.Visible = false;

				TempActors.Add(fish);
				output.Add(fish);
			}

			HashSet<long> tempIDs = new HashSet<long>();

			// other temp objects
			foreach (var def in level.ObjectInfo)
			{
				if (!SnapUtils.IsActor(def))
					continue;

				var actorDef = (ActorDef)def;	

				foreach (var state in actorDef.StateGraph.States)
				{
					foreach (var block in state.Blocks)
					{
						if (block.Spawn != null)
							tempIDs.Add(block.Spawn.ID);
					}
				}
			}

			if (Id == "18")
			{
				tempIDs.Add(58);
				tempIDs.Add(59);
			}

			foreach (long id in tempIDs)
			{
				long count = 1; // assume just one by default
				switch (id)
				{
					case 58: count = 3; break; // growlithe
					case 59: count = 3; break; // arcanine
					case 132: count = 4; break; // ditto
					case 1030: count = 10; break; // lava splash
					case 80: // slowbro, and related objects
					case 603:
					case 1002:
						count = 2; break;
					case 89: // all grimers can evolve
						count = 4; break;
				}

				int tempIndex = defs.FindIndex(d => d.ID == id);
                Debug.Assert(tempIndex >= 0);

				for (int i = 0; i < count; i++)
				{
					ObjectSpawn fakeSpawn = new ObjectSpawn
					{
						ID = id,
						Behaviour = 0,
						Position = Vector3.zero,
						Euler = Vector3.zero,
						Scale = Vector3.one
					};

					var actor = SnapUtils.CreateActor(data[tempIndex], fakeSpawn, (ActorDef)defs[tempIndex], this);
					actor.Visible = false;

					TempActors.Add(actor);
					AllActors.Add(actor);
					output.Add(actor);
				}
			}

			return output.ToArray();
		}

		public void PrepareToRender(GfxDevice device, GfxRenderInstManager renderInstManager, ViewerRenderInput viewerInput, bool _flush = false)
		{
			if (_flush)
			{
				renderInstManager.SetCurrentList(RenderInstListMain);
			}

			Particles.PrepareToRender(device, renderInstManager, viewerInput);

			ZeroOne.PrepareToRender(device, renderInstManager, viewerInput, this);

			for (int i = 0; i < Projectiles.Count; i++)
				Projectiles[i].PrepareToRender(device, renderInstManager, viewerInput, this);

			for (int i = 0; i < Splashes.Count; i++)
				Splashes[i].PrepareToRender(device, renderInstManager, viewerInput, this);
		}

		public void Destroy(GfxDevice device)
		{
			ZeroOneData.Destroy(device);
			for (int i = 0; i < ProjData.Count; i++)
				ProjData[i].Destroy(device);
		}

    }
}
