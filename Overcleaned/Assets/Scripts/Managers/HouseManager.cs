using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public partial class HouseManager : MonoBehaviourPun, IServiceOfType
{
	private struct EndGameProgressionStorage
	{
		public float progression;
		public int team;
	}

	//Static values;
	public static float CleanPercentage => GetCleanPercentage();
	public static TimeSpan RemainingTime => GetRemainingTime();

	//Events
	public static event TimeChanged OnTimeChanged;

	//Delegates for events
	public delegate void TimeChanged(TimeSpan newtime);

	//Progression tracking
    private static CleanableObject[] cleanableObjects;
	private static WieldableCleanableObject[] wieldableCleanableObjects;
	private static int totalWeightOfAllCleanables;

	//Time tracking
	public int gameTimeInMinutes;
	private static DateTime targetTime;
	private TimeSpan lastTimeSpan;
	private bool endOfTimerWasReached;

	//EndGame
	private List<EndGameProgressionStorage> endGameProgressionStorage = new List<EndGameProgressionStorage>();

	#region Initalize Service
	private void Awake()
	{
		OnInitialise();
		cleanableObjects = FindObjectsOfType<CleanableObject>();
		wieldableCleanableObjects = FindObjectsOfType<WieldableCleanableObject>();
	}
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		totalWeightOfAllCleanables = GetTotalWeight();

		if (PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(StartTimeTracking), RpcTarget.AllBuffered);

		OnTimeChanged += EndGame;
	}

	private void Update()
	{
		if (!endOfTimerWasReached)
			UpdateTimer();
	}

	#region Cleaning Progression
	public static float GetCleanPercentage()
	{
		int weightCleaned = 0;

		for (int i = 0; i < cleanableObjects.Length; i++)
		{
			if (cleanableObjects[i].IsCleaned)
			{
				weightCleaned += cleanableObjects[i].cleaningWeight;
			}
		}

		for (int i = 0; i < wieldableCleanableObjects.Length; i++)
		{
			if (wieldableCleanableObjects[i].IsCleanedAndStored)
			{
				weightCleaned += wieldableCleanableObjects[i].cleaningWeight;
			}
		}

		return (weightCleaned / totalWeightOfAllCleanables) * 100;
	}

	private static int GetTotalWeight()
	{
		int toReturn = 0;

		for (int i = 0; i < cleanableObjects.Length; i++)
		{
			toReturn += cleanableObjects[i].cleaningWeight;
		}

		for (int i = 0; i < wieldableCleanableObjects.Length; i++)
		{
			toReturn += wieldableCleanableObjects[i].cleaningWeight;
		}

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
		return DateTime.Now.Add(TimeSpan.Parse("00:" + gameTimeInMinutes + ":00"));
	}

	private void UpdateTimer()
	{
		if (RemainingTime.TotalSeconds <= 0)
		{
			print("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
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
		print(remainingTime.TotalSeconds);
		if (remainingTime.TotalSeconds <= 0)
		{
			print("BBBBBB");
			if (!PhotonNetwork.IsMasterClient)
			{
				print("CCCCCCC");
				photonView.RPC(nameof(RecieveProgressionInformation), RpcTarget.MasterClient, CleanPercentage, NetworkManager.localPlayerInformation.team);
			}
			else
			{
				print("DDDDDD");
				RecieveProgressionInformation(CleanPercentage, NetworkManager.localPlayerInformation.team);
			}
		}
	}

	private void CalculateWinner()
	{
		int winner = -1;

		for (int i = 0; i < endGameProgressionStorage.Count; i++)
		{
			if (winner == -1 || endGameProgressionStorage[i].progression > endGameProgressionStorage[winner].progression)
			{
				winner = i;
			}
		}

		photonView.RPC(nameof(HasWonOrLost), RpcTarget.All, winner);
	}

	[PunRPC]
	private void RecieveProgressionInformation(float percentage, int teamNumber)
	{
		endGameProgressionStorage.Add(new EndGameProgressionStorage { progression = percentage, team = teamNumber });

		if (ServiceLocator.GetServiceOfType<NetworkManager>().debugMode || endGameProgressionStorage.Count == 2)
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

}
