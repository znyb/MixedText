using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ImageData
{
    public string name;
    public char character;
    public Vector2 size;
    public Rect uv;
}

[CreateAssetMenu(menuName = "AtlasData")]
public class AtlasData : ScriptableObject ,ISerializationCallbackReceiver
{
    public Texture2D myAtlas;
    public int myAtlasWidth;
    public int myAtlasHeight;
    public List<ImageData> myImageDatas = new List<ImageData>();
    Dictionary<string, ImageData> myDataDic;

    public void OnAfterDeserialize()
    {
        myDataDic = new Dictionary<string, ImageData>();
        foreach(var data in myImageDatas)
        {
            if(myDataDic.ContainsKey(data.name))
            {
                Debug.LogError("name repeated :" + data.name);
            }
            else
            {
                myDataDic.Add(data.name, data);
            }
        }
    }

    public void OnBeforeSerialize()
    {
    }

    public ImageData GetImageData(string imageName)
    {
        if(string.IsNullOrEmpty(imageName))
        {
            return null;
        }
        ImageData data = null;
        myDataDic.TryGetValue(imageName, out data);
        return data;
    }
}
