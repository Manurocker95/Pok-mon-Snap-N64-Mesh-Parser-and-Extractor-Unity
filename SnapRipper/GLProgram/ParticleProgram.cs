using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualPhenix.Nintendo64
{
    public class ParticleProgram : DeviceProgram
    {

        public static long A_Position = 0;

        public static long UbSceneParams = 0;
        public static long UbDrawParams = 1;

        public ParticleProgram()
        {
            Name = "Snap_Particles";
        }
    }

}
