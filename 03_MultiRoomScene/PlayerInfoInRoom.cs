using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum PlayerColor { start = 0, bronze, cobalt, gold, ruby, saphire, veredian, end }
public enum PlayerWeapon { start = 0, axe, dwarfAxe, dwarfMace, end }


public class PlayerInfoInRoom : MonoBehaviourPun, IPunObservable
{
    public bool IsFinishedInitialization = false;
    public bool isHost=false;
    public bool isReady = false;
    public string nickname = "";
    public string userID = "";
    public PlayerColor playerColor = PlayerColor.start + 1;
    public PlayerWeapon playerWeapon = PlayerWeapon.start + 1;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //- Bool
        //- Int
        //- string
        //- char
        //- short
        //- float
        //- PhotonPlayer
        //- Vector3
        //- Vector2
        //- Quaternion
        //- PhotonViewID
        if (stream.IsWriting)
        {
            stream.SendNext(IsFinishedInitialization);
            stream.SendNext(isHost);
            stream.SendNext(isReady);
            stream.SendNext(nickname);
            stream.SendNext(userID);
            stream.SendNext((int)playerColor);
            stream.SendNext((int)playerWeapon);
        }
        else
        {
            IsFinishedInitialization = (bool)stream.ReceiveNext();
            isHost = (bool)stream.ReceiveNext();
            isReady = (bool)stream.ReceiveNext();
            nickname = (string)stream.ReceiveNext();
            userID = (string)stream.ReceiveNext();
            playerColor = (PlayerColor)((int)stream.ReceiveNext());
            playerWeapon = (PlayerWeapon)((int)stream.ReceiveNext());
        }
    }

    private void Awake()
    {
       
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
