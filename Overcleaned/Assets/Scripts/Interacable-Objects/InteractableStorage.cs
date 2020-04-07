using System;
using System.Linq;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class InteractableStorage : InteractableObject 
{
    [Header("Storage Settings:")]
    [SerializeField]
    private int[] accepted_ItemIDs;

    [SerializeField]
    private Animator noItem_Animator;

    [SerializeField]
    private Animator wrongItem_Animator;

    #region ### Private Variables ###
    private enum TooltipType 
    {
        NoItem = 0,
        WrongItem = 1
    }
    #endregion

    #region ### Constants ###
    private const float DISPLAY_TOOLTIP_TIMER = 2;
    private const float LOCK_TIMER = 2;
    #endregion

    #region ### RPC Calls ###
    private void Set_DisplayStateTooltip(int tooltipType) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_DisplayStateTooltip), RpcTarget.All, tooltipType);
            return;
        }

        Stream_DisplayStateTooltip(tooltipType);
    }

    [PunRPC]
    private void Stream_DisplayStateTooltip(int tooltipType) 
    {
        DisplayAnimator((TooltipType)tooltipType);
    }
    #endregion

    public override void Interact(PlayerInteractionController interactionController)
    {
        DeInteract(interactionController);

        if (IsLocked == true) 
        {
            return;
        }

        LockTemporarily();

        if (interactionController.currentlyWielding) 
        {
            if(DoesAllowForObjectID(interactionController.currentlyWielding.toolID)) 
            {
                if (interactionController.currentlyWielding.GetType() == typeof(WieldableCleanableObject))
                {
                    WieldableCleanableObject wieldable = (WieldableCleanableObject)interactionController.currentlyWielding;

                    if (wieldable.isCleaned)
                    {
                        interactionController.DropObject(wieldable);
                        wieldable.StoreObject();

                        ObjectPool.Set_ObjectBackToPool(wieldable.photonView.ViewID);
                        return;
                    }
                }
            }

            Set_DisplayStateTooltip((int)TooltipType.WrongItem);
            return;
        }

        Set_DisplayStateTooltip((int)TooltipType.NoItem);
        return;
    }

    public override void DeInteract(PlayerInteractionController interactionController)
    {
        interactionController.DeinteractWithCurrentObject();
    }

    private bool DoesAllowForObjectID(int objectID) => accepted_ItemIDs.Contains(objectID);

    private async void DisplayAnimator(TooltipType type) 
    {
        const string POPUP_BOOLID = "Popup";

        Animator toDisplay = (type == TooltipType.NoItem) ? noItem_Animator : wrongItem_Animator;

        toDisplay.SetBool(POPUP_BOOLID, true);

        await Task.Delay(TimeSpan.FromSeconds(DISPLAY_TOOLTIP_TIMER));

        toDisplay.SetBool(POPUP_BOOLID, false);
    }

    private async void LockTemporarily() 
    {
        IsLocked = true;

        await Task.Delay(TimeSpan.FromSeconds(LOCK_TIMER));

        IsLocked = false;
    }
}
