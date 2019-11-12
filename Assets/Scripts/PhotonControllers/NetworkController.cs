using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public static NetworkController networkController;
    public AudioSource BG;

    private void Awake()
    {
        networkController = this;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }

    public void ReloadApp()
    {
        BG.Stop();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
