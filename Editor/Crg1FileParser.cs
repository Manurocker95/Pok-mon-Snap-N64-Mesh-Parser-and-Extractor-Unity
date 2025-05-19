#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using VirtualPhenix.Nintendo64;

public static class Crg1FileParser
{
    [System.Serializable]
    public class User
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public bool IsMale { get; set; }
        public uint Age { get; set; }

    }
    public static byte[] SerializeToBytes(object obj)
    {
        using var ms = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, obj);
        return ms.ToArray();
    }
    [MenuItem("Tools/Create test CRG1 File")]
    public static void CreateTestCrg1File()
    {
        User u = new User()
        {
            Name = "Username",
            Username = "My user",
            IsMale = true,
            Age = 25
        };

        var buffer = VP_BYML.Write(new VP_ArrayBufferSlice(SerializeToBytes(u)), FileType.CRG1, "", false);
        System.IO.File.WriteAllBytes(Application.dataPath + "/CustomCRG1/user.crg1", buffer.Buffer);
    }

    [MenuItem("Tools/Read test CRG1 File")]
    public static void ReadTestCrg1File()
    {
        var path = Application.dataPath + "/CustomCRG1/user.crg1";
        if (System.IO.File.Exists(path))
        {
            var o = VP_BYML.Parse(System.IO.File.ReadAllBytes(path), FileType.CRG1);
            Debug.Log(o.GetType()); 
        }
    }

    [MenuItem("Tools/Parse CRG1 File")]
    public static void ParseCrg1File()
    {
        string path = EditorUtility.OpenFilePanel("Select .crg1 File", Application.streamingAssetsPath, "crg1");

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("No file selected.");
            return;
        }

        var o = VP_BYML.Parse(System.IO.File.ReadAllBytes(path), FileType.CRG1);
        Debug.Log(o.GetType());
    }

    public static Dictionary<string, int> ExtractFields(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        int fileSize = data.Length;

        if (fileSize < 0x18)
            throw new Exception("File too short to contain extended CRG1 header.");

        string magic = System.Text.Encoding.ASCII.GetString(data, 0, 4);
        if (magic != "CRG1")
            throw new Exception($"Unexpected magic: {magic}");

        int strKeyTableOffset = ReadUInt32BE(data, 0x04);
        int rootNodeOffset = ReadUInt32BE(data, 0x14); // hasPathTable = true → root at 0x14

        List<string> strKeyTable = ReadStringTable(data, strKeyTableOffset);

        if (rootNodeOffset >= data.Length)
            throw new Exception("Root node offset is outside file bounds.");

        byte nodeType = data[rootNodeOffset];
        if (nodeType != 0xA0)
            throw new Exception($"Root node is not a dictionary (0x{nodeType:X2})");

        int numEntries = ReadUInt24BE(data, rootNodeOffset + 1);
        int entryBase = rootNodeOffset + 4;

        if (entryBase + numEntries * 8 > data.Length)
            throw new Exception("Dictionary entries exceed file bounds.");

        var result = new Dictionary<string, int>();

        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = entryBase + i * 8;
            int keyIndex = ReadUInt24BE(data, entryOffset);
            string key = keyIndex < strKeyTable.Count ? strKeyTable[keyIndex] : $"__invalid_{keyIndex}";
            byte entryType = data[entryOffset + 3];
            int valueOffset = ReadUInt32BE(data, entryOffset + 4);

            if (entryType == 0xD3 && valueOffset + 4 <= data.Length) // Int
            {
                int value = ReadInt32BE(data, valueOffset);
                if (key == "Name" || key == "StartAddress" || key == "CodeStartAddress" || key == "PhotoStartAddress")
                {
                    result[key] = value;
                }
            }
        }

        // Calcula tamaños si es posible
        if (result.TryGetValue("CodeStartAddress", out int codeStart) &&
            result.TryGetValue("PhotoStartAddress", out int photoStart))
        {
            result["CodeSize"] = photoStart - codeStart;
        }

        if (result.TryGetValue("PhotoStartAddress", out int photoStartAddr))
        {
            result["PhotoSize"] = fileSize - photoStartAddr;
        }

        return result;
    }

    private static List<string> ReadStringTable(byte[] data, int offset)
    {
        var result = new List<string>();

        if (offset >= data.Length)
            throw new Exception("StringTable offset out of bounds.");

        byte type = data[offset];
        if (type != 0xC1)
            throw new Exception($"StringTable node has invalid type: 0x{type:X2}");

        int count = ReadUInt24BE(data, offset + 1);
        int baseOffset = offset + 4;

        for (int i = 0; i < count; i++)
        {
            int relative = ReadUInt32BE(data, baseOffset + i * 4);
            int strOffset = offset + relative;
            if (strOffset >= data.Length) break;
            result.Add(ReadNullTerminatedString(data, strOffset));
        }

        return result;
    }

    private static string ReadNullTerminatedString(byte[] data, int start)
    {
        int end = start;
        while (end < data.Length && data[end] != 0)
            end++;

        return System.Text.Encoding.UTF8.GetString(data, start, end - start);
    }

    private static int ReadUInt24BE(byte[] data, int offset)
    {
        return (data[offset] << 16) | (data[offset + 1] << 8) | data[offset + 2];
    }

    private static int ReadUInt32BE(byte[] data, int offset)
    {
        return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }

    private static int ReadInt32BE(byte[] data, int offset)
    {
        return unchecked((int)(uint)ReadUInt32BE(data, offset));
    }
}
#endif
