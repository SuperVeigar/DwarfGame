using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum CC { Start = 0, Stun, Knockback, End}

/// <summary>
/// 플레이어의 생존, 공격, 기술, 속도 등과 같이 계산에 사용되는 각종 수치를 관리하는 클래스
/// MasterClient에서 각종 계산 완료. m_isDead도 검사. Animation은 
/// </summary>
public class PlayerStatus : MonoBehaviourPun
{
    public GameObject[] m_buffObjectArray;
    

    public int m_playerNum { get; private set; }
    public float m_maxHP { get; private set; }
    public float m_currentHP { get; private set; }
    public bool m_isFinishedInitBuff { get; private set; }
    public bool m_isDead { get; private set; }
    private float m_maxAP;
    private float m_currentAP;
    private float m_amountChargingAP;
    private float m_damageAttack1;
    private float m_damageAttack2;
    private float m_damageAttack3;
    private float m_damageAttackR;    
    private float m_str;
    private float m_vit;
    private float m_agi;
    private float m_spr;
    private float m_weaponAttack;
    private float m_moveSpeedFactor;
    private float m_animSpeed;
    private float m_stunDuration = 0f;
    private float m_stunEndTime = 0f;        
    private PlayerMove m_playerMove;
    private BuffEntity[] m_buffArray; // att, def, heal, spd


    // Start is called before the first frame update
    void Start()
    {
        m_playerMove = GetComponent<PlayerMove>();        
        InitBuffArray();        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDead ||
            !photonView.IsMine) return;
        
        UpdateAP();
        CheckStunEndTime();
        UpdateBuff();

#if UNITY_EDITOR
        DecreaseHP();
#endif 
    }

    private void FixedUpdate()
    {
        if (m_isDead ||
            !photonView.IsMine) return;
    }
   
    public void GetHit(float damage, PlayerState attackType, Vector3 hitPos, float ccTime = 0f)
    {
        if (m_isDead) return;

        if(PhotonNetwork.IsMasterClient)
        {
            float damageUpopDefence = damage;
            if(m_buffArray.Length > (int)Buff.defence &&
                m_buffArray[(int)Buff.defence - 1].isUsing)
            {
                damageUpopDefence -= m_buffArray[(int)Buff.defence - 1].buffAmount1;
            }
            
            if(damageUpopDefence < 0f)
            {
                damageUpopDefence = 0f;
            }
            m_currentHP -= damageUpopDefence;

            if (m_currentHP <= 0f)
            {
                m_currentHP = 0f;
                m_isDead = true;
            }

            photonView.RPC("ApplyHP", RpcTarget.All, m_currentHP, m_isDead, attackType, hitPos, ccTime);
        }        
    }

    #region Photon Network
    [PunRPC]
    public void SetPlayerNum(int num)
    {
        m_playerNum = num;
    }
    [PunRPC]
    public void ApplyHP(float hp)
    {
        m_currentHP = hp;
    }
    [PunRPC]
    public void ApplyHP(float hp, bool isDead, PlayerState attackType, Vector3 hitPos, float ccTime)
    {
        m_currentHP = hp;        
        m_isDead = isDead;
        m_stunDuration = ccTime;

        if (photonView.IsMine)
        {
            if (m_isDead)
            {
                Die();
            }
            else
            {
                if (m_stunDuration > 1f)
                {
                    m_stunEndTime = Time.time + m_stunDuration;
                    m_stunDuration = 0f;
                }
                m_playerMove.SetDamageAnimation(attackType, hitPos);
            }

        }
    }
    [PunRPC]
    public void SetSkinBuff(PlayerColor playerColor)
    {
        ResetValues(); 

        switch (playerColor)
        {
            case PlayerColor.bronze:
                m_str = GetComponent<PlayerSkinChanger>().GetBronzeStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetBronzeStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetBronzeStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetBronzeStat(3);
                break;
            case PlayerColor.cobalt:
                m_str = GetComponent<PlayerSkinChanger>().GetCobaltStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetCobaltStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetCobaltStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetCobaltStat(3);
                break;
            case PlayerColor.gold:
                m_str = GetComponent<PlayerSkinChanger>().GetGoldStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetGoldStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetGoldStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetGoldStat(3);
                break;
            case PlayerColor.ruby:
                m_str = GetComponent<PlayerSkinChanger>().GetRubyStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetRubyStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetRubyStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetRubyStat(3);
                break;
            case PlayerColor.saphire:
                m_str = GetComponent<PlayerSkinChanger>().GetSaphireStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetSaphireStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetSaphireStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetSaphireStat(3);
                break;
            case PlayerColor.veredian:
                m_str = GetComponent<PlayerSkinChanger>().GetVeredianStat(0);
                m_vit = GetComponent<PlayerSkinChanger>().GetVeredianStat(1);
                m_agi = GetComponent<PlayerSkinChanger>().GetVeredianStat(2);
                m_spr = GetComponent<PlayerSkinChanger>().GetVeredianStat(3);
                break;
        }

        m_damageAttack1 += m_str;
        m_damageAttack2 += m_str;
        m_damageAttack3 += m_str;
        m_damageAttackR += m_str;

        m_maxHP = m_maxHP + m_vit * 15f;
        m_currentHP = m_maxHP;

        m_moveSpeedFactor = m_moveSpeedFactor + m_agi * 0.04f;        
        m_animSpeed = m_animSpeed + m_agi * 0.04f;
        

        m_amountChargingAP = m_amountChargingAP + m_spr * 2f;
    }

    [PunRPC]
    public void SetWeaponOption(PlayerWeapon playerWeapon )
    {
        switch (playerWeapon)
        {
            case PlayerWeapon.axe:
                m_weaponAttack = 0;
                m_animSpeed += 0.15f;
                m_moveSpeedFactor += 0.04f;
                break;
            case PlayerWeapon.dwarfAxe:
                m_weaponAttack = 2;
                m_animSpeed += 0.07f;
                m_moveSpeedFactor += 0.02f;
                break;
            case PlayerWeapon.dwarfMace:
                m_weaponAttack = 4;
                break;
        }
        m_damageAttack1 += m_weaponAttack;
        m_damageAttack2 += m_weaponAttack;
        m_damageAttack3 += m_weaponAttack;
        m_damageAttackR += m_weaponAttack;

        GetComponent<PlayerMove>().SetMoveSpeed(m_moveSpeedFactor);
        GetComponent<Animator>().SetFloat("AnimationSpeed", m_animSpeed);
    }
    [PunRPC]
    public void ReceiveBuff(Buff buff, float buffTime, float buffAmount1, float buffAmount2)
    {
        if(buff > Buff.start_none &&
            buff < Buff.end)
        {
            m_buffArray[(int)buff - 1].isHaving = true;
            m_buffArray[(int)buff - 1].buffTime = buffTime;
            m_buffArray[(int)buff - 1].buffElapsedTime = buffTime;
            m_buffArray[(int)buff - 1].buffAmount1 = buffAmount1;
            m_buffArray[(int)buff - 1].buffAmount2 = buffAmount2;
        }
    }
    [PunRPC]
    public void UseBuff(int num)
    {
        m_buffArray[num].Use();

        if(num == (int)Buff.fast - 1)
        {
            SetSpeedBuff(true);
        }
    }
    public void SetSpeedBuff(bool isOn)
    {
        if(isOn)
        {
            GetComponent<PlayerMove>().SetMoveSpeed(m_moveSpeedFactor + m_buffArray[(int)Buff.fast - 1].buffAmount1);
            GetComponent<Animator>().SetFloat("AnimationSpeed", m_animSpeed + m_buffArray[(int)Buff.fast - 1].buffAmount2);
        }
        else
        {
            GetComponent<PlayerMove>().SetMoveSpeed(m_moveSpeedFactor);
            GetComponent<Animator>().SetFloat("AnimationSpeed", m_animSpeed);
        }
    }
    [PunRPC]
    public void ResetBuff(int num)
    {
        m_buffArray[num].Reset();

        if (num == (int)Buff.fast - 1)
        {
            SetSpeedBuff(false);
        }
    }
    #endregion   

    #region External Function
    public void GetAttackAndState(out PlayerState playerState, out float attack)
    {
        playerState = m_playerMove.m_playerState;
        switch (m_playerMove.m_playerState)
        {
            case PlayerState.Attack1:
                attack = m_damageAttack1;
                break;
            case PlayerState.Attack2:
                attack = m_damageAttack2;
                break;
            case PlayerState.Attack3:
                attack = m_damageAttack3;
                break;
            case PlayerState.AttackR_Axe:
            case PlayerState.AttackR_DwarfAxe:
            case PlayerState.AttackR_DwarfMace:
                attack = m_damageAttackR;
                break;
            default:
                attack = m_damageAttack1;
                break;
        }

        if (m_buffArray.Length > (int)Buff.attack &&
            m_buffArray[(int)Buff.attack - 1].isUsing)
        {
            attack += m_buffArray[(int)Buff.attack - 1].buffAmount1;
        }
    }
    public float GetHPRatio()
    {
        return ((int)(m_currentHP / m_maxHP * 100) * 0.01f);
    }
    public float GetAPRatio()
    {
        return ((int)(m_currentAP / m_maxAP * 100) * 0.01f);
    }
    public void UseAP(float ap)
    {
        m_currentAP -= ap;
        if (m_currentAP < 0)
        {
            m_currentAP = 0;
        }

    }
    public bool IsEnoughAP(float ap)
    {
        if (m_currentAP >= ap) return true;
        return false;
    }

    // Parameter num is enum Buff minus 1, which is number in array.
    public bool IsHavingBuff(int num)
    {
        if (m_buffArray.Length < num &&
            m_buffArray[num] == null) return false;

        return m_buffArray[num].isHaving;
    }
    public bool IsUsingBuff(int num)
    {
        if (m_buffArray.Length < num &&
            m_buffArray[num] == null) return false;

        return m_buffArray[num].isUsing;
    }
    public bool IsAvailableBuff(int num)
    {
        if (m_buffArray.Length < num &&
            m_buffArray[num] == null) return false;

        return m_buffArray[num].isHaving && !m_buffArray[num].isUsing;
    }
    public float CalculateTimeRatio(int num)
    {
        if (m_buffArray.Length < num ||
            m_buffArray[num].buffTime == 0f) return 0f;

        return 1f- m_buffArray[num].buffElapsedTime / m_buffArray[num].buffTime;
    }
    public bool IsDead()
    {
        return m_isDead;
    }
    #endregion External Function

    #region Internal Function
    void ResetValues()
    {
        m_maxHP = 100;
        m_currentHP = m_maxHP;
        m_maxAP = 100;
        m_currentAP = m_maxAP;
        m_amountChargingAP = 7f;
        m_damageAttack1 = 4;
        m_damageAttack2 = 7;
        m_damageAttack3 = 11;
        m_damageAttackR = 16;
        m_str = 0;
        m_vit = 0;
        m_agi = 0;
        m_spr = 0;
        m_weaponAttack = 0;
        m_moveSpeedFactor = 1;
        m_animSpeed = 1;
    }    

    void Die()
    {
        GameManager.instance.FindPlayersToObserve();
        GameManager.instance.RemovePlayerFromTheListToObserve(gameObject);
        GameManager.instance.GameOver();
        m_playerMove.Die();
    }

    void CheckStunEndTime()
    {
        if(m_playerMove.m_playerState == PlayerState.Stunned &&
            Time.time > m_stunEndTime)
        {
            m_playerMove.RecoverStunState();
        }
    }

    void UpdateAP()
    {
        m_currentAP += m_amountChargingAP * Time.deltaTime;

        if(m_currentAP > m_maxAP)
        {
            m_currentAP = m_maxAP;
        }
    }

    void UpdateBuff()
    {
        for(int i = 0; i< m_buffArray.Length; i++)
        {
            m_buffArray[i].UpdateBuff();

            if(m_buffArray[i].buff == Buff.heal)
            {
                m_currentHP += m_buffArray[i].GetHealAmount();

                if(m_currentHP > m_maxHP)
                {
                    m_currentHP = m_maxHP;
                }
                photonView.RPC("ApplyHP", RpcTarget.All, m_currentHP);
            }

            if(m_buffArray[i].IsFinished())
            {
                photonView.RPC("ResetBuff", RpcTarget.All, i);
            }
        }
    }

    void InitBuffArray()
    {
        int arrayLength = (int)Buff.end - 1;
        m_buffArray = new BuffEntity[arrayLength];

        for (int i = 0; i < arrayLength; i++)
        {
            if (m_buffObjectArray[i] != null)
            {
                m_buffArray[i] = m_buffObjectArray[i].GetComponent<BuffEntity>();
                m_buffArray[i].buff = (Buff)(i + 1);
            }
        }

        m_isFinishedInitBuff = true;
    }

    void DecreaseHP()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_currentHP -= 10f;
            photonView.RPC("ApplyHP", RpcTarget.All, m_currentHP);
        }
    }

    private void OnDestroy()
    {
        GameManager.instance.RemovePlayerFromTheListToObserve(gameObject);
    }

    #endregion Internal Function
}
