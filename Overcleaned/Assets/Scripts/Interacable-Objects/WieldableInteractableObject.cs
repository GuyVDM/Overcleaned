using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WieldableInteractableObject : WieldableObject, IWieldableInteractable 
{
    [Header("Wieldable Use Event:")]
    [SerializeField]
    private UnityEvent interactable_Function;

    /// <summary>
    /// Function to initiate some event related to this item.
    /// </summary>
    /// <param name="playerInteractionController"></param>
    public void Use_WieldableObject(PlayerInteractionController playerInteractionController) 
    {
        interactable_Function?.Invoke();
    }
}
