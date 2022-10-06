using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PlayerState { Ready = 0, IdleAndRun, Jump, Land, Attack1, Attack2, Attack3, AttackR_Axe, AttackR_DwarfAxe, AttackR_DwarfMace, CastBuff, Die, SoftDamage, Damage, FallBack, RiseUp, Stunned }

/// <summary>
/// 이동, 회전, 애니메이션 관련 클래스
/// 유한상태머신 사용
/// Animation은 photonveiw.IsMine에서 명령
/// </summary>
public class PlayerMove : MonoBehaviourPun, IPunObservable
{
    public PlayerState m_playerState { get; private set; }
    public GameObject m_stunnedParticle;

    private float m_moveSpeed = 4.3f;
    private float m_moveSpeedFactor = 1f;
    private float m_diagonalMoveWeight = 0.7f;
    private float m_rotSpeed = 120f;
    private float m_moveSpeedWeightOnJump = 0.7f;
    private float m_jumpSpeed = 5f;
    private float m_jumpableTime;
    private float m_jumpInterval = 2f;
    private float m_distanceCheckingGround;
    private float m_distanceCheckingGroundInNormal = 1f;
    private float m_distanceCheckingGroundOnJump = 1.5f;
    private float m_distanceCheckingGroundOnDie = 0.9f;
    private bool m_isOnTheGround = false;
    private Vector3 m_moveDirection;    
    private Rigidbody m_playerRigidbody;
    private Animator m_playerAnimator;
    private PlayerInput m_playerInput;
    private PlayerStatus m_playerStatus;
    private PlayerWeapon m_myWeapon;

    // Start is called before the first frame update
    void Start()
    {
        m_playerRigidbody = GetComponent<Rigidbody>();
        m_playerAnimator = GetComponent<Animator>();
        m_playerInput = GetComponent<PlayerInput>();
        m_playerStatus = GetComponent<PlayerStatus>();
        m_jumpableTime = Time.time;
        m_distanceCheckingGround = m_distanceCheckingGroundInNormal;
        m_playerState = PlayerState.Ready;        

        if (!photonView.IsMine)
        {
            m_playerRigidbody.useGravity = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        //TestState();        
#endif
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        CheckOnGround();

        switch (m_playerState)
        {
            case PlayerState.Ready:
                OnReadyState();
                break;
            case PlayerState.IdleAndRun:
                OnIdleAndRunState();
                break;
            case PlayerState.Jump:
                OnJumpState();
                break;
            case PlayerState.Land:
                OnLandState();
                break;
            case PlayerState.Attack1:
                OnAttack1State();
                break;
            case PlayerState.Attack2:
                OnAttack2State();
                break;
            case PlayerState.Attack3:
                OnAttack3State();
                break;
            case PlayerState.AttackR_Axe:
            case PlayerState.AttackR_DwarfAxe:
            case PlayerState.AttackR_DwarfMace:
                OnAttackRState();
                break;
            case PlayerState.CastBuff:
                OnCastBuffState();
                break;
            case PlayerState.Die:
                OnDieState();
                break;
            case PlayerState.SoftDamage:
                OnSoftDamageState();
                break;
            case PlayerState.Damage:
                OnDamageState();
                break;
            case PlayerState.FallBack:
                OnFallBackState();
                break;
            case PlayerState.RiseUp:
                OnRiseUpState();
                break;
            case PlayerState.Stunned:
                OnStunnedState();
                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)m_playerState);
        }
        else
        {
            m_playerState = (PlayerState)stream.ReceiveNext();
        }
    }
    [PunRPC]
    public void SetTrigger(string parameter)
    {
        m_playerAnimator.SetTrigger(parameter);
    }   
    [PunRPC]
    public void TurnOnKinematicOnNetwowrk()
    {
        m_playerRigidbody.isKinematic = true;
        m_playerRigidbody.useGravity = false;
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<BoxCollider>().enabled = true;
    }
    [PunRPC]
    void SetStunParticle(bool onoff)
    {
        m_stunnedParticle.SetActive(onoff);
    }
    public void SetDamageAnimation(PlayerState attackType, Vector3 hitPoint)
    {
        if (!photonView.IsMine) return;

        m_distanceCheckingGround = m_distanceCheckingGroundInNormal;
        m_playerAnimator.applyRootMotion = true;

        if (attackType == PlayerState.Attack1 ||
            attackType == PlayerState.Attack2)
        {
            switch (m_playerState)
            {
                case PlayerState.Jump:
                    FallBack(hitPoint);
                    break;
                case PlayerState.AttackR_Axe:
                    SoftDamage();
                    break;
                case PlayerState.AttackR_DwarfAxe:
                    Damage();
                    break;
                case PlayerState.AttackR_DwarfMace:
                    FallBack(hitPoint);
                    break;
                case PlayerState.SoftDamage:
                    Damage();
                    break;
                case PlayerState.Damage:
                    FallBack(hitPoint);
                    break;
                default:
                    SoftDamage();
                    break;
            }
        }
        else if (attackType == PlayerState.Attack3)
        {
            switch (m_playerState)
            {
                default:
                    FallBack(hitPoint);
                    break;
            }
        }
        else if (attackType == PlayerState.AttackR_Axe)
        {
            switch (m_playerState)
            {
                default:
                    Stunned();
                    break;
            }
        }
        else if (attackType == PlayerState.AttackR_DwarfAxe)
        {
            switch (m_playerState)
            {
                case PlayerState.Jump:
                    FallBack(hitPoint);
                    break;
                case PlayerState.Damage:
                    FallBack(hitPoint);
                    break;
                default:
                    Damage();
                    break;
            }
        }
        else if (attackType == PlayerState.AttackR_DwarfMace)
        {
            switch (m_playerState)
            {
                default:
                    FallBack(hitPoint);
                    break;
            }
        }
    }
    public void SetMoveSpeed(float value)
    {
        m_moveSpeedFactor = value;
    }
    

    #region Ready State
    void OnReadyState()
    {
        CheckWeaponLoaded();
    }
    void CheckWeaponLoaded()
    {
        if (CheckAnimation("LOAD WEAPON", 1.0f))
        {
            m_playerAnimator.SetBool("IsMovable", true);
            m_playerState = PlayerState.IdleAndRun;
            m_myWeapon = GetComponent<PlayerWeaponChanger>().m_myWeapon;              
        }
    }
    #endregion Ready State

    #region Idle and Run State
    void OnIdleAndRunState()
    {
        RotatePlayer();
        MovePlayer();
        JumpPlayer();
        Attack1();
        AttackR();
        CastBuff();
    }

    void MovePlayer()
    {
        Vector3 moveDirection = transform.forward * m_playerInput.forward + transform.right * m_playerInput.right;
        m_moveDirection = moveDirection;

        if(m_playerInput.forward > 0 &&
            m_playerInput.right != 0)
        {
            m_moveDirection *= m_diagonalMoveWeight;
        }
        else if(m_playerInput.forward < 0&&
            m_playerInput.right != 0)
        {
            m_moveDirection = m_moveDirection * m_diagonalMoveWeight * m_diagonalMoveWeight;
        }

        m_playerRigidbody.MovePosition(transform.position + m_moveDirection * m_moveSpeed * m_moveSpeedFactor * Time.deltaTime);

        m_playerAnimator.SetFloat("Forward", m_playerInput.forward);
        m_playerAnimator.SetFloat("Right", m_playerInput.right);
    } 

    void RotatePlayer()
    {
        float angle = m_playerInput.rotation * m_rotSpeed * Time.deltaTime;

        m_playerRigidbody.rotation = m_playerRigidbody.rotation * Quaternion.Euler(0, angle, 0);
    }
    

    void JumpPlayer()
    {
        if (m_playerState == PlayerState.IdleAndRun &&
            m_isOnTheGround &&
            m_playerInput.jump &&
            m_jumpableTime <= Time.time)
        {
            m_playerState = PlayerState.Jump;
            photonView.RPC("SetTrigger", RpcTarget.All, ("Jump"));

            StartCoroutine(ExcuteJump());           
        }
    }

    IEnumerator ExcuteJump()
    {
        m_jumpableTime = Time.time + m_jumpInterval;
        m_playerAnimator.applyRootMotion = false;

        Vector3 jumpDirection = m_moveDirection * m_moveSpeed * m_moveSpeedFactor * m_moveSpeedWeightOnJump + Vector3.up * m_jumpSpeed;               

        yield return new WaitForSeconds(0.3f);
        
        m_playerRigidbody.AddForce(jumpDirection, ForceMode.Impulse);
    }

    void Attack1()
    {
        if (!m_isOnTheGround) return;

        if(m_playerState == PlayerState.IdleAndRun &&
            CheckAttackable() &&
            m_playerInput.mouseL)
        {
            photonView.RPC("SetTrigger",RpcTarget.All,("Attack1"));
            m_playerState = PlayerState.Attack1;
        }
    }
    void AttackR()
    {
        if (!m_isOnTheGround) return;

        if (m_playerState == PlayerState.IdleAndRun && 
            CheckAttackable() &&
            m_playerInput.mouseR)
        {
            float neededAp = 0f;
            switch(m_myWeapon)
            {
                case PlayerWeapon.axe:
                    neededAp = 40f;
                    if(GetComponent<PlayerStatus>().IsEnoughAP(neededAp))
                    {
                        photonView.RPC("SetTrigger", RpcTarget.All, ("AttackRAxe"));
                        m_playerState = PlayerState.AttackR_Axe;
                        GetComponent<PlayerStatus>().UseAP(neededAp);
                    }                    
                    break;
                case PlayerWeapon.dwarfAxe:
                    neededAp = 60f;
                    if (GetComponent<PlayerStatus>().IsEnoughAP(neededAp))
                    {
                        photonView.RPC("SetTrigger", RpcTarget.All, ("AttackRDwarfAxe"));
                        m_playerState = PlayerState.AttackR_DwarfAxe;
                        GetComponent<PlayerStatus>().UseAP(neededAp);
                    }
                        
                    break;
                case PlayerWeapon.dwarfMace:
                    neededAp = 70f;
                    if (GetComponent<PlayerStatus>().IsEnoughAP(neededAp))
                    {
                        photonView.RPC("SetTrigger", RpcTarget.All, ("AttackRDwarfMace"));
                        m_playerState = PlayerState.AttackR_DwarfMace;
                        GetComponent<PlayerStatus>().UseAP(neededAp);
                    }                        
                    break;
            }
        }
    }
    void CastBuff()
    {
        if (!m_isOnTheGround) return;

        if (m_playerState == PlayerState.IdleAndRun &&
            !CheckAnimation("FURY", 0.1f) &&
            m_playerStatus != null)
        {
            if (m_playerInput.buffAtt &&
                m_playerStatus.IsAvailableBuff((int)Buff.attack - 1))
            {
                photonView.RPC("UseBuff", RpcTarget.All, (int)Buff.attack - 1);
                CastBuffAnim();
            }
            else if (m_playerInput.buffDef &&
                m_playerStatus.IsAvailableBuff((int)Buff.defence - 1))
            {
                photonView.RPC("UseBuff", RpcTarget.All, (int)Buff.defence - 1);
                CastBuffAnim();
            }
            else if (m_playerInput.buffheal &&
                m_playerStatus.IsAvailableBuff((int)Buff.heal - 1))
            {
                photonView.RPC("UseBuff", RpcTarget.All, (int)Buff.heal - 1);
                CastBuffAnim();
            }
            else if (m_playerInput.buffSpd &&
                m_playerStatus.IsAvailableBuff((int)Buff.fast - 1))
            {
                photonView.RPC("UseBuff", RpcTarget.All, (int)Buff.fast - 1);
                CastBuffAnim();
            }
        }
    }
    void CastBuffAnim()
    {
        photonView.RPC("SetTrigger", RpcTarget.All, ("Fury"));
        m_playerState = PlayerState.CastBuff;
    }
    #endregion Idle and Run State

    #region Jump and Land State
    void OnJumpState()
    {
        if(m_playerRigidbody.velocity.y < 0 &&
            m_distanceCheckingGround == m_distanceCheckingGroundInNormal)
        {
            m_distanceCheckingGround = m_distanceCheckingGroundOnJump;
        }
        if(CheckAnimation("STAY ON AIR WEAPON", 0.1f) &&
            m_isOnTheGround)
        {
            photonView.RPC("SetTrigger",RpcTarget.All,("Land"));
            m_playerState = PlayerState.Land;
        }
    }
    void OnLandState()
    {
        if (CheckAnimation("LAND WEAPON", 0.7f))
        {
            m_playerAnimator.applyRootMotion = true;
            photonView.RPC("SetTrigger",RpcTarget.All,("Idle"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    #endregion Jump and Land State

    #region Attack State
    void OnAttack1State()
    {
        if (CheckAnimation("ATTACK 1", 0.8f) &&
            m_playerInput.mouseL)
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Attack2"));
            m_playerState = PlayerState.Attack2;
        }
        else if (CheckAnimation("ATTACK 1", 1f))
        {
            photonView.RPC("SetTrigger",RpcTarget.All,("AttackCancel"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    void OnAttack2State()
    {
        if (CheckAnimation("ATTACK 2", 0.8f) &&
            m_playerInput.mouseL)
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Attack3"));
            m_playerState = PlayerState.Attack3;
        }
        else if (CheckAnimation("ATTACK 2", 1f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("AttackCancel"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    void OnAttack3State()
    {
        if (CheckAnimation("ATTACK 3", 1f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("AttackCancel"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }

    void OnAttackRState()
    {
        if (CheckAnimation("ATTACK R AXE", 0.7f) ||
            CheckAnimation("ATTACK R DWARF AXE", 1f) ||
            CheckAnimation("ATTACK R DWARF MACE", 1f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("AttackCancel"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
   
    #endregion Attack State

    #region Fury State
    void OnCastBuffState()
    {
        if (CheckAnimation("FURY", 0.6f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Idle"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    #endregion

    #region Die State
    public void Die()
    {
        m_distanceCheckingGround = m_distanceCheckingGroundOnDie;
        photonView.RPC("SetTrigger", RpcTarget.All, ("Die"));
        photonView.RPC("SetOnDieState", RpcTarget.All, m_myWeapon);
        m_playerState = PlayerState.Die;
    }

    void OnDieState()
    {
        if(m_isOnTheGround)
        {
            photonView.RPC("TurnOnKinematicOnNetwowrk", RpcTarget.All);
        }
    }
    #endregion Die State

    #region Soft Damage State
    void SoftDamage()
    {
        photonView.RPC("SetTrigger", RpcTarget.All, ("SoftDamage"));
        m_playerState = PlayerState.SoftDamage;
        m_playerInput.ResetAxis();
    }

    void OnSoftDamageState()
    {
        if (CheckAnimation("SOFT DAMAGE", 1f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Idle"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    #endregion Soft Damage State

    #region Damage State
    void Damage()
    {
        photonView.RPC("SetTrigger", RpcTarget.All, ("Damage"));
        m_playerState = PlayerState.Damage;
        m_playerInput.ResetAxis();
    }

    void OnDamageState()
    {
        if (CheckAnimation("DAMAGE", 0.75f))
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Idle"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    #endregion Damage State

    #region Fall back state
    void FallBack(Vector3 hitPos)
    {
        //Vector2 myForward = new Vector2(transform.forward.x, transform.forward.z);
        //Vector2 charPos = new Vector2(hitPos.x, hitPos.z);
        //float angle = Vector2.SignedAngle(myForward, charPos - new Vector2(transform.position.x, //transform.position.z));
        //Debug.Log(angle);
        //transform.Rotate(Vector3.up, angle);

        transform.LookAt(hitPos, Vector3.up);
        photonView.RPC("SetTrigger", RpcTarget.All, ("FallBack"));
        m_playerState = PlayerState.FallBack;        
    }

    void OnFallBackState()
    {
        if (CheckAnimation("FALL BACK", 0.8f) && 
            m_isOnTheGround)
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("RiseUp"));
            m_playerState = PlayerState.RiseUp;
        }
    }

    void OnRiseUpState()
    {
        if (CheckAnimation("RISE UP", 0.8f) &&
            m_isOnTheGround)
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Idle"));
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }

    }
    #endregion Fall back state

    #region Stunned state
    void Stunned()
    {
        photonView.RPC("SetTrigger", RpcTarget.All, ("Stunned"));
        photonView.RPC("SetStunParticle", RpcTarget.All, true);
        m_playerState = PlayerState.Stunned;
    }
    void OnStunnedState()
    {
        
    }
    public void RecoverStunState()
    {
        if (m_playerState == PlayerState.Stunned &&
            m_isOnTheGround)
        {
            photonView.RPC("SetTrigger", RpcTarget.All, ("Idle"));
            photonView.RPC("SetStunParticle", RpcTarget.All, false);
            m_playerState = PlayerState.IdleAndRun;
            m_playerInput.ResetAxis();
        }
    }
    #endregion Stunned state

    #region Sub Function
    bool CheckAnimation(string animName, float normalizedTime)
    {
        if (m_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(animName) &&
            m_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTime)
            return true;

        return false;
    }

    void CheckOnGround()
    {
        RaycastHit raycastHit;
        Vector3 startPos = transform.position + Vector3.up;

        m_isOnTheGround = Physics.BoxCast(startPos, transform.lossyScale * 0.5f, Vector3.up * -1, out raycastHit, transform.rotation, m_distanceCheckingGround);
    }
    
    bool CheckAttackable()
    {
        if (!CheckAnimation("ATTACK 1", 0.1f) &&
            !CheckAnimation("ATTACK R AXE", 0.1f) &&
            !CheckAnimation("ATTACK R DWARF AXE", 0.1f) &&
            !CheckAnimation("ATTACK R DWARF MACE", 0.1f)) return true;

        return false;
    }
    #endregion Sub Function

    #region For Test
    void PrintPlayerState()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Player State : " + m_playerState);
        }
    }
    void TestState()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SoftDamage();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            Damage();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            FallBack(Vector3.zero);
        }
    }
    #endregion For Test
}
