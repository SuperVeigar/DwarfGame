using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPun
{

    public float forward;
    public float right;
    public float rotation;
    public float forwardRaw;
    public float rightRaw;   
    public bool jump = false;
    public bool mouseL = false;
    public bool mouseR = false;
    public bool buffAtt = false;
    public bool buffDef = false;
    public bool buffheal = false;
    public bool buffSpd = false;
    

    private float acceleration = 0.06f;
    private float backwardWeight = 0.65f;
    private float leftRightWeight = 0.65f;
    private float mouseKeyOnTime = 0f;
    private float jumpKeyOnTime = 0f;
    private float keyOnInterval = 0.1f;
    private bool pushedJumpKey = false;
    private bool switchCam = false;
    private bool exit = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if(!GameManager.instance.m_isGameOver &&
            !GameManager.instance.m_isPaused)
        {
            GetKey();
        }
        if(GameManager.instance.m_isObserving)
        {
            GetKeyOnObservingMode();
        }

        CheckExitKey();       

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        AccelerateAxis();

        rotation = Input.GetAxis("Mouse X");
    }

    void AccelerateAxis()
    {
        #region Forward
        if (Input.GetKey(KeyCode.W))
        {
            forwardRaw += acceleration;

            if(forwardRaw > 1f)
            {
                forwardRaw = 1f;
            }
        }
        else if(Input.GetKey(KeyCode.S))
        {
            forwardRaw -= acceleration;

            if (forwardRaw < -1f)
            {
                forwardRaw = -1f;
            }
        }

        if(forwardRaw > 0 &&
            !(Input.GetKey(KeyCode.W)))
        {
            forwardRaw -= acceleration;

            if (forwardRaw < 0)
            {
                forwardRaw = 0;
            }
        }
        else if (forwardRaw < 0 &&
            !(Input.GetKey(KeyCode.S)))
        {
            forwardRaw += acceleration;

            if (forwardRaw > 0)
            {
                forwardRaw = 0;
            }
        }
        #endregion

        #region Right
        if (Input.GetKey(KeyCode.D))
        {
            rightRaw += acceleration;

            if (rightRaw > 1f)
            {
                rightRaw = 1f;
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rightRaw -= acceleration;

            if (rightRaw < -1f)
            {
                rightRaw = -1f;
            }
        }

        if (rightRaw > 0 &&
            !(Input.GetKey(KeyCode.D)))
        {
            rightRaw -= acceleration;

            if (rightRaw < 0)
            {
                rightRaw = 0;
            }
        }
        else if (rightRaw < 0 &&
            !(Input.GetKey(KeyCode.A)))
        {
            rightRaw += acceleration;

            if (rightRaw > 0)
            {
                rightRaw = 0;
            }
        }
        #endregion

        #region Forward and Right
        if (forwardRaw >= 0)
        {
            forward = forwardRaw;
        }
        else
        {
            forward = forwardRaw * backwardWeight;
        }
        right = rightRaw * leftRightWeight;
        #endregion
    }

    public void ResetAxis()
    {
        forwardRaw = 0;
        rightRaw = 0;        
    }

    // Not used
    void GetAxis()
    {
        forwardRaw = (int)(Input.GetAxis("Vertical") * 100) * 0.01f;
        rightRaw = (int)(Input.GetAxis("Horizontal") * 100) * 0.01f;

        if (forwardRaw >= 0)
        {
            forward = forwardRaw;
        }
        else
        {
            forward = forwardRaw * backwardWeight;
        }
        right = rightRaw * leftRightWeight;
    }

    void GetKeyOnObservingMode()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switchCam = true;
        }
    }

    void CheckExitKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exit = true;
        }
    }

    void GetKey()
    {
        if (Input.GetMouseButton(0))
        {
            mouseL = true;
            mouseR = false;
            mouseKeyOnTime = Time.time + keyOnInterval;
        }
        else if (Input.GetMouseButton(1))
        {
            mouseL = false;
            mouseR = true;
            mouseKeyOnTime = Time.time + keyOnInterval;
        }
        if (mouseKeyOnTime < Time.time)
        {
            mouseL = false;
            mouseR = false;
        }

        if (Input.GetKey(KeyCode.Space) &&
            !pushedJumpKey)
        {
            jump = true;
            pushedJumpKey = true;
            jumpKeyOnTime = Time.time + keyOnInterval;
        }
        if (jumpKeyOnTime < Time.time)
        {
            jump = false;
        }
        if(!Input.GetKey(KeyCode.Space) &&
            pushedJumpKey)
        {
            pushedJumpKey = false;
        }

        buffAtt = Input.GetKey(KeyCode.Alpha1);
        buffDef = Input.GetKey(KeyCode.Alpha2);
        buffheal = Input.GetKey(KeyCode.Alpha3);
        buffSpd = Input.GetKey(KeyCode.Alpha4);
    }

    public bool GetSwitchingCamKey()
    {
        if(switchCam)
        {
            switchCam = false;
            return true;
        }

        return false;
    }

    public bool GetExitKey()
    {
        if (exit)
        {
            exit = false;
            return true;
        }

        return false;
    }
}
