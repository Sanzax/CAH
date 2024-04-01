using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(TextMeshProUGUI))]
public class ColorPaletteTextColor : MonoBehaviour
{
    TextMeshProUGUI textMesh;

    [SerializeField]
    ColorName color;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        ColorPaletteManager.Instance.AddColorPaletteTextColor(this);
        ChangeImageColor();
    }

    public void ChangeImageColor()
    {
        textMesh.color = ColorPaletteManager.Instance.GetCurrentColorPalette().colors[(int)color].color;
    }


}
