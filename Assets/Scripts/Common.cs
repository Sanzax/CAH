using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public static class Common
{
    public static bool IsRoomPropertyTrue(string propertyName)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[propertyName] == null)
            return false;
        return (bool)PhotonNetwork.CurrentRoom.CustomProperties[propertyName];
    }

    public static bool IsPlayerPropertyTrue(Player player, string propertyName)
    {
        if (player.CustomProperties[propertyName] == null)
            return false;
        return (bool)player.CustomProperties[propertyName];
    }

    public static bool IsPlayerPropertyTrue(string propertyName)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties[propertyName] == null)
            return false;
        return (bool)PhotonNetwork.LocalPlayer.CustomProperties[propertyName];
    }

    public static void SetCustomPropertiesOfRoom(Room room, ExitGames.Client.Photon.Hashtable hashTable)
    {
        if (room == null)
        {
            Debug.LogWarning("Tried to access properties of a room that doesnt exist");
            return;
        }
        room.SetCustomProperties(hashTable);
    }

    public static void SetCustomPropertiesOfPlayer(Player player, ExitGames.Client.Photon.Hashtable hashTable)
    {
        if (player == null)
        {
            Debug.LogWarning("Tried to access properties of a player that doesnt exist");
            return;
        }
        player.SetCustomProperties(hashTable);
    }

    public static Player FindPlayer(string playerName)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == playerName)
            {
                return player;
            }
        }
        return null;
    }

    public static PlayerScore FindPlayerScorePanel(Player player, Transform scoreboard)
    {
        foreach (Transform playerScore in scoreboard)
        {
            PlayerScore playerScoreScript = playerScore.GetComponent<PlayerScore>();
            if (playerScoreScript.photonView.Owner == player)
            {
                return playerScoreScript;
            }
        }
        return null;
    }

    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Transform FindFirstActiveChildIn(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf)
            {
                return child;
            }
        }
        return null;
    }
}
