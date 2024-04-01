using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CardManager : MonoBehaviourPun
{
    [SerializeField] GameObject whiteCardPrefab;
    [SerializeField] GameObject blackCardPrefab;

    [SerializeField] Transform hand;
    [SerializeField] Transform slots;
    [SerializeField] TextMeshProUGUI blackCardText;

    List<string> whiteCards, blackCards;
    bool isCardsAqquired;

    public static readonly int MAX_AMOUNT_OF_CARDS = 10;

    LogManager logManager;

    AnswerManager answerManager;
    GameObjectActivator gameObjectActivator;
    BlackCardVote blackCardVote;
    GameSettings gameSettings;

    string url = "https://rentry.co/s4as98v4/raw";

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        answerManager = GetComponent<AnswerManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
        blackCardVote = GetComponent<BlackCardVote>();
        gameSettings = GetComponent<GameSettings>();
    }

    [PunRPC]
    public void DrawWhiteCard(string text)
    {
        GameObject card = Instantiate(whiteCardPrefab, hand);
        card.GetComponent<WhiteCard>().SetText(text);
    }

    [PunRPC]
    public int GetWhiteCardsAmount()
    {
        return hand.childCount;
    }

    [PunRPC]
    public void DeleteCardsOnSlots()
    {
        foreach (Transform slotGroup in slots)
        {
            foreach (Transform slot in slotGroup)
            {
                DeleteCard(slot);
            }
        }
    }

    void DeleteCard(Transform slot)
    {
        bool slotHasCardInIt = slot.childCount > 0;
        if (!slotHasCardInIt)
            return;

        GameObject card = slot.GetChild(0).gameObject;
        Destroy(card);
    }

    [PunRPC]
    public void ReturnCardsToHand()
    {
        Transform slotGroup = gameObjectActivator.GetActiveSlotGroup();

        if (slotGroup == null)
            return;

        foreach (Transform slot in slotGroup)
        {
            ReturnCardToHandFrom(slot);
        }

        slotGroup.gameObject.SetActive(false);
    }

    void ReturnCardToHandFrom(Transform slot)
    {
        bool slotHasCardInIt = slot.childCount > 0;
        if (!slotHasCardInIt)
            return;

        WhiteCard whiteCard = slot.GetChild(0).GetComponent<WhiteCard>();
        whiteCard.BackToHand();
    }

    [PunRPC]
    public void ClearHand()
    {
        foreach (Transform card in hand)
        {
            Destroy(card.gameObject);
        }
    }

    public Transform GetHand()
    {
        return hand;
    }

    [PunRPC]
    public void SwitchBlackCard(string text)
    {
        blackCardText.text = text;
    }

    public string DrawBlackCard()
    {
        List<string> blackCardsFromRoom = new List<string>(GetBlackCardsFromCurrentRoom());

        string blackCardText = ExpandUnderscoresInBlackCard(blackCardsFromRoom[0], 3);

        blackCardsFromRoom.RemoveAt(0);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCards", blackCardsFromRoom.ToArray() }
        });

        return blackCardText;
    }

    string ExpandUnderscoresInBlackCard(string currentBlackCard, int expandAmount)
    {
        string blackCardText = currentBlackCard;
        string[] splits = blackCardText.Split('_');

        blackCardText = "";
        string underscores = CombineUnderscores(expandAmount);

        foreach (string split in splits)
        {
            blackCardText += split + underscores;
        }

        return blackCardText.Substring(0, blackCardText.Length - expandAmount);
    }

    string CombineUnderscores(int expandAmount)
    {
        string underscores = "";
        for (int i = 0; i < expandAmount; i++)
        {
            underscores += "_";
        }
        return underscores;
    }

    public int HowManyUnderscores(string s)
    {
        int count = 0;
        foreach (char c in s)
        {
            if (c == '_') count++;
        }
        return count / 3;
    }

    public void DrawWhiteCardsForEachPlayer()
    {
        List<string> whiteCardsFromRoom = new List<string>(GetWhiteCardsFromCurrentRoom());
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            DrawWhiteCardsForPlayer(player, whiteCardsFromRoom);
        }

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "WhiteCards", whiteCardsFromRoom.ToArray() }
        });
    }

    public void DrawWhiteCardsForPlayer(Player player, List<string> whiteCardsFromRoom)
    {
        int currentAmountOfCards = player.CustomProperties["CardAmount"] != null ? (int)player.CustomProperties["CardAmount"] : 0;
        int howManyCardsPlayerNeeds = MAX_AMOUNT_OF_CARDS - currentAmountOfCards;

        for (int i = 0; i < howManyCardsPlayerNeeds; i++)
        {
            DrawWhiteCardForPlayer(player, whiteCardsFromRoom);
        }
        Common.SetCustomPropertiesOfPlayer(player, new ExitGames.Client.Photon.Hashtable
        {
            { "CardAmount", MAX_AMOUNT_OF_CARDS }
        });
    }

    void DrawWhiteCardForPlayer(Player player, List<string> whiteCardsFromRoom)
    {
        string whiteCardText = whiteCardsFromRoom[0];
        photonView.RPC("DrawWhiteCard", player, whiteCardText);
        whiteCardsFromRoom.RemoveAt(0);
    }

    public void InitCards()
    {
        int blankCardAmount = CalculateBlankCardAmount();
        logManager.NewLogForAll("Added " + blankCardAmount + " blank cards.");
        string[] wCards = ShuffledWhiteCards(whiteCards, blankCardAmount);
        string[] bCards = ShuffledBlackCards(blackCards);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "WhiteCards", wCards },
            { "BlackCards", bCards }
        });
    }

    int CalculateBlankCardAmount()
    {
        string blankCradDropDownText = gameSettings.GetBlankCardsDropDown().options[gameSettings.GetBlankCardsDropDown().value].text;
        string blankCradDropDownTextWithoutPercentageSign = blankCradDropDownText.Split(' ')[0];
        float blankCardAmountPercentage = Int32.Parse(blankCradDropDownTextWithoutPercentageSign) / 100f;
        return Mathf.RoundToInt(whiteCards.Count * blankCardAmountPercentage);
    }

    public string[] ShuffledWhiteCards(List<string> wCards, int blankCardAmount)
    {
        List<string> cards = new List<string>(wCards);

        AddBlankCards(cards, blankCardAmount);

        Common.Shuffle(cards);
        return cards.ToArray();
    }

    void AddBlankCards(List<string> cards, int blankCardAmount)
    {
        for (int i = 0; i < blankCardAmount; i++)
        {
            cards.Add("_");
        }
    }

    public string[] ShuffledBlackCards(List<string> bCards)
    {
        List<string> cards = new List<string>(bCards);
        Common.Shuffle(cards);

        return cards.ToArray();
    }

    IEnumerator GetCardsFromWebsite(string url)
    {
        UnityWebRequest www = null;
        while (!isCardsAqquired)
        {
            www = UnityWebRequest.Get(url);
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            bool isErrorFoundInWebRequest = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
            if (isErrorFoundInWebRequest)
            {
                logManager.NewLog("Cards could not be found: " + www.error);
                logManager.NewLog("Retrying...");
                yield return new WaitForSeconds(3);
                continue;
            }
            isCardsAqquired = true;
        }

        whiteCards = GetCardsFromText(www.downloadHandler.text, "--White--", "--/White--");
        blackCards = GetCardsFromText(www.downloadHandler.text, "--Black--", "--/Black--");
        logManager.NewLog("Cards found. " + whiteCards.Count + " white cards and " + blackCards.Count + " black cards.");
    }

    List<string> GetCardsFromText(string text, string startLine, string endLine)
    {
        List<string> cards = new List<string>();

        string[] split = text.Split('\n');

        bool start = false;

        foreach (string line in split)
        {
            if (line.Contains(startLine))
            {
                start = true;
                continue;
            }
            if (line.Contains(endLine))
                break;

            if (!start)
                continue;

            cards.Add(line.Substring(0, line.Length - 1));
        }
        return cards;
    }

    public void TryGettingCardsFromWebsite()
    {
        StartCoroutine(GetCardsFromWebsite(url));
    }

    public bool CardsAqquired()
    {
        return isCardsAqquired;
    }

    string[] GetBlackCardsFromCurrentRoom()
    {
        return (string[])PhotonNetwork.CurrentRoom.CustomProperties["BlackCards"];
    }

    public string[] GetWhiteCardsFromCurrentRoom()
    {
        return (string[])PhotonNetwork.CurrentRoom.CustomProperties["WhiteCards"];
    }

    string[] GetCardTexts()
    {
        List<string> cards = new List<string>();

        foreach (Transform whiteCard in hand)
        {
            WhiteCard whiteCardScript = whiteCard.GetComponent<WhiteCard>();
            cards.Add(whiteCardScript.GetText());
        }
        return cards.ToArray();
    }

    [PunRPC]
    public void SavePlayerCards()
    {
        string[] cards = GetCardTexts();

        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
        {
            { "Cards", cards }
        });
    }

    public void WhenJoiningAgainCreateOldCardsBack()
    {
        string[] cards = (string[])PhotonNetwork.LocalPlayer.CustomProperties["Cards"];
        foreach (string card in cards)
        {
            DrawWhiteCard(card);
        }
    }

    [PunRPC]
    public void CreateBlackCardsForVote(string[] blackCardTexts)
    {
        foreach (string blackCardText in blackCardTexts)
        {
            GameObject blackCard = Instantiate(blackCardPrefab, blackCardVote.GetBlackCardVoteView());
            blackCard.transform.GetComponentInChildren<TextMeshProUGUI>().text = blackCardText;
        }
    }

    public string[] GetBlackCardTextsForVote()
    {
        int blackCardAmount = (int)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardVoteAmount"];
        string[] blackCardTexts = new string[blackCardAmount];

        List<string> blackCardsFromRoom = new List<string>(GetBlackCardsFromCurrentRoom());

        for (int i = 0; i < blackCardAmount; i++)
        {
            string blackCardText = ExpandUnderscoresInBlackCard(blackCardsFromRoom[i], 3);
            blackCardTexts[i] = blackCardText;
        }

        blackCardsFromRoom.RemoveRange(0, blackCardAmount);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCards", blackCardsFromRoom.ToArray() },
            { "BlackCardsOnTable", blackCardTexts }
        });

        return blackCardTexts;
    }

    public BlackCard SelectBlackCardByRandomAccordingToVotes()
    {
        List<int> blackCardVotes = new List<int>();
        foreach (Transform blackCard in blackCardVote.GetBlackCardVoteView())
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            blackCardVotes.Add(blackCardScript.GetVotes());
        }

        int blackCardIndex = WeightedRandom.GetRandomIndex(blackCardVotes);
        BlackCard finalBlackCard = blackCardVote.GetBlackCardVoteView().GetChild(blackCardIndex).GetComponent<BlackCard>();
        return finalBlackCard;
    }

    public void UpdateBlackCardVotes(int[] votes)
    {
        foreach (Transform blackCard in blackCardVote.GetBlackCardVoteView())
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            blackCardScript.SetVotes(votes[blackCard.transform.GetSiblingIndex()]);
        }
    }

    public void UpdateAnswerVotes(int[] votes)
    {
        foreach (Transform answer in answerManager.Answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            answerScript.SetVotes(votes[answer.transform.GetSiblingIndex()]);
        }
    }

    public void SetBlackCardVotes()
    {
        int blackCardAmount = (int)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardVoteAmount"];
        int[] blackCardVotes = new int[blackCardAmount];
        if(blackCardVote.GetBlackCardVoteView().childCount == 0)
        {
            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
            {
                {"BlackCardVotes", new int[blackCardAmount] }
            });
            return;
        }

        foreach(Transform blackCard in blackCardVote.GetBlackCardVoteView())
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            blackCardVotes[blackCard.GetSiblingIndex()] = blackCardScript.GetVotes();
        }

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            {"BlackCardVotes", blackCardVotes }
        });
    }

    [PunRPC]
    void SelectBlackCardWhichWon(int blackCardIndex)
    {
        blackCardVote.GetBlackCardVoteView().GetChild(blackCardIndex).GetComponent<BlackCard>().Select();
    }

    public void UnSelectAllBlackCards()
    {
        foreach (Transform blackCard in blackCardVote.GetBlackCardVoteView())
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            blackCardScript.UnSelect();
        }
    }

    [PunRPC]
    public void DestroyBlackCards()
    {
        foreach (Transform blackCard in blackCardVote.GetBlackCardVoteView())
        {
            Destroy(blackCard.gameObject);
        }
    }

}
