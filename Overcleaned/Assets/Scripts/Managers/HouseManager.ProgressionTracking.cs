using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HouseManager : MonoBehaviour, IServiceOfType
{
	public static float CleanPercentage => GetCleanPercentage();

    private static CleanableObject[] cleanableObjects;
	private static int totalWeightOfAllCleanables;

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

	private void Start()
	{
		totalWeightOfAllCleanables = GetTotalWeight();
	}

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

}
