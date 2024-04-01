using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField nameInput;
    List<RoomInfo> currentRoomList = new List<RoomInfo>();
    [SerializeField] NameColorHandler nameColor;
    bool wasPrevioslyHere;

    LogManager logManager;
    CardManager cardManager;
    AnswerManager answerManager;
    GameObjectActivator gameObjectActivator;
    BlackCardVote blackCardVote;
    RoundManager roundManager;
    Scoreboard scoreboard;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        logManager = GetComponent<LogManager>();
        cardManager = GetComponent<CardManager>();
        answerManager = GetComponent<AnswerManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
        blackCardVote = GetComponent<BlackCardVote>();
        roundManager = GetComponent<RoundManager>();
        scoreboard = GetComponent<Scoreboard>();
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        logManager.NewLog("Connected to server!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        logManager.NewLog("You created room " + PhotonNetwork.CurrentRoom.Name);
        SetRoomProperties();

        SetUpMasterClient(PhotonNetwork.LocalPlayer);

        cardManager.TryGettingCardsFromWebsite();
    }

    void SetRoomProperties()
    {
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsGameStarted", false },
            { "IsRoundInProgress", false },
            { "IsSelectorSelecting", false },
            { "Answers", null },
            { "AnswerLength", 0 },
            { "RoundNumber", 0 },
            { "PointsToWin", 4 },
            { "OldSelector", -1 },
            { "PlayerNames", new string[] { PhotonNetwork.LocalPlayer.NickName } },
            { "PlayerData", null},
            { "BlackCardText", "" },
            { "BlackCardVoteEnabled", false },
            { "BlackCardVoteCurrentlyOn", false },
            { "BlackCardVoteAmount", 0 },
            { "BlackCardsOnTable", null },
            { "BlackCardVotes", null},
            { "AnswerVotes", null},
            { "GameMode", "Normal" },
            { "OldSelectorNumber", null },
        });
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.LocalPlayer.NickName = nameInput.text;
        logManager.NewLogForAll("<b><#" + ColorUtility.ToHtmlStringRGB(nameColor.GetColor())+ ">" + PhotonNetwork.LocalPlayer.NickName + "</color></b>" + " joined the game!");
        gameObjectActivator.SetMainMenuActive(false);
        gameObjectActivator.SetLobbyViewActive(true);
        cardManager.DestroyBlackCards();
        SetPlayerProperties();
        wasPrevioslyHere = LoadPlayerDataIfTheyHavePreviouslyBeenHere(PhotonNetwork.LocalPlayer.NickName);
        LateComers(wasPrevioslyHere);
        Invoke("DelayedLateComers", 1f);
        Invoke("CreatePlayerScorePanel", .1f);
        AddNewPlayerToList();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        gameObjectActivator.ChangeStopGameButtonInteractabality(false);
    }

    bool LoadPlayerDataIfTheyHavePreviouslyBeenHere(string userName)
    {
        List<string[]> playerDataList = new List<string[]>(GetPlayerDataListFromRoom(PhotonNetwork.CurrentRoom));

        foreach (string[] playerData in playerDataList)
        {
            if (playerData[0] != userName)
                continue;

            logManager.NewLogForAll("We missed you.");

            if (Common.IsRoomPropertyTrue("IsGameStarted"))
            {
                string[] cards = new string[10];
                for (int i = 0; i < CardManager.MAX_AMOUNT_OF_CARDS; i++)
                {
                    cards[i] = playerData[i + 2];
                }

                Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
                {
                    { "Score", Int32.Parse(playerData[1]) },
                    { "Cards", cards },
                });

                StartCoroutine(DelayedLoad(playerData));
            }

            playerDataList.Remove(playerData);
            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerData", playerDataList.Count == 0 ? null : playerDataList.ToArray() }
            });

            return true;
        }
        return false;
    }

    IEnumerator DelayedLoad(string[] playerData)
    {
        yield return new WaitForSeconds(.5f);

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundNumber"] == Int32.Parse(playerData[14]))
        {
            if((Common.IsRoomPropertyTrue("IsSelectorSelecting") && (string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == "Vote") ||
                (playerData[13] == "true"))
            {
                Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
                {
                    { "IsReady", playerData[12] == "true" }
                });
                scoreboard.SetReadyOnScorePanel(PhotonNetwork.LocalPlayer, playerData[12] == "true" ? "Ready" : "");
                cardManager.UpdateAnswerVotes(answerManager.GetAnswerVotes());
            }
            if ((playerData[13] == "true"))
            {
                cardManager.UpdateBlackCardVotes(blackCardVote.GetBlackCardVotes());
            }
        }
    }

    void LateComers(bool wasPreviouslyHere)
    {
        if (!Common.IsRoomPropertyTrue("IsGameStarted"))
            return;

        gameObjectActivator.SetSelectorViewActive(false);
        gameObjectActivator.SetInGameViewActive(true);
        gameObjectActivator.SetLobbyViewActive(false);
        cardManager.SwitchBlackCard((string)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardText"]);

        if (Common.IsRoomPropertyTrue("IsSelectorSelecting"))
            answerManager.CreateAnswersForLateComer();

        if(Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            cardManager.CreateBlackCardsForVote((string[])PhotonNetwork.CurrentRoom.CustomProperties["BlackCardsOnTable"]);

        if(!Common.IsRoomPropertyTrue("IsSelectorSelecting") && !Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
            gameObjectActivator.ShowSlot((int)PhotonNetwork.CurrentRoom.CustomProperties["AnswerLength"] - 1);

        if (wasPreviouslyHere)
            return;

        List<string> whiteCardsFromRoom = new List<string>(cardManager.GetWhiteCardsFromCurrentRoom());
        cardManager.DrawWhiteCardsForPlayer(PhotonNetwork.LocalPlayer, whiteCardsFromRoom);
        cardManager.SavePlayerCards();
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "WhiteCards", whiteCardsFromRoom.ToArray() }
        });
    }

    void DelayedLateComers()
    {
        if (!Common.IsRoomPropertyTrue("IsGameStarted"))
            return;

        scoreboard.UpdatePlayerScoreboardsIfYouComeLate();

        if (!wasPrevioslyHere)
            return;

        cardManager.WhenJoiningAgainCreateOldCardsBack();
        foreach (Transform scorePanel in scoreboard.GetScoreboard())
        {
            PlayerScore playerScoreScript = scorePanel.GetComponent<PlayerScore>();
            if (playerScoreScript.photonView.Owner != PhotonNetwork.LocalPlayer)
                continue;
            int score = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            playerScoreScript.photonView.RPC("UpdateScore", RpcTarget.All, score);
        }
    }

    string[][] GetPlayerDataListFromRoom(RoomInfo room)
    {
        string[][] playerDataList = (string[][])room.CustomProperties["PlayerData"];
        if (playerDataList == null)
            return new string[0][];
        return playerDataList;
    }

    void SetPlayerProperties()
    {
        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
        {
            { "Id", PhotonNetwork.CurrentRoom.PlayerCount - 1 },
            { "Score", 0 },
            { "CardAmount", 0 },
            { "IsReady", false },
            { "IsSelector", false },
            { "Color", ColorUtility.ToHtmlStringRGB(nameColor.GetColor()) },
            { "Answer", new string[] { } },
            { "Cards", new string[10] { "", "", "", "", "", "", "", "", "", ""} },
            { "BlackCardSelected", true }
        });
    }

    void CreatePlayerScorePanel()
    {
        GameObject playerScorePanel = PhotonNetwork.Instantiate("PlayerScorePanel", new Vector3(0, 0, 0), Quaternion.identity);
        PlayerScore playerScoreScript = playerScorePanel.GetComponent<PlayerScore>();
        playerScoreScript.photonView.RPC("Init", RpcTarget.AllBuffered, new float[] { nameColor.GetColor().r, nameColor.GetColor().g, nameColor.GetColor().b });
    }

    void AddNewPlayerToList()
    {
        string[] arr = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerNames"] == null ? new string[] { } : (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerNames"];
        List<string> playerNames = new List<string>(arr);
        playerNames.Add(PhotonNetwork.LocalPlayer.NickName);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable 
        { 
            { "PlayerNames", playerNames.ToArray() } 
        });
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        logManager.NewLog(newMasterClient.NickName + " is the new host");
        SetUpMasterClient(newMasterClient);
        scoreboard.SetReadyOnScorePanel(PhotonNetwork.LocalPlayer, "Host");
    }

    void SetUpMasterClient(Player newMasterClient)
    {
        gameObjectActivator.ChangeStopGameButtonInteractabality(PhotonNetwork.LocalPlayer == newMasterClient);
        
        if (Common.IsRoomPropertyTrue("IsGameStarted"))
            return;

        gameObjectActivator.SetGameSettingsViewActive(true);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        
        logManager.NewLog("RIP... " + "<b><#" + (string)otherPlayer.CustomProperties["Color"] + ">" + otherPlayer.NickName + "</color></b>" + " left the game.");

        if(PhotonNetwork.IsMasterClient && Common.IsRoomPropertyTrue("IsGameStarted"))
            SavePlayerDataIncaseTheyComeBack(otherPlayer);

        RemovePlayerFromList(otherPlayer);
        SelectorLeft(otherPlayer);
    }

    void SavePlayerDataIncaseTheyComeBack(Player player)
    {
        string[] playerData = new string[15];

        playerData[0] = player.NickName;
        playerData[1] = ((int)player.CustomProperties["Score"]).ToString();
        playerData[12] = (bool)player.CustomProperties["IsReady"] ? "true" : "false";
        playerData[13] = (bool)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardVoteCurrentlyOn"] ? "true" : "false";
        playerData[14] = ((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundNumber"]).ToString();

        for (int i = 0; i < CardManager.MAX_AMOUNT_OF_CARDS; i++)
        {
            string card = ((string[])player.CustomProperties["Cards"])[i];
            playerData[i + 2] = card;
        }

        List<string[]> playerDataList = new List<string[]>(GetPlayerDataListFromRoom(PhotonNetwork.CurrentRoom));

        playerDataList.Add(playerData);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable 
        {
            { "PlayerData", playerDataList.ToArray() }
        });
    }

    void RemovePlayerFromList(Player player)
    {
        List<string> playerNames = new List<string>((string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerNames"]);
        playerNames.Remove(player.NickName);
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerNames", playerNames.ToArray() }
        });
    }

    void SelectorLeft(Player player)
    {
        bool wasPlayerSelector = (bool)player.CustomProperties["IsSelector"];
        if (!wasPlayerSelector)
            return;

        logManager.NewLog("Selector left. New round begins!");
        roundManager.StartNewRound(1, true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        currentRoomList = roomList;
    }

    public List<RoomInfo> GetRoomList()
    {
        return currentRoomList;
    }

    public void RefreshLobby()
    {
        PhotonNetwork.JoinLobby();
    }

}
