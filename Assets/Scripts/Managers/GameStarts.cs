using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GameStarts : MonoBehaviourPun
{
    LogManager logManager;
    CardManager cardManager;
    GameObjectActivator gameObjectActivator;
    RoundManager roundManager;
    GameSettings gameSettings;

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        cardManager = GetComponent<CardManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
        roundManager = GetComponent<RoundManager>();
        gameSettings = GetComponent<GameSettings>();
    }

    public void StartGameButtonPressed()
    {
        StartCoroutine(WhenGameStarts());
    }

    public IEnumerator WhenGameStarts()
    {
        cardManager.InitCards();

        yield return new WaitForSeconds(1);

        int pointsToWin = Int32.Parse(gameSettings.GetPointsToWinDropDown().options[gameSettings.GetPointsToWinDropDown().value].text);
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "IsGameStarted", true },
            { "RoundNumber", 0 },
            { "PointsToWin", pointsToWin },
        });

        logManager.NewLogForAll("Game started! First to get " + pointsToWin + " points wins!");

        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    public void StartGame()
    {
        cardManager.DeleteCardsOnSlots();
        gameObjectActivator.HideSlots();
        gameObjectActivator.SetGameSettingsViewActive(false);
        gameObjectActivator.SetLobbyViewActive(false);
        gameObjectActivator.SetInGameViewActive(true);

        roundManager.StartNewRound(0);
    }


}
