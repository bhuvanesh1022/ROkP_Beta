using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviourPun, IPunObservable
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
    public List<GameObject> VictimRunners = new List<GameObject>();

    public GameObject HitTextPanel;
    public TextMeshProUGUI Thrower;
    public TextMeshProUGUI Victim;
    public string ThrowerName;

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

        for (int i = 0; i < DM.Runners.Count; i++)
        {
            DM.Runners[i].canRace = true;
        }
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
        //SpeedPoweredRunners.Remove(SpeedPoweredRunners[LocalPlayer.GetComponent<PlayerController>().speedingPlayerIndex].gameObject);
    }

    public void ThrowUp()
    {
        PowerUpBtns[1].SetActive(false);

        GameObject throwingObj = PhotonNetwork.Instantiate("Thrown", LocalPlayer.GetComponent<PlayerController>().SpawnPoint.position, Quaternion.identity);
        throwingObj.GetComponent<PowerController>().Thrower = ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex];
        //ThrowPoweredRunners.Remove(ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex].gameObject);
    }

    public void ShowHitText(string thrower, string victim)
    {
        pv.RPC("OnHit", RpcTarget.AllBuffered, thrower, victim);

    }

    [PunRPC]
    public void OnHit(string thrower, string victim)
    {
        ThrowerName = thrower;
        Thrower.text = ThrowerName;
        Victim.text = victim;
        StartCoroutine(EnablingHitText());
    }

    IEnumerator EnablingHitText()
    {
        HitTextPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        HitTextPanel.SetActive(false);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ThrowerName);
        }

        if (stream.IsReading)
        {
            ThrowerName = (string)stream.ReceiveNext();
        }
    }
}
