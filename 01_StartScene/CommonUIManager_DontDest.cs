using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CommonUIManager_DontDest : MonoBehaviour
{
    private static CommonUIManager_DontDest m_instance;
    public static CommonUIManager_DontDest instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<CommonUIManager_DontDest>();
            }
            return m_instance;
        }
    }

    public Image m_fadeImage;
    public Image m_muteIcon;
    public Text m_connectedText;
    public AudioClip m_battleStartHornSound;

    private bool m_isFirst = true;
    private Animator m_uiAnimator;



    private void Awake()
    {
        var obj = FindObjectsOfType<CommonUIManager_DontDest>();
        if (obj.Length == 1)
        {
          DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        m_uiAnimator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        SetConncetionInfom();
    }

    public void FadeOut()
    {
        m_isFirst = false;
        m_uiAnimator.SetTrigger("FadeOut");
    }
    public void FadeIn()
    {
        if(!m_isFirst)
        {
            m_uiAnimator.SetTrigger("FadeIn");
        }        
    }
    public bool IsFadedOut()
    {
        if(m_fadeImage != null &&
            m_fadeImage.color.a == 1)
        {
            return true;
        }

        return false;
    }
    public bool IsFadedIn()
    {
        if (m_fadeImage != null &&
            m_fadeImage.color.a == 0)
        {
            return true;
        }

        return false;
    }

    public void SetMuteIcon(bool isMute)
    {
        m_muteIcon.enabled = isMute;
    }

    public void PlayBattleStartHornSound()
    {
        GetComponent<AudioSource>().PlayOneShot(m_battleStartHornSound);
    }

    void SetConncetionInfom()
    {
        if (PhotonNetwork.IsConnected)
        {
            m_connectedText.gameObject.SetActive(true);
        }
        else
        {
            m_connectedText.gameObject.SetActive(false);
        }
    }

    void CheckPhotonNetwork()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("Is Connected : " + PhotonNetwork.IsConnected);
            Debug.Log("In Lobby : " + PhotonNetwork.InLobby);
            Debug.Log("In Romm : " + PhotonNetwork.InRoom);
        }
    }
}
