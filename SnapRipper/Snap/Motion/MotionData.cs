using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualPhenix.Nintendo64.PokemonSnap
{
    public class MotionData
    {
        public long StateFlags;

        public List<Motion> CurrMotion = new List<Motion>();
        public int CurrBlock;
        public double[] StoredValues = new double[6];

        public float PathParam;
        public float Start = -1;
        public float AuxStart = -1;
        public Vector3 StartPos = Vector3.zero;
        public float MovingYaw;
        public float YSpeed;
        public float ForwardSpeed;

        public Vector3 RefPosition = Vector3.zero;
        public Vector3 Destination = Vector3.zero;
        public Vector3 LastImpact = Vector3.zero;
        public bool IgnoreGround;
        public int GroundType = 0;
        public float GroundHeight = 0;

        public TrackPath Path;

        public MotionData()
        {
            Reset();
        }

        public void Reset()
        {
            StateFlags = 0;

            CurrMotion.Clear();
            CurrBlock = 0;

            PathParam = 0;
            Start = -1;
            AuxStart = -1;
            StartPos = Vector3.zero;
            MovingYaw = 0;
            YSpeed = 0;
            ForwardSpeed = 0;
            IgnoreGround = false;
            RefPosition = Vector3.zero;
            Destination = Vector3.zero;
            LastImpact = Vector3.zero;
        }
    }
}
