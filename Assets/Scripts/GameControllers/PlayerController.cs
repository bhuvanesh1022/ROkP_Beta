using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Anima2D;
using TMPro;

public class PlayerController : MonoBehaviourPun, IPunObservable
{

    private DataManager dataManager;
    private GameController gameController;
    private PlayerMovement playerMovement;
    private int totalRunners;

    public float FinishDistance;
    public string UserName;
    public bool isWon;
    public bool isFinished;
    public bool canRace;
    public bool startTimer;
    public float elapsedTime;
    public int speedingPlayerIndex;
    public int throwingPlayerIndex;
    public int NoOfJumps;
    public int NoOfThrows;
    public int NoOfHits;
    public int NoOfSpeedBoost;

    public enum RunnerState { run, speedRun, stun};
    public RunnerState currentState;
    public Transform SpawnPoint;
    public GameObject TriggeredBy;
    public Sprite[] posSprites;
    public Sprite ScoreBoardSprite;
    public SpriteMeshInstance[] skin;
    public PhotonView pv;

    private void Awake()
    {
        dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        playerMovement = GetComponent<PlayerMovement>();
        canRace = false;

        if (pv.IsMine)
        {
            gameController.LocalPlayer = gameObject;
        }
    }

    private void Start()
    {
        isFinished = false;
        isWon = false;

        if (!dataManager.Runners.Contains(GetComponent<PlayerController>()))
        {
            dataManager.Runners.Add(GetComponent<PlayerController>());
        }

        if (pv.IsMine)
        {
            UserName = PhotonNetwork.NickName;

            for (int i = 0; i < skin.Length; i++)
            {
                skin[i].sortingOrder = skin[i].sortingOrder + 10;
            }
        }
        else
        {
            UserName = pv.Owner.NickName;
        }

    }

    private void Update()
    {
        if (gameController.SpeedPoweredRunners.Contains(gameObject))
        {
            speedingPlayerIndex = gameController.SpeedPoweredRunners.FindIndex(x => x.GetComponent<PlayerController>().UserName == UserName);
        }
        else
        {
            speedingPlayerIndex = -1;
        }


        if (gameController.ThrowPoweredRunners.Contains(gameObject))
        {
            throwingPlayerIndex = gameController.ThrowPoweredRunners.FindIndex(x => x.GetComponent<PlayerController>().UserName == UserName);
        }
        else
        {
            throwingPlayerIndex = -1;
        }

        //if (!canRace)
            //canRace = gameController.startRace;

        if (!isFinished)
        {
            pv.RPC("SyncPosition", RpcTarget.AllBuffered, null);
        }


    }

    [PunRPC]
    public void SyncPosition()
    {
        totalRunners = dataManager.Runners.Count;

        for (int i = 0; i < totalRunners; i++)
        {
            dataManager.RunnerPositions[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < totalRunners; i++)
        {
            if (dataManager.Runners[i].gameObject != gameObject)
            {
                if (dataManager.DistanceToFinish[i] > 0)
                {
                    dataManager.RunnerPositions[i].gameObject.SetActive(true);
                }
                else
                {
                    dataManager.RunnerPositions[i].gameObject.SetActive(false);
                }
                dataManager.RunnerPositions[i].value = dataManager.DistanceToFinish[i];
            }
            else
            {
                dataManager.RunnerPositions[i].gameObject.SetActive(true);

                FinishDistance = Vector3.Distance(transform.position, gameController.finishLine.position);
                dataManager.RunnerPositions[i].value = 1 - (FinishDistance / gameController.trackLength);
                dataManager.DistanceToFinish[i] = 1 - (FinishDistance / gameController.trackLength);
                dataManager.RunnerPosSprites[i].sprite = dataManager.Runners[i].ScoreBoardSprite;
            }
        }

        for (int i = 0; i < totalRunners; i++)
        {

        }

        dataManager.RunnersPositioned = dataManager.Runners;

        dataManager.RunnersPositioned.Sort
            (delegate (PlayerController a, PlayerController b)
                {
                    return 
                        (a.FinishDistance).CompareTo (b.FinishDistance);
                }
            );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggeredBy = collision.gameObject;

        switch (TriggeredBy.tag)
        {
            case "Finish":

                isFinished = true;
                FinishedRace();
                break;

            case "Thrown":

                if (!gameController.VictimRunners.Contains(gameObject))
                {
                    gameController.VictimRunners.Add(gameObject);
                }

                TriggeredBy.GetComponent<Collider2D>().enabled = false;
                Destroy(TriggeredBy, .5f);
                break;
        }

    }

    public void FinishedRace()
    {
        if (!dataManager.FinishedRunners.Contains(GetComponent<PlayerController>()))
        {
            dataManager.FinishedRunners.Add(GetComponent<PlayerController>());

            for (int i = 0; i < dataManager.FinishedRunners.Count; i++)
            {
                if (i == 0)
                {
                    dataManager.FinishedRunners[i].isWon = true;
                }

                if (dataManager.FinishedRunners[i].isFinished)
                {
                    if (dataManager.FinishedRunners[i].isWon)
                        dataManager.FinishedRunners[i].playerMovement.m_Animator.SetBool("win", true);

                    else
                        dataManager.FinishedRunners[i].playerMovement.m_Animator.SetBool("loss", true);
                }
            }
        }

        if (pv.IsMine)
        {
            pv.RPC("ShowScoreCard", RpcTarget.AllBuffered, null);
        }
    }

    [PunRPC]
    public void ShowScoreCard()
    {
        if (gameController.LocalPlayer.GetComponent<PlayerController>().isFinished)
        {
            StartCoroutine(EnablingScoreBoard());

            int maxJump = 0, maxThrow = 0, maxHit = 0, maxSpeedBoost = 0;
            string maxJumpUser = "", maxThrowUser = "", maxHitUser = "", maxSpeedBoostUser = "";

            for (int i = 0; i < dataManager.Runners.Count; i++)
            {
                if (maxJump < dataManager.Runners[i].GetComponent<PlayerController>().NoOfJumps)
                {
                    maxJump = dataManager.Runners[i].GetComponent<PlayerController>().NoOfJumps;
                    maxJumpUser = dataManager.Runners[i].GetComponent<PlayerController>().UserName;
                }

                if (maxThrow < dataManager.Runners[i].GetComponent<PlayerController>().NoOfThrows)
                {
                    maxThrow = dataManager.Runners[i].GetComponent<PlayerController>().NoOfThrows;
                    maxThrowUser = dataManager.Runners[i].GetComponent<PlayerController>().UserName;
                }

                if (maxHit < dataManager.Runners[i].GetComponent<PlayerController>().NoOfHits)
                {
                    maxHit = dataManager.Runners[i].GetComponent<PlayerController>().NoOfHits;
                    maxHitUser = dataManager.Runners[i].GetComponent<PlayerController>().UserName;
                }

                if (maxSpeedBoost < dataManager.Runners[i].GetComponent<PlayerController>().NoOfSpeedBoost)
                {
                    maxSpeedBoost = dataManager.Runners[i].GetComponent<PlayerController>().NoOfSpeedBoost;
                    maxSpeedBoostUser = dataManager.Runners[i].GetComponent<PlayerController>().UserName;
                }
            }

            gameController.maxUsers[0].text = maxJumpUser;
            gameController.maxUsers[1].text = maxThrowUser;
            gameController.maxUsers[2].text = maxHitUser;
            gameController.maxUsers[3].text = maxSpeedBoostUser;

        }
    }

    IEnumerator EnablingScoreBoard()
    {
        yield return new WaitForSeconds(0.5f);
        gameController.ScoreBoard.SetActive(true);
        yield return new WaitForSeconds(2.0f);

        for (int i = 0; i < dataManager.FinishedRunners.Count; i++)
        {
            gameController.ScoreCards[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dataManager.FinishedRunners[i].UserName;
            float timeElapsed = dataManager.FinishedRunners[i].elapsedTime;
            gameController.ScoreCards[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = timeElapsed.ToString("00.00");
            gameController.RunnerInScorecard[i].sprite = dataManager.FinishedRunners[i].GetComponent<PlayerController>().ScoreBoardSprite;
            gameController.RunnerInScorecard[i].color = Color.white;
            gameController.RunnerInScorecard[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public void GotAttack()
    {
        gameController.VictimRunners.Remove(gameObject);
        gameController.shakeCamera(0.15f, 0.2f, 2.0f);

        if (pv.IsMine)
        {
            StartCoroutine(Attacked());
            NoOfHits++;
        }
    }

    IEnumerator Attacked()
    {
        canRace = false;
        StartCoroutine(StunRunner(transform.position));

        yield return new WaitForSeconds(2f);
        canRace = true;
    }

    IEnumerator StunRunner(Vector3 pos)
    {
        while (!canRace)
        {
            transform.position = pos;
            yield return null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFinished);
            stream.SendNext(UserName);
            stream.SendNext(NoOfJumps);
            stream.SendNext(NoOfThrows);
            stream.SendNext(NoOfHits);
            stream.SendNext(NoOfSpeedBoost);
            stream.SendNext(elapsedTime);
        }
        else if (stream.IsReading)
        {
            isFinished = (bool) stream.ReceiveNext();
            UserName = (string) stream.ReceiveNext();
            NoOfJumps = (int)stream.ReceiveNext();
            NoOfThrows = (int)stream.ReceiveNext();
            NoOfHits = (int)stream.ReceiveNext();
            NoOfSpeedBoost = (int)stream.ReceiveNext();
            elapsedTime = (float)stream.ReceiveNext();
        }
    }

}
