using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Anima2D;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private DataManager dataManager;
    private GameController gameController;
    private PlayerMovement playerMovement;
    private Vector3 SpawnPoint;
    private int totalRunners;

    public float FinishDistance;
    public string UserName;
    public bool isWon;
    public bool isFinished;
    public bool canRace;

    public enum RunnerState { run, speedRun, stun};
    public RunnerState currentState;
    public Sprite[] posSprites;
    public Sprite ScoreBoardSprite;
    public SpriteMeshInstance[] skin;
    public GameObject throwingObj;
    public PhotonView pv;




    private void Awake()
    {
        dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        playerMovement = GetComponent<PlayerMovement>();
        canRace = false;

        gameController.PowerUpBtns[0].GetComponent<Button>().onClick.RemoveAllListeners();
        gameController.PowerUpBtns[0].GetComponent<Button>().onClick.AddListener(() => SpeedUp());

        gameController.PowerUpBtns[1].GetComponent<Button>().onClick.RemoveAllListeners();
        gameController.PowerUpBtns[1].GetComponent<Button>().onClick.AddListener(() => ThrowUp());
    }

    private void Start()
    {
        isFinished = false;
        isWon = false;
        canRace = false;

        if (!dataManager.Runners.Contains(GetComponent<PlayerController>()))
        {
            dataManager.Runners.Add(GetComponent<PlayerController>());
        }

        if (pv.IsMine)
        {
            UserName = PhotonNetwork.NickName;
            for (int i = 0; i < skin.Length; i++)
            {
                skin[i].sortingOrder++;
            }
        }
        else
        {
            UserName = pv.Owner.NickName;
        }

    }

    private void Update()
    {
        if(!canRace)
            canRace = gameController.startRace;

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
        if (collision.CompareTag("Finish"))
        {
            isFinished = true;
            FinishedRace();
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
    }

    public void SpeedUp()
    {
        gameController.PowerUpBtns[0].SetActive(false);
        StartCoroutine(BoostSpeed());
    }

    public IEnumerator BoostSpeed()
    {
        float temp = playerMovement.runspeed;
        dataManager.m_TargetSpeed += 30;
        dataManager.m_MaxRunForce += 1000;

        yield return new WaitForSeconds(1.0f);

        dataManager.m_TargetSpeed -= 30;
        dataManager.m_MaxRunForce -= 1000;
        playerMovement.runspeed = temp;
    }

    public void ThrowUp()
    {
        gameController.PowerUpBtns[1].SetActive(false);
        Debug.Log(UserName + " is throwing...");
        SpawnPoint = transform.position + new Vector3(2.0f, 2.0f, 0.0f);
        throwingObj = PhotonNetwork.Instantiate("Thrown", SpawnPoint, Quaternion.identity);
        throwingObj.GetComponent<PowerUpManager>().Slinger = UserName;
    }

    public void GotAttack()
    {
        StartCoroutine(Attacked());
    }

    IEnumerator Attacked()
    {
        canRace = false;
        playerMovement.m_Rigidbody2D.velocity = Vector2.zero;

        yield return new WaitForSeconds(1.5f);
        canRace = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFinished);
            stream.SendNext(UserName);
        }
        else if (stream.IsReading)
        {
            isFinished = (bool) stream.ReceiveNext();
            UserName = (string) stream.ReceiveNext();
        }
    }

}
