using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class CleanableObject : InteractableObject, IPunObservable
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
    private bool lockedForOthers = false;
    #endregion

    #region ### RPC Calls ###

    [PunRPC]
    private void Cast_ObjectStateToClean() 
    {
        CleanAndLockObjectLocally();
    }

    private void Set_ObjectStateToClean() 
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Cast_ObjectStateToClean), RpcTarget.AllBuffered);
            return;
        }

        Cast_ProgressBarCreation();
    }

    [PunRPC]
    private void Cast_ProgressBarCreation()
    {
        progressBar = Instantiate(Resources.Load("ProgressBar") as GameObject, Vector3.zero, Quaternion.identity).GetComponentInChildren<ProgressBar>();
        progressBar.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);
        progressBar.Set_Tooltip(tooltip);
        progressBar.Set_ActionName(actionName);
    }

    private void Create_ProgressBar() 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            if (PhotonNetwork.IsMasterClient) 
            {
                photonView.RPC(nameof(Cast_ProgressBarCreation), RpcTarget.AllBuffered);
            }
            return;
        }

        Cast_ProgressBarCreation();
    }

    [PunRPC]
    private void Cast_ProgressBarEnableState(bool isEnabled) 
    {
        progressBar.enabled = isEnabled;
    }

    private void Set_ProgressBarEnableState(bool isEnabled) 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Cast_ProgressBarEnableState), RpcTarget.AllBuffered, isEnabled);
            return;
        }

        Cast_ProgressBarEnableState(isEnabled);
    }

    [PunRPC]
    private void Cast_LockingState(bool isLocked)
    {
        this.IsLocked = IsLocked;
        Debug.Log("IsLocked is: " + IsLocked);
    }

    private void Set_LockingState(bool isLocked) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Cast_LockingState), RpcTarget.OthersBuffered, isLocked);
            return;
        }

        Cast_LockingState(isLocked);
    }

    #endregion

    protected virtual void Awake() => Create_ProgressBar();

    public override void Interact(PlayerInteractionController interactionController)
    {
        if(IsLocked) 
        {
            interactionController.DeinteractWithCurrentObject();
            return;
        }

        if(lockedForOthers == false) 
        {
            lockedForOthers = true;
            Set_LockingState(true);           
        }

        if (IsCleaned == false) 
        {
            CleaningProgression += Time.deltaTime;

            if (progressBar.enabled == false) 
            {
                Set_ProgressBarEnableState(true);
            }

            progressBar.Set_CurrentProgress(CleaningProgression / cleaningTime);
            progressBar.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);

            if (CleaningProgression >= cleaningTime)
            {
                OnCleanedObject(interactionController);
                interactionController.DeinteractWithCurrentObject();
            }
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController) 
    {
        IsLocked = false;
        CleaningProgression = 0;

        if (progressBar.enabled == true)
        {
            Set_ProgressBarEnableState(false);
        }

        if (lockedForOthers == true) 
        {
            lockedForOthers = false;
            Set_LockingState(false);
        }

        progressBar.Set_CurrentProgress(0);
        interactionController.DeinteractWithCurrentObject();
    }

    public virtual void OnCleanedObject(PlayerInteractionController interactionController) 
    {
        CleaningStateOfHouse -= penalty;
        Set_ObjectStateToClean();
    }

    public virtual void DirtyObject() 
    {
        OnDirtyObject?.Invoke();
        CleaningStateOfHouse += penalty;
        IsCleaned = false;
        IsLocked = false;
    }

    private void CleanAndLockObjectLocally() 
    {
        OnInteractionWhenCleaned?.Invoke();
        IsCleaned = true;
        IsLocked = true;
        Debug.Log("Succesfully cleaned object!");
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (progressBar != null)
        {
            if (stream.IsWriting) 
            {
                stream.SendNext(CleaningProgression);
            }
            else if (stream.IsReading) 
            {
                progressBar.Set_CurrentProgress((float)stream.ReceiveNext() / cleaningTime);
            }
        }
    }
}
