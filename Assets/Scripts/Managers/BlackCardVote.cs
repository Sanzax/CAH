using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class BlackCardVote : MonoBehaviourPun
{
    LogManager logManager;
    CardManager cardManager;
    GameObjectActivator gameObjectActivator;
    RoundManager roundManager;
    Scoreboard scoreboard;

    [SerializeField] Transform blackCardVoteView;

    private bool blackCardVoted;

    private void Awake()
    {
        logManager = GetComponent<LogManager>();
        cardManager = GetComponent<CardManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
        roundManager = GetComponent<RoundManager>();
        scoreboard = GetComponent<Scoreboard>();
    }

    [PunRPC]
    public void BlackCardVoteStart()
    {
        gameObjectActivator.SetSelectorViewActive(false);
        gameObjectActivator.SetBlackCardVoteViewActive(true);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if ((bool)player.CustomProperties["IsSelector"])
            {
                Common.SetCustomPropertiesOfPlayer(player, new ExitGames.Client.Photon.Hashtable
                {
                    {"IsSelector", false }
                });
                break;
            }
        }

        cardManager.SwitchBlackCard("Vote for the best black card.");
        BlackCardVoteMaster();
    }

    void BlackCardVoteMaster()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;


        //Reset them
        cardManager.SetBlackCardVotes();

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCardVoteCurrentlyOn", true },
            { "BlackCardText", "Vote for the best black card." }
        });

        string[] blackCardTexts = cardManager.GetBlackCardTextsForVote();

        photonView.RPC("CreateBlackCardsForVote", RpcTarget.All, blackCardTexts);
    }

    public void ReadyButtonPressed()
    {
        if (Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn"))
        {
            SendVoteResult();
            return;
        }
    }

    void SendVoteResult()
    {
        gameObjectActivator.SetReadyButtonInteractability(false);

        int blackCardIndex = FindBlackCardIndex();

        cardManager.UnSelectAllBlackCards();

        scoreboard.SetReadyOnScorePanel(PhotonNetwork.LocalPlayer, "Ready");

        Common.SetCustomPropertiesOfPlayer(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable
        {
            { "IsReady", true }
        });

        photonView.RPC("IncreaseVote", RpcTarget.All, blackCardIndex);
    }

    int FindBlackCardIndex()
    {
        foreach (Transform blackCard in blackCardVoteView)
        {
            BlackCard blackCardScript = blackCard.GetComponent<BlackCard>();
            if (!blackCardScript.IsSelected())
                continue;

            return blackCard.GetSiblingIndex();
        }
        return 0;
    }

    public IEnumerator StartNewRoundAfterBlackCardIsVoted()
    {
        while (true)
        {
            bool everyoneHasVoted = Common.IsRoomPropertyTrue("BlackCardVoteCurrentlyOn") && scoreboard.AreEveryoneReady() && !blackCardVoted;
            if (everyoneHasVoted)
                break;
            yield return null;
        }

        BlackCard blackCardWithMostVotes = cardManager.SelectBlackCardByRandomAccordingToVotes();
        string blackCardTextWithMostVotes = blackCardWithMostVotes.GetText();

        logManager.NewLogForAll("\"" + blackCardTextWithMostVotes + "\"" + " won the vote.");
        photonView.RPC("SelectBlackCardWhichWon", RpcTarget.All, blackCardWithMostVotes.transform.GetSiblingIndex());

        roundManager.AreAnswersCreated = false;
        blackCardVoted = true;
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable { { "BlackCardVoteCurrentlyOn", false } });

        yield return new WaitForSeconds(3);

        photonView.RPC("SetBlackCardVoteViewActive", RpcTarget.All, false);
        photonView.RPC("DestroyBlackCards", RpcTarget.All);
        roundManager.ResetSomePlayerValues();

        string blackCardText = blackCardTextWithMostVotes;
        roundManager.SetUpBlackCardAndSelectNewSelector(blackCardText);

        blackCardVoted = false;

        yield return new WaitForSeconds(.5f);

        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable { { "IsRoundInProgress", true } });

        yield return new WaitForSeconds(4f);

        roundManager.IsVoteOver = false;
    }

    public int[] GetBlackCardVotes()
    {
        if ((int[])PhotonNetwork.CurrentRoom.CustomProperties["BlackCardVotes"] == null)
        {
            int[] votes = new int[(int)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardAmount"]];

            Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
            {
                { "BlackCardVotes",  votes }
            });
            return new int[(int)PhotonNetwork.CurrentRoom.CustomProperties["BlackCardAmount"]];
        }
        return (int[])PhotonNetwork.CurrentRoom.CustomProperties["BlackCardVotes"];
    }

    public Transform GetBlackCardVoteView()
    {
        return blackCardVoteView;
    }

    [PunRPC]
    void IncreaseVote(int blackCardIndex)
    {
        blackCardVoteView.GetChild(blackCardIndex).GetComponent<BlackCard>().IncreaseVote();
        cardManager.SetBlackCardVotes();
    }
}
