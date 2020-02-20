using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolInteractableObject : CleanableObject
{
    public enum ToolInteractableType 
    {
        ToBeCleaned, // This means wether this object should be cleaned by using the tool.
        Cleans // This means wether the object cleans the tool at hand.
    }

    [Header("Tool Interactable Settings:")]
    [SerializeField]
    private int required_ToolID;

    [SerializeField]
    private ToolInteractableType toolInteractableType;

    #region ### Private Variables ###
    private const float TIME_TILL_NOTE_VANISHES_BASE = 3;
    private float noteTimer;

    protected const string POPUP_BOOLNAME = "Popup";
    protected Animator notool_Animator;

    protected const float DELAY_BASE_NOTOOLTIP = 1;
    protected float delayTimer_NoToolTip = 0;

    protected bool noToolTip_IsDelayed = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        notool_Animator = SpawnManager.SpawnObjectBasedOnConnectionState("NoToolNote", Vector3.zero, Quaternion.identity).GetComponentInChildren<Animator>();
        notool_Animator.transform.root.position = transform.position + object_ui_Offset;
    }

    protected virtual void Update()
    {
        if(notool_Animator.GetBool(POPUP_BOOLNAME) == true) 
        {
            noteTimer -= Time.deltaTime;

            if(noteTimer < 0) 
            {
                notool_Animator.SetBool(POPUP_BOOLNAME, false);
            }
        }
    }

    protected virtual void Set_Notifications()
    {
        if (noToolTip_IsDelayed == true)
        {
            if (delayTimer_NoToolTip > 0)
            {
                delayTimer_NoToolTip -= Time.deltaTime;
                return;
            }
            else
            {
                noToolTip_IsDelayed = false;
            }
        }

        noteTimer = TIME_TILL_NOTE_VANISHES_BASE;
        notool_Animator.SetBool(POPUP_BOOLNAME, true);
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        if(IsLocked == true) 
        {
            DeInteract(interactionController);
            interactionController.DeinteractWithCurrentObject();
        }

        if(interactionController.currentlyWielding?.toolID != required_ToolID) 
        {
            notool_Animator.transform.root.position = transform.position + object_ui_Offset;
            Set_Notifications();
            interactionController.DeinteractWithCurrentObject();
        }

        if (interactionController.currentlyWielding != null)
        {
            if (required_ToolID == interactionController.currentlyWielding.toolID)
            {
                noteTimer = 0;
                base.Interact(interactionController);
                return;
            }
        }
    }

    public override void CleanObject(PlayerInteractionController interactionController)
    {
        if (toolInteractableType == ToolInteractableType.ToBeCleaned) 
        {
            base.CleanObject(interactionController);
            return;
        }

        //In this scenario, the tool will be effected instead of this object.
        noToolTip_IsDelayed = true;
        delayTimer_NoToolTip = DELAY_BASE_NOTOOLTIP;
        interactionController.currentlyWielding.OnToolInteractionComplete();
        CleaningProgression = 0;
    }

    public override void DirtyObject()
    {
        if (toolInteractableType == ToolInteractableType.ToBeCleaned)
        {
            base.DirtyObject();
            return;
        }
    }
}
