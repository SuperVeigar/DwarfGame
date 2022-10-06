using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonSoundManager_DontDest : MonoBehaviour
{
    private static CommonSoundManager_DontDest m_instance;
    public static CommonSoundManager_DontDest instance
    {
        get
        {
            if(m_instance == null )
            {
                m_instance = FindObjectOfType<CommonSoundManager_DontDest>();
            }
            return m_instance;
        }
    }

    public AudioClip m_mainBGM;
    public AudioClip m_battleBGM;
    public AudioClip m_mouseoverSound;
    public AudioClip m_buttonClickSound;
    public bool isMute { get; private set; }


    private bool isFadingoutMainBGM = false;
    
    private AudioSource m_audioSource;
    

    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        isMute = false;

        var obj = FindObjectsOfType<CommonSoundManager_DontDest>();
        if(obj.Length == 1)
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
        PlayMainBGM();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFadingoutMainBGM();
        SetMute();
    }

    void UpdateFadingoutMainBGM()
    {
        if (isFadingoutMainBGM)
        {
            m_audioSource.volume -= (3f * Time.deltaTime);

            if (m_audioSource.volume <= 0f)
            {
                isFadingoutMainBGM = false;
            }
        }
    }

    public void PlayMouseOverSound()
    {
        m_audioSource.PlayOneShot(m_mouseoverSound);
    }

    public void PlayButtonClickSound()
    {
        m_audioSource.PlayOneShot(m_buttonClickSound);
    }

    public void PlayMainBGM()
    {
        m_audioSource.clip = m_mainBGM;
        m_audioSource.volume = 1f;
        m_audioSource.Play();
    }

    public void StopSound()
    {
        m_audioSource.Stop();
    }

    public void FadeoutMainBGM()
    {
        isFadingoutMainBGM = true;
    }

    public void PlayBattleBGM()
    {
        isFadingoutMainBGM = false;
        m_audioSource.clip = m_battleBGM;
        m_audioSource.volume = 1f;
        m_audioSource.Play();
    }

    void SetMute()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            m_audioSource.enabled = isMute;
            isMute = !isMute;
            CommonUIManager_DontDest.instance.SetMuteIcon(isMute);
        }
    }
}
