using UnityEngine;
using Photon.Pun;

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

    #region ### PUN Calls ###
    [PunRPC]
    protected void Stream_ForceFinishProgression() 
    {
        progressBar.Set_BarToFinished();
    }

    protected void Set_Stream_ForceFinishProgression() 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_ForceFinishProgression), RpcTarget.All);
            return;
        }
    }

    [PunRPC]
    protected void Stream_NoToolNoteEnabled(bool isEnabled) 
    {
        noteTimer = TIME_TILL_NOTE_VANISHES_BASE;
        notool_Animator.SetBool(POPUP_BOOLNAME, isEnabled);
    }

    protected void Set_NoToolNoteEnabled(bool isEnabled) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_NoToolNoteEnabled), RpcTarget.AllBufferedViaServer, isEnabled);
            return;
        }

        Stream_NoToolNoteEnabled(isEnabled);
    }

    #endregion

    protected override void Awake()
    {
        Create_ProgressBar();
        notool_Animator = Instantiate(Resources.Load("NoToolNote") as GameObject, Vector3.zero, Quaternion.identity).GetComponentInChildren<Animator>();
        notool_Animator.transform.root.position = transform.position + object_ui_Offset;

        if(toolInteractableType == ToolInteractableType.ToBeCleaned) 
        {
            if (NetworkManager.localPlayerInformation.team == (int)ownedByTeam) 
            {
                HouseManager.AddInteractableToObservedLists(null, this);
            }
        }
    }

    protected virtual void Update()
    {
        if (IsLocked == false) 
        {
            if (notool_Animator.GetBool(POPUP_BOOLNAME) == true) 
            {
                noteTimer -= Time.deltaTime;

                if (noteTimer < 0) 
                {
                    if (notool_Animator.GetBool(POPUP_BOOLNAME) == true) 
                    {
                        Set_NoToolNoteEnabled(false);
                    }
                }
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

        if (notool_Animator.GetBool(POPUP_BOOLNAME) == false) 
        {
            Set_NoToolNoteEnabled(true);
        }
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

    public override void OnCleanedObject(PlayerInteractionController interactionController)
    {
        if (toolInteractableType == ToolInteractableType.ToBeCleaned) 
        {
            base.OnCleanedObject(interactionController);
            return;
        }

        //In this scenario, the tool will be effected instead of this object.
        noToolTip_IsDelayed = true;
        delayTimer_NoToolTip = DELAY_BASE_NOTOOLTIP;

        interactionController.currentlyWielding.OnToolInteractionComplete();
        CleaningProgression = 0;
        DeInteract(interactionController);
        Set_Stream_ForceFinishProgression();
    }

    protected override void DirtyObject()
    {
        if (toolInteractableType == ToolInteractableType.ToBeCleaned)
        {
            base.DirtyObject();
            return;
        }
    }
}
