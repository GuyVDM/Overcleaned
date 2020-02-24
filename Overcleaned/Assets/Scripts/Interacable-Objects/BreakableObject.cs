using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

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
    private bool IsBroken { get; set; }
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
    #endregion

    protected override void Awake() 
    {
        base.Awake();
        repairProgressionUI = SpawnManager.SpawnObjectBasedOnConnectionState("RepairingProgressBar", Vector3.zero, Quaternion.identity).GetComponentInChildren<ProgressBar>();
        repairProgressionUI.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);
        repairProgressionUI.Set_ActionName(repairActionName);
        repairProgressionUI.Set_Tooltip(repairActionTooltip);
        IsBroken = true;
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
                    if (lockedForOthers == false)
                    {
                        lockedForOthers = true;
                        Set_LockingState(true);
                        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    }

                    RepairProgression += Time.deltaTime;
                    repairProgressionUI.enabled = true;
                    repairProgressionUI.Set_CurrentProgress(RepairProgression / repairTime);
                    repairProgressionUI.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);

                    if(RepairProgression >= repairTime) 
                    {
                        RepairObject();
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
        if(IsBroken) 
        {
            RepairProgression = 0;
        }

        repairProgressionUI.enabled = false;
        base.DeInteract(interactionController);
    }

    public virtual void RepairObject() 
    {
        onRepairObject?.Invoke();
        noToolTip_IsDelayed = true;
        delayTimer_NoToolTip = DELAY_BASE_NOTOOLTIP;

        IsBroken = false;
    }

    public override void DirtyObject()
    {
        IsBroken = true;
        onBreakObject?.Invoke();
        base.DirtyObject();
    }


}
