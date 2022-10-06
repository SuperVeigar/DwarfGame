using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Windows;

public class StartSceneManager : MonoBehaviourPunCallbacks
{
    private static StartSceneManager m_instance;
    public static StartSceneManager instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<StartSceneManager>();
            }
            return m_instance;
        }
    }

    public bool m_isPopup { get; private set; }
    public Image m_howtoBoard;
    public Image m_connectingPopup;
    public Button m_singleBtn;
    public Button m_multiBtn;
    public Button m_howtoBtn;
    public Button m_exitBtn;
    [SerializeField] Texture2D m_cursorImg;

    private int m_screenX = 1920;
    private int m_screenY = 1080;
    private float scale = 0.4f;
    private string m_gameVersion = "0";


    private void Awake()
    {
        SetWindowAndCam();        
    }

    // Start is called before the first frame update
    void Start()
    {
        m_isPopup = false;
        PhotonNetwork.GameVersion = m_gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
        CommonUIManager_DontDest.instance.FadeIn();
        Cursor.SetCursor(m_cursorImg, Vector2.zero, CursorMode.ForceSoftware);
    }

    // Update is called once per frame
    void Update()
    {
        CheckEscapeKey();
    }

    #region Set Game Window Scale and Cam Position
    void SetWindowAndCam()
    {        
        m_screenX = Screen.currentResolution.width;
        m_screenY = Screen.currentResolution.height;
        m_screenX = (int)((float)m_screenX * scale);
        m_screenY = (int)((float)m_screenY * scale);
        Screen.SetResolution(m_screenX, m_screenY, false);
    }
    #endregion

    // Single Play Top //

    // Multi Play Top //
    public void ConnectMultiServer()
    {
        if (m_isPopup) return;

        if(m_connectingPopup!=null)
        {
            PhotonNetwork.ConnectUsingSettings();

            m_isPopup = true;
            m_connectingPopup.gameObject.SetActive(true);
            StartCoroutine(SetActiveMainButtons(false));            
        }        
    }
    IEnumerator CheckFadedOut()
    {
        yield return new WaitForSeconds(0.2f);

        if(CommonUIManager_DontDest.instance.IsFadedOut())
        {
            SceneManager.LoadScene("02_MultiLobbyScene");
            StopAllCoroutines();
        }
        else
        {
            StartCoroutine(CheckFadedOut());
        }
    }
    public override void OnConnectedToMaster()
    {
        CommonUIManager_DontDest.instance.FadeOut();
        StartCoroutine(CheckFadedOut());
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // How To Top //
    public void OpenPopupBoard()
    {
        if (m_howtoBoard != null)
        {
            m_howtoBoard.gameObject.SetActive(true);
            m_isPopup = true;
            StartCoroutine(SetActiveMainButtons(false));
        }
    }
    public void ClosePopupBoard()
    {
        if (m_howtoBoard != null)
        {
            m_howtoBoard.gameObject.SetActive(false);
            m_isPopup = false;
            StartCoroutine(SetActiveMainButtons(true));
        }
    }
    private IEnumerator SetActiveMainButtons(bool interactable)
    {
        yield return new WaitForSeconds(0.2f);

        m_singleBtn.interactable = interactable;
        m_multiBtn.interactable = interactable;
        m_howtoBtn.interactable = interactable;
        m_exitBtn.interactable = interactable;
    }

    // Exit Top //
    public void ExitGame()
    {
        if (m_isPopup) return;


#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif

    }
    void CheckEscapeKey()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }        
    }

}
