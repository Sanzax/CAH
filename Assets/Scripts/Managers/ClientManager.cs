using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ClientManager : MonoBehaviourPun
{
    [SerializeField] Transform answers;
    [SerializeField] Transform scoreboard;

    [SerializeField] Transform blackCardVoteView;

    [SerializeField] Button readyButton;
    [SerializeField] Button stopGameButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button createButton;
    [SerializeField] TMP_Dropdown pointsDropdown;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] TMP_InputField roomNameInput;


    const int MAX_PLAYER_NAME_LENGTH = 25;

    NetworkManager networkManager;
    LogManager logManager;
    CardManager cardManager;
    AnswerManager answerManager;
    GameObjectActivator gameObjectActivator;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        logManager = GetComponent<LogManager>();
        networkManager = GetComponent<NetworkManager>();
        cardManager = GetComponent<CardManager>();
        answerManager = GetComponent<AnswerManager>();
        gameObjectActivator = GetComponent<GameObjectActivator>();
    }

    void Start()
    {
        joinButton.interactable = false;
        createButton.interactable = false;
    }

    public void CreateRoomButtonPressed()
    {
        if (NameIsTooLong())
            return;

        if (DoesRoomExist())
        {
            logManager.NewLog("Room already exists.");
            return;
        }

        CreateRoom();
    }

    bool NameIsTooLong()
    {
        if(nameInput.text.Length > MAX_PLAYER_NAME_LENGTH)
        {
            logManager.NewLog("Name is too long.");
            return true;
        }
        return false;
    }

    void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            IsOpen = true,
            MaxPlayers = 10,
            CustomRoomPropertiesForLobby = new string[] { "PlayerNames", }
        };
        PhotonNetwork.CreateRoom(roomNameInput.text, options);
    }

    bool DoesRoomExist()
    {
        foreach (RoomInfo room in networkManager.GetRoomList())
        {
            if (room.Name == roomNameInput.text)
            {
                return true;
            }
        }
        return false;
    }

    public void JoinRoomButtonPressed()
    {
        if (NameIsTooLong())
            return;

        RoomInfo roomToJoin = FindRoomInfo();

        if (roomToJoin == null)
        {
            logManager.NewLog("Room called" + roomNameInput.text + " does not exist.");
            return;
        }

        if(!CheckIfNameIsAvailable(roomToJoin))
        {
            logManager.NewLog("Name is already in use.");
            return;
        }

        if(((string[][])roomToJoin.CustomProperties["PlayerData"]) != null)
        {
            if (((string[][])roomToJoin.CustomProperties["PlayerData"]).Length > 0)
            {
                logManager.NewLog(((string[][])roomToJoin.CustomProperties["PlayerData"]).Length.ToString());
                logManager.NewLog(((string[][])roomToJoin.CustomProperties["PlayerData"])[0][0]);
            }
        }

        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    bool CheckIfNameIsAvailable(RoomInfo roomToJoin)
    {
        string[] temp = (string[])roomToJoin.CustomProperties["PlayerNames"];
        if (temp == null)
            return true;

        foreach (string playerName in temp)
        {
            if (playerName == nameInput.text)
            {
                return false;
            }
        }
        return true;
    }

    RoomInfo FindRoomInfo()
    {
        foreach (RoomInfo room in networkManager.GetRoomList())
        {
            if (room.Name == roomNameInput.text)
            {
                return room;
            }
        }
        return null;
    }

    public void CheckIfPlayerNameAndRoomNameAreTyped()
    {
        if (nameInput.text.Length > 0 && roomNameInput.text.Length > 0)
        {
            createButton.interactable = true;
            joinButton.interactable = true;
        }
        else
        {
            createButton.interactable = false;
            joinButton.interactable = false;
        }
    }

    public void BackButtonPressed()
    {
        if(gameObjectActivator.IsMainMenuViewActive())
        {
            Application.Quit();
            return;
        }

        cardManager.ClearHand();
        cardManager.DeleteCardsOnSlots();
        answerManager.DeleteAnswers();
        gameObjectActivator. HideSlots();
        gameObjectActivator.SetInGameViewActive(false);
        gameObjectActivator.SetGameSettingsViewActive(false);
        gameObjectActivator.SetLobbyViewActive(false);
        gameObjectActivator.SetMainMenuActive(true);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
    }
}
