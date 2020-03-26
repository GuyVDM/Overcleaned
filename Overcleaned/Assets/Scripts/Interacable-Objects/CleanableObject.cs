using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class CleanableObject : InteractableObject, IPunObservable
{

    [Header("Tweakable Parameters:")]
    [SerializeField]
    public int cleaningWeight = 25;

    [SerializeField]
    private float cleaningTime = 5;

    [Header("Event Params:")]
    [SerializeField]
    private UnityEvent OnCleaned;

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
    public bool IsCleaned { get; set; }

    protected float CleaningProgression { get; set; }
    #endregion

    #region ### Private Variables ###
    protected ProgressBar progressBar;
    #endregion

    #region ### RPC Calls ###

    [PunRPC]
    protected void Stream_ObjectStateToDirty()
    {
        DirtyObject();
    }

    public void Set_ObjectStateToDirty() 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_ObjectStateToDirty), RpcTarget.AllBuffered);
            return;
        }

        Stream_ObjectStateToDirty();
    }

    [PunRPC]
    protected void Stream_ObjectStateToClean() 
    {
        CleanAndLockObjectLocally();
        HouseManager.InvokeOnObjectStatusCallback();
    }

    protected void Set_ObjectStateToClean() 
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_ObjectStateToClean), RpcTarget.AllBuffered);
            return;
        }

        Stream_ObjectStateToClean();
    }

    [PunRPC]
    protected void Stream_ProgressBarCreation()
    {
        progressBar = Instantiate(Resources.Load("ProgressBar") as GameObject, Vector3.zero, Quaternion.identity).GetComponentInChildren<ProgressBar>();
        progressBar.Set_LocalPositionOfPrefabRootTransform(transform, object_ui_Offset);
        progressBar.Set_Tooltip(tooltip);
        progressBar.Set_ActionName(actionName);
    }

    protected void Create_ProgressBar() 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            if (PhotonNetwork.IsMasterClient) 
            {
                photonView.RPC(nameof(Stream_ProgressBarCreation), RpcTarget.AllBuffered);
            }
            return;
        }

        Stream_ProgressBarCreation();
    }

    [PunRPC]
    protected void Stream_ProgressBarEnableState(bool isEnabled) 
    {
        progressBar.enabled = isEnabled;
    }

    protected void Set_ProgressBarEnableState(bool isEnabled) 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_ProgressBarEnableState), RpcTarget.AllBuffered, isEnabled);
            return;
        }

        Stream_ProgressBarEnableState(isEnabled);
    }
    #endregion

    private void CleanAndLockObjectLocally() 
    {
        OnCleaned?.Invoke();
        IsCleaned = true;
        IsLocked = true;

        Debug.Log("Succesfully cleaned object!");
        HouseManager.InvokeOnObjectStatusCallback();
    }

    protected virtual void Awake()
    {
        Debug.Log(NetworkManager.localPlayerInformation.team == (int)ownedByTeam);
        if (NetworkManager.localPlayerInformation.team == (int)ownedByTeam) 
        {
            HouseManager.AddInteractableToObservedLists(null, this);
        }

        Create_ProgressBar();
    }

    protected virtual void DirtyObject() 
    {
        OnDirtyObject?.Invoke();
        IsCleaned = false;
        IsLocked = false;

        Debug.Log("Succesfully dirtied the object!");
        HouseManager.InvokeOnObjectStatusCallback();
    }

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
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
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
        Set_ObjectStateToClean();
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(CleaningProgression);
        } 
        else if (stream.IsReading)
        {
            float streamvalue = (float)stream.ReceiveNext();
            Debug.Log(streamvalue);
            progressBar.Set_CurrentProgress(streamvalue / cleaningTime);
        }
    }
}
