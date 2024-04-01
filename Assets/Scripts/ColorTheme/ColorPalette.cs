using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorObject
{
    public string name;
    public Color color;
}

[CreateAssetMenu(fileName = "Color Palette", menuName = "My Scriptable Objects/Color palette", order = 1)]
public class ColorPalette : ScriptableObject
{
    public ColorObject[] colors;
}
