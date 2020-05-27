using UnityEngine;
using UnityEngine.UI;

public class ObjectStateIndicator : MonoBehaviour
{

    private const string BOOL_POPUP_ID = "ShouldPopup";

    public enum IndicatorState 
    {
        Broken = 0,
        Dirty = 1,
        Clean = 2,
        ToBeStored = 3,
        IsPoop = 4
    }

    public enum TeamOwner
    {
        Team1 = 0,
        Team2 = 1,
        Everyone = 2
    }

    private readonly string[] tooltips = new string[]
    {
        "This object is broken.",
        "This object is dirty.",
        "This object is clean.",
        "This object needs to be stored.",
        "This object needs to be moved from your house or be thrown away."
    };
    

    [Header("Current State:")]
    [SerializeField]
    private TeamOwner teamOwner;

    [SerializeField]
    private IndicatorState currentState;

    [SerializeField]
    private Image dirtyImage;

    [SerializeField]
    private Image brokenImage;

    [SerializeField]
    private Image storedImage;

    [SerializeField]
    private Image poopImage;

    [SerializeField]
    private Text tooltip;

    [SerializeField]
    private Animator indicatorAnim;

    private void Awake() 
    {
        IndicatorManager.allIndicators.Add(this);
    }

    public void Set_TeamOwner(int teamID) 
    {
        teamOwner = (TeamOwner)teamID;
    }

    public void Set_IndicatorState(IndicatorState state) 
    {
        currentState = state;
    }

    public void Set_PopupState(bool shouldDisplay) 
    {
        if (teamOwner == (TeamOwner)NetworkManager.localPlayerInformation.team || teamOwner == TeamOwner.Everyone)
        {
            if (currentState == IndicatorState.Clean)
            {
                indicatorAnim.SetBool(BOOL_POPUP_ID, false);
                return;
            }

            indicatorAnim.SetBool(BOOL_POPUP_ID, shouldDisplay);

            if (shouldDisplay)
            {
                UpdateVisuals();
                return;
            }

            storedImage.enabled = false;
            dirtyImage.enabled = false;
            brokenImage.enabled = false;
        }
    }

    public void UpdateVisuals() 
    {
        switch (currentState) 
        {
            case IndicatorState.Broken:
                poopImage.enabled = false;
                storedImage.enabled = false;
                dirtyImage.enabled = false;
                brokenImage.enabled = true;
                break;

            case IndicatorState.Dirty:
                poopImage.enabled = false;
                storedImage.enabled = false;
                dirtyImage.enabled = true;
                brokenImage.enabled = false;
                break;

            case IndicatorState.ToBeStored:
                poopImage.enabled = false;
                storedImage.enabled = true;
                dirtyImage.enabled = false;
                brokenImage.enabled = false;
                break;

            case IndicatorState.IsPoop:
                storedImage.enabled = false;
                dirtyImage.enabled = false;
                brokenImage.enabled = false;
                poopImage.enabled = true;
                break;

            case IndicatorState.Clean:
                indicatorAnim.SetBool(BOOL_POPUP_ID, false);
                break;
        }

        tooltip.text = tooltips[(int)currentState];
    }
}
