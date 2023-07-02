using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public static class GraphCreator
{
    public static void SaveToFile(Dictionary<int, int> dict)
    {
        string json = JsonUtility.ToJson(new Serialization<int, int>(dict));
        File.WriteAllText("Assets/graph.json", json);
    }
}


// Helper class for serialization
[System.Serializable]
public class Serialization<T1, T2>
{
    [SerializeField]
    List<T1> keys;
    [SerializeField]
    List<T2> values;

    public Serialization(Dictionary<T1, T2> dict)
    {
        keys = new List<T1>(dict.Keys);
        values = new List<T2>(dict.Values);
    }
}