using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class AtlasMonitor
{
    public string path;
    public AtlasData data;
}

public class EditorConfig : ScriptableObject 
{
    private static EditorConfig _inst;
    public static EditorConfig inst
    {
        get
        {
            if(_inst == null)
            {
                _inst = AssetDatabase.LoadAssetAtPath<EditorConfig>("Assets/Editor/Config/EditorConfig.asset");
                if(_inst == null)
                {
                    _inst = CreateInstance<EditorConfig>();
                    AssetDatabase.CreateAsset(_inst, "Assets/Editor/Config/EditorConfig.asset");
                }
            }
            return _inst;
        }
    }

    public List<AtlasMonitor> atlasMonitors = new List<AtlasMonitor>();

}
