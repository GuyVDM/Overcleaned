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
	private void Awake() 
	{
		OnInitialise();
		SceneHandler.onSceneIsLoadedAndReady += SceneStart;
	}
	private void OnDestroy()
	{
		OnDeinitialise();
		SceneHandler.onSceneIsLoadedAndReady -= SceneStart;
	}
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void SceneStart(string sceneName)
	{
		photonView.RPC(nameof(ThisClientIsReady), RpcTarget.MasterClient);
	}

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
		StartCoroutine(Countdown());
	}

	private IEnumerator Countdown()
	{
		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();
		EffectsManager effectsManager = ServiceLocator.GetServiceOfType<EffectsManager>();
		UI_CountdownWindow countdownWindow = uiManager.ShowWindowReturn("Countdown Window") as UI_CountdownWindow;

		for (int i = 3; i > 0; i--)
		{
			countdownWindow.ShowText(i.ToString());
			effectsManager.PlayAudio("Countdown 2");
			yield return new WaitForSeconds(1);
		}

		countdownWindow.ShowText("GO!");
		effectsManager.PlayAudio("Countdown 1");
		yield return new WaitForSeconds(1);

		ServiceLocator.GetServiceOfType<PlayerManager>().Set_PlayerLockingstate(false);
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
