using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlescreenScene : MonoBehaviour
{
    [Header("Titlescreen References:")]
    [SerializeField]
    private Rigidbody fridgeDoor;

    [SerializeField]
    private Rigidbody car;

    [SerializeField]
    private Rigidbody frontdoor;

    [SerializeField]
    private Rigidbody frontdoor_Man;

    public void Start() 
    {
        StartCoroutine(TitlescreenLoop());
    }

    private void Update() 
    {
        car.AddForce(car.transform.forward * 10, ForceMode.Force);
    }

    private IEnumerator TitlescreenLoop() 
    {
        yield return new WaitForSeconds(2f);
        EjectDoor();

        yield return new WaitForSeconds(2f);
        EjectFrontDoor();
    }

    private void EjectDoor() 
    {
        const float FORCE = 20;

        fridgeDoor.isKinematic = false;
        fridgeDoor.AddForce(-fridgeDoor.transform.forward * FORCE, ForceMode.Impulse);
    }

    private void EjectFrontDoor() 
    {
        const float FORCE = 40;

        frontdoor.isKinematic = false;
        frontdoor.AddForce(fridgeDoor.transform.forward * FORCE, ForceMode.Impulse);

        frontdoor_Man.isKinematic = false;
        frontdoor_Man.AddForce(fridgeDoor.transform.forward * FORCE, ForceMode.Impulse);
    }
}
