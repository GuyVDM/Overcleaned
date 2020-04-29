using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class UI_RoomInformationWindow : UIWindow
{
	public class PlayerUIElement
	{
		public bool IsLocal { get; private set; }
		public bool IsReady { get; private set; }
		public string PlayerName { get; private set; }
		public int PlayerNumber { get; private set; }
		public int teamNumber { get; private set; }

		private GameObject uiObject;
		private Dropdown teamDropdown;
		private Text playerNameText, teamText, readyText;
		private Toggle readyToggle;
		private Button kickPlayer;

		public PlayerUIElement(Player player, Transform uiReference)
		{
			uiObject = uiReference.gameObject;
			teamDropdown = uiReference.Find("Team Dropdown").GetComponent<Dropdown>();
			teamText = uiReference.Find("Team Text").GetComponent<Text>();
			playerNameText = uiReference.Find("Player Name").GetComponent<Text>();
			readyToggle = uiReference.Find("Ready Toggle").GetComponent<Toggle>();
			readyText = uiReference.Find("Ready Text").GetComponent<Text>();
			kickPlayer = uiReference.Find("Kick Player Button").GetComponent<Button>();

			teamDropdown.onValueChanged.AddListener(delegate { SetTeamNumber(teamDropdown.value); });
			readyToggle.onValueChanged.AddListener(delegate { SetReadyValue(readyToggle.isOn); });
			kickPlayer.onClick.AddListener(delegate { KickPlayer(player); });
			teamText.text = teamDropdown.options[0].text;

			AssignToPlayer(player);
		}

		public void DestroyReference()
		{
			Destroy(uiObject);
		}

		public void AssignToPlayer(Player player)
		{
			IsLocal = player.IsLocal;
			PlayerName = player.NickName;
			PlayerNumber = player.ActorNumber;

			teamDropdown.gameObject.SetActive(IsLocal);
			readyToggle.gameObject.SetActive(IsLocal);
			teamText.gameObject.SetActive(!IsLocal);
			readyText.gameObject.SetActive(!IsLocal);
			kickPlayer.gameObject.SetActive(PhotonNetwork.IsMasterClient && !player.IsLocal);

			print(PlayerName);
			playerNameText.text = PlayerName;
		}

		public void AddListenerToDropDown(UnityEngine.Events.UnityAction<int> call)
		{
			teamDropdown.onValueChanged.AddListener(call);
		}

		public void AddListenerToReadyToggle(UnityEngine.Events.UnityAction<bool> call)
		{
			readyToggle.onValueChanged.AddListener(call);
		}

		public int GetTeamNumber()
		{
			return teamNumber;
		}

		public int GetTeamCount()
		{
			return teamDropdown.options.Count;
		}

		public bool GetToggleValue()
		{
			return readyToggle.isOn;
		}

		public void ChangeTeamText(int dropdownIndex)
		{
			teamText.text = teamDropdown.options[dropdownIndex].text;
			teamNumber = dropdownIndex;
		}

		public void SetReady(bool isReady)
		{
			readyText.text = isReady ? "Ready" : "Not Ready";
			SetReadyValue(isReady);
		}

		private void KickPlayer(Player playerToKick)
		{
			if (PhotonNetwork.IsMasterClient)
				PhotonNetwork.CloseConnection(playerToKick);
		}

		private void SetTeamNumber(int newTeamNumber)
		{
			teamNumber = newTeamNumber;
		}

		private void SetReadyValue(bool isReady)
		{
			IsReady = isReady;
		}
	}

	public int sceneToLoad;
	public Button startGameButton;

	[Header("Prefabs")]
	public GameObject playerInRoomElementPrefab;
	[Header("Parents")]
	public Transform playerElementParent;

	private List<PlayerUIElement> allPlayerElements = new List<PlayerUIElement>();
	private PhotonView photonView;

	public override void Awake()
	{
		base.Awake();
		photonView = GetComponent<PhotonView>();
	}

	public void Start()
	{
		NetworkManager.onPlayerListChange += UpdatePlayerList;
		NetworkManager.onMasterClientSwitch += MasterClientLeft;
		NetworkManager.onLocalPlayerLeft += LocalPlayerLeft;
	}

	protected override void OnWindowEnabled()
	{
		startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
	}

	public void LeaveServer()
	{
		if (PhotonNetwork.CurrentRoom != null)
		{
			ServiceLocator.GetServiceOfType<PhotonLobby>().LeaveRoom();
			ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Server Browser");
		}
	}

	public void StartGame()
	{
		if (ServiceLocator.GetServiceOfType<NetworkManager>().debugMode|| CanStartGame())
		{
			PhotonNetwork.CurrentRoom.IsOpen = false;
			photonView.RPC(nameof(StartGameRPC), RpcTarget.All);
		}
	}

	private void OnDestroy()
	{
		NetworkManager.onPlayerListChange -= UpdatePlayerList;
		NetworkManager.onMasterClientSwitch -= MasterClientLeft;
		NetworkManager.onLocalPlayerLeft -= LocalPlayerLeft;
	}

	#region Checks for Start Game
	private bool CanStartGame()
	{
		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();

		if (!NumberOfPlayersIsCorrect())
		{
			uiManager.ShowMessage("You need 2 or 4 players to start the game!");
			return false;
		}

		if (!TeamsAreCorrect())
		{
			uiManager.ShowMessage("All teams need to be equal!");
			return false;
		}

		if (!AllPlayersAreReady())
		{
			uiManager.ShowMessage("All players need to be ready!");
			return false;
		}

		return true;
	}

	private bool NumberOfPlayersIsCorrect()
	{
		if (PhotonNetwork.CurrentRoom.PlayerCount == 2 || PhotonNetwork.CurrentRoom.PlayerCount == 4)
			return true;

		return false;
	}

	private bool TeamsAreCorrect()
	{
		int[] teamCount = new int[allPlayerElements[0].GetTeamCount()];

		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			teamCount[allPlayerElements[i].teamNumber]++;
		}

		for (int i = 0; i < teamCount.Length; i++)
		{
			if (teamCount[i] != allPlayerElements.Count / teamCount.Length)
				return false;
		}

		return true;
	}

	private bool AllPlayersAreReady()
	{
		int success = 0;
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].IsReady)
			{
				print("READY");
				success++;
			}
		}

		if (success == allPlayerElements.Count)
			return true;
		else
			return false;
	}
	#endregion

	#region Events

	private void UpdatePlayerList(Player[] playerList, Player changedPlayer, NetworkManager.PlayerListMode playerListMode)
	{
		switch (playerListMode)
		{
			case NetworkManager.PlayerListMode.PlayerJoined:

				CreatePlayerElement(changedPlayer);

				break;

			case NetworkManager.PlayerListMode.PlayerLeft:

				DeletePlayerElement(changedPlayer);

				break;

			case NetworkManager.PlayerListMode.LocalPlayerJoined:

				for (int i = 0; i < playerList.Length; i++)
				{
					CreatePlayerElement(playerList[i]);
				}

				break;
		}
	}

	private void MasterClientLeft(Player newMasterClient)
	{
		ServiceLocator.GetServiceOfType<UIManager>().ShowMessage("The host has left the server.");
		LeaveServer();
	}

	private void LocalPlayerLeft()
	{
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			allPlayerElements[i].DestroyReference();
		}

		allPlayerElements.Clear();
	}

	#endregion

	private PlayerUIElement CreatePlayerElement(Player player)
	{
		GameObject newObject = Instantiate(playerInRoomElementPrefab, Vector3.zero, Quaternion.identity);
		newObject.transform.SetParent(playerElementParent, false);

		PlayerUIElement toReturn = new PlayerUIElement(player, newObject.transform);

		toReturn.AddListenerToDropDown(delegate { UpdatePlayerElement(player.ActorNumber, toReturn.teamNumber); });
		toReturn.AddListenerToReadyToggle(delegate { SetReady(player.ActorNumber, toReturn.GetToggleValue()); });
		allPlayerElements.Add(toReturn);

		return toReturn;
	}

	private void DeletePlayerElement(Player player)
	{
		PlayerUIElement toDestroy = FindUIElementByPlayerNumber(player.ActorNumber);
		allPlayerElements.Remove(toDestroy);
		toDestroy.DestroyReference();
	}

	private int GetNumberInTeam(int myTeamIndex)
	{
		int toReturn = 0;
		PlayerUIElement localPlayerElement = FindLocalPlayerElement();
		List<PlayerUIElement> membersOfMyTeam = new List<PlayerUIElement>();
		membersOfMyTeam.Add(localPlayerElement);

		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].GetTeamNumber() == localPlayerElement.teamNumber)
			{
				membersOfMyTeam.Add(allPlayerElements[i]);
			}
		}

		for (int i = 0; i < membersOfMyTeam.Count; i++)
		{
			if (membersOfMyTeam[i].PlayerNumber > localPlayerElement.PlayerNumber)
			{
				toReturn++;
			}
		}

		return toReturn;
	}

	private PlayerUIElement FindUIElementByPlayerNumber(int playerNumber)
	{
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].PlayerNumber == playerNumber)
			{
				return allPlayerElements[i];
			}
		}

		return null;
	}

	private PlayerUIElement FindLocalPlayerElement()
	{
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].IsLocal)
				return allPlayerElements[i];
		}

		return null;
	}

	#region Functions for RPC calls

	private void SetReady(int playerNumber, bool ready)
	{
		photonView.RPC(nameof(SetReadyRPC), RpcTarget.OthersBuffered, playerNumber, ready);
	}

	private void UpdatePlayerElement(int playerNumber, int dropdownIndex)
	{
		//NetworkManager.SetLocalPlayerInfo(dropdownIndex, GetNumberInTeam(dropdownIndex));
		photonView.RPC(nameof(UpdatePlayerElementRPC), RpcTarget.OthersBuffered, playerNumber, dropdownIndex);
	}

	#endregion

	#region RPCs

	[PunRPC]
	private void UpdatePlayerElementRPC(int playerNumber, int dropdownIndex)
	{ 
		FindUIElementByPlayerNumber(playerNumber).ChangeTeamText(dropdownIndex);
	}

	[PunRPC]
	private void StartGameRPC()
	{
		PlayerUIElement local = FindLocalPlayerElement();
		NetworkManager.SetLocalPlayerInfo(local.teamNumber, GetNumberInTeam(local.teamNumber));

		SceneHandler sceneManager = ServiceLocator.GetServiceOfType<SceneHandler>();
		sceneManager.LoadScene(sceneToLoad);
	}

	[PunRPC]
	private void SetReadyRPC(int playerNumber, bool ready)
	{
		FindUIElementByPlayerNumber(playerNumber).SetReady(ready);
	}

	#endregion
}
