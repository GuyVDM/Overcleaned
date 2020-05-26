using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{

    public float CarSpeed { get; set; }
    public float CarDrivingDuration { get; set; }

    #region ### Private Variables ###
    private Vector3 startPos;
    private Vector3 startRotation;

    private bool canMove = false;
    #endregion

    private void Start() 
    {
        startPos = transform.position;
    }

    public void SetSpeedAndDuration(float speed, float duration) 
    {
        CarSpeed = speed;
        CarDrivingDuration = duration;
    }

    private void Update() 
    {
        if(canMove) 
        {
            transform.position += transform.forward * (CarSpeed * Time.deltaTime);
        }
    }

    public void StartCar() 
    {
        if (canMove == false) 
        {
            canMove = true;
            StartCoroutine(TempCarLoop());
        }
    }

    private IEnumerator TempCarLoop() 
    {
        yield return new WaitForSeconds(CarDrivingDuration);

        StopCar();
    }

    public void StopCar() 
    {
        canMove = false;
        transform.position = startPos;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.transform.root.GetComponent<Photon.Pun.PhotonView>()) 
        {
            if (col.transform.root.GetComponent<Photon.Pun.PhotonView>().IsMine)
            {
                if (col.transform.GetComponent<PlayerController>()) 
                {
                    col.transform.GetComponent<PlayerController>().StunPlayer(5);
                }
            }
        }
    }
}
