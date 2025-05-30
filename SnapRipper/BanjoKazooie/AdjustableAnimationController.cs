using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.BanjoKazooie
{
    public class AdjustableAnimationController
    {
        private float time = 0f;
        private bool initialized = false;
        private float phaseFrames = 0f;
        private float fps;

        public AdjustableAnimationController(float fps = 30f)
        {
            this.fps = fps;
        }
        public void SetTimeFromViewerInput(ViewerRenderInput viewerInput)
        {
            time = viewerInput != null ? viewerInput.Time / 1000f : Time.time;
            if (!initialized)
            {
                initialized = true;
            
                phaseFrames -= time * fps;
            }
        }

        public void SetTimeFromUnityTime(float globalTime)
        {
            time = globalTime;
            if (!initialized)
            {
                initialized = true;
                phaseFrames -= time * fps;
            }
        }

        public void Init(float newFPS, float newPhase = 0f)
        {
            initialized = false;
            fps = newFPS;
            phaseFrames = newPhase;
        }

        public void Adjust(float newFPS, float? newPhase = null)
        {
            if (newPhase.HasValue)
            {
                phaseFrames = newPhase.Value - time * newFPS;
            }
            else
            {
                phaseFrames += (fps - newFPS) * time;
            }
            fps = newFPS;
        }

        public float GetTimeInFrames()
        {
            if (!initialized)
                return phaseFrames;
            return time * fps + phaseFrames;
        }

        public float GetTimeInSeconds()
        {
            if (fps == 0f)
                return 0f;
            if (initialized)
                return time + phaseFrames / fps;
            return phaseFrames / fps;
        }

        public void ResetPhase()
        {
            initialized = false;
            phaseFrames = 0f;
        }
    }

}
