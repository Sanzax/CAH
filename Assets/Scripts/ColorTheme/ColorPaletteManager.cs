using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPaletteManager : MonoBehaviour
{
    public static ColorPaletteManager Instance { get; private set; }

    [SerializeField] ColorPalette[] colorPalettes;

    public ColorPalette currentColorPalette;

    List<ColorPaletteImageColor> colorPaletteImageColors;
    List<ColorPaletteTextColor> colorPaletteTextColors;

    int index = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        colorPaletteImageColors = new List<ColorPaletteImageColor>();
        colorPaletteTextColors = new List<ColorPaletteTextColor>();
        currentColorPalette = colorPalettes[0];
    }

    public void SetCurrentColorPalette(int i)
    {
        index += i;

        if(index >= colorPalettes.Length)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = colorPalettes.Length-1;
        }

        currentColorPalette = colorPalettes[index];

        List<ColorPaletteImageColor> toBeRemovedImage = new List<ColorPaletteImageColor>();
        foreach (ColorPaletteImageColor c in colorPaletteImageColors)
        {
            if (c == null)
            {
                toBeRemovedImage.Add(c);
                continue;
            }
            c.ChangeImageColor();
        }

        foreach(ColorPaletteImageColor c in toBeRemovedImage)
        {
            colorPaletteImageColors.Remove(c);
        }

        List<ColorPaletteTextColor> toBeRemovedText = new List<ColorPaletteTextColor>();
        foreach (ColorPaletteTextColor c in colorPaletteTextColors)
        {
            if (c == null)
            {
                toBeRemovedText.Add(c);
                continue;
            }
            c.ChangeImageColor();
        }

        foreach(ColorPaletteTextColor c in toBeRemovedText)
        {
            colorPaletteTextColors.Remove(c);
        }
    }

    public ColorPalette GetCurrentColorPalette()
    {
        return currentColorPalette;
    }

    public void AddColorPaletteImageColor(ColorPaletteImageColor colorPaletteImageColor)
    {
        colorPaletteImageColors.Add(colorPaletteImageColor);
    }

    public void RemoveColorPaletteImageColor(ColorPaletteImageColor colorPaletteImageColor)
    {
        colorPaletteImageColors.Remove(colorPaletteImageColor);
    }

    public void AddColorPaletteTextColor(ColorPaletteTextColor colorPaletteTextColor)
    {
        colorPaletteTextColors.Add(colorPaletteTextColor);
    }
}
