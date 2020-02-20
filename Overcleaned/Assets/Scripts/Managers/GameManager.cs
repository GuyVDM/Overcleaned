using System.Collections;using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour, IServiceOfType
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

	private void SpawnLocalPlayer()
	{
		PlayerManager playerManager = PhotonNetwork.Instantiate(playerPrefab.name, teams[NetworkManager.localPlayerInformation.team - 1].teamSpawnPositions[NetworkManager.localPlayerInformation.numberInTeam].position, teams[NetworkManager.localPlayerInformation.team - 1].teamSpawnPositions[NetworkManager.localPlayerInformation.numberInTeam].rotation).GetComponent<PlayerManager>();
		playerManager.Set_PlayerColor(teams[NetworkManager.localPlayerInformation.team].teamColor);
		playerManager.Set_EnemyBasePosition(teams[NetworkManager.localPlayerInformation.team - 1].enemyTeamPosition.position);
	}
}
