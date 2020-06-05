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

	private static bool hasJoinedLobbyBefore;

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

		print("START");

		NetworkManager.onRoomListChange += ShowRoomsOnUI;

		if (hasJoinedLobbyBefore)
			Invoke(nameof(ReturnedFromGameScene), 0.5f);
	}

	private void ReturnedFromGameScene()
	{
		ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Server Browser");
	}

	public bool HostRoom(string roomName, string password = "")
	{
		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();

		if (!ServiceLocator.GetServiceOfType<NetworkManager>().CheckStringForProfanity(roomName))
		{
			uiManager.ShowMessage("Do not use profanity in your server name.");
			return false;
		}

		if (ServerNameIsAvailable(roomName))
		{
			RoomOptions roomOptions = new RoomOptions();
			roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
			roomOptions.CustomRoomProperties.Add("PW", password);
			roomOptions.MaxPlayers = maxPlayers;
			roomOptions.CustomRoomPropertiesForLobby = new string[] { "PW" };

			PhotonNetwork.CreateRoom(roomName, roomOptions, new TypedLobby("Lobby 1", LobbyType.Default));

			return true;
		}
		else
		{
			uiManager.ShowMessage("That server name is already in use.");
			return false;
		}
	}

	private bool ServerNameIsAvailable(string newServername)
	{
		for (int i = 0; i < NetworkManager.onlineRooms.Count; i++)
		{
			if (NetworkManager.onlineRooms[i].Name == newServername)
			{
				return false;
			}
		}

		return true;
	}

	public void JoinRoom(string roomName)
	{
		PhotonNetwork.JoinRoom(roomName);
	}

	public bool ServerHasPassword(string roomName)
	{
		for (int i = 0; i < NetworkManager.onlineRooms.Count; i++)
		{
			if (NetworkManager.onlineRooms[i].Name == roomName)
			{
				return (string)NetworkManager.onlineRooms[i].CustomProperties["PW"] != "";
			}
		}

		return false;
	}

	public bool PasswordMatches(string roomName, string password)
	{
		for (int i = 0; i < NetworkManager.onlineRooms.Count; i++)
		{
			if (NetworkManager.onlineRooms[i].Name == roomName)
			{
				string passwordOfRoom = (string)NetworkManager.onlineRooms[i].CustomProperties["PW"];

				if (logMode)
					print(passwordOfRoom);

				return passwordOfRoom == password;
			}
		}

		return false;
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
		print("SHOW ROOMS IN UI");

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

		if (!hasJoinedLobbyBefore)
			ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Titlescreen");
		else
			ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Server Browser");

		hasJoinedLobbyBefore = true;
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