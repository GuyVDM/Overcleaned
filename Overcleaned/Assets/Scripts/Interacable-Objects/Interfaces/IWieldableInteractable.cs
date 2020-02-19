using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWieldableInteractable
{

    /// <summary>
    /// Functions main purpose is for when you can for example, hit someone with a certain wieldable object. 
    /// </summary>
    /// <param name="playerInteractionController"></param>
    void Use_WieldableObject(PlayerInteractionController playerInteractionController);

}
