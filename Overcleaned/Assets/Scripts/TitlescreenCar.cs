using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlescreenCar : MonoBehaviour
{
    private Animator thisAnimator;

    const float CRASH_FORCE = 50;

    // Start is called before the first frame update
    private void Start()
    {
        thisAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void CrashCar()
    {
        thisAnimator.enabled = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * CRASH_FORCE, ForceMode.Impulse);
    }

    public void OnCollisionEnter(Collision o) 
    {
        if(o.transform.GetComponent<Animator>()) 
        {
            o.transform.GetComponent<Animator>().enabled = false;
            Rigidbody body = o.transform.GetComponent<Rigidbody>();
            body.isKinematic = false;
            GetComponent<Rigidbody>().AddForce(transform.forward * CRASH_FORCE, ForceMode.Impulse);
            body.AddForce(transform.forward * CRASH_FORCE);
            DisableRigid();
        }
    }

    public async void DisableRigid() 
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
