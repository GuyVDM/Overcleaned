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

		PhotonNetwork.JoinLobby(new TypedLobby("Standard Lobby", LobbyType.Default));
	}

	public override void OnJoinedLobby()
	{
		if (logMode)
			Debug.Log("Connected to Lobby");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		onlineRooms = roomList;
		ShowRoomsOnUI();
	}

	#endregion

}
