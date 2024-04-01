using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerManager : MonoBehaviourPun
{
    [SerializeField] GameObject answerPrefab;
    [SerializeField] Transform answers;

    [SerializeField] GridLayoutGroup gridLayout;

    public bool IsVoted { get; set; }

    public Transform Answers { get { return answers; } }

    [PunRPC]
    public void ShowAnswerVotes()
    {
        foreach(Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            answerScript.ShowVoteText();
        }
    }

    public int[] GetAnswerVotes()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties["AnswerVotes"] == null)
        {
            int[] votes = new int[answers.childCount];

            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
            {
                { "AnswerVotes",  votes }
            });
            return votes;
        }
        return (int[])PhotonNetwork.CurrentRoom.CustomProperties["AnswerVotes"];
    }

    public void SetAnswerVotes()
    {
        int answerAmount = answers.childCount;
        int[] answerVotes = new int[answerAmount];
        if (answers.childCount == 0)
        {
            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
            {
                {"AnswerVotes", new int[answerAmount] }
            });
            return;
        }

        foreach (Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            answerVotes[answer.GetSiblingIndex()] = answerScript.GetVotes();
        }

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            {"AnswerVotes", answerVotes }
        });
    }

    public Answer FindSelectedAnswer()
    {
        foreach (Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            if (answerScript.GetIsSelected())
            {
                return answerScript;
            }
        }
        return null;
    }

    public Answer FindAnswerWithMostVotes()
    {
        List<int> answerVotes = new List<int>();
        foreach (Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            answerVotes.Add(answerScript.GetVotes());
        }

        int answerIndex= WeightedRandom.GetRandomIndex(answerVotes);
        Answer finalAnswer = answers.GetChild(answerIndex).GetComponent<Answer>();
        return finalAnswer;
    }

    public void SaveAnswer(Transform slotGroup, int answerLength)
    {
        string[] answer = CreateAnswer(slotGroup, answerLength);
        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
        {
            { "Answer", answer },
            { "IsReady", true },
            { "CardAmount", 10 - answerLength }
        });
    }

    string[] CreateAnswer(Transform slotGroup, int answerLength)
    {
        string[] answer = new string[answerLength + 1];
        answer[0] = PhotonNetwork.LocalPlayer.NickName;

        for (int i = 1; i < answerLength + 1; i++)
        {
            WhiteCard whiteCard = slotGroup.GetChild(i - 1).GetChild(0).GetComponent<WhiteCard>();
            answer[i] = whiteCard.GetText();
        }
        return answer;
    }

    [PunRPC]
    public void DeleteAnswers()
    {
        foreach (Transform answer in answers)
        {
            Destroy(answer.gameObject);
        }
    }

    public void CreateAnswers()
    {
        List<string[]> answerList = PutAnswersIntoAList();
        Common.Shuffle(answerList);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "Answers", answerList.ToArray() }
        });

        int answerLength = (int)PhotonNetwork.CurrentRoom.CustomProperties["AnswerLength"];
        photonView.RPC("CreateAnswersForEveryone", RpcTarget.All, answerList.ToArray(), answerLength);
    }

    public void CreateAnswersForLateComer()
    {
        int answerLength = (int)PhotonNetwork.CurrentRoom.CustomProperties["AnswerLength"];
        CreateAnswersForEveryone((string[][])PhotonNetwork.CurrentRoom.CustomProperties["Answers"], answerLength);
    }



    [PunRPC]
    public void CreateAnswersForEveryone(string[][] playerAnswers, int answerLength)
    {
        for (int i = 0; i < playerAnswers.Length; i++)
        {
            CreateAnswer(playerAnswers, answerLength, i);
        }

        ReConfigureAnswerHolderValues(answerLength);
    }

    Answer CreateAnswer(string[][] playerAnswers, int answerLength, int i)
    {
        GameObject answer = Instantiate(answerPrefab, answers);
        Answer answerScript = answer.GetComponent<Answer>();
        answerScript.SetCount(answerLength);
        answerScript.Init(playerAnswers[i][0], playerAnswers[i]);
        return answerScript;
    }

    public void ReConfigureAnswerHolderValues(int numberOfCardsInAnswer)
    {
        switch (numberOfCardsInAnswer)
        {
            case 1:
                gridLayout.padding.left = 7;
                gridLayout.cellSize = new Vector2(187, gridLayout.cellSize.y);
                gridLayout.constraintCount = 7;
                gridLayout.spacing = new Vector2(15, gridLayout.spacing.y);
                break;
            case 2:
                gridLayout.padding.left = 5;
                gridLayout.cellSize = new Vector2(374, gridLayout.cellSize.y);
                gridLayout.constraintCount = 4;
                gridLayout.spacing = new Vector2(11, gridLayout.spacing.y);
                break;
            case 3:
                gridLayout.padding.left = 2;
                gridLayout.cellSize = new Vector2(561, gridLayout.cellSize.y);
                gridLayout.constraintCount = 2;
                gridLayout.spacing = new Vector2(6, gridLayout.spacing.y);
                break;
        }
    }

    List<string[]> PutAnswersIntoAList()
    {
        List<string[]> answerList = new List<string[]>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if ((bool)player.CustomProperties["IsSelector"])
                continue;

            string[] playerAnswer = (string[])player.CustomProperties["Answer"];
            answerList.Add(playerAnswer);
        }
        return answerList;
    }

    [PunRPC]
    public void HighlightAnswer(string winnerName)
    {
        foreach(Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            if (answerScript.GetPlayerName() != winnerName)
                continue;

            answerScript.Select();
        }
    }

    [PunRPC]
    public void IncreaseVoteOnAnswer(string ownerName)
    {
        foreach (Transform answer in answers)
        {
            Answer answerScript = answer.GetComponent<Answer>();
            if (answerScript.GetPlayerName() != ownerName)
                continue;

            answerScript.IncreaseVote();
        }
        SetAnswerVotes();
    }
}
