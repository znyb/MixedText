using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CharacterFontAdjust : EditorWindow
{
    AtlasData atlasData;
    Font font;

    [MenuItem("Window/CharacterFontAdjust")]
    static void Init()
    {
        GetWindow<CharacterFontAdjust>();
    }

    private void OnGUI()
    {
        atlasData = EditorGUILayout.ObjectField(atlasData, typeof(AtlasData),false) as AtlasData;
        font = EditorGUILayout.ObjectField(font, typeof(Font), false) as Font;

        if (atlasData != null && font != null)
        {
            if(GUILayout.Button("Set Font CharacterInfo"))
            {
                CharacterInfo[] characters = new CharacterInfo[atlasData.myImageDatas.Count];
                for (int i = 0; i < atlasData.myImageDatas.Count; i++)
                {
                    ImageData data = atlasData.myImageDatas[i];
                    var c = new CharacterInfo();
                    c.index = data.character;
                    c.advance = (int)data.size.x;
                    c.maxX = (int)data.size.x;
                    c.minY = (int)-data.size.y / 2;
                    c.maxY = (int)data.size.y / 2;
                    c.uvBottomLeft = data.uv.min;
                    c.uvTopRight = data.uv.max;
                    characters[i] = c;
                }
                font.characterInfo = characters;
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
            }

            int count = atlasData.myImageDatas.Count;
            EditorGUILayout.BeginVertical();
            for(int i = 0; i < count;i++)
            {
                EditorGUILayout.BeginHorizontal();
                char character = atlasData.myImageDatas[i].character;
                EditorGUILayout.LabelField(character.ToString(),GUILayout.Width(20));
                EditorGUILayout.Space();
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    SetCharacter(character,-1,-1);
                }
                EditorGUILayout.LabelField("left", GUILayout.Width(50));
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    SetCharacter(character, 1, -1);
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    SetCharacter(character, -1, 1);
                }
                EditorGUILayout.LabelField("right", GUILayout.Width(50));
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    SetCharacter(character, 1, 1);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }

    void SetCharacter(char ch,int add,int right)
    {
        CharacterInfo[] characters = new CharacterInfo[font.characterInfo.Length];
        int index = ch;
        float w = (float)add / atlasData.myAtlas.width;
        for (int i=0;i<font.characterInfo.Length;i++)
        {
            CharacterInfo character = font.characterInfo[i];
            if (character.index == index)
            {
                character.advance += add;
                character.maxX += add;
                if (right > 0)
                {
                    character.uvBottomRight += Vector2.right * w;
                }
                else
                {
                    character.uvBottomLeft += Vector2.left * w;
                }
            }
            characters[i] = character;
        }
        font.characterInfo = characters;
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
    }
}
