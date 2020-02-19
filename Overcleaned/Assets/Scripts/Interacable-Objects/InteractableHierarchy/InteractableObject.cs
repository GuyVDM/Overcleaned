using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class InteractableObject : MonoBehaviourPunCallbacks, IInteractableObject 
{

    public bool IsLocked { get; protected set; }

    public abstract void Interact(PlayerInteractionController interactionController);

    public abstract void DeInteract(PlayerInteractionController interactionController);

}