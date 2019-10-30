﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using System;

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
    [SerializeField] private Animator m_Animator;

    #endregion

    #region [Public Variables]

    public float GroundCheckWi;
    public float GroundCheckHi;
    public float WallCheckWi;
    public float WallCheckHi;
    public float runspeed;
    public PhotonView pv;

    #endregion

    #region [Private Variables]

    private Rigidbody2D m_Rigidbody2D;
    private Vector3 iniScale;
    private Vector3 iniPos;
    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 smoothMove;
    private bool facingFront;
    private bool isWallJumping;
    private bool isSliding;
    private bool m_Grounded;
    private bool m_WallInFront;
    private PlayerController m_playerController;
    private DataManager m_dataManager;

    bool rn;
    bool idl;

    #endregion

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_playerController = GetComponent<PlayerController>();
        m_dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
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

        if (isWallJumping)
            isWallJumping &= !m_Grounded;

        if (m_WallInFront && !isWallJumping)
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
    }

    private void FixedUpdate()
    {
        if (m_playerController.canRace)
        {
            if (pv.IsMine)
                MoveMe();
            else
                SmoothMove();
        }

        m_WallInFront = Physics2D.OverlapBox(m_WallCheck.position, new Vector2(WallCheckWi, WallCheckHi), 0, m_WhatIsWall);
        m_Grounded = Physics2D.OverlapBox(m_GroundCheck.position, new Vector2(GroundCheckWi, GroundCheckHi), 0, m_WhatIsGround);

        rn = !isWallJumping && !m_WallInFront && facingFront && m_Grounded;
        idl = m_WallInFront && !isWallJumping && !isSliding;

    }

    private void MoveMe()
    {
        if (!isWallJumping && !m_WallInFront && facingFront)
            Run();

        //if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        //Jump();

        //#if UNITY_EDITOR

        //        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() /**/ )
        //            Jump();
        //#endif  

        #if UNITY_ANDROID
            
           if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject())
               {
                    Touch touch = Input.GetTouch(0);
                    if(touch.phase == TouchPhase.Began)
                    Jump();
                }
        #else

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() /**/ )
                    Jump();

        #endif


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


        m_Animator.SetBool("run", rn);
        m_Animator.SetBool("wallslide", isSliding);
        m_Animator.SetBool("idle", idl);
    }

    private void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, smoothMove, Time.deltaTime * 10);
    }

    void Run()
    {
        if (m_playerController.isFinished)
        {
            runspeed = 0;
            m_Rigidbody2D.velocity = Vector2.zero;
        }

        if (m_Rigidbody2D.velocity.x <= m_dataManager.m_BaseSpeed)
            runspeed += Time.deltaTime * m_dataManager.m_MaxRunForce;       
        else if (m_Rigidbody2D.velocity.x > m_dataManager.m_BaseSpeed && m_Rigidbody2D.velocity.x <= m_dataManager.m_TargetSpeed)
            runspeed += Time.deltaTime * m_dataManager.m_MaxRunForce / 10.0f;
        else if (m_Rigidbody2D.velocity.x > m_dataManager.m_TargetSpeed)
            runspeed = m_dataManager.m_TargetSpeed;


        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(runspeed, m_Rigidbody2D.velocity.y);

        // And then smoothing it out and applying it to the character
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }

    void Jump()
    {
        m_Animator.SetTrigger("Jump");

        if (m_Grounded)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.AddForce(new Vector2(m_Rigidbody2D.velocity.x, m_dataManager.m_JumpForce), ForceMode2D.Impulse);

        }
        else
        {
            Debug.Log(m_Grounded);
            if (m_WallInFront)
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                facingFront = !facingFront;
                transform.localScale = scale;
                m_Rigidbody2D.velocity = new Vector2(0, 0);

                if (!facingFront)
                    m_Rigidbody2D.AddForce(new Vector2(-m_dataManager.m_WallJumpForce, m_dataManager.m_WallJumpAmplitude), ForceMode2D.Impulse);
                else
                    m_Rigidbody2D.AddForce(new Vector2(m_dataManager.m_WallJumpForce, m_dataManager.m_WallJumpAmplitude), ForceMode2D.Impulse);

                isWallJumping = true;
                //StartCoroutine(WallJump());
            }
        }
    }

    //IEnumerator WallJump()
    //{
    //    yield return new WaitForSeconds(1f);
    //    /*
    //    while (!m_Grounded)
    //    {
    //        yield return null;
    //    }
    //    */
    //    isWallJumping &= !m_Grounded;
    //}

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