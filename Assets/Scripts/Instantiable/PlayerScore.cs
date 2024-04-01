using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviourPun
{
    RectTransform rt;
    Transform scoreBoard;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI readyText;

    [SerializeField] string playerColor;
    [SerializeField] Color color;

    [PunRPC]
    void Init(float[] colorValues)
    {
        scoreBoard = GameObject.FindGameObjectWithTag("ScoreBoard").transform;
        transform.SetParent(scoreBoard);
        rt = GetComponent<RectTransform>();

        rt.localScale = new Vector3(1, 1, 1);
        color = new Color(colorValues[0], colorValues[1], colorValues[2]);
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = color;
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = photonView.Owner.NickName;

        SetName();

        if (photonView.Owner.IsMasterClient)
        {
            UpdateIsReady("Host");
        }
    }

    [PunRPC]
    public void SetName()
    {
        nameText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    public void UpdateScore(int newScore)
    {
        scoreText.text ="<b>" + newScore + "</b> Points!";
    }

    [PunRPC]
    public void UpdateIsReady(string text)
    {
        readyText.text = text;
    }

    [PunRPC]
    void HighLight()
    {
        rt.GetComponent<Image>().color = ColorPaletteManager.Instance.currentColorPalette.colors[2].color;
        scoreText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        readyText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        Invoke("BackToNormalColor", 3);
    }

    void BackToNormalColor()
    {
        rt.GetComponent<Image>().color = ColorPaletteManager.Instance.currentColorPalette.colors[0].color;
        scoreText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        readyText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
    }

    public Color GetColor()
    {
        return color;
    }
}
