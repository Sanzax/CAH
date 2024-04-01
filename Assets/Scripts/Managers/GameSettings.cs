using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown pointsToWinDropdown;
    [SerializeField] TMP_Dropdown blankCardsDropdown;
    [SerializeField] TMP_Dropdown blackCardVoteDropDown;
    [SerializeField] TMP_Dropdown gamemodeDropDown;
    [SerializeField] Toggle blackCardVoteToggle;

    public TMP_Dropdown GetPointsToWinDropDown()
    {
        return pointsToWinDropdown;
    }

    public TMP_Dropdown GetBlankCardsDropDown()
    {
        return blankCardsDropdown;
    }

    public void GameModeDropDownValueChanged()
    {
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "GameMode", gamemodeDropDown.options[gamemodeDropDown.value].text }
        });
    }

    public void BlackCardVoteToggleValueChanged()
    {
        blackCardVoteDropDown.interactable = blackCardVoteToggle.isOn;
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCardVoteAmount", Int32.Parse(blackCardVoteDropDown.options[blackCardVoteDropDown.value].text) },
            { "BlackCardVoteEnabled", blackCardVoteToggle.isOn }
        });
    }

    public void BlackCardDropDownValueChanged()
    {
        Common.SetCustomPropertiesOfRoom(PhotonNetwork.CurrentRoom, new ExitGames.Client.Photon.Hashtable
        {
            { "BlackCardVoteAmount", Int32.Parse(blackCardVoteDropDown.options[blackCardVoteDropDown.value].text) },
        });
    }

}
