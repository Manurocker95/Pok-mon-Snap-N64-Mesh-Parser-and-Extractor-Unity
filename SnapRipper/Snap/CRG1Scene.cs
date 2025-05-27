using System.Collections.Generic;
using UnityEngine;

public class CRG1Scene
{
    [SerializeField] private string m_id;
    [SerializeField] private string m_name;
    [SerializeField] private string m_path;

    public CRG1Scene(string id, string name, string path)
    {
        m_id = id;
        m_name = name;
        m_path = path;
    }

    /*public List<VP_ArrayBufferSlice> LoadArrayBuffers(DataFetcher dataFetcher)
    {
        var fileList = new List<string> { m_id, "0E", "magikarp" };

        switch (m_id)
        {
            case "10": // beach
                fileList.Add("pikachu");
                break;
            case "12": // tunnel
                fileList.AddRange(new[] { "pikachu", "zubat" });
                break;
            case "16": // river
                fileList.AddRange(new[] { "pikachu", "bulbasaur" });
                break;
            case "14": // cave
                fileList.AddRange(new[] { "pikachu", "bulbasaur", "zubat" });
                break;
        }

        var result = new List<NamedArrayBufferSlice>();
        foreach (var name in fileList)
        {
            var slice = dataFetcher.FetchData(m_path);
            if (slice != null)
                result.Add(slice);
        }

        return result;
    }*/
}
