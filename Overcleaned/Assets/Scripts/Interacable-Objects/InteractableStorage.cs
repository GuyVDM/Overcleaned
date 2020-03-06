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

            }
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController)
    {
    }

    private bool DoesAllowForObjectID(int objectID) => accepted_ItemIDs.Contains(objectID);
}
