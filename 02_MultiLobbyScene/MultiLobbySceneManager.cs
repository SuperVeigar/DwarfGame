using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MultiLobbySceneManager : MonoBehaviourPunCallbacks
{
    private static MultiLobbySceneManager m_instance;
    public static MultiLobbySceneManager instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<MultiLobbySceneManager>();
            }
            return m_instance;
        }
    }

    public InputField m_roomNameInput;
    public InputField m_nickNameInput;
    public GameObject m_roomPrefab;
    public Transform m_scrollContent;
    public bool m_isJoing = false;

    private bool isCreating = false;
    private bool isQuit = false;
    private Dictionary<string, GameObject> m_roomDict = new Dictionary<string, GameObject>();
    private TypedLobby m_lobby;


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.JoinLobby();
        if(m_roomNameInput != null)
        {
            m_roomNameInput.characterLimit = 12;
            m_roomNameInput.onValueChanged.AddListener((word) => m_roomNameInput.text = Regex.Replace(word, @"[^0-9a-zA-Z°¡-ÆR]", ""));
        }
        CommonUIManager_DontDest.instance.FadeIn();
        //m_lobby = new TypedLobby("def", LobbyType.SqlLobby);
        //PhotonNetwork.GetCustomRoomList(m_lobby, "C0 = '1'");
    }

    // Update is called once per frame
    void Update()
    {
        MoveWithTabkey();
        CheckEscapeKey();
    }

    #region Function from outside
    public bool IsAnyButtonPushed()
    {
        if (m_isJoing ||
            isCreating ||
            isQuit)
            return true;

        return false;
    }
    public void BackToStartScene()
    {
        if (IsAnyButtonPushed()) return;

        PhotonNetwork.LeaveLobby();
        isQuit = true;
    }
    // Create Room
    public void CreateRoom()
    {
        if (IsAnyButtonPushed()) return;


        //Debug.Log("CreateRoom");        
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = 4;
        ro.PublishUserId = true;
        //ro.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "C0", "1" } };
        //ro.CustomRoomPropertiesForLobby = new string[] { "C0" };

        if (string.IsNullOrEmpty(m_nickNameInput.text))
        {
            m_nickNameInput.text = $"BABO_{Random.Range(1, 100):000}";
        }
        if (string.IsNullOrEmpty(m_roomNameInput.text))
        {
            m_roomNameInput.text = $"ROOM_{Random.Range(1, 100):000}";
        }

        PhotonNetwork.NickName = m_nickNameInput.text;
        PhotonNetwork.CreateRoom(m_roomNameInput.text, ro);
        isCreating = true;
        //PhotonNetwork.CreateRoom(m_roomNameInput.text, ro, m_lobby);
    }
    #endregion Function from outside

    #region Photon Network
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isQuit)
        {
            //Debug.Log("OnDisconnected and load startScene");
            FindObjectOfType<CommonUIManager_DontDest>().FadeOut();
            StartCoroutine(CheckFadedOut());
        }
        else
        {
            //Debug.Log("OnDisconnected and re-connect");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
    }
    public override void OnJoinedRoom()
    {
        FindObjectOfType<CommonUIManager_DontDest>().FadeOut();
        StartCoroutine(CheckFadedOut());
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Debug.Log("OnRoomListUpdate");
        GameObject tempRoom = null;
        foreach (var room in roomList)
        {
            if (room.RemovedFromList == true)
            {
                m_roomDict.TryGetValue(room.Name, out tempRoom);
                Destroy(tempRoom);
                m_roomDict.Remove(room.Name);
            }
            else
            {
                if(room.IsOpen)
                {
                    if (m_roomDict.ContainsKey(room.Name) == false)
                    {
                        GameObject _room = Instantiate(m_roomPrefab, m_scrollContent);
                        _room.GetComponent<RoomData>().roomInfo = room;
                        m_roomDict.Add(room.Name, _room);
                    }
                    else
                    {
                        m_roomDict.TryGetValue(room.Name, out tempRoom);
                        tempRoom.GetComponent<RoomData>().roomInfo = room;
                    }
                }
                else
                {
                    m_roomDict.TryGetValue(room.Name, out tempRoom);
                    Destroy(tempRoom);
                    m_roomDict.Remove(room.Name);
                }
            }
        }
    }
    public override void OnLeftLobby()
    {
        //Debug.Log("OnLeftLobby");
        PhotonNetwork.Disconnect();
    }
    #endregion Photon Network

    #region Inner Function
    IEnumerator CheckFadedOut()
    {
        yield return new WaitForSeconds(0.2f);

        if (CommonUIManager_DontDest.instance.IsFadedOut())
        {
            if (isQuit)
            {
                SceneManager.LoadScene("01_StartScene");
            }
            else if (isCreating)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.LoadLevel("03_MultiRoomScene");
                }
                StopAllCoroutines();
            }

        }
        else
        {
            StartCoroutine(CheckFadedOut());
        }
    }
    void MoveWithTabkey()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                next.Select();
            }
        }
    }
    void CheckEscapeKey()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            BackToStartScene();
        }
    }
    #endregion Inner Function

    #region Test Function

    #endregion




}
