using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MultiRoomSceneManager : MonoBehaviourPunCallbacks
{
    private static MultiRoomSceneManager m_instance;
    public static MultiRoomSceneManager instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<MultiRoomSceneManager>();
            }
            return m_instance;
        }
    }
    public List<GameObject> m_charboxList = new List<GameObject>();
    public Button m_startBtn;
    public Button m_leaveBtn;
    public GameObject m_playerInfoPrefab;
    public GameObject m_loadingObject;

    private bool isQuit = false;
    private bool m_isLoadingGame = false;
    private List<PlayerInfoInRoom> m_playerInfoList = new List<PlayerInfoInRoom>();
    private PlayerInfoInRoom m_myPlayerInfo;
       

    // Start is called before the first frame update
    // 클라이언트에서 한 번 실행.
    void Start()
    {
        CommonUIManager_DontDest.instance.FadeIn();
        InitializeMyPlayerInfo();
        StartCoroutine(UpdatePlayerInfoList());
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.InRoom)
        {
            UpdateCharBoxList();
            CheckEscapeKey();

            if(!m_isLoadingGame)
            {
                CheckStartGame();
            }
        }
                
        DebugLogPlayerInfo();
    }

    #region Call from outside
    public void SetStartOrReady()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            bool startGame = false;

            if (PhotonNetwork.PlayerList.Length == m_playerInfoList.Count)
            {
                startGame = true;

                for (int i = 0; i < m_playerInfoList.Count; i++)
                {
                    if (!m_playerInfoList[i].isHost &&
                        !m_playerInfoList[i].isReady)
                    {
                        startGame = false;                        
                    }
                }

                if (startGame)
                {
                    m_myPlayerInfo.isReady = true;
                    photonView.RPC("LoadGame", RpcTarget.All);
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }
        }
        else
        {
            m_myPlayerInfo.isReady = !m_myPlayerInfo.isReady;
        }        
    }
    public bool IsLoadingGame()
    {
        return m_isLoadingGame;
    }
    #endregion Call from outside


    #region Photon Network Callbacks
    public void LeaveRoom()
    {
        isQuit = true;
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }        
    }
    public override void OnLeftRoom()
    {
        //Debug.Log("OnLeftRoom");
        CommonUIManager_DontDest.instance.FadeOut();
        StartCoroutine(CheckFadedOut());
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("OnPlayerEnteredRoom");
        StopCoroutine(UpdatePlayerInfoList());
        StartCoroutine(UpdatePlayerInfoList());
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("OnPlayerLeftRoom");
        SetHost();
        StopCoroutine(UpdatePlayerInfoList());
        StartCoroutine(UpdatePlayerInfoList());        
    }
    #endregion

    #region PUN
    [PunRPC]
    public void LoadGame()
    {
        SavePlayerInfos();

        m_isLoadingGame = true;

        for(int i = 0; i< m_charboxList.Count; i++)
        {
            m_charboxList[i].GetComponent<PlayerSelection>().SetActiveFalse();                    
        }
        
        m_startBtn.interactable = false;
        m_leaveBtn.interactable = false;

        m_loadingObject.GetComponentInChildren<Text>().text = "LOAD GAME";
        m_loadingObject.SetActive(true);

        StartCoroutine(LoadGameStep1());
        StartCoroutine(LoadGameStep2());
        StartCoroutine(LoadGameStep3());
        StartCoroutine(LoadGameStep4());

        CommonSoundManager_DontDest.instance.FadeoutMainBGM();
        CommonUIManager_DontDest.instance.PlayBattleStartHornSound();
    }
    #endregion PUN


    #region Inner Function   
    void CheckStartGame()
    {
        
    }
    IEnumerator CheckFadedOut()
    {
        yield return new WaitForSeconds(0.2f);

        if (CommonUIManager_DontDest.instance.IsFadedOut())
        {
            if (isQuit)
            {
                SceneManager.LoadScene("02_MultiLobbyScene");
            }
            else if(m_isLoadingGame)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.LoadLevel("05_PlayScene_2");
                }                
            }
        }
        else
        {
            StartCoroutine(CheckFadedOut());
        }
    }
    void CheckEscapeKey()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveRoom();
        }        
    }
    IEnumerator UpdatePlayerInfoList()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        m_playerInfoList.Clear();
        if (PhotonNetwork.PlayerList.Length > m_playerInfoList.Count)
        {
            PlayerInfoInRoom[] playerInfoList = FindObjectsOfType<PlayerInfoInRoom>();
            for (int i = 0; i < playerInfoList.Length; i++)
            {
                if (!m_playerInfoList.Exists(x=>x.userID == playerInfoList[i].userID) && 
                    playerInfoList[i].IsFinishedInitialization)
                {
                    m_playerInfoList.Add(playerInfoList[i]);
                }                
            }
            if (PhotonNetwork.PlayerList.Length > m_playerInfoList.Count)
            {
                StartCoroutine(UpdatePlayerInfoList());
            }
            else
            {
                //DebugLogPlayerUserID();
            }
        }              
    }
    void UpdateCharBoxList()
    {
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            for (int k = 0; k < m_playerInfoList.Count; k++)
            {                
                if (PhotonNetwork.PlayerList[i].UserId == m_playerInfoList[k].userID)
                {
                    PlayerSelection playerSelection = m_charboxList[i].GetComponent<PlayerSelection>();
                    playerSelection.m_playerInfo = m_playerInfoList[k];
                    m_charboxList[i].SetActive(true);
                }
            }
        }

        for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < 4; i++)
        {
            m_charboxList[i].SetActive(false);
        }
    }
    void InitializeMyPlayerInfo()
    {
        GameObject playerInfo = PhotonNetwork.Instantiate(m_playerInfoPrefab.name, Vector3.zero, Quaternion.identity);
        m_myPlayerInfo = playerInfo.GetComponent<PlayerInfoInRoom>();
        m_myPlayerInfo.nickname = PhotonNetwork.NickName;
        m_myPlayerInfo.userID = PhotonNetwork.LocalPlayer.UserId;
        SetHost();
        m_myPlayerInfo.IsFinishedInitialization = true;
    }
    void SetHost()
    {
        m_myPlayerInfo.isHost = PhotonNetwork.IsMasterClient;
        if (m_myPlayerInfo.isHost)
        {
            m_startBtn.GetComponentInChildren<Text>().text = "START";
        }
        else
        {
            m_startBtn.GetComponentInChildren<Text>().text = "READY";
        }
    }
    IEnumerator LoadGameStep1()
    {
        yield return new WaitForSecondsRealtime(1f);
        m_loadingObject.GetComponentInChildren<Text>().text = "LOAD GAME .";
    }
    IEnumerator LoadGameStep2()
    {
        yield return new WaitForSecondsRealtime(2f);
        m_loadingObject.GetComponentInChildren<Text>().text = "LOAD GAME . .";
    }
    IEnumerator LoadGameStep3()
    {
        yield return new WaitForSecondsRealtime(3f);
        m_loadingObject.GetComponentInChildren<Text>().text = "LOAD GAME . . .";
    }
    IEnumerator LoadGameStep4()
    {
        yield return new WaitForSecondsRealtime(4f);
        CommonUIManager_DontDest.instance.FadeOut();
        StartCoroutine(CheckFadedOut());
    }
    void SavePlayerInfos()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            PlayerInfoInRoom playerInfo = m_charboxList[i].GetComponent<PlayerSelection>().m_playerInfo;
            if (playerInfo != null)
            {
                GetComponent<DataManagement>().AddJson(i, playerInfo.userID, playerInfo.nickname, playerInfo.playerColor, playerInfo.playerWeapon);
            }
        }
        GetComponent<DataManagement>().SaveJson();
    }
    #endregion

    #region For Test
    void DebugLogPlayerInfo()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            for(int i = 0; i< PhotonNetwork.PlayerList.Length; i++)
            {
                Debug.Log("Player List : " + PhotonNetwork.PlayerList[i].NickName);
            }

            for(int i = 0; i< m_playerInfoList.Count; i++)
            {
                Debug.Log("Player Info List : " + m_playerInfoList[i].isHost + " " + m_playerInfoList[i].nickname + " " + m_playerInfoList[i].userID + " "  + m_playerInfoList[i].playerColor + " " + m_playerInfoList[i].playerWeapon);
            }
        }
    }
    void DebugLogPlayerUserID()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.PublishUserId);
        for(int i = 0; i< PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            Debug.Log("PhotonNetwork.PlayerList[" + i + "] : " + PhotonNetwork.PlayerList[i].UserId);
        }
        for (int i = 0; i < m_playerInfoList.Count; i++)
        {
            Debug.Log("m_playerInfoList[" + i + "] : " + m_playerInfoList[i].userID);
        }
    }
    #endregion

   
}
