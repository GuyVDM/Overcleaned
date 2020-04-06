using System.Collections;using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPun, IServiceOfType
{
	[System.Serializable]
	public struct TeamProperties
	{
		public Color teamColor;
		public Transform enemyTeamPosition;
		public Transform[] teamSpawnPositions;
	}

	[Header ("Player Spawning")]
	public GameObject playerPrefab;
	public TeamProperties[] teams;

	private int clientsReady;

	#region Initalize Service
	private void Awake() => OnInitialise();
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		SpawnLocalPlayer();
	}

	#region Player Spawning

	private void SpawnLocalPlayer()
	{
		PlayerManager playerManager = PhotonNetwork.Instantiate(playerPrefab.name, teams[NetworkManager.localPlayerInformation.team].teamSpawnPositions[NetworkManager.localPlayerInformation.numberInTeam].position, Quaternion.identity).GetComponent<PlayerManager>();
		playerManager.Set_PlayerColor(teams[NetworkManager.localPlayerInformation.team].teamColor);
		playerManager.Set_EnemyBasePosition(teams[NetworkManager.localPlayerInformation.team].enemyTeamPosition.position);

		photonView.RPC(nameof(ThisClientIsReady), RpcTarget.MasterClient);
	}

	[PunRPC]
	private void ThisClientIsReady()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		clientsReady++;
		if (clientsReady == PhotonNetwork.CountOfPlayers)
		{
			photonView.RPC(nameof(StartCountdown), RpcTarget.All);
		}
	}

	[PunRPC]
	private void StartCountdown()
	{
		StartCoroutine(nameof(Countdown));
	}

	private IEnumerator Countdown()
	{
		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();
		UI_CountdownWindow countdownWindow = uiManager.ShowWindowReturn("Countdown Window") as UI_CountdownWindow;

		for (int i = 0; i < 3; i++)
		{
			countdownWindow.SetNewNumber(i.ToString());
			yield return new WaitForSeconds(1);
		}

		uiManager.HideAllWindows();
	}

	#endregion
}

[System.Serializable]
public struct KeyBinding
{
	public string buttonName;
	public UnityEvent functionality;
}
