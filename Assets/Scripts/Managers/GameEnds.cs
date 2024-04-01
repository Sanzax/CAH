using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameEnds : MonoBehaviourPun
{
    [SerializeField] Transform scoreboard;

    LogManager logManager;
    CardManager cardManager;
    AnswerManager answerManager;
    GameObjectActivator gameObjectActivator;

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        cardManager = GetComponent<CardManager>();
        answerManager = GetComponent<AnswerManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
    }


    [PunRPC]
    public void ToLobby()
    {
        gameObjectActivator.SetInGameViewActive(false);
        gameObjectActivator.SetLobbyViewActive(true);
        gameObjectActivator.SetBlackCardVoteViewActive(false);
        answerManager.DeleteAnswers();
        gameObjectActivator.HideSlots();
        ResetScorePanels();
        cardManager.DeleteCardsOnSlots();
        cardManager.ClearHand();
        ResetPlayerCustomProperties();
        gameObjectActivator.SetSelectorViewActive(false);
        cardManager.DestroyBlackCards();
        ToLobbyMaster();
    }

    [PunRPC]
    public void ResetScorePanels()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerScore playerScoreScript = Common.FindPlayerScorePanel(player, scoreboard);
            string hostText = player.IsMasterClient ? "Host" : "";
            playerScoreScript.UpdateScore(0);
            playerScoreScript.UpdateIsReady(hostText);
        }
    }

    void ResetPlayerCustomProperties()
    {
        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
        {
            { "Score", 0 },
            { "CardAmount", 0 },
            { "IsReady", false },
            { "IsSelector", false }
        });
    }


    public void ToLobbyMaster()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        gameObjectActivator.SetGameSettingsViewActive(true);
        ResetRoomCustomProperties();
    }

    void ResetRoomCustomProperties()
    {
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsGameStarted", false },
            { "IsRoundInProgress", false },
            { "IsSelectorSelecting", false },
            { "RoundNumber", 0 },
            { "BlackCardVoteCurrentlyOn", false }
        });
    }

    public void StopGameButtonPressed()
    {
        logManager.NewLogForAll("Host closed the game.");
        photonView.RPC("ToLobby", RpcTarget.All);
    }

}
