using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchmakingRoomController : MonoBehaviourPunCallbacks
{
    public static MatchmakingRoomController roomController;

    [SerializeField] private int multiPlayerSceneIndex;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private Transform roomsContainer;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private GameObject playerListingPrefab;
    [SerializeField] private TMP_Text roomNameDisplay;
    [SerializeField] private Image mainPanel;
    [SerializeField] private DataController dataControl;
    [SerializeField] private GameObject tracksPanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private GameObject[] characters;
    [SerializeField] private Sprite[] avatars;
    [SerializeField] private GameObject WaitingPanel;
    [SerializeField] private GameObject startButton;
    [SerializeField] private List<string> PlayerNames = new List<string>();

    private GameObject templisting;
    public int characterSelected;
    public PhotonView pv;

    private void Awake()
    {

        if (roomController == null)
        {
            roomController = this;
        }
        else
        {
            if (roomController != this)
            {
                Destroy(roomController.gameObject);
                roomController = this;
            }
        }
        DontDestroyOnLoad(gameObject);

        characterSelected = 0;

    }

    void ClearRoomListings()
    {
        for (int i = roomsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(roomsContainer.GetChild(i).gameObject);
        }
    }

    void ClearPlayerListings()
    {
        for (int i = playersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playersContainer.GetChild(i).gameObject);
        }
    }

    void ListPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            templisting = Instantiate(playerListingPrefab, playersContainer);
            TMP_Text tempText = templisting.transform.GetChild(0).GetComponent<TMP_Text>();
            tempText.text = player.NickName;

            Image tempImg = templisting.transform.GetChild(1).GetComponent<Image>();
            int CharacterID = (int)player.CustomProperties["Avatar"];
            tempImg.sprite = avatars[CharacterID];

            //CharacterSelection(CharacterID);

            if (PhotonNetwork.IsMasterClient)
                tracksPanel.SetActive(true);
            else
                WaitingPanel.SetActive(true);

            //if (!PlayerNames.Contains(player.NickName))
            //{
            //    PlayerNames.Add(player.NickName);
            //    //characterSelected++;
            //}

            //Debug.Log(PlayerNames.Count);

            //CharacterSelection((int)player.CustomProperties["Avatar"]);
        }
        dataControl.myCharacter = characters[(int)PhotonNetwork.LocalPlayer.CustomProperties["Avatar"]].name;
    }

    public override void OnJoinedRoom()
    {
        Vector4 col = mainPanel.GetComponent<Image>().color;
        col.w = 0.0f;
        mainPanel.GetComponent<Image>().color = col;

        roomPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;

        characterSelected++;
        ClearPlayerListings();
        ListPlayer();
        Debug.Log("Joined");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        characterSelected++;
        ClearPlayerListings();
        ListPlayer();
        Debug.Log("Entered");

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        characterSelected--;
        ClearPlayerListings();
        ListPlayer();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(characterSelected == PhotonNetwork.PlayerList.Length);

            if (characterSelected == PhotonNetwork.PlayerList.Length)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel(multiPlayerSceneIndex);

            }
        }
    }

    IEnumerator RejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    public void BackOnClick()
    {
        ClearRoomListings();
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(RejoinLobby());
    }

    public void TrackSelection(int TrackID)
    {
        //SyncTrackSelection(TrackID);
        pv.RPC("SyncTrackSelection", RpcTarget.AllBuffered, TrackID);
    }

    [PunRPC]
    public void SyncTrackSelection(int ID)
    {
        //dataControl.currentTrack = tracks[ID].name;
        dataControl.trackID = ID;
        tracksPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForOthers());
        }
    }

    public void CharacterSelection(int CharacterID)
    {
        dataControl.myCharacter = characters[CharacterID].name;

        if (PhotonNetwork.IsMasterClient)
            tracksPanel.SetActive(true);
        else
            WaitingPanel.SetActive(true);

        Debug.Log("conn");

        pv.RPC("CharacterJoined", RpcTarget.AllBuffered, null);
    }

    [PunRPC]
    public void CharacterJoined()
    {
        if (!PlayerNames.Contains(PhotonNetwork.LocalPlayer.NickName))
        {
            PlayerNames.Add(PhotonNetwork.LocalPlayer.NickName);
        }


        Debug.Log(characterSelected);
    }

    IEnumerator WaitForOthers()
    {
        while (characterSelected < PhotonNetwork.PlayerList.Length - 1)
        {
            Debug.Log("Waiting");
            yield return new WaitForSeconds(0.02f);
        }

        WaitingPanel.SetActive(false);
        startButton.SetActive(true);
    }

}
