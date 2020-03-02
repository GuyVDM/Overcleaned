using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public partial class HouseManager : MonoBehaviourPun, IServiceOfType
{
	public static float CleanPercentage => GetCleanPercentage();
	public static int RemainingTime => GetRemainingTime(); 

	//Progression tracking
    private static CleanableObject[] cleanableObjects;
	private static int totalWeightOfAllCleanables;

	//Time tracking
	public int gameTimeInMinutes;
	private static DateTime targetTime;

	#region Initalize Service
	private void Awake()
	{
		OnInitialise();
		cleanableObjects = FindObjectsOfType<CleanableObject>();
	}
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	//private void Start()
	//{
	//	totalWeightOfAllCleanables = GetTotalWeight();

	//	if (PhotonNetwork.IsMasterClient)
	//		photonView.RPC(nameof(GetRemainingTime), RpcTarget.AllBuffered);
	//}

	//private void Update()
	//{
	//	print(RemainingTime);
	//}

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

		return (weightCleaned / totalWeightOfAllCleanables) * 100;
	}

	private static int GetTotalWeight()
	{
		int toReturn = 0;

		for (int i = 0; i < cleanableObjects.Length; i++)
		{
			toReturn += cleanableObjects[i].cleaningWeight;
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

	public static int GetRemainingTime()
	{
		return targetTime.Subtract(DateTime.Now).Minutes;
	}

	private DateTime CalculateTargetTime()
	{
		return DateTime.Now.Add(TimeSpan.Parse("00:" + gameTimeInMinutes + ":00"));
	}

	#endregion

}
