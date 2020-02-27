using UnityEngine;
using Photon.Pun;

public class PlayerInteractionController : MonoBehaviourPunCallbacks
{
    private const float RAY_LENGTH = 1f;

    [Header("References & Parameters:")]
    [SerializeField]
    private GameObject hand;

    [SerializeField]
    private LayerMask interactableMask;

    [SerializeField]
    private Transform arrow_Selection_UX;

    [Header("Debugging:")]
    public WieldableObject currentlyWielding;

    public InteractableObject currentSelected;

    public InteractableObject currentlyInteracting;

    #region ### Private Variables ###
    private readonly KeyCode interactKey = KeyCode.E;
    private readonly KeyCode dropWieldableKey = KeyCode.F;
    private readonly KeyCode useWieldableKey = KeyCode.Return;

    private Vector3 arrow_UX_Offset = new Vector3(0, 2, 0);

    private bool forceDrop = false;
    #endregion

    #region ### RPC Calls ###
    [PunRPC]
    private void Cast_ThrowObject(int objectID, bool hasForceDropped) 
    {
        const float THROW_FORCE_FORWARD = 10;
        const float THROW_FORCE_UP = 3;

        Vector3 throwVelocity = hasForceDropped ? Vector3.zero : (transform.forward * THROW_FORCE_FORWARD) + (transform.up * THROW_FORCE_UP);

        Transform thrownObject = NetworkManager.GetViewByID(objectID).transform;

        thrownObject.transform.SetParent(null);
        thrownObject.transform.GetComponent<Rigidbody>().isKinematic = false;
        thrownObject.transform.GetComponent<Rigidbody>().AddForceAtPosition(throwVelocity, transform.position, ForceMode.Impulse);
    }

    [PunRPC()]
    private void Cast_PickupObject(int handID, int objectID, Vector3 rotation, Vector3 localPosition) 
    {
        Transform currentlyWielded = NetworkManager.GetViewByID(objectID).transform;
        Transform handToChildTo = NetworkManager.GetViewByID(handID).transform;

        currentlyWielded.transform.SetParent(hand.transform);
        currentlyWielded.transform.localPosition = localPosition;
        currentlyWielded.transform.localEulerAngles = rotation;
        currentlyWielded.transform.GetComponent<Rigidbody>().isKinematic = true;
    }
    #endregion

    private void Update()
    {
        CheckForInteractables();
        Interact();
    }

    private void Interact() 
    {
        #region ### When starting to interact ###
        if (Input.GetKey(interactKey) && currentSelected != null) 
        {
            if (currentSelected.IsLocked == false && HasAccessToInteract(currentSelected))
            {
                arrow_Selection_UX.gameObject.SetActive(false);
                currentlyInteracting = currentSelected;
                currentlyInteracting.Interact(this);
            }
        }
        #endregion

        #region ### When Interacting ###
        if (currentlyInteracting) 
        {
            if (Input.GetKey(interactKey) == false || (currentSelected != currentlyInteracting))
            {
                currentlyInteracting.DeInteract(this);
            }
        }
        #endregion

        #region ### When dropping or using your wieldable ###
        if(Input.GetKeyDown(dropWieldableKey)) 
        {
            if(currentlyWielding != null) 
            {
                currentlyWielding.DeInteract(this);
            }
        }

        if(Input.GetKey(useWieldableKey)) 
        {
            if(currentlyWielding != null) 
            {
                if (currentlyWielding.GetType() == typeof(WieldableInteractableObject))
                {
                    WieldableInteractableObject currentItem = (WieldableInteractableObject)currentlyWielding;
                    currentItem.Use_WieldableObject(this);
                }
            }
        }
        #endregion
    }

    private void CheckForInteractables() 
    {
        Ray interactableRay = new Ray(transform.position, transform.forward);
        RaycastHit hitPoint;
        Debug.DrawRay(transform.position, transform.forward, Color.red, RAY_LENGTH);

        if (Physics.Raycast(interactableRay, out hitPoint, RAY_LENGTH, interactableMask)) 
        {
            if (hitPoint.transform.GetComponent<InteractableObject>() != null) 
            {
                InteractableObject observedObject = hitPoint.transform.GetComponent<InteractableObject>();

                if (observedObject.IsLocked == false && HasAccessToInteract(observedObject))
                {
                    if (currentSelected == null) 
                    {
                        Select(observedObject);
                    } 
                    else if (currentSelected != observedObject)
                    {
                        DeSelect(currentSelected);
                        Select(observedObject);
                    }
                }
                return;
            }
        }

        if(currentSelected != null) 
        {
            DeSelect(currentSelected);
        }
    }

    private void Select(InteractableObject observedObject) 
    {
        currentSelected = observedObject;
        arrow_Selection_UX.gameObject.SetActive(true);
        arrow_Selection_UX.position = observedObject.transform.position + arrow_UX_Offset;
    }

    private void DeSelect(InteractableObject observedObject) 
    {
        currentSelected = null;
        arrow_Selection_UX.gameObject.SetActive(false);
    }

    public void PickupObject(WieldableObject wieldableObject, Vector3 localHandOffset, Vector3 localRotationOffset) 
    {
        if(currentlyWielding == null) 
        {
            currentlyWielding = wieldableObject;

            if(NetworkManager.IsConnectedAndInRoom) 
            {
                photonView.RPC(nameof(Cast_PickupObject), RpcTarget.AllBuffered,
                hand.GetPhotonView().ViewID,
                wieldableObject.gameObject.GetPhotonView().ViewID,
                localRotationOffset,
                localHandOffset
                );
                return;
            }

            Cast_PickupObject(
                hand.GetPhotonView().ViewID,
                wieldableObject.gameObject.GetPhotonView().ViewID,
                localRotationOffset,
                localHandOffset
            );
        }
    }

    public void DropObject(WieldableObject wieldableObject)
    {
        if (currentlyWielding != null) 
        {
            currentlyWielding = null;
            if (NetworkManager.IsConnectedAndInRoom) 
            {
                photonView.RPC(nameof(Cast_ThrowObject), RpcTarget.AllBuffered, wieldableObject.gameObject.GetPhotonView().ViewID, forceDrop);
                forceDrop = false;
                return;
            }
        }
    }

    public void ForceDropObject() 
    {
        if (currentlyWielding != null) 
        {
            if (currentlyWielding.GetType() == typeof(WieldableCleanableObject))
            {
                forceDrop = true;
                DropObject(currentlyWielding);
            }
        }
    }

    public void DeinteractWithCurrentObject() 
    {
        if (currentlyInteracting != null) 
        {
            currentlyInteracting = null;
        }
    }

    #region ### Collision Checks ###
    private void OnCollisionEnter(Collision collision) 
    {
        if(collision.transform.root.GetComponent<PlayerManager>()) 
        {
            PlayerManager otherPlayer = collision.transform.root.GetComponent<PlayerManager>();

            if((int)otherPlayer.team != NetworkManager.localPlayerInformation.team) 
            {
                ForceDropObject();
            }
        }   
    }
    #endregion

    #region ### Interact Checks ###
    private static bool HasAccessToInteract(InteractableObject interactableObject)
    {
        if (interactableObject.ownedByTeam == InteractableObject.OwnedByTeam.Everyone) 
        {
            return true;
        }

        if ((int)interactableObject.ownedByTeam == NetworkManager.localPlayerInformation.team)
        {
            return true;
        }

        return false;
    }
    #endregion
}
