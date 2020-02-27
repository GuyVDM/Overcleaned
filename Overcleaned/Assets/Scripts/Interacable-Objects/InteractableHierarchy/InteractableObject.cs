using Photon.Pun;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviourPunCallbacks, IInteractableObject 
{

    public enum OwnedByTeam
    {
        Team1 = 0,
        Team2 = 1,
        Everyone = 2
    }

    public bool IsLocked { get; protected set; }

    public abstract void Interact(PlayerInteractionController interactionController);

    public abstract void DeInteract(PlayerInteractionController interactionController);

    [Header("Team Management")]
    [SerializeField]
    protected OwnedByTeam ownedByTeam = OwnedByTeam.Everyone;

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

    #region ### Static Accessor Property ###
    private static HouseManager m_houseManager;
    private HouseManager HouseManager => m_houseManager ?? (m_houseManager = ServiceLocator.GetServiceOfType<HouseManager>());
    #endregion

    #region ### Static Checks ###
    protected static bool IsAllowedAccess(OwnedByTeam teamOwner)
    {
        if(teamOwner == OwnedByTeam.Everyone) 
        {
            return true;
        }

        if(NetworkManager.localPlayerInformation.team == (int)teamOwner) 
        {
            return true;
        }

        return false;
    }

    #endregion

    #region ### Private Variables ###
    protected bool lockedForOthers = false;
    #endregion
}