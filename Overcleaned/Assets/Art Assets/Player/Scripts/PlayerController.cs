using System;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{

    public Vector3 currentVelo = Vector3.zero;

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

    private const string PARTICLE_VFX_ID = "Knockout_FX";

    private bool isStunned = false;

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

    #region ### RPC Calls ###
    [PunRPC]
    private void Stream_StunParticlesAtPosition(Vector3 offset) => effectsManager.PlayParticle(PARTICLE_VFX_ID, offset, Quaternion.identity);

    private void Set_StunParticlesAtPosition(Vector3 offset)
    {
        effectsManager.PlayParticle(PARTICLE_VFX_ID, offset, Quaternion.identity);

        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_StunParticlesAtPosition), RpcTarget.Others, offset);
        }
    }

    [PunRPC]
    private void Stream_StunPlayer(float duration, Vector3 velocity, int teamId)
    {
        if (teamId != NetworkManager.localPlayerInformation.team)
        {
            MyRigidBody.AddForceAtPosition(velocity, transform.position);
            StunPlayer(duration);
        }
    }
    #endregion 

    private void Awake() => effectsManager = ServiceLocator.GetServiceOfType<EffectsManager>();

    private void FixedUpdate()
    {
        if (isStunned == false && PlayerManager.LockedComponents == false)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        currentVelo = MyRigidBody.velocity;

        const float MOVELERPTIME = 0.1f;
        const float NORMALIZE_SPEEDMOD = 0.67f;

        float normalizedSpeed = Mathf.Abs(InputAxes.x) * NORMALIZE_SPEEDMOD + Mathf.Abs(InputAxes.y) * NORMALIZE_SPEEDMOD;
        normalizedSpeed = Mathf.Clamp(normalizedSpeed, 0, 1);

        moveDirection = GetDirection() * normalizedSpeed;

        if (myAnimationState == AnimationState.Stunned)
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, MOVELERPTIME);
            return;
        }

        if (InputFound)
        {
            Quaternion newRot = Quaternion.LookRotation(GetDirection());
            Quaternion lookRot = Quaternion.RotateTowards(transform.rotation, newRot, maxTurnSpeed * Time.deltaTime);

            Vector3 adjustedVelocity = (new Vector3(InputAxes.x, 0, InputAxes.y) * speed / normalizedSpeed) * Time.deltaTime;

            adjustedVelocity.x = Mathf.Clamp(adjustedVelocity.x, -(speed * Time.deltaTime), (speed * Time.deltaTime));
            adjustedVelocity.z = Mathf.Clamp(adjustedVelocity.z, -(speed * Time.deltaTime), (speed * Time.deltaTime));

            adjustedVelocity.x = float.IsNaN(adjustedVelocity.x) ? 0 : adjustedVelocity.x;
            adjustedVelocity.z = float.IsNaN(adjustedVelocity.z) ? 0 : adjustedVelocity.z;

            adjustedVelocity.y = MyRigidBody.velocity.y;

            transform.eulerAngles = new Vector3(0, lookRot.eulerAngles.y, 0);

            MyRigidBody.velocity = adjustedVelocity;
        }
        else 
        {
            MyRigidBody.velocity = new Vector3(0, MyRigidBody.velocity.y, 0);
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
        if (isStunned == false)
        {
            isStunned = true;

            Set_StunParticlesAtPosition(transform.position + stunParticle_Offset);
            Set_PlayerAnimationState(AnimationState.Stunned);

            ResetToIdle(duration);
        }
    }

    //Resets player animationstate to idle after stun event has been called for example.
    private async void ResetToIdle(float duration)
    {
        await Task.Delay(TimeSpan.FromSeconds(duration));
        Set_PlayerAnimationState(AnimationState.Idle);

        isStunned = false;
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = Vector3.zero;

        direction += Input.GetKey(KeyCode.W) ? Vector3.forward : Input.GetKey(KeyCode.S) ? Vector3.back : Vector3.zero;
        direction += Input.GetKey(KeyCode.A) ? Vector3.left : Input.GetKey(KeyCode.D) ? Vector3.right : Vector3.zero;

        return direction;
    }
}