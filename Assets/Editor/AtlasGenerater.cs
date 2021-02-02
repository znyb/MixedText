using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;
using System.Linq;
using System.IO;

public class AtlasGenerater : AssetPostprocessor 
{
    static Queue<AtlasMonitor> dirtyAtlas = new Queue<AtlasMonitor>();

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            foreach (var monitor in EditorConfig.inst.atlasMonitors)
            {
                if (path.StartsWith(monitor.path))
                {
                    RefreshAtlas(monitor);
                }
            }
        }

        foreach(string path in deletedAssets)
        {
            foreach (var monitor in EditorConfig.inst.atlasMonitors)
            {
                if (path.StartsWith(monitor.path))
                {
                    RefreshAtlas(monitor);
                }
            }
        }
    }

    void OnPreprocessTexture()
    {
        foreach (var monitor in EditorConfig.inst.atlasMonitors)
        {
            string texturePath = AssetDatabase.GetAssetPath(monitor.data);
            texturePath = Path.ChangeExtension(texturePath, ".png");
            if(assetPath.ToLower() == texturePath.ToLower())
            {
                TextureImporter textureImporter = assetImporter as TextureImporter;
                TextureImporterSettings settings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(settings);
                settings.alphaIsTransparency = true;
                settings.mipmapEnabled = false;
                textureImporter.SetTextureSettings(settings);
                break;
            }
        }
    }

    static void RefreshAtlas(AtlasMonitor monitor)
    {
        if(!dirtyAtlas.Contains(monitor))
        {
            dirtyAtlas.Enqueue(monitor);
        }
        EditorApplication.update -= UpdateAtlas;
        EditorApplication.update += UpdateAtlas;
    }

    static void UpdateAtlas()
    {
        if (dirtyAtlas.Count == 0)
        {
            EditorApplication.update -= UpdateAtlas;
            return;
        }

        AtlasMonitor monitor = dirtyAtlas.Dequeue();
        if (monitor != null && monitor.data != null)
        {
            Texture2D[] textures = AssetDatabase.FindAssets("t:Texture2D", new string[] { monitor.path })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path =>
                {
                    Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    var t = new Texture2D(t2d.width, t2d.height,t2d.format,false);
                    Graphics.CopyTexture(t2d, t);
                    t.name = t2d.name;
                    return t;
                }).ToArray();

            Texture2D atlas = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            Rect[] rects = atlas.PackTextures(textures, 0);

            monitor.data.myImageDatas.Clear();
            for (int i = 0; i < textures.Length; i++)
            {
                ImageData data = new ImageData();
                data.name = textures[i].name;
                data.size = new Vector2(textures[i].width, textures[i].height);
                data.uv = rects[i];
                monitor.data.myImageDatas.Add(data);
            }
            monitor.data.OnAfterDeserialize();

            string atlasPath = AssetDatabase.GetAssetPath(monitor.data);
            atlasPath = Path.ChangeExtension(atlasPath, ".png");
            if(File.Exists(atlasPath))
            {
                File.Delete(atlasPath);
            }
            File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
            AssetDatabase.Refresh();
            monitor.data.myAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            monitor.data.myAtlasWidth = monitor.data.myAtlas.width;
            monitor.data.myAtlasHeight = monitor.data.myAtlas.height;

            EditorUtility.SetDirty(monitor.data);
            AssetDatabase.SaveAssets();

            Debug.Log("更新图集：" + monitor.data.name);
        }
    }
}
