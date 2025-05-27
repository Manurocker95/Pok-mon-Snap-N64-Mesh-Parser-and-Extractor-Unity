using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualPhenix.Nintendo64.BanjoKazooie;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class Magnemite : Actor
    {
        public static long MCenter = 0;
        public static long Counter = 0;

        private bool matchedAngle = false;
        private List<Actor> others = new List<Actor>();

        public Magnemite(RenderData renderData, ObjectSpawn spawn, ActorDef def, LevelGlobals globals, bool isEgg = false) : base(renderData, spawn, def, globals, isEgg)
        {
        }

        protected override MotionResult AuxStep(ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            switch (this.CurrAux)
            {
                case 0x802E39A0:
                    if (this.others.Count == 0)
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            if (i == this.Spawn.Behaviour)
                                continue;

                            this.others.Add(GfxPlatformUtils.AssertExists(globals.AllActors.FirstOrDefault(
                                a => a.Spawn.ID == this.Spawn.ID && a.Spawn.Behaviour == i
                            )));
                        }
                    }

                    if (Magnemite.MCenter == 0)
                    {
                        double aDist = Vector3.Distance(this.Translation, this.others[0].Translation);
                        double bDist = Vector3.Distance(this.Translation, this.others[1].Translation);

                        if (aDist < 300 || bDist < 300)
                        {
                            Magnemite.MCenter = this.Spawn.Behaviour;
                            return MotionResult.Done;
                        }
                    }
                    else
                    {
                        if (this.others[0].Spawn.Behaviour != Magnemite.MCenter)
                        {
                            var a = this.others[1];
                            this.others[1] = this.others[0];
                            this.others[0] = a;
                        }

                        if (Vector3.Distance(this.Translation, this.others[0].Translation) < 300 &&
                            this.ReceiveSignal(this, 0x2C, globals))
                        {
                            return MotionResult.Done;
                        }
                    }
                    break;

                case 0x802E480C:
                    this.others[0].ReceiveSignal(this, (long)InteractionType.PesterHit, globals);
                    return MotionResult.Done;

                case 0x802E4844:
                    this.others[0].ReceiveSignal(this, (long)InteractionType.AppleHit, globals);
                    return MotionResult.Done;
            }

            return MotionResult.None;
        }

        protected override MotionResult CustomMotion(long param, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            this.Translation = this.others[0].Renderers[1].ModelMatrix.GetColumn(3);

            if (this.matchedAngle)
            {
                this.Euler = new Vector3(this.Euler.x, this.others[0].Euler.y, this.Euler.z);
            }
            else
            {
                this.matchedAngle = SnapUtils.StepYawTowards(this.Euler, this.others[0].Euler.y, System.Math.PI / 90, viewerInput.DeltaTime / 1000.0);
            }

            return MotionResult.Update;
        }

        protected override MotionResult StateOverride(long addr, ViewerRenderInput viewerInput, LevelGlobals globals)
        {
            if (addr == 0x802E4434)
            {
                this.MotionData.Destination = this.others[0].Renderers[1].ModelMatrix.GetColumn(3);
            }
            else if (addr == 0x802E4668 && Magnemite.Counter >= 2)
            {
                globals.SendGlobalSignal(this, 0x2D);
            }

            return MotionResult.None;
        }

        protected override void StartBlock(LevelGlobals globals)
        {
            switch (this.Def.StateGraph.States[(int)this.CurrState].StartAddress)
            {
                case 0x802E4668:
                    if (this.CurrAnimation != this.others[0].CurrAnimation)
                        this.SetAnimation(this.others[0].CurrAnimation);
                    break;

                case 0x802E45B4:
                    Magnemite.Counter++;
                    break;
            }

            base.StartBlock(globals);
        }
    }

}
