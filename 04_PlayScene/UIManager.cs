using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;
    public static UIManager instance
    {
        get
        {
            if(m_instance == null )
            {
                m_instance = FindObjectOfType<UIManager>();
            }
            return m_instance;
        }
    }

    public Image m_hpBar;
    public Image m_apBar;
    public Image[] m_graymaskArray;
    public Text m_InfoForObserver;
    public Text m_hpText;
    public Text m_winText;
    public Text m_defeatText;
    public Text m_1stText;
    public Text m_2ndText;
    public Text m_3rdText;
    public Text m_4thText;
    public Button m_leaveBtn;
    public Button m_observeBtn;
    public GameObject m_resultWindow;
    public GameObject m_exitWindow;


    private GameObject m_myCharacter;
    private PlayerStatus m_playerStatus;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }

    #region Internal Function
    void UpdateUI()
    {
        if(m_myCharacter != null)
        {
            UpdateHPAP();
            UpdateBuff();
        }        
    }

    void UpdateHPAP()
    {
        m_hpBar.fillAmount = m_playerStatus.GetHPRatio();
        m_apBar.fillAmount = m_playerStatus.GetAPRatio();
        m_hpText.text = (string)((int)m_playerStatus.m_currentHP + " / " + (int)m_playerStatus.m_maxHP);
    }

    void UpdateBuff()
    {
        if (m_playerStatus == null ||
            !m_playerStatus.m_isFinishedInitBuff) return;

        for(int i = 0; i < (int)Buff.end - 1; i++)
        {
            if (m_graymaskArray[i] == null) return;

            if(!m_playerStatus.IsHavingBuff(i) )
            {
                m_graymaskArray[i].fillAmount = 1f;
            }
            else if(m_playerStatus.IsHavingBuff(i) &&
                !m_playerStatus.IsUsingBuff(i))
            {
                m_graymaskArray[i].fillAmount = 0f;
            }
            else if(m_playerStatus.IsHavingBuff(i) &&
                m_playerStatus.IsUsingBuff(i))
            {
                m_graymaskArray[i].fillAmount = m_playerStatus.CalculateTimeRatio(i);
            }
        }
    }
    
    #endregion Internal Function

    #region External Function
    public void SetMyCharacter(GameObject myCharacter)
    {
        m_myCharacter = myCharacter;
        m_playerStatus = m_myCharacter.GetComponent<PlayerStatus>();
    }
    public void SetResultTextPos(int playerCount)
    {
        switch(playerCount)
        {
            case 4:
                m_1stText.rectTransform.localPosition = new Vector3(-280, 130, 0);
                m_2ndText.rectTransform.localPosition = new Vector3(-280, 0, 0);
                m_3rdText.rectTransform.localPosition = new Vector3(-280, -120, 0);
                m_4thText.rectTransform.localPosition = new Vector3(-280, -240, 0);
                break;
            case 3:
                m_1stText.rectTransform.localPosition = new Vector3(-280, 70, 0);
                m_2ndText.rectTransform.localPosition = new Vector3(-280, -70, 0);
                m_3rdText.rectTransform.localPosition = new Vector3(-280, -200, 0);
                m_4thText.gameObject.SetActive(false);
                break;
            case 2:
                m_1stText.rectTransform.localPosition = new Vector3(-280, 60, 0);
                m_2ndText.rectTransform.localPosition = new Vector3(-280, -90, 0);
                m_3rdText.gameObject.SetActive(false);
                m_4thText.gameObject.SetActive(false);
                break;
        }
    }
    public void SetResultTextName(int rank, string name)
    {
        switch (rank)
        {
            case 4:
                m_4thText.text = "4th   " + "<color=#64FF00>" + name + "</color>";
                break;
            case 3:
                m_3rdText.text = "3rd   " + "<color=#64FF00>" + name + "</color>";
                break;
            case 2:
                m_2ndText.text = "2nd   " + "<color=#64FF00>" + name + "</color>";
                break;
            case 1:
                m_1stText.text = "1st   " + "<color=#FF0074>" + name + "</color>";
                break;
        }
    }
    public void SetWinDefeatText(int myRank)
    {
        if(myRank == 1)
        {
            m_defeatText.enabled = false;
        }
        else
        {
            m_winText.enabled = false;
        }
    }
    public void PopupResult(bool isEnd)
    {
        if(isEnd)
        {
            m_observeBtn.gameObject.SetActive(false);
            m_leaveBtn.transform.localPosition = new Vector3(0, -385, 0);
        }

        m_resultWindow.SetActive(true);
    }
    public void CloseResult()
    {
        m_resultWindow.SetActive(false);
    }

    public void ShowInfoForObserver()
    {
        m_InfoForObserver.enabled = true;
    }

    public void PopupExitWindow()
    {
        m_exitWindow.gameObject.SetActive(true);
    }
    public void CloseExitWindow()
    {
        m_exitWindow.gameObject.SetActive(false);
    }
    #endregion External FUnction
}
