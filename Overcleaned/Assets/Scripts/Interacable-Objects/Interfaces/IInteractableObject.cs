using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{

    void Interact(PlayerInteractionController interactionController);

    void DeInteract(PlayerInteractionController interactionController);

}
