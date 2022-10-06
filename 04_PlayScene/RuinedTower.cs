using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum RuinedTowerState {  start = 0, idle, ready, buffOn, end}
public enum Buff { start_none = 0, attack, defence, heal, fast, end}
public class RuinedTower : MonoBehaviourPun
{
    public GameObject m_buffAttack;
    public GameObject m_buffDeffence;
    public GameObject m_buffHeal;
    public GameObject m_buffFast;

    private float m_buffOnDelay = 0.4f;
    private float m_buffOnElapsedTime = 0f;    
    private Buff m_buffNum;
    private RuinedTowerState m_state = RuinedTowerState.idle;
    private SphereCollider m_sphereCollider;
    private AudioSource m_audiosource;

    // Start is called before the first frame update
    void Start()
    {
        m_sphereCollider = GetComponent<SphereCollider>();
        m_sphereCollider.enabled = false;
        m_audiosource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        switch (m_state)
        {
            case RuinedTowerState.idle:
                break;
            case RuinedTowerState.ready:
                UpdateOnReadyState();
                break;
            case RuinedTowerState.buffOn:
                break;
        }
        
    }

    void UpdateOnReadyState()
    {
        m_buffOnElapsedTime += Time.deltaTime;

        if(m_buffOnElapsedTime > m_buffOnDelay)
        {
            photonView.RPC("SetBuffOnState", RpcTarget.All);
        }        
    }

    [PunRPC]
    public void SetBuffOnState()
    {
        m_state = RuinedTowerState.buffOn;
        m_sphereCollider.enabled = true;
    }

    [PunRPC]
    public void SpawnBuff(Buff buffNum)
    {
        if(m_state == RuinedTowerState.idle &&
            m_sphereCollider != null)
        {
            m_state = RuinedTowerState.ready;
            m_buffNum = buffNum;
            m_buffOnElapsedTime = 0f;

            switch (m_buffNum)
            {
                case Buff.attack:
                    m_buffAttack.SetActive(true);
                    break;
                case Buff.defence:
                    m_buffDeffence.SetActive(true);
                    break;
                case Buff.heal:
                    m_buffHeal.SetActive(true);
                    break;
                case Buff.fast:
                    m_buffFast.SetActive(true);
                    break;
            }
        }        
    }
    [PunRPC]
    public void TurnOffBuff(Buff buffNum)
    {
        if (!CommonSoundManager_DontDest.instance.isMute) m_audiosource.Play();

        switch (m_buffNum)
        {
            case Buff.attack:
                m_buffAttack.SetActive(false);
                break;
            case Buff.defence:
                m_buffDeffence.SetActive(false);
                break;
            case Buff.heal:
                m_buffHeal.SetActive(false);
                break;
            case Buff.fast:
                m_buffFast.SetActive(false);
                break;
        }

        m_state = RuinedTowerState.idle;
        m_buffNum = Buff.start_none;
        m_sphereCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;


        PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();

        if (playerStatus != null)
        {
            switch(m_buffNum)
            {
                case Buff.attack:
                    playerStatus.GetComponent<PhotonView>().RPC("ReceiveBuff", RpcTarget.All, m_buffNum, 15f, 3f, 0f);
                    break;
                case Buff.defence:
                    playerStatus.GetComponent<PhotonView>().RPC("ReceiveBuff", RpcTarget.All, m_buffNum, 15f, 2f, 0f);
                    break;
                case Buff.heal:
                    playerStatus.GetComponent<PhotonView>().RPC("ReceiveBuff", RpcTarget.All, m_buffNum, 2f, 20f, 0f);
                    break;
                case Buff.fast:
                    playerStatus.GetComponent<PhotonView>().RPC("ReceiveBuff", RpcTarget.All, m_buffNum, 15f, 0.08f, 0.08f);
                    break;
            }

            photonView.RPC("TurnOffBuff", RpcTarget.All, m_buffNum);
        }

    }
}
