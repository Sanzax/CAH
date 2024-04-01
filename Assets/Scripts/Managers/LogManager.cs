using UnityEngine;
using TMPro;
using Photon.Pun;
using System;
using UnityEngine.UI;

public class LogManager : MonoBehaviourPun
{
    public GameObject messagePrefab;
    public GameObject log;
    public ScrollRect scrollRect;

    public TMP_InputField inputField;
    bool isPrevFocus;

    public Transform scorePanels;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            inputField.ActivateInputField();
        }
        if (isPrevFocus)
        {
            if (Input.GetKeyDown(KeyCode.Return) && inputField.text != "")
            {
                SendMsg();
            }
        }
        isPrevFocus = inputField.isFocused;
    }

    public void SendMsg()
    {
        foreach (Transform scorePanel in scorePanels)
        {
            PlayerScore playerScoreScript = scorePanel.GetComponent<PlayerScore>();
            if (playerScoreScript.photonView.Owner != PhotonNetwork.LocalPlayer)
                continue;

            NewLogForAll("<b><#" + ColorUtility.ToHtmlStringRGB(playerScoreScript.GetColor()) + ">"  + PhotonNetwork.LocalPlayer.NickName +  "</color>:</b> " + inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            break;
        }
    }

    [PunRPC]
    public void NewLog(string message)
    {
        DateTime currentTime = DateTime.Now;
        string hour = currentTime.Hour.ToString().Length == 1 ? "0" + currentTime.Hour : currentTime.Hour.ToString();
        string minute = currentTime.Minute.ToString().Length == 1 ? "0" + currentTime.Minute : currentTime.Minute.ToString();

        GameObject messageObject = Instantiate(messagePrefab, log.transform);
        messageObject.GetComponent<TextMeshProUGUI>().text = "<b>[" + hour + ":" + minute + "]</b> " + message;

        AutoScroll();
    }

    void AutoScroll()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(log.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void NewLogForAll(string log)
    {
        photonView.RPC("NewLog", RpcTarget.All, log);
    }
}