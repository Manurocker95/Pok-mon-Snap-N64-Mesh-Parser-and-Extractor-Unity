using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using VirtualPhenix.Nintendo64;

public class SnapExtractor : MonoBehaviour
{

    public string crg1Path = "10_arc.crg1"; // En Assets/StreamingAssets
    public string romPath = "rom.z64"; // En Assets/StreamingAssets
    public bool m_debugging;

    public bool m_isPokemon;
    private byte[] romData;
    public string CRGFile = "10";
    public string CRG1Path => crg1Path;

    private string OutputDir => Path.Combine(Application.dataPath, "../Extracted");

    void Start()
    {
        string romFullPath = Path.Combine(Application.dataPath+"/CRG1/", crg1Path);



        Debug.Log("=============================================");
        romData = File.ReadAllBytes(romFullPath);
        CRGLevelArchive LevelArchive = (CRGLevelArchive)VP_BYML.Parse<CRGLevelArchive>(romData, FileType.CRG1);
        Debug.Log("=============================================");

        if (m_debugging)
            LevelArchive.Log();

        Debug.Log("=============================================");

        // StartCoroutine(Extract());
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Pokemon Snap/Test CRG1")]
#endif
    static void TestCRG1Debug()
    {
        var snapExtractor = FindObjectOfType<SnapExtractor>();
        var crg1Path = snapExtractor != null ? snapExtractor.CRG1Path : "10_arc.crg1";

        string romFullPath = Path.Combine(Application.dataPath + "/CRG1/", crg1Path);
        LogCRG1Level(romFullPath, snapExtractor);
    }

    static void LogCRG1Level(string romFullPath, SnapExtractor snapExtractor)
    {
        Debug.Log("=============================================");
        var romData = File.ReadAllBytes(romFullPath);
        if (snapExtractor == null || !snapExtractor.m_isPokemon)
        {
            var level = (CRGLevelArchive)VP_BYML.Parse<CRGLevelArchive>(romData, FileType.CRG1);
            level.Log();
        }
        else
        {
            var pk = (CRGPokemonArchive)VP_BYML.Parse<CRGPokemonArchive>(romData, FileType.CRG1);
            pk.Log();
        }
        Debug.Log("=============================================");
    }

  
}
