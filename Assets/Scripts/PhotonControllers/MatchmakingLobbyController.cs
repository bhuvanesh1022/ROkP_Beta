using Photon.Pun;
using Photon.Realtime; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchmakingLobbyController : MonoBehaviourPunCallbacks
{
    public static MatchmakingLobbyController lobbyController;
    public Hashtable myCustomProperties = new Hashtable();

    [SerializeField] private GameObject lobbyConnectBtn;
    [SerializeField] private GameObject loadingBtn;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject avatarPanel;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private int avatarId;
    [SerializeField] private bool canRace;
    [SerializeField] private GameObject[] avatarSelectors;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Transform roomsContainer;
    [SerializeField] private GameObject roomListingPrefab;
    [SerializeField] private DataController DataControl;
    [SerializeField] private GameObject roomCreateBtn;


    private List<RoomInfo> roomListings;
    private string roomName;
    private int roomSize = 4;

    private void Awake()
    {
        lobbyController = this;
    }

    public void Update()
    {
        if (playerNameInput.text.Length >= 2)
            lobbyConnectBtn.GetComponent<Button>().interactable = true;
        else
            lobbyConnectBtn.GetComponent<Button>().interactable = false;

        if (roomNameInput.text.Length >= 2)
            roomCreateBtn.GetComponent<Button>().interactable = true;
        else
            roomCreateBtn.GetComponent<Button>().interactable = false;

    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyConnectBtn.SetActive(true);
        loadingBtn.SetActive(false);
        roomListings = new List<RoomInfo>();

        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
            {
                PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
        }
        playerNameInput.text = PhotonNetwork.NickName;
   }

    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        DataControl.myName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
    }

    public void PlayerAvatarUpdate(int Id)
    {
        PhotonNetwork.LocalPlayer.TagObject = Id;
        //myCustomProperties["Avatar"] = Id;
        myCustomProperties.Add("Avatar", Id);
        PhotonNetwork.LocalPlayer.SetCustomProperties(myCustomProperties);

        for (int i = 0; i < avatarSelectors.Length; i++)
        {
            if (i == Id)
                avatarSelectors[i].GetComponent<AvatarSelection>().isSelected = true;
            else
                avatarSelectors[i].GetComponent<AvatarSelection>().isSelected = false;
        }

        JoinLobbyOnClick();
    }

    public void ShowAvatarSelection()
    {
        titlePanel.SetActive(false);
        avatarPanel.SetActive(true);
    }


    public void JoinLobbyOnClick()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int tempIndex;
        foreach (RoomInfo room in roomList)
        {
            if (roomListings != null)
            {
                tempIndex = roomListings.FindIndex(ByName(room.Name));
            }
            else
            {
                tempIndex = -1;
            }

            if (tempIndex != -1)
            {
                roomListings.RemoveAt(tempIndex);
                Destroy(roomsContainer.GetChild(tempIndex).gameObject);
            }

            if (room.PlayerCount > 0)
            {
                roomListings.Add(room);
                ListRoom(room);
            }
        }
    }

    static System.Predicate<RoomInfo> ByName(string name)
    {
        return delegate(RoomInfo room) 
        {
            return room.Name == name;
        };
    }

    void ListRoom(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject tempListing = Instantiate(roomListingPrefab, roomsContainer);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();
            tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }

    public void OnRoomNameChanged(string nameIn)
    {
        roomName = nameIn;
    }

    public void OnRoomSizeChanged(string sizeIn)
    {
        roomSize = int.Parse(sizeIn);
    }

    public void CreateRoom()
    {
        Debug.Log("Creating room now");
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        PhotonNetwork.CreateRoom(roomName, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room but failed, there must already be a room with the same name.");
    }

    public void MatchmakingCancel()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
    }

}
