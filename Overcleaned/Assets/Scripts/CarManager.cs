using Photon.Pun;
using System.Collections;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [System.Serializable]
    public struct CarBehaviour 
    {
        public Car car;
        public float carSpeed;
        public float carDuration;
    }

    [Header("Car Settings:")]
    [SerializeField]
    private CarBehaviour[] allCars;

    private void Start() 
    {
        HouseManager.OnFinishedCountdown += StartLoop;

        for(int i = 0; i < allCars.Length; i++) 
        {
            allCars[i].car.SetSpeedAndDuration(allCars[i].carSpeed, allCars[i].carDuration);
        }
    }

    private void OnDestroy()
    {
        HouseManager.OnFinishedCountdown -= StartLoop;
    }

    public void StartLoop() 
    {
        StartCoroutine(CarLoop());
    }

    private IEnumerator CarLoop() 
    {
        const float TIME_BASE = 5;

        while(true) 
        {
            for (int i = 0; i < allCars.Length; i++) 
            {
                yield return new WaitForSeconds(TIME_BASE);
                allCars[i].car.StartCar();
            }
        }
    }
}
