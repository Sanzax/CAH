using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorManager : MonoBehaviourPun
{
    [SerializeField] Transform scoreboard;

    ClientManager clientManager;
    LogManager logManager;

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        clientManager = GetComponent<ClientManager>();
    }

    public void SelectSelector()
    {
        Player newSelector = NextSelector();

        Common.SetCustomPropertiesOfPlayer(newSelector, new ExitGames.Client.Photon.Hashtable
        {
            { "IsSelector", true },
            { "IsReady", true }
        });

        foreach (Transform scorePanel in scoreboard)
        {
            PlayerScore playerScoreScript = scorePanel.GetComponent<PlayerScore>();
            if (playerScoreScript.photonView.Owner != newSelector)
                continue;

            playerScoreScript.photonView.RPC("UpdateIsReady", RpcTarget.All, "Selector");
        }

        clientManager.photonView.RPC("HideSlots", newSelector);
        clientManager.photonView.RPC("SetSelectorViewActive", newSelector, true);

        int roundNumber = (int)((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundNumber"] + 1);
        logManager.NewLogForAll("<b><#" + (string)newSelector.CustomProperties["Color"] + ">" + newSelector.NickName + "</color></b>" + " is the new selector!");

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "OldSelector", newSelector.ActorNumber }
        });
    }

    Player NextSelector()
    {
        Player oldSelector = FindOldSelector();
        Player newSelector;
        if (oldSelector == null)
        {
            newSelector = SelectRandomSelector();
            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable { { "OldSelectorNumber", newSelector.ActorNumber } });
            return newSelector;
        }

        ResetOldSelector(oldSelector);

        newSelector = oldSelector.GetNext();

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable { { "OldSelectorNumber", newSelector.ActorNumber } });

        return newSelector;
    }

    Player FindOldSelector()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int selectorNumber = PhotonNetwork.CurrentRoom.CustomProperties["OldSelectorNumber"] == null ? -1 : (int)PhotonNetwork.CurrentRoom.CustomProperties["OldSelectorNumber"];
            if (player.ActorNumber == selectorNumber)
            {
                return player;
            }
        }
        return null;
    }

    Player SelectRandomSelector()
    {
        return PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount)];
    }

    void ResetOldSelector(Player oldSelector)
    {
        Common.SetCustomPropertiesOfPlayer(oldSelector, new ExitGames.Client.Photon.Hashtable
        {
            { "IsSelector", false }
        });
        photonView.RPC("SetSelectorViewActive", oldSelector, false);
    }

}
