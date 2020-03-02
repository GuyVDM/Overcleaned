using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks, IServiceOfType
{
	#region Initalize Service
	private void Awake() => OnInitialise();
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	public enum PlayerListMode
	{
		PlayerLeft,
		PlayerJoined,
		LocalPlayerJoined,
	}

	//Variables
	public bool logMode;

	//static events
	public static event OnRoomListChange onRoomListChange;
	public static event OnPlayerListChange onPlayerListChange;
	public static event OnMasterClientSwitch onMasterClientSwitch;
	public static event OnLocalPlayerLeft onLocalPlayerLeft;

	//delegates for static events
	public delegate void OnRoomListChange(List<RoomInfo> allRooms);
	public delegate void OnPlayerListChange(Player[] allPlayers, Player changedPlayer, PlayerListMode playerListMode);
	public delegate void OnMasterClientSwitch(Player newMasterClient);
	public delegate void OnLocalPlayerLeft();

	public static LocalPlayerInformation localPlayerInformation { get; private set; }

	private List<RoomInfo> onlineRooms = new List<RoomInfo>();

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	public static void SetLocalPlayerInfo(int team, int numberInTeam)
	{
		localPlayerInformation.team = team;
		localPlayerInformation.numberInTeam = numberInTeam;
	}

	public void SetLocalPlayerNickname(string nickname)
	{
		PhotonNetwork.LocalPlayer.NickName = nickname;
	}

	public void SetLocalPlayerNickname(InputField inputField)
	{
		SetLocalPlayerNickname(inputField.text);
	}

	private void ReturnToMainMenu()
	{
		SceneHandler sceneHandler = ServiceLocator.GetServiceOfType<SceneHandler>();
		sceneHandler.LoadScene(0);
	}

	#region Callbacks

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		if (logMode)
			Debug.Log("Room List Update");

		for (int i = 0; i < roomList.Count; i++)
		{
			if (roomList[i].MaxPlayers > 0 && roomList[i].PlayerCount > 0 && roomList[i].IsOpen)
			{
				if (onlineRooms.Contains(roomList[i]))
					onlineRooms.Remove(roomList[i]);

				onlineRooms.Add(roomList[i]);
			}
			else
			{
				if (onlineRooms.Contains(roomList[i]))
					onlineRooms.Remove(roomList[i]);
			}
		}

		print(onlineRooms.Count);
		onRoomListChange(onlineRooms);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		ReturnToMainMenu();
	}



	public override void OnJoinedLobby()
	{
		localPlayerInformation = new LocalPlayerInformation(PhotonNetwork.LocalPlayer);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (logMode)
			Debug.Log("Player List Update");

		onPlayerListChange?.Invoke(PhotonNetwork.PlayerList, newPlayer, PlayerListMode.PlayerJoined);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (logMode)
			Debug.Log("Player List Update");

		onPlayerListChange?.Invoke(PhotonNetwork.PlayerList, otherPlayer, PlayerListMode.PlayerLeft);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if (logMode)
			Debug.Log("Master client switched");

		PhotonNetwork.CurrentRoom.IsOpen = false;
		onMasterClientSwitch?.Invoke(newMasterClient);
	}

	public override void OnJoinedRoom()
	{
		if (logMode)
			Debug.Log("Player List Update");

		onPlayerListChange?.Invoke(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer, PlayerListMode.LocalPlayerJoined);
	}

	public override void OnLeftRoom()
	{
		if (logMode)
			Debug.Log("Local player Left");

		onLocalPlayerLeft?.Invoke();
	}
    #endregion

    #region Statics
    public static bool IsConnectedAndInRoom => PhotonNetwork.IsConnected && PhotonNetwork.InRoom;

    public static PhotonView GetViewByID(int viewID) => PhotonNetwork.PhotonViews.Where(o => o.ViewID == viewID).First();
    #endregion
}
