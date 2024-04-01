using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviourPun
{
    ClientManager clientManager;
    LogManager logManager;
    CardManager cardManager;
    AnswerManager answerManager;
    GameObjectActivator gameObjectActivator;
    BlackCardVote blackCardVote;
    SelectorManager selectorManager;
    Scoreboard scoreboard;

    public bool AreAnswersCreated { get; set; }
    public bool IsVoteOver { get; set; }

    const int REQUIRED_PLAYER_COUNT = 3;

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        cardManager = GetComponent<CardManager>();
        answerManager = GetComponent<AnswerManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
        clientManager = GetComponent<ClientManager>();
        selectorManager = GetComponent<SelectorManager>();
        blackCardVote = GetComponent<BlackCardVote>();
        scoreboard = GetComponent<Scoreboard>();
        AreAnswersCreated = false;
    }

    private void Update()
    {
        CheckIfAnswersCanBeCreated();
        CheckIfEveryOneHasVoted();
        ActivateGameStartButton();
    }

    void CheckIfEveryOneHasVoted()
    {
        if (PhotonNetwork.CurrentRoom == null || !PhotonNetwork.IsMasterClient)
            return;

        bool canAnnounceWinner = !IsVoteOver && (string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Vote" &&
            Common.IsRoomPropertyTrue("IsSelectorSelecting") && scoreboard.AreEveryoneReady();

        if (!canAnnounceWinner)
            return;

        IsVoteOver = true;
        Answer selectedAnswer = answerManager.FindAnswerWithMostVotes();
        AnnounceWinner(selectedAnswer);
        photonView.RPC("ShowAnswerVotes", RpcTarget.All);
    }

    void CheckIfAnswersCanBeCreated()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!Common.IsRoomPropertyTrue("IsGameStarted"))
            return;

        if (Common.IsRoomPropertyTrue("IsSelectorSelecting"))
            return;

        if (!Common.IsRoomPropertyTrue("IsRoundInProgress"))
            return;

        if (Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            return;

        if (AreAnswersCreated)
            return;

        if (!scoreboard.AreEveryoneReady())
            return;

        if (!EnoughPlayers())
            return;

        answerManager.CreateAnswers();

        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Vote")
        {
            ResetSomePlayerValues();
        }

        AreAnswersCreated = true;

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsSelectorSelecting", true }
        });
    }

    [PunRPC]
    public void StartNewRound(float delay, bool playerLeft = false)
    {
        StartCoroutine(NewRound(delay));
    }

    public IEnumerator NewRound(float delay, bool playerLeft = false)
    {
        if(playerLeft)
        {
            photonView.RPC("ReturnCardsToHand", RpcTarget.All);
            cardManager.DestroyBlackCards();
        }

        yield return new WaitForSeconds(delay);

        int roundNumber = (int)((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundNumber"] + 1);
        logManager.NewLog("Round " + roundNumber + ". ");

        answerManager.IsVoted = false;
        gameObjectActivator.SetReadyButtonInteractability(false);
        answerManager.DeleteAnswers();

        StartCoroutine(NewRoundMaster());
    }

    public IEnumerator NewRoundMaster()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            yield break;

        AreAnswersCreated = false;

        if (SomeoneHasWon())
            yield break;

        ResetSomePlayerValues();
        cardManager.DrawWhiteCardsForEachPlayer();

        if(!Common.IsRoomPropertyTrue("BlackCardVoteEnabled"))
        {
            string blackCardText = cardManager.DrawBlackCard();
            SetUpBlackCardAndSelectNewSelector(blackCardText);
        }
        else
        {
            photonView.RPC("BlackCardVoteStart", RpcTarget.All);
            StartCoroutine(blackCardVote.StartNewRoundAfterBlackCardIsVoted());
        }

        yield return new WaitForSeconds(.5f);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            photonView.RPC("SavePlayerCards", player);
        }

        if (Common.IsRoomPropertyTrue("BlackCardVoteEnabled"))
            yield break;

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable{ { "IsRoundInProgress", true } });

        yield return new WaitForSeconds(4f);

        IsVoteOver = false;
    }

    public void SetUpBlackCardAndSelectNewSelector(string blackCardText)
    {
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCardText", blackCardText }
        });

        int answerLength = cardManager.HowManyUnderscores(blackCardText);
        SetupNewRoundProperties(answerLength);
        clientManager.photonView.RPC("ShowSlot", RpcTarget.All, answerLength - 1);

        if((string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Normal")
            selectorManager.SelectSelector();

        cardManager.photonView.RPC("SwitchBlackCard", RpcTarget.All, blackCardText);
    }

    bool SomeoneHasWon()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if ((int)player.CustomProperties["Score"] == (int)PhotonNetwork.CurrentRoom.CustomProperties["PointsToWin"])
            {
                logManager.NewLogForAll("<b><#" + (string)player.CustomProperties["Color"] + ">" + player.NickName + "</color></b>" + " won the game!");

                clientManager.photonView.RPC("ToLobby", RpcTarget.All);
                return true;
            }
        }
        return false;
    }

    public void SetupNewRoundProperties(int answerLength)
    {
        string[] answers = new string[(PhotonNetwork.CurrentRoom.PlayerCount - 1) * answerLength + PhotonNetwork.CurrentRoom.PlayerCount - 1];
        for (int i = 0; i < answers.Length; i++)
            answers[i] = "";

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsSelectorSelecting", false },
            { "RoundNumber", (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundNumber"] + 1 },
            { "AnswerLength", answerLength },
            { "Answers", answers }
        });
    }

    public void ResetSomePlayerValues()
    {
        scoreboard.ResetSelectorTexts();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Common.SetCustomPropertiesOfPlayer(player, new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", false },
                { "Answer", null }
            });
        }
    }

    public void ReadyButtonPressed()
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsSelector"])
            SelectorPressedReadyButton();
        else if(!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsSelectorSelecting"])
            NonSelectorPressedReadyButton();
        else
            VoteWhiteCard();
    }

    void VoteWhiteCard()
    {
        if (answerManager.IsVoted)
            return;

        if (Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            return;

        scoreboard.SetReadyOnScorePanel(PhotonNetwork.LocalPlayer, "Ready");
        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable { { "IsReady", true } });
        gameObjectActivator.SetReadyButtonInteractability(false);

        Answer selectedAnswer = answerManager.FindSelectedAnswer();
        string winnerName = selectedAnswer.GetPlayerName();
        photonView.RPC("IncreaseVoteOnAnswer", RpcTarget.All, winnerName);

        selectedAnswer.UnSelect();
        answerManager.IsVoted = true;
    }

    void NonSelectorPressedReadyButton()
    {
        if (Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            return;

        Transform slotGroup = gameObjectActivator.GetActiveSlotGroup();
        int answerLength = slotGroup.childCount;
        answerManager.SaveAnswer(slotGroup, answerLength);

        scoreboard.SetReadyOnScorePanel(PhotonNetwork.LocalPlayer, "Ready");
        cardManager.DeleteCardsOnSlots();
        slotGroup.gameObject.SetActive(false);
        gameObjectActivator.SetReadyButtonInteractability(false);
    }

    void SelectorPressedReadyButton()
    {
        gameObjectActivator.SetReadyButtonInteractability(false);
        Answer selectedAnswer = answerManager.FindSelectedAnswer();
        AnnounceWinner(selectedAnswer);
    }

    void AnnounceWinner(Answer selectedAnswer)
    {
        string winnerName = selectedAnswer.GetPlayerName();
        photonView.RPC("HighlightAnswer", RpcTarget.All, winnerName);
        logManager.NewLogForAll(ConstructLogMessage(selectedAnswer, winnerName));
        scoreboard.UpdateRounWinnerScore(winnerName);
        photonView.RPC("RoundIsOver", PhotonNetwork.MasterClient);
        photonView.RPC("StartNewRound", RpcTarget.All, 3f, false);
    }

    string ConstructLogMessage(Answer selectedAnswer, string roundWinnerName)
    {
        string answerTexts = CombineCardTexts(selectedAnswer);
        Player roundWinner = Common.FindPlayer(roundWinnerName);
        string pluralText = selectedAnswer.GetCards().childCount == 1 ? " won the round with card: " : " won the round with cards: ";
        string voteText = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Vote" ? " They got " + selectedAnswer.GetVotes() + " votes." : "";
        return "<b><#" + (string)roundWinner.CustomProperties["Color"] + ">" + roundWinner.NickName + "</color></b>" + pluralText + answerTexts + "." + voteText;
    }

    string CombineCardTexts(Answer selectedAnswer)
    {
        string output = "";
        foreach (Transform card in selectedAnswer.GetCards())
        {
            output += "\"" + card.GetComponentInChildren<TextMeshProUGUI>().text + "\"";
            bool cardIsNotLastOnAnswer = card.GetSiblingIndex() < selectedAnswer.GetCards().childCount - 1;
            if (cardIsNotLastOnAnswer)
                output += ", ";
        }
        return output;
    }

    public bool EnoughPlayers()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount >= REQUIRED_PLAYER_COUNT;
    }

    [PunRPC]
    public void RoundIsOver()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsRoundInProgress", false }
        });
    }

    void ActivateGameStartButton()
    {
        if (!PhotonNetwork.IsMasterClient || Common.IsRoomPropertyTrue("IsGameStarted"))
            return;

        gameObjectActivator.SetGameStartButtonInterActable(EnoughPlayers() && cardManager.CardsAqquired());
    }
}
