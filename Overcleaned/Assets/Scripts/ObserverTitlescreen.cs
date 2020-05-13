using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverTitlescreen : MonoBehaviour
{
    private Animator thisAnimator;

    private bool canMove = false;

    private void Start() 
    {
        thisAnimator = GetComponent<Animator>();
    }

    private void Update() 
    {
        const float MOVEMENT_SPEED = 2;

        if(canMove) 
        {
            transform.position += transform.forward * MOVEMENT_SPEED * Time.deltaTime;
        }
    }

    public async void StartAnimation() 
    {
        canMove = true;

        await Task.Delay(TimeSpan.FromSeconds(3));

        canMove = false;
        thisAnimator.SetBool("Switch", true);

        await Task.Delay(TimeSpan.FromSeconds(3));

        thisAnimator.SetBool("Switch", false);

        await Task.Delay(TimeSpan.FromSeconds(0.3f));

        canMove = true;

        await Task.Delay(TimeSpan.FromSeconds(10));

        canMove = false;
    }
}
