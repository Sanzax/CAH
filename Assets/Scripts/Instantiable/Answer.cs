using UnityEngine;
using TMPro;
using Photon.Pun;

public class Answer : MonoBehaviour
{
    [SerializeField] GameObject whiteCardPrefab;
    [SerializeField] Transform cards;

    TextMeshProUGUI voteText;
    
    int count = 1;
    bool isSelected = false;
    string playerName;

    int votes = 0;

    public void Init(string name, string[] whiteCardTexts)
    {
        playerName = name;

        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(whiteCardPrefab, cards);
            card.GetComponent<WhiteCard>().SetIsUsedInsideAnswer(true);
        }
        SetWhiteCardTexts(whiteCardTexts);

        if(playerName == PhotonNetwork.LocalPlayer.NickName)
        {
            foreach(Transform card in cards)
            {
                WhiteCard whiteCard = card.GetComponent<WhiteCard>();
                whiteCard.SetDisabledImageActive(true);
            }
        }

        Transform firstCard = cards.GetChild(0);
        voteText = firstCard.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    public void ShowVoteText()
    {
        voteText.gameObject.SetActive(true);
    }

    void SetWhiteCardTexts(string[] whiteCardTexts)
    {
        for(int i = 1; i < whiteCardTexts.Length; i++)
        {
            cards.GetChild(i-1).GetComponentInChildren<TextMeshProUGUI>().text = whiteCardTexts[i];
        }
    }

    [PunRPC]
    public void Select()
    {
        isSelected = true;
        foreach(Transform card in transform.GetChild(0))
        {
            WhiteCard whiteCardScript = card.GetComponent<WhiteCard>();
            whiteCardScript.ChangeToSelectedColor();
            whiteCardScript.SetDisabledImageActive(false);
        }
    }

    public void UnSelect()
    {
        isSelected = false;
        foreach (Transform card in transform.GetChild(0))
        {
            card.GetComponent<WhiteCard>().ChangeToNormalColor();
        }
    }

    public Transform GetCards()
    {
        return cards;
    }

    public int GetCount()
    {
        return count;
    }

    public void SetCount(int i)
    {
        count = i;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public bool GetIsSelected()
    {
        return isSelected;
    }

    public void IncreaseVote()
    {
        votes++;
        voteText.text = "Votes: " + votes;
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
}
