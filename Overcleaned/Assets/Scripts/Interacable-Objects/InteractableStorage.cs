using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableStorage : InteractableObject 
{
    [Header("Storage Settings:")]
    [SerializeField]
    private int[] accepted_ItemIDs;

    public override void Interact(PlayerInteractionController interactionController)
    {
        if(interactionController.currentlyWielding) 
        {
            if(DoesAllowForObjectID(interactionController.currentlyWielding.toolID)) 
            {
                if (interactionController.currentlyWielding.GetType() == typeof(WieldableCleanableObject)) 
                {
                    WieldableCleanableObject wieldable = (WieldableCleanableObject)interactionController.currentlyWielding;

                    interactionController.DropObject(wieldable);
                    
                    //Activate function that stores the object, also in onenable, reset all things

                    ObjectPool.Set_ObjectBackToPool(wieldable.photonView.ViewID);
                    return;
                }
            }
        }

        DeInteract(interactionController);
    }

    public override void DeInteract(PlayerInteractionController interactionController)
    {
        interactionController.DeinteractWithCurrentObject();
    }

    private bool DoesAllowForObjectID(int objectID) => accepted_ItemIDs.Contains(objectID);
}
