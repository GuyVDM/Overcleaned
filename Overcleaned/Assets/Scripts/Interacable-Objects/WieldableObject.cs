using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class WieldableObject : InteractableObject, IPunObservable
{

    public enum Handed_Type
    {
        H1Handed = 0,
        H2Handed = 1
    }

    [Header("Tweakable Parameters:")]
    public int toolID;

    public Handed_Type handedType = 0;

    [Space(10)]

    [SerializeField]
    private Vector3 pickup_Offset;

    [SerializeField]
    private Vector3 rotation_Offset;

    #region ### Private Variables ###
    private Collider triggerField;
    #endregion

    #region ### RPC Calls ###
    [PunRPC]
    protected virtual void Stream_OnInteractionComplete() 
    {
        Debug.Log("Completed interaction!");
    }
    #endregion

    private void Awake() => GetTriggerField();

    private void GetTriggerField() 
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in allColliders) 
        {
            if (col.isTrigger == true)
            {
                triggerField = col;
                return;
            }
        }

        Debug.LogWarning($"[Wieldable] Object of name { gameObject.name } has no trigger collider attached for detection, thus cannot be interacted with.");
        this.enabled = false;
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        interactionController.DeinteractWithCurrentObject();

        if (IsLocked == false) 
        {
            if (lockedForOthers == false)
            {
                lockedForOthers = true;
                Set_LockingState(true);
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            if (interactionController.currentlyWielding != null) 
            {
                interactionController.DropObject(interactionController.currentlyWielding);
            }

            interactionController.PickupObject(this, pickup_Offset, rotation_Offset);
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController)
    {
        if (lockedForOthers == true)
        {
            lockedForOthers = false;
            Set_LockingState(false);
            interactionController.DropObject(this);
        }
    }

    /// <summary>
    /// A function used to initiate some event whenever a tool has been used for a interactable.
    /// </summary>
    public void OnToolInteractionComplete() 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_OnInteractionComplete), RpcTarget.AllBuffered);
            return;
        }

        Stream_OnInteractionComplete();
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if(IsLocked == false && lockedForOthers == false) 
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.eulerAngles);
            }
            else 
            if(stream.IsReading) 
            {
                transform.position = (Vector3)stream.ReceiveNext();
                transform.eulerAngles = (Vector3)stream.ReceiveNext();
            }
        }
    }
}
