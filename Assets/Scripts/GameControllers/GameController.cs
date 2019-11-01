using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GameController : MonoBehaviourPun
{
    public static GameController gameController;

    private DataController DC;
    private DataManager DM;
    private Camera myCam;
    private GameObject Runner;

    public float trackLength;
    public GameObject[] Tracks;
    public List<Transform> finishPoints = new List<Transform>();
    public Transform finishLine;
    public GameObject LocalPlayer;
    public GameObject startBtn;
    public GameObject waitingSign;
    public GameObject[] countdown;
    public List<GameObject> PowerUpBtns = new List<GameObject>();
    public List<GameObject> SpeedPoweredRunners = new List<GameObject>();
    public List<GameObject> ThrowPoweredRunners = new List<GameObject>();
    public List<GameObject> GotHitRunners = new List<GameObject>();

    public bool startRace;

    public PhotonView pv;

    private void Awake()
    {
        if (gameController == null)
        {
            gameController = this;
        }

        DC = GameObject.FindWithTag("DataController").GetComponent<DataController>();
        DM = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        myCam = Camera.main;
        startRace = false;
    }

    void Start()
    {
        CreatePlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("SyncTracks", RpcTarget.AllBuffered, DC.trackID);
            startBtn.SetActive(true);
        }
        else
        {
            waitingSign.SetActive(true);
        }
    }

    private void CreatePlayer()
    {
        Runner = PhotonNetwork.Instantiate(Path.Combine(DC.myCharacter), Vector3.zero, Quaternion.identity).gameObject;
        //Runner.GetComponent<PlayerController>().UserName = DC.myName;
        myCam.GetComponent<CameraFollow>().target = Runner.transform;
    }

    [PunRPC]
    public void SyncTracks(int ID)
    {
        Tracks[DC.trackID].SetActive(true);
        finishLine = finishPoints[ID];
        trackLength = Vector3.Distance(Vector3.zero, finishLine.position);
    }


    public void StartRace()
    {
        pv.RPC("LetsGo", RpcTarget.AllBuffered, null);
    }

    [PunRPC]
    public void LetsGo()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        startBtn.SetActive(false);
        waitingSign.SetActive(false);

        for (int i = 0; i < countdown.Length; i++)
        {
            countdown[i].SetActive(true);
            yield return new WaitForSeconds(1f);
            countdown[i].SetActive(false);
        }

        startRace = true;
    }

    public void ReloadApp()
    {
        SceneManager.LoadScene(0);
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    public void SpeedUp()
    {
        PowerUpBtns[0].SetActive(false);
        StartCoroutine(BoostSpeed());
    }

    public IEnumerator BoostSpeed()
    {
        float temp = SpeedPoweredRunners[LocalPlayer.GetComponent<PlayerController>().speedingPlayerIndex].GetComponent<PlayerMovement>().runspeed;
        DM.m_TargetSpeed += 30;
        DM.m_MaxRunForce += 1000;

        yield return new WaitForSeconds(1.0f);

        DM.m_TargetSpeed -= 30;
        DM.m_MaxRunForce -= 1000;
        SpeedPoweredRunners[LocalPlayer.GetComponent<PlayerController>().speedingPlayerIndex].GetComponent<PlayerMovement>().runspeed = temp;
    }

    public void ThrowUp()
    {
        PowerUpBtns[1].SetActive(false);

        GameObject throwingObj = PhotonNetwork.Instantiate("Thrown", LocalPlayer.GetComponent<PlayerController>().SpawnPoint.position, Quaternion.identity);
        throwingObj.GetComponent<PowerController>().Thrower = LocalPlayer;

        Debug.Log(ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex].GetComponent<PlayerController>().UserName + " is throwing...");
        ThrowPoweredRunners.Remove(gameObject);
    }
}
