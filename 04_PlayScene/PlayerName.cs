using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerName : MonoBehaviourPun
{
    public GameObject m_name;

    private GameObject m_playerCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        m_playerCamera = GameObject.Find("PlayerCamera");
    }

    // Update is called once per frame
    void Update()
    {
        if(m_playerCamera != null)
        {
            UpdateLookAt();
        }
    }

    void UpdateLookAt()
    {
        Vector3 dirVec = m_name.transform.position - m_playerCamera.transform.position;
        Vector3 loockAtPos = m_name.transform.position + dirVec.normalized;
        m_name.transform.LookAt(loockAtPos);
    }

    [PunRPC]
    public void SetName(string name)
    {
        m_name.GetComponent<TextMesh>().text = name;
    }


}
