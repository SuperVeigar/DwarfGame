using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static GameManager m_instance;
    public static GameManager instance
    {
        get
        {
            if(m_instance == null )
            {
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    public bool m_isObserving { get; private set; }
    public bool m_isGameOver { get; private set; }
    public bool m_isPaused { get; private set; }
    public GameObject m_playerPrefab;
    public GameObject[] m_startposArray;
    public GameObject m_myCharacter { get; private set; }
    public GameObject[] m_ruinedTowerArray;
    public AudioClip m_buffHornSound;

    private int m_myRank;
    private int m_totalPlayerCount;
    private int m_currentPlayerCount;
    private float m_buffDelayTime = 8f;
    private float m_buffElapsedTime = 0f;
    private List<GameObject> m_playerList;
    private SaveData m_loadData;
    private GameObject m_playerToObserve;
    
    
    // Start is called before the first frame update
    void Start()
    {
        photonView.RPC("InitGame", RpcTarget.All, PhotonNetwork.PlayerList.Length);
        if (PhotonNetwork.IsMasterClient)
        {
            m_loadData = GetComponent<DataManagement>().LoadJson();
            for(int i = 0; i<m_loadData.m_dataSize; i++)
            {
                photonView.RPC("InstantiatePlayer", RpcTarget.All, i, m_loadData.m_userID[i], m_loadData.m_nickname[i], m_loadData.m_playerColor[i], m_loadData.m_playerWeapon[i]);
            }
            m_totalPlayerCount = m_loadData.m_dataSize;
            m_currentPlayerCount = m_totalPlayerCount;
        }                   
        CommonUIManager_DontDest.instance.FadeIn();
        StartCoroutine(CheckFadeIn());
        CommonSoundManager_DontDest.instance.PlayBattleBGM();
        m_playerList = new List<GameObject>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {        
        CheckLeaveGame();

        if (PhotonNetwork.IsMasterClient)
        {
            CheckBuffTime();
        }

        if(m_isObserving &&
            m_myCharacter != null &&
            m_myCharacter.GetComponent<PlayerInput>().GetSwitchingCamKey())
        {
            FindAnotherCamToObserve();
        }

#if UNITY_EDITOR
        SetCursor();
#endif
    }

    #region PUN
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_currentPlayerCount);
        }
        else
        {
            m_currentPlayerCount = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void InstantiatePlayer(int num, string userID, string nickname, PlayerColor color, PlayerWeapon weapon)
    {
        if (PhotonNetwork.LocalPlayer.UserId == userID)
        {
            m_myCharacter = PhotonNetwork.Instantiate(m_playerPrefab.name, m_startposArray[num].transform.position, m_startposArray[num].transform.rotation);
            m_myCharacter.GetPhotonView().RPC("ChangeSkinOnNetwork", RpcTarget.All, color);
            m_myCharacter.GetPhotonView().RPC("SetWeapon", RpcTarget.All, weapon);
            m_myCharacter.GetPhotonView().RPC("SetName", RpcTarget.All, nickname);
            m_myCharacter.GetPhotonView().RPC("SetSkinBuff", RpcTarget.All, color);
            m_myCharacter.GetPhotonView().RPC("SetWeaponOption", RpcTarget.All, weapon);
            m_myCharacter.GetPhotonView().RPC("SetPlayerNum", RpcTarget.All, num);
            UIManager.instance.SetMyCharacter(m_myCharacter);
        }
    }
    [PunRPC]
    void PlayBuffHornSound()
    {
        if (!CommonSoundManager_DontDest.instance.isMute) GetComponent<AudioSource>().PlayOneShot(m_buffHornSound);
    }
    [PunRPC]
    void InformPlayerDeadToMaster(string nickname)
    {
        photonView.RPC("SetResultTextName", RpcTarget.All, m_currentPlayerCount, nickname);
        m_currentPlayerCount--;    
        
        if(m_currentPlayerCount == 1)
        {
            photonView.RPC("Call1stResult", RpcTarget.All);
        }
    }
    [PunRPC]
    void Call1stResult()
    {
        if(!m_isGameOver)
        {
            GameOver(true);
        }
    }
    [PunRPC]
    void SetResultTextName(int rank, string name)
    {
        UIManager.instance.SetResultTextName(rank, name);

        if(!m_isPaused)
        {
            if (m_myRank > 2)
            {
                UIManager.instance.PopupResult(false);
            }
            if (rank == 1)
            {
                UIManager.instance.PopupResult(true);
            }
        }        
    }
    [PunRPC]
    void InitGame(int totalPlayerCount)
    {
        m_totalPlayerCount = totalPlayerCount;
        UIManager.instance.SetResultTextPos(m_totalPlayerCount);
    }

    public override void OnLeftRoom()
    {        
        CommonUIManager_DontDest.instance.FadeOut();
        StartCoroutine(CheckFadedOut());
    }
    #endregion PUN

    #region External Function
    public void GameOver(bool isWin = false)
    {
        m_isGameOver = true;
        if (isWin) m_myRank = 1;
        else m_myRank = m_currentPlayerCount;
        UIManager.instance.SetWinDefeatText(m_myRank);
        photonView.RPC("InformPlayerDeadToMaster", RpcTarget.MasterClient, m_myCharacter.GetComponent<PlayerName>().m_name.GetComponent<TextMesh>().text);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void LeaveThisGame()
    {
        if (!PhotonNetwork.InRoom) return;

        GameOver();

        PhotonNetwork.LeaveRoom();        
        CommonSoundManager_DontDest.instance.StopSound();
    }
    public void ObserveThisGame()
    {
        m_isObserving = true;
        UIManager.instance.CloseResult();
        UIManager.instance.ShowInfoForObserver();
        FindAnotherCamToObserve();
    }
    public void FindPlayersToObserve()
    {
        PlayerStatus[] playerStatusArray = FindObjectsOfType<PlayerStatus>();
        for (int i = 0; i<playerStatusArray.Length; i++)
        {
            GameObject player = playerStatusArray[i].gameObject;

            if (player != null)
            {
                m_playerList.Add(player);
            }
        }

        Debug.Log("FindPlayersToObserve / m_playerList : " + m_playerList.Count);
    }

    public void RemovePlayerFromTheListToObserve(GameObject player)
    {
        m_playerList.Remove(player);

        Debug.Log("RemovePlayerInTheListToObserve / m_playerList : " + m_playerList.Count);
    }

    public void PauseGame()
    {
        m_isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ResumeGame()
    {
        m_isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void CancelExitGame()
    {
        UIManager.instance.CloseExitWindow();
        ResumeGame();
    }
    #endregion External Function

    #region Inner Function
    IEnumerator CheckFadeIn()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        if(CommonUIManager_DontDest.instance.IsFadedIn())
        {
            m_myCharacter.GetPhotonView().RPC("LoadWeaponOnNetwork", RpcTarget.All);
        }
        else
        {
            StartCoroutine(CheckFadeIn());
        }
    }
    IEnumerator CheckFadedOut()
    {
        yield return new WaitForSeconds(0.2f);

        if (CommonUIManager_DontDest.instance.IsFadedOut())
        {
            SceneManager.LoadScene("02_MultiLobbyScene");
            CommonSoundManager_DontDest.instance.PlayMainBGM();
        }
        else
        {
            StartCoroutine(CheckFadedOut());
        }
    }
    void CheckBuffTime()
    {
        m_buffElapsedTime += Time.deltaTime;

        if(m_buffElapsedTime >= m_buffDelayTime)
        {
            for(int i = 0; i < m_ruinedTowerArray.Length; i++)
            {
                Buff buffNum = (Buff)Random.Range((int)(Buff.start_none + 1), (int)Buff.end);

                m_ruinedTowerArray[i].GetPhotonView().RPC("SpawnBuff", RpcTarget.All, buffNum);
                photonView.RPC("PlayBuffHornSound", RpcTarget.All);
            }           

            m_buffElapsedTime = 0f;
        }
    }    
    void FindAnotherCamToObserve()
    {
        for (int i = 0; i < m_playerList.Count; i++)
        {
            if (m_playerList[i].GetComponent<PlayerStatus>().m_isDead)
            {
                RemovePlayerFromTheListToObserve(m_playerList[i]);
            }
        }

        if (m_playerToObserve == null)
        {
            for(int i = 0; i<m_playerList.Count; i++)
            {
                if(!m_playerList[i].GetComponent<PlayerStatus>().m_isDead) m_playerToObserve = m_playerList[i];
            }            
        }
        else
        {
            int playerIndex = m_playerList.FindIndex(currentPlayer => currentPlayer.GetComponent<PlayerStatus>().m_playerNum == m_playerToObserve.GetComponent<PlayerStatus>().m_playerNum);

            playerIndex++;

            if(playerIndex >= m_playerList.Count)
            {
                playerIndex = 0;
            }

            if(m_playerList[playerIndex] != null) m_playerToObserve = m_playerList[playerIndex];
        }

        if (m_playerToObserve != null) m_playerToObserve.GetComponent<CameraSetup>().ObserveOthers();
    }
    void CheckLeaveGame()
    {
        if(m_myCharacter != null &&
            m_myCharacter.GetComponent<PlayerInput>().GetExitKey())
        {
            if(!m_isPaused)
            {                
                UIManager.instance.PopupExitWindow();
                PauseGame();
            }
            else
            {
                CancelExitGame();
            }
        }
    }

    void SetCursor()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    #endregion Inner Function
}
