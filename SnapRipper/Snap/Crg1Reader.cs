using System;
using System.IO;
using UnityEngine;

public class Crg1ReaderMono : MonoBehaviour
{
    public struct LevelArchive
    {
        public uint Name;
        public uint StartAddress;
        public uint CodeStartAddress;
        public uint PhotoStartAddress;

        public byte[] Data;
        public byte[] Code;
        public byte[] Photo;
    }

    private void Start()
    {
        string path = Application.dataPath + "/CRG1/10_arc.crg1";
        LevelArchive lvArchive = LoadFromCrg(path);

        Debug.Log("================================");
        Debug.Log("Name: " + lvArchive.Name);
        Debug.Log("StartAddress: " + lvArchive.StartAddress);
        Debug.Log("CodeStartAddress: " + lvArchive.CodeStartAddress);
        Debug.Log("PhotoStartAddress: " + lvArchive.PhotoStartAddress);
        Debug.Log("Data size: " + lvArchive.Data.Length);
        Debug.Log("Code size: " + lvArchive.Code.Length);
        Debug.Log("Photo size: " + lvArchive.Photo.Length);
        Debug.Log("================================");
    }

    public static LevelArchive LoadFromCrg(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        int dataOffset = 284;
        int dataSize = 610976;

        int codeOffset = 611264;
        int codeSize = 537792;

        int photoOffset = 1149060;
        int photoSize = 156976;


        uint name = ReadBE(fileData, 0x04);
        uint startAddr = ReadBE(fileData, 0xDC);
        uint codeAddr = ReadBE(fileData, 0xE4);
        uint photoAddr = ReadBE(fileData, 0xF4);

        return new LevelArchive
        {
            Name = name,
            StartAddress = startAddr,
            CodeStartAddress = codeAddr,
            PhotoStartAddress = photoAddr,
            Data = Slice(fileData, dataOffset, dataSize),
            Code = Slice(fileData, codeOffset, codeSize),
            Photo = Slice(fileData, photoOffset, photoSize)
        };
    }

    private static uint ReadBE(byte[] data, int offset)
    {
        return (uint)(
            (data[offset + 0] << 24) |
            (data[offset + 1] << 16) |
            (data[offset + 2] << 8) |
            (data[offset + 3]));
    }

    private static byte[] Slice(byte[] src, int offset, int length)
    {
        if (offset < 0 || offset + length > src.Length)
        {
            Debug.LogWarning($"Invalid slice: offset={offset}, length={length}, file size={src.Length}");
            return Array.Empty<byte>();
        }

        byte[] slice = new byte[length];
        Array.Copy(src, offset, slice, 0, length);
        return slice;
    }
}
