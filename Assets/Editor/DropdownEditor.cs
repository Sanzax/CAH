using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorPaletteImageColor))]
public class DropdownEditor : Editor
{
    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ColorPaletteImageColorEvent script = (ColorPaletteImageColorEvent)target;

        GUIContent arrayLabel = new GUIContent("Color");

        string[] options = new string[4] { "Very Dark", "Dark", "Light", "Very lightt"};

        script.index = EditorGUILayout.Popup(arrayLabel, script.index, options);
    }

    string[] CreateOptionNames(ColorPaletteImageColorEvent colorPalette)
    {
        List<string> options = new List<string>();
        foreach (ColorObject colorObject in ColorPaletteManager.Instance.GetCurrentColorPalette().colors)
        {
            options.Add(colorObject.name);
        }
        return options.ToArray();
    }*/
}
