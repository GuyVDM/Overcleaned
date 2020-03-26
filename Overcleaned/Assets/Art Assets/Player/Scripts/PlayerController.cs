using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Parameters:")]
    [SerializeField]
    private float speed = 6.0f;

    [SerializeField]
    private float maxTurnSpeed = 720.0f;

    #region ### Input Detection ###
    private static bool InputFound => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    private static Vector2 InputAxes => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    #endregion

    #region ### Properties ###
    private Animator m_Animator = null;
    private Animator MyAnimator
    {
        get => m_Animator ?? (m_Animator = GetComponent<Animator>());
    }

    private Rigidbody m_rigidBody = null;
    private Rigidbody MyRigidBody
    {
        get => m_rigidBody ?? (m_rigidBody = GetComponent<Rigidbody>());
    }
    #endregion

    #region ### Private Variables ###
    private readonly Vector3 stunParticle_Offset = new Vector3(0, 1.5f, 0);

    private Vector3 moveDirection = Vector3.zero;

    private EffectsManager effectsManager;
    #endregion

    #region ### Animation States ###
    public enum AnimationState
    {
        Idle = 0,
        Stunned = 1,
        Walking = 2,
        TwoHandPickup = 3
    }

    private AnimationState myAnimationState;
    #endregion

    private void Awake() => effectsManager = ServiceLocator.GetServiceOfType<EffectsManager>();

    private void FixedUpdate() => MovePlayer();

    private void Update()
    {
        //TODO: Create a trigger for this.
        if (Input.GetButtonDown("Jump")) 
        {
            StunPlayer(5f);
        }
    }

    private void MovePlayer()
    {
        const float MOVELERPTIME = 0.1f;
        const float NORMALIZE_SPEEDMOD = 0.67f;

        float normalizedSpeed = Mathf.Abs(InputAxes.x) * NORMALIZE_SPEEDMOD + Mathf.Abs(InputAxes.y) * NORMALIZE_SPEEDMOD;
        normalizedSpeed = Mathf.Clamp(normalizedSpeed, Mathf.NegativeInfinity, 1);

        moveDirection = GetDirection() * speed;

        if (myAnimationState == AnimationState.Stunned)
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, MOVELERPTIME);
            return;
        }

        if (InputFound)
        {
            Quaternion newRot = Quaternion.LookRotation(GetDirection());
            Quaternion lookRot = Quaternion.RotateTowards(transform.rotation, newRot, maxTurnSpeed * Time.deltaTime);

            transform.eulerAngles = new Vector3(0, lookRot.eulerAngles.y, 0);

            MyRigidBody.AddForceAtPosition(moveDirection, transform.position, ForceMode.Force);
        }

        if (InputAxes == Vector2.zero) 
        {
            Set_PlayerAnimationState(AnimationState.Idle);
            return;
        }

        Set_PlayerAnimationState(AnimationState.Walking);
    }

    //Manages all player animationstates. 
    public void Set_PlayerAnimationState(AnimationState animationState)
    {
        const string ANIM_STATE_STRING = "State";
        MyAnimator.SetInteger(ANIM_STATE_STRING, (int)animationState);
        myAnimationState = animationState;
    }

    //Stuns the player for X time and prevents movement.
    public void StunPlayer(float duration)
    {
        effectsManager.PlayParticle("Knockout_FX", transform.position + stunParticle_Offset, Quaternion.identity);
        Set_PlayerAnimationState(AnimationState.Stunned);
        ResetToIdle(duration);

    }

    //Resets player animationstate to idle after stun event has been called for example.
    private async void ResetToIdle(float duration)
    {
        await Task.Delay(TimeSpan.FromSeconds(duration));
        Set_PlayerAnimationState(AnimationState.Idle);
    }

    private Vector3 GetDirection() 
    {
        Vector3 direction = Vector3.zero;

        if(Input.GetKey(KeyCode.W)) 
        {
            direction += Vector3.forward;
        }

        if(Input.GetKey(KeyCode.S)) 
        {
            direction += Vector3.back;
        }

        if(Input.GetKey(KeyCode.A)) 
        {
            direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D)) 
        {
            direction += Vector3.right;
        }

        return direction;
    }
}