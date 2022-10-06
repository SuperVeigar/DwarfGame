using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerSelection : MonoBehaviour
{
    public Text m_nicknameText;
    public Text m_colorText;
    public Text m_weaponText;
    public Image m_readyImage;
    public Image m_hostImage;
    public Image[] m_strGage;
    public Image[] m_vitGage;
    public Image[] m_agiGage;
    public Image[] m_sprGage;
    public Button m_colorLeft;
    public Button m_colorRight;
    public Button m_weaponLeft;
    public Button m_weaponRight;
    public GameObject m_character;
    public PlayerInfoInRoom m_playerInfo;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (m_playerInfo != null &&
            !MultiRoomSceneManager.instance.IsLoadingGame())
        {
            UpdatePlayerInfo();            
        }
        
    }

    void UpdatePlayerInfo()
    {       
        m_nicknameText.text = m_playerInfo.nickname;

        m_readyImage.gameObject.SetActive(m_playerInfo.isReady);
        m_hostImage.gameObject.SetActive(m_playerInfo.isHost);

        m_colorLeft.gameObject.SetActive(IsMine());
        m_colorRight.gameObject.SetActive(IsMine());
        m_weaponLeft.gameObject.SetActive(IsMine());
        m_weaponRight.gameObject.SetActive(IsMine());

        UpdateColor();
        UpdateWeapon();
        UpdateStat();
    }
    public void SetActiveFalse()
    {
        m_readyImage.gameObject.SetActive(true);
        m_colorLeft.gameObject.SetActive(false);
        m_colorRight.gameObject.SetActive(false);
        m_weaponLeft.gameObject.SetActive(false);
        m_weaponRight.gameObject.SetActive(false);
    }

    public void ChangeColorToLeft()
    {
        m_playerInfo.playerColor -= 1;        
    }
    public void ChangeColorToRight()
    {
        m_playerInfo.playerColor += 1;
    }
    public void ChangWeaponToLeft()
    {
        m_playerInfo.playerWeapon -= 1;
        
    }
    public void ChangWeaponToRight()
    {
        m_playerInfo.playerWeapon += 1;
    }   
    void UpdateColor()
    {
        if (m_playerInfo.playerColor == PlayerColor.start)
        {
            m_playerInfo.playerColor = PlayerColor.end - 1;
        }
        else if (m_playerInfo.playerColor >= PlayerColor.end)
        {
            m_playerInfo.playerColor = PlayerColor.start + 1;
        }      

        switch (m_playerInfo.playerColor)
        {
            case PlayerColor.bronze:
                m_colorText.text = "SKIN : BRONZE";
                break;
            case PlayerColor.cobalt:
                m_colorText.text = "SKIN : COBALT";
                break;
            case PlayerColor.gold:
                m_colorText.text = "SKIN : GOLD";
                break;
            case PlayerColor.ruby:
                m_colorText.text = "SKIN : RUBY";
                break;
            case PlayerColor.saphire:
                m_colorText.text = "SKIN : SAPHIRE";
                break;
            case PlayerColor.veredian:
                m_colorText.text = "SKIN : VEREDIAN";
                break;
            default:
                m_colorText.text = "SKIN : BRONZE";
                break;
        }

        m_character.GetComponent<PlayerSkinChanger>().ChangeSkin(m_playerInfo.playerColor);
    }
    void UpdateWeapon()
    {
        if (m_playerInfo.playerWeapon == PlayerWeapon.start)
        {
            m_playerInfo.playerWeapon = PlayerWeapon.end - 1;
        }
        else if (m_playerInfo.playerWeapon >= PlayerWeapon.end)
        {
            m_playerInfo.playerWeapon = PlayerWeapon.start + 1;
        }

        switch (m_playerInfo.playerWeapon)
        {
            case PlayerWeapon.axe:
                m_weaponText.text = "WEAPON : AXE";
                break;
            case PlayerWeapon.dwarfAxe:
                m_weaponText.text = "WEAPON : \nDWARF AXE";
                break;
            case PlayerWeapon.dwarfMace:
                m_weaponText.text = "WEAPON : \nDWARF MACE";
                break;
            default:
                m_weaponText.text = "WEAPON : AXE";
                break;
        }

        m_character.GetComponent<PlayerWeaponChanger>().ChangeWeapon(m_playerInfo.playerWeapon);
    }
    void UpdateStat()
    {
        // str, vit, agi, spr
        int[] stat = new int[] { 3, 3, 3, 3 };

        switch (m_playerInfo.playerColor)
        {
            case PlayerColor.bronze:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetBronzeStat(i);
                }
                break;
            case PlayerColor.cobalt:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetCobaltStat(i);
                }
                break;
            case PlayerColor.gold:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetGoldStat(i);
                }
                break;
            case PlayerColor.ruby:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetRubyStat(i);
                }
                break;
            case PlayerColor.saphire:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetSaphireStat(i);
                }
                break;
            case PlayerColor.veredian:
                for (int i = 0; i < 4; i++)
                {
                    stat[i] += m_character.GetComponent<PlayerSkinChanger>().GetVeredianStat(i);
                }
                break;
        }
        switch (m_playerInfo.playerWeapon)
        {
            case PlayerWeapon.axe:
                stat[2] += 2;
                break;
            case PlayerWeapon.dwarfAxe:
                stat[0] += 1;
                stat[2] += 1;
                break;
            case PlayerWeapon.dwarfMace:
                stat[0] += 2;
                break;
        }

        for (int j = 0; j < stat[0]; j++)
        {
            m_strGage[j].enabled = true;
        }
        for (int k = stat[0]; k < 8; k++)
        {
            m_strGage[k].enabled = false;
        }

        for (int j = 0; j < stat[1]; j++)
        {
            m_vitGage[j].enabled = true;
        }
        for (int k = stat[1]; k < 8; k++)
        {
            m_vitGage[k].enabled = false;
        }

        for (int j = 0; j < stat[2]; j++)
        {
            m_agiGage[j].enabled = true;
        }
        for (int k = stat[2]; k < 8; k++)
        {
            m_agiGage[k].enabled = false;
        }               

        for (int j = 0; j < stat[3]; j++)
        {
            m_sprGage[j].enabled = true;
        }
        for (int k = stat[3]; k < 8; k++)
        {
            m_sprGage[k].enabled = false;
        }

    }
    bool IsMine()
    {
        return PhotonNetwork.LocalPlayer.UserId == m_playerInfo.userID;
    }
    void ResetValues()
    {
        m_playerInfo = null;
        m_nicknameText.text = "";
        m_colorText.text = "";
        m_weaponText.text = "";

        m_readyImage.gameObject.SetActive(false);
        m_hostImage.gameObject.SetActive(false);
        m_colorLeft.gameObject.SetActive(false);
        m_colorRight.gameObject.SetActive(false);
        m_weaponLeft.gameObject.SetActive(false);
        m_weaponRight.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ResetValues();        
    }
}
