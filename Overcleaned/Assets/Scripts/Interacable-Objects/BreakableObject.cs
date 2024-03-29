﻿using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System;

public class BreakableObject : ToolInteractableObject, IPunObservable
{
    [Header("Breakable Specifics:")]
    [SerializeField]
    private int repair_ToolID;

    [SerializeField]
    private float repairTime = 4;

    [SerializeField]
    private string repairActionName;

    [SerializeField]
    private string repairActionTooltip;

    [SerializeField]
    private UnityEvent onBreakObject;

    [SerializeField]
    private UnityEvent onRepairObject;

    #region ### Properties ###
    public bool IsBroken { get; set; }
    private float RepairProgression { get; set; }
    #endregion

    #region ### Private Variables ###
    private ProgressBar repairProgressionUI;
    #endregion

    #region ### RPC Calls ###
    [PunRPC]
    protected void Stream_SetRepairState(bool isBroken)
    {
        this.IsBroken = isBroken;

        if(IsBroken) 
        {
            onBreakObject?.Invoke();
            return;
        }

        onRepairObject?.Invoke();
    }

    protected void Set_RepairState(bool isBroken) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_SetRepairState), RpcTarget.AllBuffered, isBroken);
            return;
        }

        Stream_SetRepairState(isBroken);
    }
    
    [PunRPC]
    protected void Stream_RepairProgressbarEnableState(bool isEnabled) 
    {
        repairProgressionUI.enabled = isEnabled;
    }

    protected void Set_RepairProgressbarEnableState(bool isEnabled) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_RepairProgressbarEnableState), RpcTarget.AllBuffered, isEnabled);
            return;
        }

        Stream_RepairProgressbarEnableState(isEnabled);
    }

    [PunRPC]
    protected void Stream_BreakableProgressBarCreation()
    {
        repairProgressionUI = Instantiate(Resources.Load("RepairingProgressBar") as GameObject, Vector3.zero, Quaternion.identity).GetComponentInChildren<ProgressBar>();
        repairProgressionUI.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);
        repairProgressionUI.Set_Tooltip(repairActionTooltip);
        repairProgressionUI.Set_ActionName(repairActionName);
    }

    protected void Set_RepairProgressBar()
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            if (PhotonNetwork.IsMasterClient) 
            {
                photonView.RPC(nameof(Stream_BreakableProgressBarCreation), RpcTarget.AllBuffered);
        
            }
            return;
        }

        Stream_BreakableProgressBarCreation();
    }

    [PunRPC]
    protected void Stream_BreakableProgressbarFinish() 
    {
        repairProgressionUI.Set_BarToFinished();
    }

    protected void Set_BreakableProgressbarFinish() 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_BreakableProgressbarFinish), RpcTarget.All);
            return;
        }

        Stream_BreakableProgressbarFinish();
    }
    #endregion

    protected override void Awake() 
    {
        IsBroken = true;

        base.Awake();
    }

    protected override void Start()
    {
        Set_RepairProgressBar();

        base.Start();
    }

    protected override void Set_IndicatorStartState()
    {
        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Broken);
    }

    protected override void OnStartInteraction()
    {
        if (passedFirstFrame == false) 
        {
            passedFirstFrame = true;

            if (IsBroken) 
            {
                Debug.Log("Playing repair loop");
                interactionSoundNumber = ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudioMultiplayer("Repair Loop", volume: 0.5f, audioMixerGroup: "Sfx", spatialBlend: 1, audioPosition: transform.position);
            }
            else
            {
                Debug.Log("Playing clean loop");
                interactionSoundNumber = ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudioMultiplayer("Clean Loop", audioMixerGroup: "Sfx", spatialBlend: 1, audioPosition: transform.position);
            }
        }
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        if (IsLocked == true) 
        {
            DeInteract(interactionController);
            interactionController.DeinteractWithCurrentObject();
        }

        if (IsBroken == true) 
        {
            if (interactionController.currentlyWielding != null)
            {
                if (interactionController.currentlyWielding.toolID == repair_ToolID)
                {
                    OnStartInteraction();

                    if (lockedForOthers == false)
                    {
                        lockedForOthers = true;
                        Set_LockingState(true);
                        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    }

                    RepairProgression += Time.deltaTime;

                    if (repairProgressionUI.enabled == false) 
                    {
                        Set_RepairProgressbarEnableState(true);
                    }

                    repairProgressionUI.Set_CurrentProgress(RepairProgression / repairTime);

                    if(RepairProgression >= repairTime) 
                    {
                        passedFirstFrame = false;
                        ServiceLocator.GetServiceOfType<EffectsManager>().StopAudioMultiplayer(interactionSoundNumber);
                        RepairObject();
                        Set_BreakableProgressbarFinish();
                    }
                    return;
                }
            }

            notool_Animator.transform.root.position = transform.position + object_ui_Offset;
            Set_Notifications();
            return;
        }

        base.Interact(interactionController);
    }

    public override void DeInteract(PlayerInteractionController interactionController)
    {
        ServiceLocator.GetServiceOfType<EffectsManager>().StopAudioMultiplayer(interactionSoundNumber);

        if (IsBroken) 
        {
            if (IsLocked == false)
            {
                Set_RepairProgressbarEnableState(false);
                RepairProgression = 0;
            }
        }

        base.DeInteract(interactionController);
    }

    public virtual void RepairObject() 
    {
        onRepairObject?.Invoke();
        
        if(toolInteractableType == ToolInteractableType.ToBeCleaned) 
        {
            indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Dirty);
        }
        else 
        {
            indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Clean);
        }

        noToolTip_IsDelayed = true;
        delayTimer_NoToolTip = DELAY_BASE_NOTOOLTIP;

        Set_RepairProgressbarEnableState(false);
        Set_RepairState(false);
    }

    protected override void DirtyObject()
    {
        RepairProgression = 0;
        CleaningProgression = 0;

        IsBroken = true;
        Set_ProgressBarEnableState(false);

        onBreakObject?.Invoke();
        base.DirtyObject();

        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Broken);
        ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudio("Machine Break", audioMixerGroup: "Sfx");
    }

    public override void OnDisable() 
    {
        progressBar.Set_BarToFinished();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (IsBroken)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(RepairProgression);
            } 
            else if (stream.IsReading) 
            {
                float valueReceived = (float)stream.ReceiveNext();
                repairProgressionUI.Set_CurrentProgress(valueReceived / repairTime);
            }

            return;
        }

        base.OnPhotonSerializeView(stream, info);
    }
}
