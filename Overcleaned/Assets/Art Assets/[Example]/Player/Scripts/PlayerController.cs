using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This script moves the character controller forward
// and sideways based on the arrow keys.
// It also jumps when pressing space.
// Make sure to attach a character controller to the same game object.
// It is recommended that you make only one call to Move or SimpleMove per frame.

public class PlayerController : MonoBehaviour
{
    #region player movement variables
    CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    #endregion 

    internal Animator animator;
    public enum AnimationState { Idle = 0, Stunned = 1, Walking = 2, TwoHandPickup = 3 }
    AnimationState myState;

    #region Horizontal and Vertical axes
    private float horizontalAxis;
    private float verticalAxis;
    #endregion

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        GetAxes();
        Movement();
    }

    /// fetches input axes and stores them in varaibles for easy access
    private void GetAxes()
    {
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");
    }

    /// handles playermovement and rotation
    private void Movement()
    {       
        if (myState == AnimationState.Idle || myState == AnimationState.Walking || myState == AnimationState.TwoHandPickup)
        {
            moveDirection = new Vector3(horizontalAxis, 0.0f, verticalAxis);
            moveDirection *= speed;
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        characterController.Move(moveDirection * Time.deltaTime);

        if (myState == AnimationState.Stunned)
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, .1f);
    }

    public void SetPlayerAnimationState(AnimationState state)
    {
        animator.SetInteger("State", (int)state);
        myState = state;
    }

    public void StunPlayer(float duration)
    {
        //EffectsManager.instance.PlayParticle("ParticleName", Vector.zero, Quaternion.identity, [meer mogelijke parameters])
        SetPlayerAnimationState(AnimationState.Stunned);
        StartCoroutine("ResetToIdle", duration);

    }

    private IEnumerator ResetToIdle(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetPlayerAnimationState(AnimationState.Idle);
    }


    //TODO: New rotation method which interpolates rotation overtime.
}