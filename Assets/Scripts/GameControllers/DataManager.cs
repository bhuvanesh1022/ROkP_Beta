using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviourPun
{
    public List<PlayerController> Runners = new List<PlayerController>();
    public List<PlayerController> RunnersPositioned = new List<PlayerController>();
    public List<PlayerController> FinishedRunners = new List<PlayerController>();
    public List<Slider> RunnerPositions = new List<Slider>();
    public List<Image> RunnerPosSprites = new List<Image>();
    public List<float> DistanceToFinish = new List<float>();

    public float m_BaseSpeed;
    public float m_TargetSpeed;
    public float m_MaxRunForce;
    public float m_JumpForce;
    public float m_GravityMultiplier;
    public float m_TerminalSpeed;
    public float m_WallJumpForce;
    public float m_WallJumpAmplitude;
    public float m_WallSlideGravity;

    public Image Positioned;
    public Sprite[] Positions;
}
