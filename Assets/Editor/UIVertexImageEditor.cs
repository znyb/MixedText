using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIVertexImage))]
public class UIVertexImageEditor : GraphicEditor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
