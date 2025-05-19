using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualPhenix.Nintendo64;

[System.Serializable]
public class CRGPokemonArchive 
{
    public virtual VP_ArrayBufferSlice Data { get; set; }
    public virtual VP_ArrayBufferSlice Code { get; set; }
    public virtual VP_ArrayBufferSlice Photo { get; set; }

    public virtual uint StartAddress { get; set; }
    public virtual uint CodeStartAddress { get; set; }
    public virtual uint PhotoStartAddress { get; set; }


    public CRGPokemonArchive()
    {

    }

    public CRGPokemonArchive(VP_BYML.NodeDict dict)
    {
        FromNodeDict(dict);
    }

    public virtual void FromNodeDict(VP_BYML.NodeDict dict)
    {
        if (dict == null || dict.Count == 0) return;

        foreach (var kvp in dict)
        {
            Debug.Log(kvp.Key);
        }


        Data = (VP_ArrayBufferSlice)dict["Data"].Data;
        Code = (VP_ArrayBufferSlice)dict["Code"].Data;
        Photo = (VP_ArrayBufferSlice)dict["Photo"].Data;
        StartAddress = (uint)dict["StartAddress"].Data;
        CodeStartAddress = (uint)dict["CodeStartAddress"].Data;
        PhotoStartAddress = (uint)dict["PhotoStartAddress"].Data;
    }

    public virtual void Log()
    {
        Debug.Log("========== POKEMON ARCHIVE ==========");
        Debug.Log($"StartAddress: 0x{this.StartAddress:X8} ({this.StartAddress})");
        Debug.Log($"CodeStartAddress: 0x{this.CodeStartAddress:X8} ({this.CodeStartAddress})");
        Debug.Log($"PhotoStartAddress: 0x{this.PhotoStartAddress:X8} ({this.PhotoStartAddress})");
        Debug.Log($"Data: {this.Data}");
        Debug.Log($"Code: {this.Code}");
        Debug.Log($"Photo: {this.Photo}");
        Debug.Log("===================================");
    }

}
