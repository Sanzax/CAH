using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameObjectActivator : MonoBehaviour
{
    [SerializeField] Transform slots;
    [SerializeField] Transform selectorView;
    [SerializeField] Transform mainMenuView;
    [SerializeField] Transform gameSettingsView;
    [SerializeField] Transform inGameView;
    [SerializeField] Transform lobbyView;
    [SerializeField] Transform blackCardVoteView;
    [SerializeField] Button readyButton;
    [SerializeField] Button gameStartButton;
    [SerializeField] Button stopGameButton;

    private void Start()
    {
        SetReadyButtonInteractability(false);
    }


    public void SetGameStartButtonInterActable(bool b)
    {
        gameStartButton.interactable = b;
    }

    public bool IsMainMenuViewActive()
    {
        return mainMenuView.gameObject.activeSelf;
    }

    [PunRPC]
    public void HideSlots()
    {
        foreach (Transform slotGroup in slots)
        {
            slotGroup.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void ShowSlot(int index)
    {
        slots.GetChild(index).gameObject.SetActive(true);
    }

    public Transform GetSlots()
    {
        return slots;
    }

    public Transform GetActiveSlotGroup()
    {
        foreach (Transform sg in slots)
        {
            if (sg.gameObject.activeSelf)
            {
                return sg;
            }
        }
        return null;
    }

    public void ChangeStopGameButtonInteractabality(bool b)
    {
        stopGameButton.interactable = b;
    }

    public void SetReadyButtonInteractability(bool b)
    {
        readyButton.interactable = b;
    }

    [PunRPC]
    public void SetSelectorViewActive(bool b)
    {
        selectorView.gameObject.SetActive(b);
    }

    public void SetGameSettingsViewActive(bool b)
    {
        gameSettingsView.gameObject.SetActive(b);
    }

    public void SetMainMenuActive(bool b)
    {
        mainMenuView.gameObject.SetActive(b);
    }

    public void SetInGameViewActive(bool b)
    {
        inGameView.gameObject.SetActive(b);
    }

    [PunRPC]
    public void SetBlackCardVoteViewActive(bool b)
    {
        blackCardVoteView.gameObject.SetActive(b);
    }

    [PunRPC]
    public void SetLobbyViewActive(bool b)
    {
        lobbyView.gameObject.SetActive(b);
    }
}
