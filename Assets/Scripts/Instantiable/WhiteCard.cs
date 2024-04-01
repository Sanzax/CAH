using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class WhiteCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform rt;
    [SerializeField] Image image;
    [SerializeField] ColorPaletteImageColor colorPaletteImageColor;

    AnswerManager answerManager;

    CardManager cardManager;
    GameObjectActivator gameObjectActivator;

    [SerializeField] TMP_InputField inputField;

    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] TextMeshProUGUI inputText;
    [SerializeField] TextMeshProUGUI inputPlaceholderText;

    [SerializeField] RectTransform disabledImage;

    bool isInSlot;
    bool isUsedInsideAnswer;

    void Start()
    {
        GameObject manager = GameObject.FindGameObjectWithTag("GameManager");
        answerManager = manager.GetComponent<AnswerManager>();
        cardManager = manager.GetComponent<CardManager>();
        gameObjectActivator = manager.GetComponent<GameObjectActivator>();

        inputText.enableWordWrapping = true;
    }

    public void SetText(string text)
    {
        textComponent.text = text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int clickCount = eventData.clickCount;

        if (clickCount != 1)
            return;

        if (isUsedInsideAnswer)
        {
            InsideAnswer();
        }
        else
        {
            InsideHandOrSlot();
        }
    }

    void InsideHandOrSlot()
    {
        if (Common.IsPlayerPropertyTrue("IsReady") || Common.IsPlayerPropertyTrue("IsSelector") || Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            return;

        if (isInSlot)
            BackToHand();
        else
            GoToEmptySlot();
    }

    void InsideAnswer()
    {
        bool canBeSelected = ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsSelector"] || 
            (string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Vote") && !disabledImage.gameObject.activeSelf &&
            !answerManager.IsVoted;

        if (!canBeSelected)
            return;

        UnselectAllAnswers();

        SelectCurrentAnswer();

        gameObjectActivator.SetReadyButtonInteractability(true);
    }

    void UnselectAllAnswers()
    {
        foreach (Transform answer in transform.parent.parent.parent)
        {
            answer.GetComponent<Answer>().UnSelect();
        }
    }

    void SelectCurrentAnswer()
    {
        transform.parent.parent.GetComponent<Answer>().Select();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUsedInsideAnswer)
            ChangeToSelectedColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isUsedInsideAnswer)
            ChangeToNormalColor();
    }

    public void ChangeToSelectedColor()
    {
        image.color = ColorPaletteManager.Instance.currentColorPalette.colors[2].color;
        textComponent.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        inputText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        inputPlaceholderText.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
    }

    public void ChangeToNormalColor()
    {
        image.color = ColorPaletteManager.Instance.currentColorPalette.colors[3].color;
        textComponent.color = ColorPaletteManager.Instance.currentColorPalette.colors[0].color;
        inputText.color = ColorPaletteManager.Instance.currentColorPalette.colors[0].color;
        inputPlaceholderText.color = ColorPaletteManager.Instance.currentColorPalette.colors[0].color;
    }

    public void BackToHand()
    {
        transform.SetParent(cardManager.GetHand());

        isInSlot = false;
        gameObjectActivator.SetReadyButtonInteractability(false);

        if (textComponent.text == "")
            ResetBlankCardSettings();
    }

    void ResetBlankCardSettings()
    {
        textComponent.text = "_";
        inputField.gameObject.SetActive(false);
        inputField.DeactivateInputField();
        inputField.text = "";
    }

    void GoToEmptySlot()
    {
        Transform slotGroup = Common.FindFirstActiveChildIn(gameObjectActivator.GetSlots());

        foreach(Transform slot in slotGroup)
        {
            if (slot.childCount != 0)
                continue;

            GoToSlot(slot);

            if (textComponent.text == "_")
            {
                SetUpBlankCardSettings();
            }

            if(IsSlotGroupFull(slotGroup))
            {
                gameObjectActivator.SetReadyButtonInteractability(true);
            }
            break; 
        }
    }

    bool IsSlotGroupFull(Transform slotGroup)
    {
        foreach (Transform slot in slotGroup)
        {
            if (slot.childCount == 0)
            {
                return false;
            }
        }
        return true;
    }

    void SetUpBlankCardSettings()
    {
        textComponent.text = "";
        inputField.gameObject.SetActive(true);
        inputField.Select();
        inputField.ActivateInputField();
    }

    void GoToSlot(Transform slot)
    {
        transform.SetParent(slot);
        rt.position = slot.position;
        isInSlot = true;
    }

    public bool IsBlankCard()
    {
        return inputField.gameObject.activeSelf;
    }

    public string GetText()
    {
        return IsBlankCard() ?  inputText.text : textComponent.text;
    }

    public bool GetIsUsedInsideAnswer()
    { 
        return isUsedInsideAnswer;
    }

    public void SetIsUsedInsideAnswer(bool b)
    {
        isUsedInsideAnswer = b;
    }

    public void SetDisabledImageActive(bool b)
    {
        disabledImage.gameObject.SetActive(b);
    }
}