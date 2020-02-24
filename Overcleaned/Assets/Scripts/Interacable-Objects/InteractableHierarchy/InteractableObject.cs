using Photon.Pun;

public abstract class InteractableObject : MonoBehaviourPunCallbacks, IInteractableObject 
{

    public bool IsLocked { get; protected set; }

    public abstract void Interact(PlayerInteractionController interactionController);

    public abstract void DeInteract(PlayerInteractionController interactionController);

    #region ### RPC Calls ###
    [PunRPC]
    protected void Cast_LockingState(bool isLocked) => this.IsLocked = isLocked;

    protected void Set_LockingState(bool isLocked)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Cast_LockingState), RpcTarget.OthersBuffered, isLocked);
            return;
        }

        Cast_LockingState(isLocked);
    }
    #endregion

    #region ### Private Variables ###
    protected bool lockedForOthers = false;
    #endregion
}