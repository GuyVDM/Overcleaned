using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobby : MonoBehaviourPunCallbacks, IServiceOfType
{
	[Header("Photon Settings")]
	public string gameVersion;
	public byte maxPlayers; 

	[Header("UI")]
	public UI_ServerBrowser serverBrowser;

	[Header("Debugging")]
	public bool logMode;

	#region Initalize Service
	private void Awake() => OnInitialise();
	private void OnDestroy()
	{
		NetworkManager.onRoomListChange -= ShowRoomsOnUI;
		OnDeinitialise();
	}
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		if (!PhotonNetwork.IsConnected)
			ConnectWithPhoton();

		NetworkManager.onRoomListChange += ShowRoomsOnUI;
	}

	public bool HostRoom(string roomName)
	{
		if (ServiceLocator.GetServiceOfType<NetworkManager>().ServerNameIsAvailable(roomName))
		{
			PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayers }, new TypedLobby("Lobby 1", LobbyType.Default));
			return true;
		}
		else
		{
			ServiceLocator.GetServiceOfType<UIManager>().ShowMessage("That server name is already in use.");
			return false;
		}
	}

	public void JoinRoom(string roomName)
	{
		PhotonNetwork.JoinRoom(roomName);
	}

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	private void ConnectWithPhoton()
	{
		PhotonNetwork.ConnectUsingSettings();
		PhotonNetwork.GameVersion = gameVersion;
	}

	private void ShowRoomsOnUI(List<RoomInfo> allRooms)
	{
		serverBrowser.UpdateRoomInfoButtons(allRooms);
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

		ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Name Input");
	}

	public override void OnJoinedRoom()
	{
		if (logMode)
			Debug.Log("Joined a Room");

		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();
		uiManager.ShowWindow("Room Information");
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		if (logMode)
			Debug.LogError("Room Join Failed: " + message);
	}

	#endregion

}