using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class UI_RoomInformationWindow : UIWindow
{

	public int sceneToLoad;
	public Button startGameButton;

	[Header("Prefabs")]
	public GameObject playerInRoomElementPrefab;
	[Header("Parents")]
	public Transform playerElementParent;

	private List<PlayerInRoomElement> allPlayerElements = new List<PlayerInRoomElement>();
	private PhotonView photonView;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	public override void Start()
	{
		base.Start();
		NetworkManager.onPlayerListChange += UpdatePlayerList;
		NetworkManager.onMasterClientSwitch += MasterClientLeft;
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
		if (PhotonLobby.DebugMode() || CanStartGame())
		{
			PlayerInRoomElement local = FindLocalPlayerElement();
			NetworkManager.SetLocalPlayerInfo(local.GetDropdownIndex(), GetNumberInTeam(local.GetDropdownIndex()));
			photonView.RPC("StartGameRPC", RpcTarget.All);
		}
	}

	private void OnDestroy()
	{
		NetworkManager.onPlayerListChange -= UpdatePlayerList;
		NetworkManager.onMasterClientSwitch -= MasterClientLeft;
	}

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
		int[] teamCount = new int[allPlayerElements[0].GetDropdownLength()];

		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			teamCount[allPlayerElements[i].GetDropdownIndex()]++;
		}

		for (int i = 0; i < teamCount.Length; i++)
		{
			if (teamCount[i] != teamCount.Length / 2)
				return false;
		}

		return true;
	}

	private bool AllPlayersAreReady()
	{
		int success = 0;
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].IsReady())
			{
				success++;
			}
		}

		if (success == allPlayerElements.Count)
			return true;
		else
			return false;
			
	}

	private void UpdatePlayerList(Player[] playerList, Player changedPlayer)
	{
		foreach (PlayerInRoomElement element in allPlayerElements)
		{
			Destroy(element.gameObject);
		}

		allPlayerElements.Clear();

		for (int i = 0; i < playerList.Length; i++)
		{
			CreatePlayerElement(playerList[i]);
		}
	}

	private void MasterClientLeft(Player newMasterClient)
	{
		ServiceLocator.GetServiceOfType<UIManager>().ShowMessage("The host has left the server.");
		LeaveServer();
	}

	private PlayerInRoomElement CreatePlayerElement(Player player)
	{
		GameObject newObject = Instantiate(playerInRoomElementPrefab, Vector3.zero, Quaternion.identity);
		newObject.transform.SetParent(playerElementParent, false);
		PlayerInRoomElement pire = newObject.GetComponent<PlayerInRoomElement>();
		pire.Init(player.IsLocal);
		pire.ChangePlayerName(player.NickName);
		pire.AddListenerToDropdown(delegate { UpdatePlayerElement(player.ActorNumber - 1, pire.GetDropdownIndex()); });
		pire.AddListenerToToggle(delegate { SetReady(player.ActorNumber - 1, pire.GetToggleValue()); });
		allPlayerElements.Add(pire);

		return pire;
	}

	private void UpdatePlayerElement(int playerElementIndex, int dropdownIndex)
	{
		NetworkManager.SetLocalPlayerInfo(dropdownIndex, GetNumberInTeam(dropdownIndex));
		photonView.RPC("UpdatePlayerElementRPC", RpcTarget.OthersBuffered, playerElementIndex, dropdownIndex);
	}

	private int GetNumberInTeam(int myTeamIndex)
	{
		List<PlayerInRoomElement> allElementsInMyTeam = new List<PlayerInRoomElement>();
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].GetDropdownIndex() == myTeamIndex)
			{
				allElementsInMyTeam.Add(allPlayerElements[i]);
			}
		}

		for (int i = 0; i < allElementsInMyTeam.Count; i++)
		{
			if (allElementsInMyTeam[i].isLocal)
				return i;
		}

		return -1;
	}

	private void SetReady(int playerElementIndex, bool ready)
	{
		photonView.RPC("SetReadyRPC", RpcTarget.OthersBuffered, playerElementIndex, ready);
	}

	private PlayerInRoomElement FindLocalPlayerElement()
	{
		for (int i = 0; i < allPlayerElements.Count; i++)
		{
			if (allPlayerElements[i].isLocal)
				return allPlayerElements[i];
		}

		return null;
	}

	#region RPCs

	[PunRPC]
	private void UpdatePlayerElementRPC(int playerElementIndex, int dropdownIndex)
	{
		allPlayerElements[playerElementIndex].ChangeTeam(dropdownIndex);
	}

	[PunRPC]
	private void StartGameRPC()
	{
		SceneHandler sceneManager = ServiceLocator.GetServiceOfType<SceneHandler>();
		sceneManager.LoadScene(sceneToLoad);

	}

	[PunRPC]
	private void SetReadyRPC(int playerElementIndex, bool ready)
	{
		allPlayerElements[playerElementIndex].SetToggle(ready);
	}

	#endregion
}
