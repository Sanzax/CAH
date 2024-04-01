using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public enum ColorName
{
    VeryDark,
    Dark,
    Light,
    VeryLight
}

[RequireComponent(typeof(Image))]
public class ColorPaletteImageColor : MonoBehaviour
{
    Image image;

    [SerializeField]
    ColorName colorName;

    [Range(0, 255)]
    [SerializeField] int alpha = 255;

    public Color Color { get { return ColorPaletteManager.Instance.GetCurrentColorPalette().colors[(int)colorName].color; } private set { } }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        ColorPaletteManager.Instance.AddColorPaletteImageColor(this);
        ChangeImageColor();
    }

    public void ChangeImageColor()
    {
        if(alpha != 255f)
        {
            image.color = new Color(Color.r, Color.g, Color.b, alpha / 255f);
            return;
        }
        image.color = Color;
    }


}
