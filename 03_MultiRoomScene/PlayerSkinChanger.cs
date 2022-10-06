using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 스킨 변경해주는 클래스
/// 각 스킨에 따른 버프 적용
/// </summary>
public class PlayerSkinChanger : MonoBehaviourPun
{
    // armor, body, cloak 순서
    public List<Material> m_bronzeMaterialList = new List<Material>();
    public List<Material> m_cobaltMaterialList = new List<Material>();
    public List<Material> m_goldMaterialList = new List<Material>();
    public List<Material> m_rubyMaterialList = new List<Material>();
    public List<Material> m_saphireMaterialList = new List<Material>();
    public List<Material> m_veredianMaterialList = new List<Material>();    

    public PlayerColor m_playerColor;
    public GameObject m_armorObj;
    public GameObject m_helmetObj;
    public GameObject m_bodyObj;
    public GameObject m_cloakObj;

    // str, vit, agi, spr
    private int[] m_bronzeStat = new int[] { 1, 2, 0, 0 };
    private int[] m_cobaltStat = new int[] { 0, 0, 2, 1 };
    private int[] m_goldStat = new int[] { 0, 3, 0, 0 };
    private int[] m_rubyStat = new int[] { 3, 0, 0, 0 };
    private int[] m_saphireStat = new int[] { 0, 0, 0, 3 };
    private int[] m_veredianStat = new int[] { 0, 0, 3, 0 };

    private Material[] m_armorMaterial;
    private Material[] m_helmetMaterial;
    private Material[] m_bodyMaterial;
    private Material[] m_cloakMaterial;

    private void Awake()
    {
        m_armorMaterial = m_armorObj.GetComponent<SkinnedMeshRenderer>().materials;
        m_helmetMaterial = m_helmetObj.GetComponent<SkinnedMeshRenderer>().materials;
        m_bodyMaterial = m_bodyObj.GetComponent<SkinnedMeshRenderer>().materials;
        m_cloakMaterial = m_cloakObj.GetComponent<SkinnedMeshRenderer>().materials;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSkin(PlayerColor playerColor)
    {
        switch (playerColor)
        {
            case PlayerColor.bronze:
                m_armorMaterial[0] = m_bronzeMaterialList[0];
                m_helmetMaterial[0] = m_bronzeMaterialList[0];
                m_bodyMaterial[0] = m_bronzeMaterialList[1];
                m_cloakMaterial[0] = m_bronzeMaterialList[2];
                break;
            case PlayerColor.cobalt:
                m_armorMaterial[0] = m_cobaltMaterialList[0];
                m_helmetMaterial[0] = m_cobaltMaterialList[0];
                m_bodyMaterial[0] = m_cobaltMaterialList[1];
                m_cloakMaterial[0] = m_cobaltMaterialList[2];
                break;
            case PlayerColor.gold:
                m_armorMaterial[0] = m_goldMaterialList[0];
                m_helmetMaterial[0] = m_goldMaterialList[0];
                m_bodyMaterial[0] = m_goldMaterialList[1];
                m_cloakMaterial[0] = m_goldMaterialList[2];
                break;
            case PlayerColor.ruby:
                m_armorMaterial[0] = m_rubyMaterialList[0];
                m_helmetMaterial[0] = m_rubyMaterialList[0];
                m_bodyMaterial[0] = m_rubyMaterialList[1];
                m_cloakMaterial[0] = m_rubyMaterialList[2];
                break;
            case PlayerColor.saphire:
                m_armorMaterial[0] = m_saphireMaterialList[0];
                m_helmetMaterial[0] = m_saphireMaterialList[0];
                m_bodyMaterial[0] = m_saphireMaterialList[1];
                m_cloakMaterial[0] = m_saphireMaterialList[2];
                break;
            case PlayerColor.veredian:
                m_armorMaterial[0] = m_veredianMaterialList[0];
                m_helmetMaterial[0] = m_veredianMaterialList[0];
                m_bodyMaterial[0] = m_veredianMaterialList[1];
                m_cloakMaterial[0] = m_veredianMaterialList[2];
                break;
            default:
                m_armorMaterial[0] = m_bronzeMaterialList[0];
                m_helmetMaterial[0] = m_bronzeMaterialList[0];
                m_bodyMaterial[0] = m_bronzeMaterialList[1];
                m_cloakMaterial[0] = m_bronzeMaterialList[2];
                break;
        }
        m_armorObj.GetComponent<SkinnedMeshRenderer>().materials = m_armorMaterial;
        m_helmetObj.GetComponent<SkinnedMeshRenderer>().materials = m_helmetMaterial;
        m_bodyObj.GetComponent<SkinnedMeshRenderer>().materials = m_bodyMaterial;
        m_cloakObj.GetComponent<SkinnedMeshRenderer>().materials = m_cloakMaterial;
    }

    public int GetBronzeStat(int num)
    {
        return m_bronzeStat[num];
    }
    public int GetCobaltStat(int num)
    {
        return m_cobaltStat[num];
    }
    public int GetGoldStat(int num)
    {
        return m_goldStat[num];
    }
    public int GetRubyStat(int num)
    {
        return m_rubyStat[num];
    }
    public int GetSaphireStat(int num)
    {
        return m_saphireStat[num];
    }
    public int GetVeredianStat(int num)
    {
        return m_veredianStat[num];
    }

    [PunRPC]
    public void ChangeSkinOnNetwork(PlayerColor playerColor)
    {
        m_playerColor = playerColor;
        ChangeSkin(playerColor);
    }
}
