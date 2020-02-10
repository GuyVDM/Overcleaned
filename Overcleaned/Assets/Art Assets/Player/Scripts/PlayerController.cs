using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class PlayerController : MonoBehaviour
{
    #region player movement variables
    private Vector3 moveDirection = Vector3.zero;
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float maxTurnSpeed = 720.0f;
    #endregion

    #region Input Detection
    private static bool InputFound => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    private static Vector2 InputAxes => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    #endregion

    private Animator m_Animator = null;
    private Animator MyAnimator
    {
        get => m_Animator ?? (m_Animator = GetComponent<Animator>());
    }
    private CharacterController m_CharacterController = null;
    private CharacterController MyCharacterController
    {
        get => m_CharacterController ?? (m_CharacterController = GetComponent<CharacterController>());
    }


    #region animationStates
    public enum AnimationState
    {
        Idle = 0,
        Stunned = 1,
        Walking = 2,
        TwoHandPickup = 3
    }
    AnimationState myState;
    #endregion


    //Moves and rotates the player.
    void FixedUpdate() => MovePlayer();
    private void MovePlayer()
    {
        const float MOVELERPTIME = 0.1f;

        //Prevents player from moving when Animation state is set to stunned
        if (myState == AnimationState.Stunned)
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, MOVELERPTIME);
            return;
        }
        //Handles movement and rotation for player
        if (InputFound)
        {
            Quaternion newRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRot, maxTurnSpeed * Time.deltaTime);
            moveDirection = new Vector3(InputAxes.x, 0.0f, InputAxes.y) * speed;
            MyCharacterController.Move(moveDirection * Time.deltaTime);
        }

        //Sets Animation states
        if (InputAxes == Vector2.zero)
            SetPlayerAnimationState(AnimationState.Walking);
        else
            SetPlayerAnimationState(AnimationState.Idle);
    }

    //Manages all player animationstates. 
    public void SetPlayerAnimationState(AnimationState state)
    {
        const string ANIM_STATE_STRING = "State";
        MyAnimator.SetInteger(ANIM_STATE_STRING, (int)state);
        myState = state;
    }

    //Stuns the player for X time and prevents movement.
    public void StunPlayer(float duration)
    {
        //EffectsManager.instance.PlayParticle("ParticleName", Vector.zero, Quaternion.identity, [meer mogelijke parameters])
        SetPlayerAnimationState(AnimationState.Stunned);
        ResetToIdle(duration);

    }

    //Resets player animationstate to idle after stun event has been called for example.
    private async void ResetToIdle(float duration)
    {
        await Task.Delay(TimeSpan.FromSeconds(duration));
        SetPlayerAnimationState(AnimationState.Idle);
    }
}