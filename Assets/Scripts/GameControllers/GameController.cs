using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class GameController : MonoBehaviourPun, IPunObservable
{
    public static GameController gameController;

    private AudioControl AC;
    private AudioSource AS;
    private DataController DC;
    private DataManager DM;
    private Camera myCam;
    private GameObject Runner;
    private MatchmakingRoomController roomController;
    private DirectorCall directorCall;

    public float trackLength;
    public GameObject[] Tracks;
    public List<Transform> finishPoints = new List<Transform>();
    public Transform finishLine;
    public List<Transform> podiums = new List<Transform>();
    public Transform WinningPodium;
    public GameObject LocalPlayer;
    public List<Transform> spawnPoints = new List<Transform>();
    public GameObject startBtn;
    public GameObject waitingSign;
    public GameObject[] countdown;
    public List<GameObject> PowerUpBtns = new List<GameObject>();
    public List<GameObject> SpeedPoweredRunners = new List<GameObject>();
    public List<GameObject> ThrowPoweredRunners = new List<GameObject>();
    public List<GameObject> VictimRunners = new List<GameObject>();
    public GameObject ScoreBoard;
    public List<GameObject> ScoreCards = new List<GameObject>();
    public List<Image> RunnerInScorecard = new List<Image>();
    public List<TextMeshProUGUI> maxUsers = new List<TextMeshProUGUI>();
    public GameObject HitTextPanel;
    public GameObject speedLine;
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

        directorCall = GetComponent<DirectorCall>();
        AS = GetComponent<AudioSource>();
        AC = GameObject.FindWithTag("AudioManager").GetComponent<AudioControl>();
        DC = GameObject.FindWithTag("DataController").GetComponent<DataController>();
        DM = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        roomController = GameObject.FindWithTag("RoomController").GetComponent<MatchmakingRoomController>();
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

    public void Update()
    {
        for (int i = 0; i < DM.Runners.Count; i++)
        {
            if (!DM.Runners[i].isFinished && DM.Runners[i].startTimer)
                DM.Runners[i].elapsedTime += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (PowerUpBtns[0].activeInHierarchy)
            {
                SpeedUp();
            }
            else if (PowerUpBtns[1].activeInHierarchy)
            {
                ThrowUp();
            }
        }

    }

    private void CreatePlayer()
    {
        Runner = PhotonNetwork.Instantiate(Path.Combine(DC.myCharacter), Vector3.zero, Quaternion.identity).gameObject;
        myCam.GetComponent<CameraFollow>().target = Runner.transform;

        if (!Runner.GetComponent<DirectorController>())
        {
            Runner.transform.position = gameController.spawnPoints[roomController.enteredAt - 1].position;
        }
        else
        {
            Runner.transform.position = myCam.transform.position;
            directorCall.director.SetActive(true);
            DM.Positioned.transform.parent.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void SyncTracks(int ID)
    {
        Tracks[DC.trackID].SetActive(true);
        finishLine = finishPoints[ID];
        WinningPodium = podiums[ID];
        trackLength = Vector3.Distance(Vector3.zero, finishLine.position);
    }


    public void StartRace()
    {
        if (directorCall.charactersToFollow.Count != 0)
        {
            directorCall.ChangeDirectorMode(1);
        }

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
        AC.GetComponent<AudioSource>().Stop();

        for (int i = 0; i < countdown.Length; i++)
        {
            countdown[i].SetActive(true);
            yield return new WaitForSeconds(1f);
            countdown[i].SetActive(false);
        }

        startRace = true;
        int a = Random.Range(0, AC.BG_Game.Length);
        AC.GetComponent<AudioSource>().clip = AC.BG_Game[a];
        AC.GetComponent<AudioSource>().Play();

        for (int i = 0; i < DM.Runners.Count; i++)
        {
            DM.Runners[i].canRace = true;
            DM.Runners[i].startTimer = true;
        }
    }

    public void ReloadApp()
    {
        AC.GetComponent<AudioSource>().Stop();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    public IEnumerator CameraRushIn()
    {
        if (!gameController.LocalPlayer.GetComponent<DirectorController>())
        {
            myCam.GetComponent<CameraFollow>().offset.y = 2;
            while (myCam.orthographicSize > 5)
            {
                myCam.orthographicSize -= 0.5f;
                yield return null;
            }
            myCam.orthographicSize = 7f;

            yield return new WaitForSeconds(1.0f);

            while (myCam.orthographicSize < 10)
            {
                myCam.orthographicSize += 0.1f;
                yield return null;
            }
            myCam.orthographicSize = 10f;
            myCam.GetComponent<CameraFollow>().offset.y = 6;
        }
    }

    public void shakeCamera(float dur, float mag, float vect)
    {
        StartCoroutine(ShakyCamera(dur, mag, vect));
    }

    public IEnumerator ShakyCamera(float dur, float mag, float  vect)
    {
        Vector3 iniPos = myCam.transform.localPosition;
        float elapse = 0.0f;
        while (elapse < dur)
        {
            float x = Random.Range(-vect, vect) * mag;
            float y = Random.Range(-vect, vect) * mag;
            myCam.transform.localPosition = new Vector3(iniPos.x + x, iniPos.y + y, iniPos.z);
            elapse += Time.deltaTime;
            yield return null;
        }
        myCam.transform.localPosition = iniPos;
    }

    public IEnumerator CameraFlyOff()
    {
        if (!gameController.LocalPlayer.GetComponent<DirectorController>())
        {
            myCam.GetComponent<CameraFollow>().offset.x = 15;
            while (myCam.orthographicSize < 13)
            {
                myCam.orthographicSize += 0.5f;
                yield return null;
            }
            myCam.orthographicSize = 13f;

            yield return new WaitForSeconds(.5f);

            while (myCam.orthographicSize > 10)
            {
                myCam.orthographicSize -= 0.1f;
                yield return null;
            }
            myCam.orthographicSize = 10f;
            myCam.GetComponent<CameraFollow>().offset.x = 6.5f;
        }
    }

    public void SpeedUp()
    {
        LocalPlayer.GetComponent<PlayerController>().SpeedBoost();
        PlayAudioFX("speedBoost");
    }

    public void PlayAudioFX(string fx)
    {
        pv.RPC("PlayAudioGlobally", RpcTarget.AllBuffered, fx);
    }

    [PunRPC]
    public void PlayAudioGlobally(string powerType)
    {
        switch (powerType)
        {
            case "speedBoost":

                AS.clip = AC.SpeedBoost;
                break;

            case "hit":

                AS.clip = AC.Hit;
                break;
        }
        AS.Play();
    }

    public void ThrowUp()
    {
        PowerUpBtns[1].SetActive(false);
        StartCoroutine(CameraFlyOff());

        if (pv.IsMine)
        {
            AS.clip = AC.Thrower;
            AS.Play();
        }

        ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex].GetComponent<PlayerController>().NoOfThrows++;
        GameObject throwingObj = PhotonNetwork.Instantiate("Thrown", LocalPlayer.GetComponent<PlayerController>().SpawnPoint.position, Quaternion.identity);
        //throwingObj.GetComponent<PowerController>().Thrower = ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex];
        throwingObj.GetComponent<PowerController>().ThrowerName = ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex].GetComponent<PlayerController>().UserName;
        //ThrowPoweredRunners.Remove(ThrowPoweredRunners[LocalPlayer.GetComponent<PlayerController>().throwingPlayerIndex].gameObject);
    }

    public void GotHit(string thrower, string victim)
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
