﻿using System;
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
	public static event Action OnCleanableObjectStatusChanged;

	//Delegates for events
	public delegate void TimeChanged(TimeSpan newtime);

    //Progression tracking
    private static List<CleanableObject> cleanableObjects = new List<CleanableObject>();
	private static List<WieldableCleanableObject> wieldableCleanableObjects = new List<WieldableCleanableObject>();
	private static float totalWeightOfAllCleanables;
	private List<CleaningProgressionStorage> cleaningProgressionStorage = new List<CleaningProgressionStorage>();

	//Time tracking
	public int gameTimeInMinutes;
	private static DateTime targetTime;
	private TimeSpan lastTimeSpan;
	private bool endOfTimerWasReached;
	private bool targetTimeIsCalculated;

	//Events
	public Vector2 gameEventWaitTime;


	#region Initalize Service
	private void Awake()
	{
		OnInitialise();

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

    public static void InvokeOnObjectStatusCallback() => OnCleanableObjectStatusChanged?.Invoke();

    public static void AddInteractableToObservedLists(WieldableCleanableObject wieldableCleanableObject = null, CleanableObject cleanableObject = null) 
    {
        if (wieldableCleanableObject == null && cleanableObject == null) 
        {
            throw new Exception($"You must pass 1 of the parameters...");
        }

        if(wieldableCleanableObject != null) 
        {
            wieldableCleanableObjects.Add(wieldableCleanableObject);
        }

        if(cleanableObject != null) 
        {
            cleanableObjects.Add(cleanableObject);
        }
    }

	#region Cleaning Progression

	public static float GetCleanPercentage()
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

        Debug.Log("Stats: " + cleanableObjects.Count + " : " + wieldableCleanableObjects.Count);
        return (weightCleaned / totalWeightOfAllCleanables);
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

	public void OnObjectStatusChanged()
	{
		CleanPercentage = GetCleanPercentage();
		photonView.RPC(nameof(SyncProgressionAcrossClients), RpcTarget.All, CleanPercentage, NetworkManager.localPlayerInformation.team);
	}

	[PunRPC]
	private void SyncProgressionAcrossClients(float progression, int teamNumber)
	{
		CleaningProgressionStorage writeTo = FindStorageByTeamNumber(teamNumber);
		writeTo.progression = progression;
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

		return new CleaningProgressionStorage() { team = teamNumber }; 
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

		if (winningTeam == NetworkManager.localPlayerInformation.team)
		{
			uiManager.ShowWindow("Win Window");
		}
		else
		{
			uiManager.ShowWindow("Lose Window");
		}
	}

	#endregion

	#region Events

	private IEnumerator EventLoop()
	{
		int currentTeam = 1;

		while (!endOfTimerWasReached)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(gameEventWaitTime.x, gameEventWaitTime.y));
			photonView.RPC(nameof(StartGameEvent), RpcTarget.All, currentTeam);
			currentTeam = ChooseNextTeam(currentTeam);
		}
	}

	private int ChooseNextTeam(int currentTeam)
	{
		if (currentTeam == 1)
			return 2;
		else
			return 1;
	}

	private void StartGameEvent(int team)
	{
		if (team != NetworkManager.localPlayerInformation.team) return;

		GameEventType chosenEventType = (GameEventType)UnityEngine.Random.Range(0, (int)GameEventType.EnumSize);

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

		availableBreakableObjects[UnityEngine.Random.Range(0, availableBreakableObjects.Count)].Set_ObjectStateToDirty();
	}

	private void SpawnWieldable()
	{

	}

	#endregion

}
