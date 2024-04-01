using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameColorHandler : MonoBehaviour
{
    [SerializeField] Slider rSlider;
    [SerializeField] Slider gSlider;
    [SerializeField] Slider bSlider;

    [SerializeField] TextMeshProUGUI nameText;

    void Update()
    {
        UpdateNameColor();
    }

    void UpdateNameColor()
    {
        nameText.color = new Color(rSlider.value, gSlider.value, bSlider.value);
    }

    public Color GetColor()
    {
        return new Color(rSlider.value, gSlider.value, bSlider.value);
    }
}
