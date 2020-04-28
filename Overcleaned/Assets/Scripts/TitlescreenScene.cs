using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlescreenScene : MonoBehaviour
{
    [Header("Titlescreen References:")]
    [SerializeField]
    private Rigidbody fridgeDoor;

    [SerializeField]
    private Rigidbody fridgeMan;

    [SerializeField]
    private Animator carAnim;

    [SerializeField]
    private Rigidbody frontdoor;

    [SerializeField]
    private Rigidbody frontdoor_Man;

    [SerializeField]
    private Animator frontdoor_Animator;

    [SerializeField]
    private ObserverTitlescreen observer;

    [SerializeField]
    private Animator policecarAnim;

    public void Start() 
    {
        StartCoroutine(TitlescreenLoop());
    }

    private IEnumerator TitlescreenLoop() 
    {
        yield return new WaitForSeconds(5f);
        fridgeDoor.transform.parent.GetComponent<Rigidbody>().isKinematic = false;

        yield return new WaitForSeconds(5f);
        EjectDoor();
        EjectFridgeMan();

        yield return new WaitForSeconds(2f);
        frontdoor_Animator.SetBool("Start", true);

        yield return new WaitForSeconds(3.5f);
        frontdoor_Animator.enabled = false;
        EjectFrontDoor();
        observer.StartAnimation();

        yield return new WaitForSeconds(5.5f);
        carAnim.enabled = true;

        yield return new WaitForSeconds(4);
        policecarAnim.enabled = true;

    }

    private void EjectDoor() 
    {
        const float FORCE = 20;

        fridgeDoor.isKinematic = false;
        fridgeDoor.AddForce(-fridgeDoor.transform.forward * FORCE, ForceMode.Impulse);
    }

    private void EjectFridgeMan() 
    {
        const float FORCE = 20;

        fridgeMan.isKinematic = false;
        fridgeMan.GetComponent<Collider>().enabled = true;
        fridgeMan.AddForce(-fridgeMan.transform.forward * FORCE, ForceMode.Impulse);
    }

    private void EjectFrontDoor() 
    {
        const float FORCE = 40;

        frontdoor.isKinematic = false;
        frontdoor.AddForce(-fridgeDoor.transform.forward * FORCE, ForceMode.Impulse);

        frontdoor_Man.isKinematic = false;
        frontdoor_Man.AddForce(frontdoor_Man.transform.forward * FORCE, ForceMode.Impulse);
    }
}
