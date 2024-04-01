using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BlackCard : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] TextMeshProUGUI voteText;
    GameObjectActivator gameObjectActivator;

    int votes = 0;

    bool isSelected;

    void Awake()
    {
        gameObjectActivator = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameObjectActivator>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Common.IsPlayerPropertyTrue("IsReady"))
            return;

        Select();
        gameObjectActivator.SetReadyButtonInteractability(true);
    }
    public void Select()
    {
        isSelected = true;
        ChangeToSelectedColor();
        UnSelectOthers();
    }

    public void ChangeToSelectedColor()
    {
        image.color = ColorPaletteManager.Instance.currentColorPalette.colors[2].color;
        textComponent.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        voteText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
    }

    public void ChangeToNormalColor()
    {
        image.color = ColorPaletteManager.Instance.currentColorPalette.colors[0].color;
        textComponent.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        voteText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
    }

    void UnSelectOthers()
    {
        foreach(Transform blackCard in transform.parent)
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            if (blackCardScript == this)
                continue;
            blackCardScript.UnSelect();
        }
    }

    public void UnSelect()
    {
        isSelected = false;
        ChangeToNormalColor();
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void IncreaseVote()
    {
        votes++;
        SetVotes(votes);
    }

    public void SetVotes(int vote)
    {
        votes = vote;
        voteText.text = "Votes: " + votes;
    }

    public int GetVotes()
    {
        return votes;
    }

    public string GetText()
    {
        return textComponent.text;
    }

}
