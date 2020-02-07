using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobby : MonoBehaviourPunCallbacks
{

	[Header("Photon Settings")]
	public string gameVersion;

	private List<RoomInfo> onlineRooms = new List<RoomInfo>();

	[Header("UI")]
	public UI_ServerBrowser serverBrowser;

	[Header("Debugging")]
	public bool logMode;

	private void Start()
	{
		ConnectWithPhoton();
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			PhotonNetwork.CreateRoom("name", new RoomOptions { MaxPlayers = 2 }, new TypedLobby("Lobby 1", LobbyType.Default));
		}
	}

	private void ConnectWithPhoton()
	{
		PhotonNetwork.ConnectUsingSettings();
		PhotonNetwork.GameVersion = gameVersion;
	}

	private void ShowRoomsOnUI()
	{
		serverBrowser.UpdateRoomInfoButtons(onlineRooms);
	}

	#region Callbacks

	public override void OnConnectedToMaster()
	{
		if (logMode)
			Debug.Log("Connected to Master");

		PhotonNetwork.JoinLobby(new TypedLobby("Lobby 1", LobbyType.Default));
	}

	public override void OnJoinedLobby()
	{
		if (logMode)
			Debug.Log("Connected to Lobby");

		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();
		uiManager.ShowWindow("Server Browser");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		print(roomList.Count);
		onlineRooms = roomList;
		ShowRoomsOnUI();
	}

	public override void OnJoinedRoom()
	{
		if (logMode)
			Debug.Log("Joined a Room");
	}

	#endregion

}
