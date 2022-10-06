using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StaticBolt : MonoBehaviour
{
    public int m_playerNum { get; private set; }


    private enum StaticBoltState { start = 0, move, bump, stun, vanish, end}

    private PlayerState m_playerState;
    private float m_attack;
    private float m_stunTime;
    private BoxCollider m_boxCollider;
    private StaticBoltState m_state;


    // Start is called before the first frame update
    void Start()
    {
        m_boxCollider = GetComponent<BoxCollider>();
        m_state = StaticBoltState.move;
        m_stunTime = 1f;
        if (!CommonSoundManager_DontDest.instance.isMute) GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        switch (m_state)
        {
            case StaticBoltState.move:
                m_stunTime += (m_stunTime * m_stunTime) * Time.deltaTime;
                break;
            case StaticBoltState.bump:
                transform.localScale += Vector3.one * Time.deltaTime;

                if (transform.localScale.x >= 1.0f)
                {
                    m_state = StaticBoltState.stun;
                }
                break;
            case StaticBoltState.stun:
                m_stunTime -= Time.deltaTime;

                if (m_stunTime <= 0f)
                {
                    m_state = StaticBoltState.vanish;
                }
                break;
            case StaticBoltState.vanish:
                transform.localScale -= Vector3.one * Time.deltaTime * 7f;

                if (transform.localScale.x <= 0)
                {
                    transform.localScale = Vector3.zero;
                    PhotonNetwork.Destroy(gameObject);
                }
                break;
        }
    }

    public void SetAttackAndState(PlayerState playerState, float attack)
    {
        m_playerState = playerState;
        m_attack = attack;
    }

    public void SetPlayerNum(int num)
    {
        m_playerNum = num;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if(other.gameObject.tag == "Player" &&
            other.gameObject.GetComponent<PlayerStatus>().m_playerNum != m_playerNum)
        {
            transform.position = new Vector3(other.gameObject.transform.position.x, transform.position.y, other.gameObject.transform.position.z);
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.GetHit(m_attack, m_playerState, Vector3.zero, m_stunTime);
            }
            SetBumpState();
        }
        else if(other.gameObject.tag == "Blockable")
        {
            SetBumpState();
        }

        
    }

    void SetBumpState()
    {
        m_state = StaticBoltState.bump;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (m_boxCollider != null &&
            m_boxCollider.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_boxCollider.transform.position, 0.5f);
        }
    }
}
