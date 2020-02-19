using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WieldableObject : InteractableObject
{
    [Header("Tweakable Parameters:")]
    public int toolID;

    [Space(10)]

    [SerializeField]
    private Vector3 pickup_Offset;

    [SerializeField]
    private Vector3 rotation_Offset;

    #region ### Private Variables ###
    private Collider triggerField;
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
            if(interactionController.currentlyWielding != null) 
            {
                interactionController.DropObject(interactionController.currentlyWielding);
            }

            interactionController.PickupObject(this, pickup_Offset, rotation_Offset);
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController) => interactionController.DropObject(this);

    /// <summary>
    /// A function used to initiate some event whenever a tool has been used for a interactable.
    /// </summary>
    public virtual void OnToolInteractionComplete() => Debug.Log("Completed interaction!");
}
