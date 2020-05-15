using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Be sure to observe this component via a photonview for it to work properly...
/// </summary>
public class EventBasedStorageObject : InteractableObject, IPunObservable
{

    private static Vector3 toPoolObject = new Vector3(1000, 0, 1000);

    private enum StateOfObject 
    {
        Inactive = 0,
        Washing = 1,
        Done = 2
    }

    private enum TooltipType 
    {
        HandsFull = 0,
        ObjectIsFull = 1,
        WrongItemInHands = 2,
        StorageEmpty = 3,
        BusyWashing = 4
    }

    [Header("Object Parameters:")]
    [SerializeField]
    private StateOfObject currentState = StateOfObject.Inactive;

    [SerializeField]
    private Transform[] storagePlaces;

    [SerializeField]
    private int[] acceptedItemIds;

    [SerializeField]
    private int maxSlotsAvailable = 5;

    [SerializeField]
    private float waitingTimeInSeconds = 5;

    [SerializeField]
    private float timeTillRejectionOfObjects = 5;

    [Header("Progressbar Settings:")]
    [SerializeField]
    private string header = "Dishwasher be washing woo woo!";

    [SerializeField]
    private string tooltip = "Busy cleaning everything...";

    [SerializeField]
    private string header_Warning = "O-oh, it's making weird noises...";

    [SerializeField]
    private string tooltip_Warning = "Better unload it fast...";

    [Header("References:")]
    [SerializeField]
    private ProgressBar progressBar;

    [SerializeField]
    private ProgressBar progressBar_Warning;

    [SerializeField]
    private Animator thisAnimator;

    [SerializeField]
    private Image warning_Icon;

    [SerializeField]
    private Animator handsFull_Icon;

    [SerializeField]
    private Animator objectIsFull_Icon;

    [SerializeField]
    private Animator wrongItem_Icon;

    [SerializeField]
    private Animator storageEmpty_Icon;

    [SerializeField]
    private Animator busyWashing_Icon;

    #region ### Private Variables ###

    private List<Transform> allContainedObjects = new List<Transform>();

    private bool canDisplay = true;

    private const float DISPLAY_TOOLTIP_TIMER = 2;

    private float interactionTimerBase = 1;
    private float interactionTimer = 1;
    private float progressbarFill = 0;
    private float timer;

    private Coroutine postcleanLoop;

    private const string BOOL_ISOPEN_STRING = "Open";

    #endregion

    #region ### RPC Calls ###

    [PunRPC]
    private void Stream_StoreObject(int viewID, bool isBeingPooledFrom)
    {
        WieldableCleanableObject toStore = NetworkManager.GetViewByID(viewID).GetComponent<WieldableCleanableObject>();

        toStore.Set_RigidbodyState(!isBeingPooledFrom);
        toStore.CanBeInteractedWith = isBeingPooledFrom;

        if (isBeingPooledFrom)
        {
            toStore.transform.SetParent(null);
            toStore.transform.localPosition = transform.position + transform.forward + transform.up;
            toStore.GetComponent<Collider>().enabled = true;
            timer = 0;
            return;
        }

        allContainedObjects.Add(toStore.transform);

        toStore.transform.SetParent(storagePlaces[allContainedObjects.Count - 1]);
        toStore.transform.localPosition = Vector3.zero;
        toStore.transform.localEulerAngles = Vector3.zero;
        toStore.GetComponent<Collider>().enabled = false;
    }

    private void Set_StoreObject(int viewID, bool isBeingPooledFrom)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_StoreObject), RpcTarget.OthersBuffered, viewID, isBeingPooledFrom);
        }

        Stream_StoreObject(viewID, isBeingPooledFrom);
    }

    [PunRPC]
    private void Stream_PickupObject(int viewID) 
    {
        WieldableCleanableObject toStore = NetworkManager.GetViewByID(viewID).GetComponent<WieldableCleanableObject>();

        toStore.Set_RigidbodyState(true);
        toStore.CanBeInteractedWith = true;
        toStore.transform.SetParent(null);
        toStore.transform.localPosition = transform.position + transform.forward + transform.up;
        toStore.GetComponent<Collider>().enabled = true;
        timer = 0;
        return;
    }

    private void Set_PickupObject(int viewID)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_PickupObject), RpcTarget.OthersBuffered, viewID);
        }

        Stream_PickupObject(viewID);
    }

    [PunRPC]
    private void Stream_EnableStateWarningProgressbar(bool isEnabled) 
    {
        timer = 0;
        progressbarFill = 0;

        progressBar_Warning.enabled = isEnabled;
        warning_Icon.enabled = isEnabled;
    }

    private void Set_EnableStateWarningProgressbar(bool isEnabled) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_EnableStateWarningProgressbar), RpcTarget.Others, isEnabled);
        }

        Stream_EnableStateWarningProgressbar(isEnabled);
    }

    [PunRPC]
    private void Stream_AddForceToObject(int viewID) 
    {
        const float EJECT_FORCE = 4f;

        Rigidbody body = NetworkManager.GetViewByID(viewID).transform.GetComponent<Rigidbody>();
        body.AddForce((transform.forward + transform.up) * EJECT_FORCE, ForceMode.Impulse);
    }

    private void Set_AddForceToObject(int viewID) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_AddForceToObject), RpcTarget.OthersBuffered, viewID);
        }

        Stream_AddForceToObject(viewID);
    }

    [PunRPC]
    private void Stream_DishwasherOpenState(bool isOpen) 
    {
        thisAnimator.SetBool(BOOL_ISOPEN_STRING, isOpen);
    }

    private void Set_DishwasherOpenState(bool isOpen) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_DishwasherOpenState), RpcTarget.Others, isOpen);
        }

        Stream_DishwasherOpenState(isOpen);
    }

    [PunRPC]
    private void Stream_StateOfObject(int objectState) 
    {
        currentState = (StateOfObject)objectState;

        if(currentState == StateOfObject.Inactive) 
        {
            timer = 0;
        }
    }

    private void Set_StateOfObject(StateOfObject objectState) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_StateOfObject), RpcTarget.OthersBuffered, (int)objectState);
        }

        Stream_StateOfObject((int)objectState);
    }

    [PunRPC]
    private void Stream_DisplayStateTooltip(int tooltipType)
    {
        DisplayAnimator((TooltipType)tooltipType);
    }

    private async void Set_DisplayStateTooltip(int tooltipType)
    {
        if (canDisplay)
        {
            canDisplay = false;

            if (NetworkManager.IsConnectedAndInRoom)
            {
                photonView.RPC(nameof(Stream_DisplayStateTooltip), RpcTarget.Others, tooltipType);
            }

            Stream_DisplayStateTooltip(tooltipType);

            await Task.Delay(TimeSpan.FromSeconds(DISPLAY_TOOLTIP_TIMER));

            canDisplay = true;
        }
    }
    [PunRPC]
    private void Stream_ProgressbarPopup(bool isEnabled) 
    {
        progressBar.enabled = isEnabled;

        if (isEnabled) 
        {
            progressBar.Set_CurrentProgress(0);
            return;
        }

        progressBar.Set_BarToFinished();
    }

    private void Set_ProgressbarPopup(bool isEnabled) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_ProgressbarPopup), RpcTarget.OthersBuffered, isEnabled);
        }

        Stream_ProgressbarPopup(isEnabled);
    }

    [PunRPC]
    private void Stream_GrabItemFromObject(int viewID) 
    {
        for(int i = 0; i < allContainedObjects.Count; i++)
        {
            if(allContainedObjects[i].gameObject.GetPhotonView().ViewID == viewID) 
            {
                allContainedObjects.RemoveAt(i);
                break;
            }
        }
    }

    private void Set_GrabItemFromObject(int viewID) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_GrabItemFromObject), RpcTarget.OthersBuffered, viewID);
        }

        Stream_GrabItemFromObject(viewID);
    }

    [PunRPC]
    private void Stream_DisableKineticsOfStoredObjects() 
    {
        for(int i = 0; i < allContainedObjects.Count; i++) 
        {
            allContainedObjects[i].GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private void Set_DisableKineticsOfStoredObjects() 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_DisableKineticsOfStoredObjects), RpcTarget.OthersBuffered);
        }

        Stream_DisableKineticsOfStoredObjects();
    }

    #endregion

    private void Start()
    {
        progressBar.Set_ActionName(header);
        progressBar.Set_Tooltip(tooltip);

        progressBar_Warning.Set_ActionName(header_Warning);
        progressBar_Warning.Set_Tooltip(tooltip_Warning);
    }
         
    public override void DeInteract(PlayerInteractionController interactionController)
    {
        interactionController.DeinteractWithCurrentObject();
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        if (currentState == StateOfObject.Washing || ownedByTeam != (OwnedByTeam)NetworkManager.localPlayerInformation.team)
        {
            Set_DisplayStateTooltip((int)TooltipType.BusyWashing);
            DeInteract(interactionController);
            return;
        }

        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        if (currentState == StateOfObject.Inactive)
        {
            if (interactionController.currentlyWielding)
            {
                if (acceptedItemIds.Contains(interactionController.currentlyWielding.toolID))
                {
                    if (allContainedObjects.Count < maxSlotsAvailable)
                    {
                        if (!allContainedObjects.Contains(interactionController.currentlyWielding.transform))
                        {
                            int viewID = interactionController.currentlyWielding.photonView.ViewID;

                            interactionController.DropObject(interactionController.currentlyWielding);
                            Set_StoreObject(viewID, false);

                            Set_DishwasherOpenState(true);
                            return; 
                        }
                    }

                    Set_DisplayStateTooltip((int)TooltipType.ObjectIsFull);
                    return;
                }

                Set_DisplayStateTooltip((int)TooltipType.WrongItemInHands);
                return;
            }

            if(allContainedObjects.Count > 0) 
            {
                StartWashing();
                Set_DishwasherOpenState(false);
                return;
            }

            Set_DisplayStateTooltip((int)TooltipType.StorageEmpty);
        }

        if (currentState == StateOfObject.Done) 
        {
            if (interactionController.currentlyWielding == null)
            {
                if (allContainedObjects.Count > 0)
                {
                    Set_DishwasherOpenState(true);

                    WieldableCleanableObject toStore = NetworkManager.GetViewByID(allContainedObjects[0].gameObject.GetPhotonView().ViewID).GetComponent<WieldableCleanableObject>();
                    
                    Set_PickupObject(allContainedObjects[0].gameObject.GetPhotonView().ViewID);
                    Set_GrabItemFromObject(allContainedObjects[0].gameObject.GetPhotonView().ViewID);

                    toStore.transform.localPosition = Vector3.zero;

                    interactionController.currentSelected = this;

                    if (allContainedObjects.Count == 0)
                    {
                        if(postcleanLoop != null) 
                        {
                            StopCoroutine(postcleanLoop);
                        }

                        Set_EnableStateWarningProgressbar(false);
                        Set_StateOfObject(StateOfObject.Inactive);
                    }

                    toStore.Interact(interactionController);
                    return;
                }
            }

            Set_DisplayStateTooltip((int)TooltipType.HandsFull);
        }
    }

    private void StartWashing()
    {
        if (allContainedObjects.Count > 0) 
        {
            Set_StateOfObject(StateOfObject.Washing);

            timer = 0;
            Set_ProgressbarPopup(true);

            StartCoroutine(nameof(CleaningLoop));
        }
    }

    private IEnumerator CleaningLoop()
    {
        Set_ProgressbarPopup(true);

        while (timer < waitingTimeInSeconds)
        {
            timer += Time.deltaTime;
            progressBar.Set_CurrentProgress(timer / waitingTimeInSeconds);
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < allContainedObjects.Count; i++)
        {
            allContainedObjects[i].GetComponent<WieldableCleanableObject>().OnToolInteractionComplete();
        }

        Set_ProgressbarPopup(false);

        Set_StateOfObject(StateOfObject.Done);

        postcleanLoop = StartCoroutine(PostCleaningLoop());
    }

    private IEnumerator PostCleaningLoop() 
    {
        const float TIME_TILL_START_REJECTION = 5;

        yield return new WaitForSeconds(TIME_TILL_START_REJECTION);

        Set_EnableStateWarningProgressbar(true);

        while(timer < timeTillRejectionOfObjects && currentState != StateOfObject.Inactive) 
        {
            timer += Time.deltaTime;
            progressBar.Set_CurrentProgress(timer / waitingTimeInSeconds);
            yield return new WaitForEndOfFrame();

        }

        Set_EnableStateWarningProgressbar(false);

        for (int i = 0; i < allContainedObjects.Count; i++)
        {
            allContainedObjects[i].GetComponent<WieldableCleanableObject>().DirtyObject();
        }

        if (currentState == StateOfObject.Done) 
        {
            EjectAll();
            Set_DishwasherOpenState(true);
        }
    }

    private void EjectAll() 
    {
        Set_StateOfObject(StateOfObject.Inactive);
        Set_DisableKineticsOfStoredObjects();

        for (int i = allContainedObjects.Count - 1; i > -1; i--) 
        {
            Rigidbody body = allContainedObjects[i].GetComponent<Rigidbody>();
            allContainedObjects[i].gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
            body.gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);

            Set_StoreObject(allContainedObjects[i].gameObject.GetPhotonView().ViewID, true);
            Set_GrabItemFromObject(allContainedObjects[i].gameObject.GetPhotonView().ViewID);
            Set_AddForceToObject(body.gameObject.GetPhotonView().ViewID);

        }
    }

    public override void OnDisable() 
    {
        base.OnDisable();

        progressBar.enabled = false;
        progressBar_Warning.enabled = false;

        if(currentState == StateOfObject.Washing) 
        {
            currentState = StateOfObject.Inactive;
        }
    }

    private void Update() 
    {
        if(currentState == StateOfObject.Washing) 
        {
            progressbarFill = Mathf.Lerp(progressbarFill, timer, 0.3f * Time.deltaTime);
            progressBar.Set_CurrentProgress(timer / waitingTimeInSeconds);
        }
        else
        if(currentState == StateOfObject.Done) 
        {
            progressbarFill = Mathf.Lerp(progressbarFill, timer, 0.3f * Time.deltaTime);
            progressBar_Warning.Set_CurrentProgress(timer / waitingTimeInSeconds);
        }
    }

    private async void DisplayAnimator(TooltipType type)
    {
            const string POPUP_BOOLID = "Popup";

            Animator toDisplay = null;

            switch (type)
            {
                case TooltipType.HandsFull:
                    toDisplay = handsFull_Icon;
                    break;

                case TooltipType.ObjectIsFull:
                    toDisplay = objectIsFull_Icon;
                    break;

                case TooltipType.StorageEmpty:
                    toDisplay = storageEmpty_Icon;
                    break;

                case TooltipType.WrongItemInHands:
                    toDisplay = wrongItem_Icon;
                    break;

                case TooltipType.BusyWashing:
                    toDisplay = busyWashing_Icon;
                    break;

            }

            toDisplay.SetBool(POPUP_BOOLID, true);

            await Task.Delay(TimeSpan.FromSeconds(DISPLAY_TOOLTIP_TIMER));

            toDisplay.SetBool(POPUP_BOOLID, false);
        }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading)
        {
            timer = (float)stream.ReceiveNext();
        }
        else
        if (stream.IsWriting)
        {
            stream.SendNext(timer);
        }
    }
}
