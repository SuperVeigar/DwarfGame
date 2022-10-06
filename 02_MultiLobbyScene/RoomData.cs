using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomData : MonoBehaviour
{
    private Text m_roomNameText;
    private RoomInfo m_roomInfo;

    public InputField m_nicknameInput;

    public RoomInfo roomInfo
    {
        get
        {
            return m_roomInfo;
        }
        set
        {
            m_roomInfo = value;
            m_roomNameText.text = $"{m_roomInfo.Name + " "}({m_roomInfo.PlayerCount}/{m_roomInfo.MaxPlayers})";
            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnEnterRoom(m_roomInfo.Name));
        }
    }

    private void Awake()
    {
        m_roomNameText = GetComponentInChildren<Text>();
        m_nicknameInput = GameObject.Find("NicknameInputField").GetComponent<InputField>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnterRoom(string roomName)
    {        
        if(PhotonNetwork.InLobby &&
            !MultiLobbySceneManager.instance.IsAnyButtonPushed())
        {
            MultiLobbySceneManager.instance.m_isJoing = true;
            FindObjectOfType<CommonUIManager_DontDest>().FadeOut();
            StartCoroutine(CheckFadedOut( roomName));
        }

    }

    IEnumerator CheckFadedOut(string roomName)
    {
        yield return new WaitForSeconds(0.2f);

        if (CommonUIManager_DontDest.instance.IsFadedOut())
        {
            JoinRoom(roomName);
        }
        else
        {
            StartCoroutine(CheckFadedOut(roomName));
        }
    }

    void JoinRoom(string roomName)
    {
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = 4;
        ro.PublishUserId = true;
        //ro.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "C0", "1" } };
        //ro.CustomRoomPropertiesForLobby = new string[] { "C0" };

        if (string.IsNullOrEmpty(m_nicknameInput.text))
        {
            m_nicknameInput.text = $"DDONG_{Random.Range(1, 100):000}";
        }
        PhotonNetwork.NickName = m_nicknameInput.text;
        PhotonNetwork.JoinOrCreateRoom(roomName, ro, new TypedLobby("def", LobbyType.Default));
        //PhotonNetwork.JoinOrCreateRoom(roomName, ro, new TypedLobby("def", LobbyType.SqlLobby));
    }
}
