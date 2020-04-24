using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlescreenScene : MonoBehaviour
{
    [Header("Titlescreen References:")]
    [SerializeField]
    private Rigidbody fridgeDoor;

    [SerializeField]
    private Rigidbody playerBody;

    [SerializeField]
    private Rigidbody car;

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
    }

    public void EjectDoor() 
    {
        const float FORCE = 20;

        fridgeDoor.isKinematic = false;
        fridgeDoor.AddForce(playerBody.transform.forward * FORCE, ForceMode.Impulse);

        playerBody.isKinematic = false;
        playerBody.GetComponent<Collider>().enabled = true;
        playerBody.AddForce(playerBody.transform.forward * FORCE, ForceMode.Impulse);
    }
}
