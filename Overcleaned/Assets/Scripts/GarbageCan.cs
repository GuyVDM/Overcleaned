using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(BoxCollider))]
public class GarbageCan : InteractableObject
{
    private const int POOP_ITEM_ID = 101;

    public enum ToDisplay 
    {
        NoItemIcon = 0,
        WrongItemIcon = 1,
    }

    [Header("Garbagecan Properties:")]
    [SerializeField]
    private Animator wrongItemAnimator;

    [SerializeField]
    private Animator noItemAnimator;

    [SerializeField]
    private ParticleSystem puffSmokeEffect;

    #region ### Private Variables ###
    private PlayerManager playerManager;

    private bool IsRefreshed { get; set; } = true;
    #endregion

    public override void Interact(PlayerInteractionController interactionController)
    {
        #region ### Base Checks ###
        if (IsLocked || IsRefreshed == false)
        {
            DeInteract(interactionController);
            return;
        }

        if(playerManager == null) 
        {
            playerManager = ServiceLocator.GetServiceOfType<PlayerManager>();
            return;
        }

        LockLocallyOnTimer();
        #endregion

        if(playerManager.player_InteractionController.currentlyWielding != null) 
        {
            if(playerManager.player_InteractionController.currentlyWielding.toolID == POOP_ITEM_ID) 
            {
                WieldableObject wieldedItem = playerManager.player_InteractionController.currentlyWielding;

                playerManager.player_InteractionController.DropObject(wieldedItem);
                PhotonNetwork.Destroy(wieldedItem.gameObject.GetPhotonView());
                Set_VFX();
                return;
            }

            Set_DisplayIcon(ToDisplay.WrongItemIcon);
            return;
        }

        Set_DisplayIcon(ToDisplay.NoItemIcon);
    }

    public override void DeInteract(PlayerInteractionController interactionController) 
    {
        ///
        /// 
        ///
    }

    private async void LockLocallyOnTimer() 
    {
        IsRefreshed = false;

        await Task.Delay(TimeSpan.FromSeconds(3));

        IsRefreshed = true;
    }

    private void Set_DisplayIcon(ToDisplay toDisplay) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_DisplayIcon), RpcTarget.All, (int)toDisplay);
            return;
        }

        Stream_DisplayIcon((int)toDisplay);
    }

    [PunRPC]
    private async void Stream_DisplayIcon(int toDisplay) 
    {
        const string BOOL_POPUP_ID = "Popup";
        Animator iconAnim = toDisplay == 0 ? noItemAnimator : wrongItemAnimator;

        iconAnim.SetBool(BOOL_POPUP_ID, true);

        await Task.Delay(TimeSpan.FromSeconds(3));

        iconAnim.SetBool(BOOL_POPUP_ID, false);
    }

    private void Set_VFX()
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_VFX), RpcTarget.All);
            return;
        }

        Stream_VFX();
    }

    [PunRPC]
    private void Stream_VFX()
    {
        puffSmokeEffect.Play();
    }
}
