using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Scoreboard : MonoBehaviourPun
{
    [SerializeField] Transform scoreboard;

    public Transform GetScoreboard()
    {
        return scoreboard;
    }

    public bool AreEveryoneReady()
    {
        foreach (Transform playerScore in scoreboard)
        {
            PlayerScore playerScoreScript = playerScore.GetComponent<PlayerScore>();
            Player player = playerScoreScript.photonView.Owner;
            if (!Common.IsPlayerPropertyTrue(player, "IsReady"))
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateRounWinnerScore(string roundWinnerName)
    {
        Player roundWinner = Common.FindPlayer(roundWinnerName);
        PlayerScore playerScoreScript = Common.FindPlayerScorePanel(roundWinner, scoreboard);

        playerScoreScript.photonView.RPC("UpdateScore", RpcTarget.All, (int)roundWinner.CustomProperties["Score"] + 1);
        playerScoreScript.photonView.RPC("HighLight", RpcTarget.All);

        Common.SetCustomPropertiesOfPlayer(roundWinner, new ExitGames.Client.Photon.Hashtable
        {
            { "Score", (int)roundWinner.CustomProperties["Score"] + 1 }
        });
    }

    public void SetReadyOnScorePanel(Player player, string readyText)
    {
        PlayerScore playerScoreScript = Common.FindPlayerScorePanel(player, scoreboard);
        playerScoreScript.photonView.RPC("UpdateIsReady", RpcTarget.All, readyText);
    }

    public void ResetSelectorTexts()
    {
        foreach (Transform scorePanel in scoreboard)
        {
            PlayerScore playerScoreScript = scorePanel.GetComponent<PlayerScore>();
            playerScoreScript.photonView.RPC("UpdateIsReady", RpcTarget.All, "");
        }
    }

    public void UpdatePlayerScoreboardsIfYouComeLate()
    {
        foreach (Transform scorepanel in scoreboard)
        {
            PlayerScore playerScoreScript = scorepanel.GetComponent<PlayerScore>();

            int score = (int)playerScoreScript.photonView.Owner.CustomProperties["Score"];
            playerScoreScript.photonView.RPC("UpdateScore", PhotonNetwork.LocalPlayer, score);

            string text = (bool)playerScoreScript.photonView.Owner.CustomProperties["IsSelector"] ? "Selector" : "";

            if (text != "Selector")
            {
                text = (bool)playerScoreScript.photonView.Owner.CustomProperties["IsReady"] ? "Ready" : "";
            }

            playerScoreScript.photonView.RPC("UpdateIsReady", PhotonNetwork.LocalPlayer, text);
        }
    }
}
