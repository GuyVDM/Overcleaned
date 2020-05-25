using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class CleanableObject : InteractableObject, IPunObservable
{

    [Header("References:")]
    [SerializeField]
    private Material dirty_Material;

    [SerializeField]
    private Material cleaned_Material;

    [SerializeField]
    private MeshRenderer object_Renderer;

    [Header("Tweakable Parameters:")]
    [SerializeField]
    public int cleaningWeight = 25;

    [SerializeField]
    private float cleaningTime = 5;

    [Header("On Cleaned/Dirty Callbacks:")]
    [Tooltip("Used to play particles for example, or to turn something off/invoke an action.")]
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

    protected int interactionSoundNumber;

    #region ### Properties ###
    public bool IsCleaned { get; set; }

    protected float CleaningProgression { get; set; }
    #endregion

    #region ### Private Variables ###
    protected ProgressBar progressBar;

    protected ObjectStateIndicator indicator;

    protected bool passedFirstFrame = false;
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

        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Clean);

        object_Renderer.material = cleaned_Material;
        Debug.Log("Succesfully cleaned object!");
    }

    protected void SetupIndicator() 
    {
        GameObject indicatorObject = GameObject.Instantiate(Resources.Load("[Indicator_Prefab]") as GameObject, Vector3.zero, Quaternion.identity);

        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localPosition = Vector3.zero + Vector3.up;
        indicator = indicatorObject.GetComponent<ObjectStateIndicator>();
        indicator.Set_TeamOwner((int)ownedByTeam);
    }

    protected virtual void Set_IndicatorStartState() 
    {
        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Dirty);
    }

    protected virtual void Awake()
    {
        if (NetworkManager.localPlayerInformation.team == (int)ownedByTeam) 
        {
            HouseManager.AddInteractableToObservedLists(null, this);
        }

        Create_ProgressBar();

    }

    protected virtual void Start() 
    {
        SetupIndicator();
        Set_IndicatorStartState();
    }

    protected virtual void DirtyObject() 
    {
        OnDirtyObject?.Invoke();
        IsCleaned = false;
        IsLocked = false;

        object_Renderer.material = dirty_Material;
        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.Dirty);
        Debug.Log("Succesfully dirtied object!");

        HouseManager.InvokeOnObjectStatusCallback((int)ownedByTeam);
    }

    protected virtual void OnStartInteraction() 
    {
        if(passedFirstFrame == false) 
        {
            passedFirstFrame = true;
        }

        interactionSoundNumber = ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudioMultiplayer("Clean Loop", audioMixerGroup: "Sfx", spatialBlend: 1, audioPosition: transform.position);
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        if(IsLocked) 
        {
            interactionController.DeinteractWithCurrentObject();
            return;
        }

        OnStartInteraction();

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
                progressBar.Set_BarToFinished();
                OnCleanedObject(interactionController);
                interactionController.DeinteractWithCurrentObject();
            }
        }
    }

    public override void DeInteract(PlayerInteractionController interactionController) 
    {
        IsLocked = false;
        passedFirstFrame = false;
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

        ServiceLocator.GetServiceOfType<EffectsManager>().StopAudio(interactionSoundNumber);
    }

    public virtual void OnCleanedObject(PlayerInteractionController interactionController) 
    {
        Set_ObjectStateToClean();
        HouseManager.InvokeOnObjectStatusCallback((int)ownedByTeam);

        ServiceLocator.GetServiceOfType<EffectsManager>().PlayAudioMultiplayer("Cleaned");
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
