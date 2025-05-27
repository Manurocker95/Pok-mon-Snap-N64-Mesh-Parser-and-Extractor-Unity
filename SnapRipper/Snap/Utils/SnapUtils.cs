using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public static class SnapUtils 
    {
        public const long FakeAuxFlag = 0x1000;
        public const long FakeAux = 0x123456;
        public static double YawTowards(Vector3 end, Vector3 start)
        {
            return System.Math.Atan2(end.x - start.x, end.z - start.z);
        }

        public static Vector3 SetValueInVector3(Vector3 v, int pos, float val)
        {
            switch (pos)
            {
                case 0:
                    return new Vector3(val, v.y, v.z);
                case 1:
                    return new Vector3(v.x, val, v.z);
                default:
                    return new Vector3(v.x, v.y, val);
            }
        }

        public static float LookupValue(MotionData data, ObjParam param)
        {
            if (param.Stored == null)
            {
                return param.Constant;
            }
            else
            {
                return (float)data.StoredValues[param.Stored.Index];
            }
        }

        public static bool GroundOkay(CollisionTree collision, double x, double z)
        {
            var ground = FindGroundPlane(collision, x, z);
            switch (ground.Type)
            {
                case 0x7F66:
                case 0xFF00:
                case 0x337FB2:
                case 0x4CCCCC:
                case 0x7F6633:
                case 0x7F667F:
                case 0xFF0000:
                case 0xFF4C19:
                case 0xFF7FB2:
                    return false;
            }
            return true;
        }

        // attempt to apply the given displacement, returning whether motion was blocked
        public static bool AttemptMove(ref Vector3 pos, Vector3 end, MotionData data, LevelGlobals globals, long flags)
        {
            if (!data.IgnoreGround && !GroundOkay(globals.Level.Collision, end.x, end.z))
                return true;

            Vector3 moveScratch = end - pos;
            moveScratch.Normalize(); // then multiplies by some scale factor?

            if (!data.IgnoreGround && !GroundOkay(globals.Level.Collision, pos.x + moveScratch.x, pos.z + moveScratch.z))
                return true;

            GroundPlane ground = FindGroundPlane(globals.Level.Collision, end.x, end.z);
            data.GroundType = (int)ground.Type;
            data.GroundHeight = (float)ComputePlaneHeight(ground, (double)end.x, (double)end.z);

            if ((flags & (long)MoveFlags.ConstHeight) != 0 && pos.y != data.GroundHeight)
                return true;

            pos.x = end.x;
            pos.z = end.z;

            if ((flags & (long)MoveFlags.Ground) != 0)
                pos.y = data.GroundHeight;

            return false;
        }

        public static bool CanHearSong(Vector3 pos, LevelGlobals globals)
        {
            if (globals.CurrentSong == 0)
                return false;

            return Vector3.Distance(pos, globals.Translation) < 2500;
        }

        public static bool StepYawTowards(Vector3 euler, double target, double maxTurn, double dt)
        {
            double dist = MathHelper.AngleDist(euler.y, target);
            euler.y += (float)MathHelper.ClampRange(dist, maxTurn * dt * 30);
            return System.Math.Abs(dist) < maxTurn * dt * 30;
        }

        public static GroundPlane nullPlane()
        {
            return new GroundPlane()
            {
                Normal = Vector3.zero,
                Type = -1,
                Offset = 0
            };
        } 
        
        public static GroundPlane defaultPlane()
        {
            return new GroundPlane()
            {
                Normal = BasisVectors.Vec3UnitY,
                Type = -1,
                Offset = 0
            };
        }
        public static bool IsActor(ObjectDef def)
        {
            return def is ActorDef;
        }

        public static Vector3 GetVec3(VP_DataView view, long offset)
        {
            float x = view.GetFloat32(offset + 0x00, false);
            float y = view.GetFloat32(offset + 0x04, false);
            float z = view.GetFloat32(offset + 0x08, false);
            return new Vector3(x, y, z);
        }
        public static double GroundHeightAt(LevelGlobals globals, Vector3 pos)
        {
            return FindGroundHeight(globals.Level.Collision, pos[0], pos[2]);
        }

        public static double ComputePlaneHeight(GroundPlane plane, double x, double z)
        {
            if (plane == null)
                return 0;
            if (plane.Normal[1] == 0)
                return 0;
            return -100 * (x * plane.Normal[0] / 100.0 + z * plane.Normal[2] / 100.0 + plane.Offset) / plane.Normal[1];
        }

        public static double FindGroundHeight(CollisionTree tree, double x, double z)
        {
            return ComputePlaneHeight(FindGroundPlane(tree, x, z), x, z);
        }

        public static GroundPlane FindGroundPlane(CollisionTree tree, double x, double z)
        {
            x /= 100;
            z /= 100;
            if (tree == null)
                return defaultPlane();
            while (true)
            {
                var test = x * tree.Line[0] + z * tree.Line[1] + tree.Line[2];
                if (test > 0)
                {
                    if (tree.PosPlane != null)
                        return tree.PosPlane;
                    if (tree.PosSubtree == null)
                        return nullPlane();
                    tree = tree.PosSubtree;
                }
                else
                {
                    if (tree.NegPlane != null)
                        return tree.NegPlane;
                    if (tree.NegSubtree == null)
                        return nullPlane();
                    tree = tree.NegSubtree;
                }
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

        public static void SceneActorInit()
        {
            Pikachu.CurrDiglett = 0;
            Pikachu.TargetDiglett = 1;
            Staryu.EvolveCount = 0;
            Staryu.SeparationScale = 1;
            Magnemite.MCenter = 0;
            Magnemite.Counter = 0;
        }

        public static Actor CreateActor(BanjoKazooie.RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals)
        {
            switch (def.ID)
            {
                case 1: return new Bulbasaur(renderData, spawn, def, globals);
                case 4: return new Charmander(renderData, spawn, def, globals);
                case 5: return new Charmeleon(renderData, spawn, def, globals);
                case 7: return new Squirtle(renderData, spawn, def, globals);
                case 14: return new Kakuna(renderData, spawn, def, globals);
                case 16: return new Pidgey(renderData, spawn, def, globals);
                case 25: return new Pikachu(renderData, spawn, def, globals);
                case 37: return new Vulpix(renderData, spawn, def, globals);
                case 54: return new Psyduck(renderData, spawn, def, globals);
                case 60: return new Poliwag(renderData, spawn, def, globals);
                case 70: return new Weepinbell(renderData, spawn, def, globals);
                case 71: return new Victreebel(renderData, spawn, def, globals);
                case 79: return new Slowpoke(renderData, spawn, def, globals);
                case 81: return new Magnemite(renderData, spawn, def, globals);
                case 88: return new Grimer(renderData, spawn, def, globals);
                case 93: return new Haunter(renderData, spawn, def, globals);
                case 120: return new Staryu(renderData, spawn, def, globals);
                case 124: return new Jynx(renderData, spawn, def, globals);
                case 129: return new Magikarp(renderData, spawn, def, globals);
                case 131: return new Lapras(renderData, spawn, def, globals);
                case 137: return new Porygon(renderData, spawn, def, globals);
                case 144: return new Articuno(renderData, spawn, def, globals);
                case 145: return new Zapdos(renderData, spawn, def, globals);
                case 601: return new ArticunoEgg(renderData, spawn, def, globals, true);
                case 602: return new ZapdosEgg(renderData, spawn, def, globals, true);
                case 1026: return new MiniCrater(renderData, spawn, def, globals);
                case 1027: return new Crater(renderData, spawn, def, globals);
            }

            return new Actor(renderData, spawn, def, globals);
        }

    }
}
