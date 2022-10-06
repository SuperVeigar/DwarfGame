using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 무기 변경해주고 각 무기에 따른 특이사항 적용해주는 클래스
/// </summary>
public class PlayerWeaponChanger : MonoBehaviour
{
    public List<GameObject> m_weaponList = new List<GameObject>();
    public List<GameObject> m_weaponListForDead = new List<GameObject>();
    public PlayerWeapon m_myWeapon = (PlayerWeapon)((int)PlayerWeapon.start + 1);
    public GameObject m_staticboltPrefab;
    public GameObject m_staticboltShooter;
    public GameObject m_dwarfmaceAttackRParticle;
    public GameObject m_smokecircleParticle;

    private float m_timeToVisableWeapon = 0.5f;
    private GameObject m_myWeaponObject;    
    private PlayerStatus m_playerStatus;

    // Start is called before the first frame update
    void Start()
    {
        m_playerStatus = GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeWeapon(PlayerWeapon weapon)
    {
        if(m_weaponList.Count >= (int)PlayerWeapon.end -1)
        {
            for (int i = 0; i < m_weaponList.Count; i++)
            {
                m_weaponList[i].SetActive(false);
            }

            m_weaponList[(int)weapon - 1].SetActive(true);

            if(weapon == PlayerWeapon.axe &&
                m_staticboltShooter != null)
            {
                m_staticboltShooter.SetActive(true);
            }
        }        
    }
    public IEnumerator LoadWeapon()
    {
        yield return new WaitForSeconds(m_timeToVisableWeapon);

        ChangeWeapon(m_myWeapon);
    }
    [PunRPC]
    public void LoadWeaponOnNetwork()
    {
        GetComponent<Animator>().SetBool("StartGame", true);
        StartCoroutine(LoadWeapon());
    }
    [PunRPC]
    public void SetWeapon(PlayerWeapon weapon)
    {
        m_myWeapon = weapon;
    }
    [PunRPC]
    public void SetOnDieState(PlayerWeapon weapon)
    {
        if(m_weaponList.Count >= (int)weapon - 1 &&
            m_weaponListForDead.Count >= (int)weapon - 1 &&
            (int)weapon < (int)PlayerWeapon.end)
        {
            GameObject realWeapon = m_weaponList[(int)weapon - 1];
            GameObject animWeapon = m_weaponListForDead[(int)weapon - 1];

            animWeapon.transform.position = realWeapon.transform.position;
            animWeapon.transform.rotation = realWeapon.transform.rotation;

            realWeapon.SetActive(false);
            animWeapon.SetActive(true);

            Vector3 forceDirection = (animWeapon.transform.position - transform.position).normalized;
            animWeapon.GetComponent<Rigidbody>().AddForce(forceDirection * 100);

            StartCoroutine(TurnOnKinematicOfAnimWeapon(animWeapon));

        }        
    }    

    public void TurnOnWeaponCollider()
    {

        if (m_myWeaponObject == null &&
        m_weaponList.Count >= (int)m_myWeapon - 1)
        {
            m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
        }

        if (m_myWeaponObject != null)
        {
            m_myWeaponObject.GetComponent<Weapon>().TurnOnCollider();
        }

        if (m_playerStatus != null)
        {
            PlayerState playerState;
            float attack;
            m_playerStatus.GetAttackAndState(out playerState, out attack);
            m_myWeaponObject.GetComponent<Weapon>().SetAttackAndState(playerState, attack);
            m_myWeaponObject.GetComponent<Weapon>().SetCharPos(transform.position);
        }    
    }
    public void TurnOffWeaponCollider()
    {
        if (m_playerStatus != null)
        {
            if (m_myWeaponObject == null &&
            m_weaponList.Count >= (int)m_myWeapon - 1)
            {
                m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
            }

            if (m_myWeaponObject != null)
            {
                m_myWeaponObject.GetComponent<Weapon>().TurnOffCollider();
            }
        }
    }
    public void ShotStaticBolt()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject staticboltObject = PhotonNetwork.Instantiate(m_staticboltPrefab.name, m_staticboltShooter.transform.position + Vector3.up * 0.4f, m_staticboltShooter.transform.rotation);
            staticboltObject.GetComponent<StaticBolt>().SetPlayerNum(GetComponent<PlayerStatus>().m_playerNum);
            staticboltObject.GetComponent<Rigidbody>().AddForce(transform.forward * 1300f);         

            PlayerState playerState;
            float attack;
            m_playerStatus.GetAttackAndState(out playerState, out attack);
            staticboltObject.GetComponent<StaticBolt>().SetAttackAndState(playerState, attack);
        }
    }

    IEnumerator TurnOnKinematicOfAnimWeapon(GameObject weapon)
    {
        yield return new WaitForSeconds(1.6f);
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.GetComponent<Rigidbody>().useGravity = false;
    }

    public void TurnOnSlashParticle()
    {
        if (m_myWeaponObject == null &&
        m_weaponList.Count >= (int)m_myWeapon - 1)
        {
            m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
        }

        if (m_myWeaponObject != null)
        {
            m_myWeaponObject.GetComponent<Weapon>().TurnOnSlashParticle();
        }
    }

    public void TurnOffSlashParticle()
    {
        if (m_myWeaponObject == null &&
        m_weaponList.Count >= (int)m_myWeapon - 1)
        {
            m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
        }

        if (m_myWeaponObject != null)
        {
            m_myWeaponObject.GetComponent<Weapon>().TurnOffSlashParticle();
        }
    }

    public void TurnOnAttackRParticle()
    {
        if (m_myWeapon == PlayerWeapon.dwarfAxe)
        {
            if (m_myWeaponObject == null &&
                m_weaponList.Count >= (int)m_myWeapon - 1)
            {
                m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
            }

            if (m_myWeaponObject != null)
            {
                m_myWeaponObject.GetComponent<Weapon>().TurnOnAttackRParticle();
            }
        }
        else if (m_myWeapon == PlayerWeapon.dwarfMace)
        {
            if(m_dwarfmaceAttackRParticle != null)
            {
                m_dwarfmaceAttackRParticle.SetActive(true);
            }
        }
    }

    public void TurnOffAttackRParticle()
    {
        if (m_myWeapon == PlayerWeapon.dwarfAxe)
        {
            if (m_myWeaponObject == null &&
                m_weaponList.Count >= (int)m_myWeapon - 1)
            {
                m_myWeaponObject = m_weaponList[(int)m_myWeapon - 1];
            }

            if (m_myWeaponObject != null)
            {
                if (m_myWeapon == PlayerWeapon.dwarfAxe)
                {
                    m_myWeaponObject.GetComponent<Weapon>().TurnOffAttackRParticle();
                }
            }
        }
        else if (m_myWeapon == PlayerWeapon.dwarfMace)
        {
            if (m_dwarfmaceAttackRParticle != null)
            {
                m_dwarfmaceAttackRParticle.SetActive(false);
            }
        }
        
    }

    public void TurnOnSmokeCircle()
    {
        m_smokecircleParticle.SetActive(true);
        StartCoroutine(TurnOffSmokeCircle());
    }

    IEnumerator TurnOffSmokeCircle()
    {
        yield return new WaitForSeconds(2f);
        m_smokecircleParticle.SetActive(false);
    }
}
