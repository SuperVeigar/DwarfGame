using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviour
{
    public GameObject m_slashparticle;
    public GameObject m_attcakRParticle;

    private PlayerState m_playerState;
    private float m_attack;
    private Vector3 m_charPos;
    private BoxCollider m_boxCollider;
    private SphereCollider m_sphereCollider;
    

    // Start is called before the first frame update
    void Start()
    {
        m_boxCollider = GetComponent<BoxCollider>();
        m_sphereCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAttackAndState(PlayerState playerState, float attack)
    {
        m_playerState = playerState;
        m_attack = attack;
    }

    public void SetCharPos(Vector3 pos)
    {
        m_charPos = pos;
    }

    public void TurnOnCollider()
    {
        if(m_boxCollider != null)
        {
            m_boxCollider.enabled = true;
        }
        if (m_sphereCollider != null)
        {
            m_sphereCollider.enabled = true;
        }
    }
    public void TurnOffCollider()
    {
        if (m_boxCollider != null)
        {
            m_boxCollider.enabled = false;
        }
        if (m_sphereCollider != null)
        {
            m_sphereCollider.enabled = false;
        }
    }
    public void TurnOnSlashParticle()
    {
        if (m_slashparticle != null)
        {
            m_slashparticle.SetActive(true);
        }
    }

    public void TurnOffSlashParticle()
    {
        if (m_slashparticle != null)
        {
            m_slashparticle.SetActive(false);
        }
    }
    public void TurnOnAttackRParticle()
    {
        if (m_attcakRParticle != null)
        {
            m_attcakRParticle.SetActive(true);
        }
    }

    public void TurnOffAttackRParticle()
    {
        if (m_attcakRParticle != null)
        {
            m_attcakRParticle.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.gameObject.tag == "Player")
        {
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {                
                playerStatus.GetHit(m_attack, m_playerState, m_charPos);
            }
        }
        else if (other.gameObject.tag == "Barricade" ||
            other.gameObject.tag == "Runestone")
        {

        }
    }

    private void OnDrawGizmos()
    {
        if(m_sphereCollider != null && 
            m_sphereCollider.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_sphereCollider.transform.position, 0.5f);
        }
        if (m_boxCollider != null &&
            m_boxCollider.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_boxCollider.transform.position, 0.5f);
        }
    }
}
