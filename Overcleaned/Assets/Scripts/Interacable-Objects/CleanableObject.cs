using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class CleanableObject : InteractableObject
{
    //TODO: Move this float value to the HouseManager.Events when it exists.
    public static float CleaningStateOfHouse { get; set; }

    [Header("Tweakable Parameters:")]
    [SerializeField]
    protected float penalty = 25;

    [SerializeField]
    private float cleaningTime = 5;

    [Header("Event Params:")]
    [SerializeField]
    private UnityEvent OnInteractionWhenCleaned;

    [SerializeField]
    private UnityEvent OnDirtyObject;

    [Header("Progress UI Parameters:")]
    [SerializeField]
    private string actionName;

    [SerializeField]
    private string tooltip;

    [SerializeField]
    protected Vector3 object_ui_Offset;

    #region ### Properties ###
    protected bool IsCleaned { get; set; }
    protected float CleaningProgression { get; set; }
    #endregion

    #region ### Private Variables ###
    private ProgressBar progressBar;
    #endregion

    protected virtual void Awake() 
    {
        progressBar = Instantiate(Resources.Load("ProgressBar") as GameObject).GetComponentInChildren<ProgressBar>();
        progressBar.Set_Tooltip(tooltip);
        progressBar.Set_ActionName(actionName);
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        if(IsLocked) 
        {
            interactionController.DeinteractWithCurrentObject();
            return;
        }

        if (IsCleaned == false) 
        {
            CleaningProgression += Time.deltaTime;

            progressBar.enabled = true;
            progressBar.Set_CurrentProgress(CleaningProgression / cleaningTime);
            progressBar.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);

            if (CleaningProgression >= cleaningTime)
            {
                CleanObject(interactionController);
                interactionController.DeinteractWithCurrentObject();
            }
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController) 
    {
        IsLocked = false;
        CleaningProgression = 0;
        progressBar.enabled = false;
        progressBar.Set_CurrentProgress(0);
        interactionController.DeinteractWithCurrentObject();
    }

    public virtual void CleanObject(PlayerInteractionController interactionController) 
    {
        OnInteractionWhenCleaned?.Invoke();
        CleaningStateOfHouse -= penalty;
        IsCleaned = true;
        IsLocked = true;
    }

    public virtual void DirtyObject() 
    {
        OnDirtyObject?.Invoke();
        CleaningStateOfHouse += penalty;
        IsCleaned = false;
        IsLocked = false;
    }
}
