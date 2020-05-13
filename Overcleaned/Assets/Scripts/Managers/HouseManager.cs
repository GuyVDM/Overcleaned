using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HouseManager : MonoBehaviourPun, IServiceOfType
{
	private class CleaningProgressionStorage
	{
		public float progression;
		public int team;
	}

	private enum GameEventType
	{
		BreakCleanableObject,
		BreakCleanableObjectWithTimer,
		SpawnWieldable,

		EnumSize,
	}

	//Static values;
	public static float CleanPercentage { get; private set; }
	public static TimeSpan RemainingTime => GetRemainingTime();

	//Events
	public static event TimeChanged OnTimeChanged;
	public static event Action<int> OnCleanableObjectStatusChanged;
	public static event Action<int> OnCleaningProgressionVisualChanged;

	//Delegates for events
	public delegate void TimeChanged(TimeSpan newtime);

	//Progression tracking
	private static List<CleanableObject> cleanableObjects = new List<CleanableObject>();
	private static List<WieldableCleanableObject> wieldableCleanableObjects = new List<WieldableCleanableObject>();
	private static float totalWeightOfAllCleanables;
	private static List<CleaningProgressionStorage> cleaningProgressionStorage = new List<CleaningProgressionStorage>();

	//Time tracking
	public int gameTimeInMinutes;
	private static DateTime targetTime;
	private TimeSpan lastTimeSpan;
	private bool endOfTimerWasReached;
	private bool targetTimeIsCalculated;

	//Events
	public Vector2 gameEventWaitTime;

	[System.Serializable]
	private struct SpawnRegionAnchors
	{
		public Vector3 topleftAnchor;
		public Vector3 bottomRightAnchor;
	}

	[Header("Event Spawn Regions:")]
	[SerializeField]
	private SpawnRegionAnchors[] spawnRegions;

	[SerializeField]
	private SpawnRegionAnchors dogspawnRegion;

	[SerializeField]
	private bool shouldDisplaySpawnAnchors = true;

	#region Initalize Service
	private void Awake()
	{
		OnInitialise();
		StartCoroutine(DogSpawnLoop());

        OnCleanableObjectStatusChanged += OnObjectStatusChanged;
	}

	private void OnDestroy()
	{
		OnDeinitialise();

		OnCleanableObjectStatusChanged -= OnObjectStatusChanged;
	}

	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		totalWeightOfAllCleanables = GetTotalWeight();

		if (PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(EventLoop());
			photonView.RPC(nameof(StartTimeTracking), RpcTarget.AllBuffered);
		}

		OnTimeChanged += EndGame;
	}

	private void Update()
	{
		if (!endOfTimerWasReached && targetTimeIsCalculated)
			UpdateTimer();
	}

	public static void InvokeOnObjectStatusCallback(int teamID) => OnCleanableObjectStatusChanged?.Invoke(teamID);

	public static void AddInteractableToObservedLists(WieldableCleanableObject wieldableCleanableObject = null, CleanableObject cleanableObject = null)
	{
		if (wieldableCleanableObject == null && cleanableObject == null)
		{
			throw new Exception($"You must pass 1 of the parameters...");
		}

		if (wieldableCleanableObject != null)
		{
			wieldableCleanableObjects.Add(wieldableCleanableObject);
		}

		if (cleanableObject != null)
		{
			cleanableObjects.Add(cleanableObject);
		}
	}

	#region Cleaning Progression

	public static float Get_CleanPercentage()
	{
		float weightCleaned = 0;

		for (int i = 0; i < cleanableObjects.Count; i++)
		{
			if (cleanableObjects[i].IsCleaned)
			{
				weightCleaned += cleanableObjects[i].cleaningWeight;
			}
		}

		for (int i = 0; i < wieldableCleanableObjects.Count; i++)
		{
			if (wieldableCleanableObjects[i].IsCleanedAndStored)
			{
				weightCleaned += wieldableCleanableObjects[i].cleaningWeight;
			}
		}

		weightCleaned -= NetworkManager.localPlayerInformation.team == 0 ? HouseDirtyTriggerZone.PenaltyTeam1 : HouseDirtyTriggerZone.PenaltyTeam2;
		totalWeightOfAllCleanables += NetworkManager.localPlayerInformation.team == 0 ? HouseDirtyTriggerZone.PenaltyTeam1 : HouseDirtyTriggerZone.PenaltyTeam2;
		return (weightCleaned / totalWeightOfAllCleanables);
	}

	public static float Get_OtherTeamCleaningPercentage()
	{
		int otherTeamID = NetworkManager.localPlayerInformation.team == 0 ? 1 : 0;

		float value = 0;

		foreach (CleaningProgressionStorage storage in cleaningProgressionStorage)
		{
			print("<b> storage item of team </b>" + storage.team + "has progression of: " + storage.progression);

			if (storage.team == otherTeamID)
			{
				Debug.Log("Found storage");
				value = storage.progression;
				break;
			}
		}

		return value;
	}

	private static float GetTotalWeight()
	{
		int toReturn = 0;

		for (int i = 0; i < cleanableObjects.Count; i++)
		{
			toReturn += cleanableObjects[i].cleaningWeight;
		}

		for (int i = 0; i < wieldableCleanableObjects.Count; i++)
		{
			toReturn += wieldableCleanableObjects[i].cleaningWeight;
		}

		return toReturn;
	}

	public void OnObjectStatusChanged(int ourTeamID)
	{
		CleanPercentage = Get_CleanPercentage();
		photonView.RPC(nameof(SyncProgressionAcrossClients), RpcTarget.All, CleanPercentage, ourTeamID);
		photonView.RPC(nameof(SyncCleaningProgressionUIToTeams), RpcTarget.All, ourTeamID);
	}

	[PunRPC]
	private void SyncProgressionAcrossClients(float progression, int teamNumber)
	{
		CleaningProgressionStorage writeTo = FindStorageByTeamNumber(teamNumber);
		print("<b> progression of team </b>" + teamNumber + "changed to: " + progression);
		writeTo.progression = progression;
	}

	[PunRPC]
	private void SyncCleaningProgressionUIToTeams(int teamID)
	{
		OnCleaningProgressionVisualChanged?.Invoke(teamID);
	}

	private CleaningProgressionStorage FindStorageByTeamNumber(int teamNumber)
	{
		for (int i = 0; i < cleaningProgressionStorage.Count; i++)
		{
			if (cleaningProgressionStorage[i].team == teamNumber)
			{
				return cleaningProgressionStorage[i];
			}
		}

		print("<b> ADDED CLEANINGPROGRESSION OF TEAM: </b>" + teamNumber);
		CleaningProgressionStorage toReturn = new CleaningProgressionStorage() { team = teamNumber };
		cleaningProgressionStorage.Add(toReturn);
		return toReturn;
	}

	#endregion

	#region Time Tracking

	[PunRPC]
	public void StartTimeTracking()
	{
		targetTime = CalculateTargetTime();
	}

	public static TimeSpan GetRemainingTime()
	{
		if (targetTime != null)
			return targetTime.Subtract(DateTime.Now);
		else
			return new TimeSpan();
	}

	private DateTime CalculateTargetTime()
	{
		int minutes = gameTimeInMinutes % 60;
		int hours = (gameTimeInMinutes - minutes) / 60;

		DateTime toReturn = DateTime.Now.Add(TimeSpan.Parse(hours.ToString() + ":" + minutes.ToString() + ":00"));
		targetTimeIsCalculated = true;
		return toReturn;
	}

	private void UpdateTimer()
	{
		if (RemainingTime.TotalSeconds <= 0)
		{
			OnTimeChanged(RemainingTime);
			endOfTimerWasReached = true;
			return;
		}

		if (lastTimeSpan.Seconds != RemainingTime.Seconds)
		{
			if (lastTimeSpan.Seconds > RemainingTime.Seconds || RemainingTime.Seconds == 59 || lastTimeSpan == null)
			{
				lastTimeSpan = RemainingTime;

				OnTimeChanged(lastTimeSpan);
			}
		}
	}

	#endregion

	#region Winning and Losing

	public void ReturnToMainMenu()
	{
		ServiceLocator.GetServiceOfType<NetworkManager>().ReturnToMainMenu();
	}

	private void EndGame(TimeSpan remainingTime)
	{
		if (remainingTime.TotalSeconds <= 0)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				photonView.RPC(nameof(RecieveProgressionInformation), RpcTarget.MasterClient, CleanPercentage, NetworkManager.localPlayerInformation.team);
			}
			else
			{
				RecieveProgressionInformation(CleanPercentage, NetworkManager.localPlayerInformation.team);
			}
		}
	}

	private void CalculateWinner()
	{
		int winner = -1;

		for (int i = 0; i < cleaningProgressionStorage.Count; i++)
		{
			if (winner == -1 || cleaningProgressionStorage[i].progression > cleaningProgressionStorage[winner].progression)
			{
				winner = i;
			}
		}

		photonView.RPC(nameof(HasWonOrLost), RpcTarget.All, winner);
	}

	[PunRPC]
	private void RecieveProgressionInformation(float percentage, int teamNumber)
	{
		FindStorageByTeamNumber(teamNumber).progression = percentage;

		if (ServiceLocator.GetServiceOfType<NetworkManager>().debugMode || cleaningProgressionStorage.Count == 2)
		{
			CalculateWinner();
		}
	}

	[PunRPC]
	private void HasWonOrLost(int winningTeam)
	{
		UIManager uiManager = ServiceLocator.GetServiceOfType<UIManager>();
		EffectsManager effectsManager = ServiceLocator.GetServiceOfType<EffectsManager>();

		if (winningTeam == NetworkManager.localPlayerInformation.team)
		{
			uiManager.ShowWindow("Win Window");
			effectsManager.PlayAudio("Game Win");
		}
		else
		{
			uiManager.ShowWindow("Lose Window");
			effectsManager.PlayAudio("Game Lost");
		}
	}

	#endregion

	#region Events

	private IEnumerator DogSpawnLoop() 
    {
		if (PhotonNetwork.IsMasterClient) 
	    {
			const int SPAWN_IN_SECONDS = 60;

			yield return new WaitForSeconds(SPAWN_IN_SECONDS);

			SpawnDog();

			StartCoroutine(DogSpawnLoop());
		}

		yield break;
	}

	private void SpawnDog() 
    {
		string PREFAB_NAME = "Dog [Interactable]";

		float xPos = UnityEngine.Random.Range(dogspawnRegion.topleftAnchor.x, dogspawnRegion.bottomRightAnchor.x);
		float zPos = UnityEngine.Random.Range(dogspawnRegion.topleftAnchor.z, dogspawnRegion.bottomRightAnchor.z);

		const float HEIGHT = 5.5f;

		GameObject dog = PhotonNetwork.InstantiateSceneObject(PREFAB_NAME, new Vector3(xPos, HEIGHT, zPos), Quaternion.identity);
		dog.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
	}

	private IEnumerator EventLoop()
	{
		int currentTeam = 1;

		while (!endOfTimerWasReached)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(gameEventWaitTime.x, gameEventWaitTime.y));

			currentTeam = ChooseNextTeam(currentTeam);

			photonView.RPC(nameof(StartGameEvent), RpcTarget.All, currentTeam);
		}
	}

	private int ChooseNextTeam(int currentTeam) => currentTeam == 0 ? 1 : 0;

	[PunRPC]
	private void StartGameEvent(int team)
	{
		if (team != NetworkManager.localPlayerInformation.team) return;

		GameEventType chosenEventType = (GameEventType)UnityEngine.Random.Range(0, (int)GameEventType.EnumSize);

		Debug.Log("Calling event of type: " + chosenEventType.ToString() + " to team: " + team);

		switch (chosenEventType)
		{
			case GameEventType.BreakCleanableObject:
				BreakCleanableObject(false);
				break;
			case GameEventType.BreakCleanableObjectWithTimer:
				BreakCleanableObject(true);
				break;
			case GameEventType.SpawnWieldable:
				SpawnWieldable();
				break;
		}
	}

	private void BreakCleanableObject(bool withTimer)
	{
		List<BreakableObject> availableBreakableObjects = new List<BreakableObject>();

		for (int i = 0; i < cleanableObjects.Count; i++)
		{
			if (cleanableObjects[i].GetType() == typeof(BreakableObject))
			{
				BreakableObject breakableObject = (BreakableObject)cleanableObjects[i];
				if (!breakableObject.IsBroken)
					availableBreakableObjects.Add(breakableObject);
			}
		}

		if (availableBreakableObjects.Count > 0)
		{
			availableBreakableObjects[UnityEngine.Random.Range(0, availableBreakableObjects.Count)].Set_ObjectStateToDirty();
		}
	}

	private void SpawnWieldable()
	{
		const string WIELDABLE_POOL_ID = "[CleanableWieldables]";

		if (ObjectPool.HasPooledObjectAvailable(WIELDABLE_POOL_ID))
		{
			Vector3 instancePos = GetTeamCleanableObjectSpawnRegion();

			ObjectPool.Set_ObjectFromPool(WIELDABLE_POOL_ID, instancePos, Vector3.zero);
			ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudioMultiplayer("Pop");

			return;
		}
	}

	private Vector3 GetTeamCleanableObjectSpawnRegion()
	{
		const float SPAWN_HEIGHT = 5.5f;

		if (NetworkManager.localPlayerInformation.team > 1)
		{
			if (spawnRegions.Length > 1)
			{
				SpawnRegionAnchors anchors = spawnRegions[NetworkManager.localPlayerInformation.team];
				float randomizedXPos = UnityEngine.Random.Range(anchors.topleftAnchor.x, anchors.bottomRightAnchor.x);
				float randomizedZPos = UnityEngine.Random.Range(anchors.bottomRightAnchor.z, anchors.topleftAnchor.z);

				return new Vector3(randomizedXPos, SPAWN_HEIGHT, randomizedZPos);
			}
		}

		Debug.LogError("[HouseManager] Please assign 2 spawnregions for dirty objects.");
		return Vector3.zero;
	}

#if UNITY_EDITOR
	public void OnDrawGizmos()
	{
		const float RADIUS = 0.5f;

		if (shouldDisplaySpawnAnchors)
		{
			if (spawnRegions.Length > 1)
			{
				for (int i = 0; i < spawnRegions.Length; i++)
				{
					Gizmos.DrawSphere(spawnRegions[i].topleftAnchor, RADIUS);
					Gizmos.DrawSphere(spawnRegions[i].bottomRightAnchor, RADIUS);

					UnityEditor.Handles.Label(spawnRegions[i].topleftAnchor, "Spawnregion Anchor [Topleft]");
					UnityEditor.Handles.Label(spawnRegions[i].bottomRightAnchor, "Spawnregion Anchor [Bottomright]");
				}
			}

			Gizmos.DrawSphere(dogspawnRegion.bottomRightAnchor, RADIUS);
			Gizmos.DrawSphere(dogspawnRegion.topleftAnchor, RADIUS);

			UnityEditor.Handles.Label(dogspawnRegion.bottomRightAnchor, "Spawnregion Dog Object [Bottomright]");
			UnityEditor.Handles.Label(dogspawnRegion.topleftAnchor, "Spawnregion Anchor [Topleft]");
		}
	}
#endif
	#endregion

}
