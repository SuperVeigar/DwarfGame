using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class CameraSetup : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            CinemachineVirtualCamera playerCamera = FindObjectOfType<CinemachineVirtualCamera>();
            playerCamera.Follow = transform;
            playerCamera.LookAt = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ObserveOthers()
    {
        CinemachineVirtualCamera playerCamera = FindObjectOfType<CinemachineVirtualCamera>();
        playerCamera.Follow = transform;
        playerCamera.LookAt = transform;
    }
}
