﻿using UnityEngine;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    #region [Serialized Fields]

    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    [Range(0, 1.0f)] [SerializeField] private float JumpSpeedReduction = .9f;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private LayerMask m_WhatIsWall;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_WallCheck;

    #endregion

    #region [Public Variables]

    public float GroundCheckWi;
    public float GroundCheckHi;
    public float WallCheckWi;
    public float WallCheckHi;
    public float runspeed;
    public bool speedRunning;
    public bool stunned;
    public Rigidbody2D m_Rigidbody2D;
    public Animator m_Animator;
    public PhotonView pv;

    #endregion

    #region [Private Variables]

    private Vector3 iniScale;
    private Vector3 iniPos;
    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 smoothMove;
    private bool facingFront;
    private bool isWallJumping;
    private bool isSliding;
    private bool m_Grounded;
    private bool m_WallInFront;
    private bool canCountTouch;
    private PlayerController m_playerController;
    private DataManager m_dataManager;
    private GameController m_gameController;

    bool rn;
    bool idl;
    public bool iniBoost;
    public int countDownIs;

    #endregion

    private void Awake()
    {
        m_playerController = GetComponent<PlayerController>();
        m_dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
    }

    void Start()
    {
        m_Rigidbody2D.gravityScale = m_dataManager.m_GravityMultiplier;
        iniScale = transform.localScale;
        facingFront = true;

        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 15;
    }

    void Update()
    {
        if (m_Grounded)
        {
            transform.localScale = iniScale;
            facingFront = true;
        }

        if (Input.touchCount == 0)
        {
            canCountTouch = true;
        }

        if (m_WallInFront && !isWallJumping)
        {
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
        }

        if (isWallJumping)
            isWallJumping &= !m_Grounded;
    }

    private void FixedUpdate()
    {
        if (Input.touchCount == 1 || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (canCountTouch && !m_playerController.canRace)
            {
                if (countDownIs == m_gameController.countdown.Length - 1)
                {
                    Debug.Log("speedBoost");
                    iniBoost = true;
                }
                canCountTouch = false;
            }
        }

        //Debug.Log(m_playerController.canRace);
        if (m_playerController.canRace)
        {
            rn = !isWallJumping && !m_WallInFront && facingFront && m_Grounded;

            if (pv.IsMine)
                MoveMe();
            else
                SmoothMove();
        }
        else
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            runspeed = 0f;
        }

        //if (m_Rigidbody2D.velocity.x < -1)
        //{
        //    runspeed = 0f;
        //}

        m_WallInFront = Physics2D.OverlapBox(m_WallCheck.position, new Vector2(WallCheckWi, WallCheckHi), 0, m_WhatIsWall);
        m_Grounded = Physics2D.OverlapBox(m_GroundCheck.position, new Vector2(GroundCheckWi, GroundCheckHi), 0, m_WhatIsGround);

        idl = m_WallInFront && !isWallJumping && !isSliding;
    }

    IEnumerator IniSpeed()
    {
        yield return null;
    }

    private void MoveMe()
    {
        if (!isWallJumping && !m_WallInFront && facingFront)
            Run();            

        if (Input.touchCount == 1 || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Input.mousePosition.x > Screen.width * .25f || Input.mousePosition.y > Screen.width * .25f)
            {
                if (canCountTouch)
                {   
                    Jump();
                    canCountTouch = false;
                }
            }
        }

        if (m_Rigidbody2D.velocity.y <= -0.5f && m_WallInFront)
            isSliding = true;
        else
            isSliding = false;

        if (m_Rigidbody2D.velocity.y < 0 && !isSliding)
            m_Animator.SetFloat("fall", m_Rigidbody2D.velocity.y);
        else
            m_Animator.SetFloat("fall", 0.0f);

        if (isSliding)
            m_Rigidbody2D.velocity = new Vector2(0.0f, m_Rigidbody2D.velocity.y);

        if (m_Rigidbody2D.velocity.y <= m_dataManager.m_TerminalSpeed)
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_dataManager.m_TerminalSpeed);

        if (speedRunning)
        {
            m_Animator.SetBool("run", false);
        }
        else if (stunned)
        {
            m_Animator.SetBool("run", false);
        }
        else
        {
            m_Animator.SetBool("run", rn);
        }

        m_Animator.SetBool("wallslide", isSliding);
        m_Animator.SetBool("idle", idl);
    }

    private void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, smoothMove, Time.deltaTime * 10);
    }

    void Run()
    {
        if (iniBoost)
        {
            m_playerController.IniSpeedBoost();
        }
        else
        {
            //Debug.Log(m_Rigidbody2D.velocity);
            float m_MinRunForce = m_dataManager.m_MaxRunForce / 10.0f;
            if (m_playerController.isFinished)
            {
                runspeed = 0;
                m_Rigidbody2D.velocity = Vector2.zero;
            }

            if (runspeed < m_dataManager.m_BaseSpeed && runspeed >= 0f)
            {
                runspeed += Time.deltaTime * m_dataManager.m_MaxRunForce;
            }
            if (runspeed >= m_dataManager.m_BaseSpeed && runspeed < m_dataManager.m_TargetSpeed)
            {
                runspeed += Time.deltaTime * m_MinRunForce;
            }
            if (runspeed >= m_dataManager.m_TargetSpeed && !speedRunning)
            {
                runspeed = m_dataManager.m_TargetSpeed;
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(runspeed, m_Rigidbody2D.velocity.y);

            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        }

        if (speedRunning)
        {
            m_playerController.currentState = PlayerController.RunnerState.speedRun;
        }
        else if (stunned)
        {
            m_playerController.currentState = PlayerController.RunnerState.stun;
            runspeed = 0f;
        }
        else
        {
            m_playerController.currentState = PlayerController.RunnerState.run;
        }
    }

    void Jump()
    {
        m_Animator.SetTrigger("Jump");

        if (m_Grounded)
        {
            m_playerController.NoOfJumps++;
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.AddForce(new Vector2(m_Rigidbody2D.velocity.x, m_dataManager.m_JumpForce), ForceMode2D.Impulse);
        }
        else
        {
            if (m_WallInFront)
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                facingFront = !facingFront;
                transform.localScale = scale;
                m_Rigidbody2D.velocity = Vector2.zero;
                //runspeed = 0f;

                if (!facingFront)
                    m_Rigidbody2D.AddForce(new Vector2(-m_dataManager.m_WallJumpForce, m_dataManager.m_WallJumpAmplitude), ForceMode2D.Impulse);
                else
                    m_Rigidbody2D.AddForce(new Vector2(m_dataManager.m_WallJumpForce, m_dataManager.m_WallJumpAmplitude), ForceMode2D.Impulse);

                isWallJumping = true;
                //StartCoroutine(WallJump());
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(m_GroundCheck.position, new Vector2(GroundCheckWi, GroundCheckHi));
        Gizmos.DrawWireCube(m_WallCheck.position, new Vector2(WallCheckWi, WallCheckHi));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else if (stream.IsReading)
        {
            smoothMove = (Vector3) stream.ReceiveNext();
        }
    }
}
