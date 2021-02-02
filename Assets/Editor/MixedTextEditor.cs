using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(MixedText))]
public class MixedTextEditor : UnityEditor.UI.TextEditor 
{
    SerializedProperty myImage;
    SerializedProperty myImageAtlas;

    protected override void OnEnable()
    {
        base.OnEnable();
        myImage = serializedObject.FindProperty("myImage");
        myImageAtlas = serializedObject.FindProperty("myAtlasData");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(myImage);
        EditorGUILayout.PropertyField(myImageAtlas);
        serializedObject.ApplyModifiedProperties();
    }



    [MenuItem("GameObject/UI/MixedText")]
    static void CreatMixedText()
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("MixedText", typeof(MixedText));
                go.GetComponent<MixedText>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                Selection.activeGameObject = go;

                GameObject image = new GameObject("image", typeof(UIVertexImage));
                image.GetComponent<UIVertexImage>().raycastTarget = false;
                image.transform.SetParent(go.transform);
                image.transform.localScale = Vector3.one;
                image.transform.localRotation = Quaternion.identity;
                image.transform.localPosition = Vector3.zero;

                go.GetComponent<MixedText>().myImage = image.GetComponent<UIVertexImage>();
            }
        }
    }

    [MenuItem("CONTEXT/Text/Convert To MixedText")]
    static void TextToMixedText(MenuCommand menuCommand)
    {
        Debug.Log("CoverToMixedText");
        Text text = menuCommand.context as Text;

        var color = text.color;
        var raycastTarget = text.raycastTarget;
        var resizeTextMinSize = text.resizeTextMinSize;
        var resizeTextMaxSize = text.resizeTextMaxSize;
        var alignment = text.alignment;
        var alignByGeometry = text.alignByGeometry;
        var fontSize = text.fontSize;
        var horizontalOverflow = text.horizontalOverflow;
        var verticalOverflow = text.verticalOverflow;
        var fontStyle = text.fontStyle;
        var resizeTextForBestFit = text.resizeTextForBestFit;
        var lineSpacing = text.lineSpacing;
        var font = text.font;
        var txt = text.text;

        GameObject go = text.gameObject;

        GameObject copyGo = Object.Instantiate(go);

        Component[] components = go.GetComponents<Component>();
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] is Transform)
            {
                continue;
            }
            Object.DestroyImmediate(components[i]);
        }

        components = copyGo.GetComponents<Component>();
        foreach (var c in components)
        {
            if (c == null || c is Transform)
            {
                continue;
            }

            if (c.GetType() == typeof(Text))
            {
                MixedText mixedText = go.AddComponent<MixedText>();

                mixedText.color = color;
                mixedText.raycastTarget = raycastTarget;
                mixedText.resizeTextMinSize = resizeTextMinSize;
                mixedText.resizeTextMaxSize = resizeTextMaxSize;
                mixedText.alignment = alignment;
                mixedText.alignByGeometry = alignByGeometry;
                mixedText.fontSize = fontSize;
                mixedText.horizontalOverflow = horizontalOverflow;
                mixedText.verticalOverflow = verticalOverflow;
                mixedText.fontStyle = fontStyle;
                mixedText.resizeTextForBestFit = resizeTextForBestFit;
                mixedText.lineSpacing = lineSpacing;
                mixedText.font = font;
                mixedText.text = txt;
                mixedText.supportRichText = true;
            }
            else
            {
                Component component = go.AddComponent(c.GetType());
                EditorUtility.CopySerialized(c, component);
            }
        }

        Object.DestroyImmediate(copyGo);

        EditorUtility.SetDirty(go);

        GameObject image = new GameObject("image", typeof(UIVertexImage));
        image.GetComponent<UIVertexImage>().raycastTarget = false;
        image.transform.SetParent(go.transform);
        image.transform.localScale = Vector3.one;
        image.transform.localRotation = Quaternion.identity;
        image.transform.localPosition = Vector3.zero;

        go.GetComponent<MixedText>().myImage = image.GetComponent<UIVertexImage>();
    }
}
