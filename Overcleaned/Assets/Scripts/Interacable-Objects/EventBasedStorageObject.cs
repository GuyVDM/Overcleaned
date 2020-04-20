using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Be sure to observe this component via a photonview for it to work properly...
/// </summary>
public class EventBasedStorageObject : InteractableObject, IPunObservable
{

    // SUMMARY: How this object works
    //          Step 1. Bring dirty dishes to the object
    //          Step 2. Store the dishes (max is defined by a custom amount
    //          Step 3. Progressbar will pop up, telling the players how far it is with cleaning (during this you won't be able to interact with the object)
    //          Step 4. Progressbar will fade, making the object interactable again.
    //          Step 5. Players will be allowed to gather clean dishes from this object.
    //          Step 6. If the object is empty, it'll set back to a inactive state.
    //
    //          NOTE: Another timer will run whenever the object is done cleaning, if it's not emptied in time,
    //                in that scenario, the object will eject everything that it's currently holding, setting it to a inactive state afterwards.

    // TODO: Test current functionality for bugs before continuing on
    //       to the next part, as such building a prefab.
    //

    // TODO: Start a timer whenever the object is done cleaning,
    //       if the timer supposedly hits 0, eject and dirty all
    //       of the dishes inside.

    // TODO: Display all dishes on the rack, make animations of opening the door and rack rolling out.



    private static Vector3 toPoolObject = new Vector3(1000, 0, 1000);

    private enum StateOfObject 
    {
        Inactive = 0,
        Washing = 1,
        Done = 2
    }

    [Header("Object Parameters:")]
    [SerializeField]
    private StateOfObject currentState = StateOfObject.Inactive;

    [SerializeField]
    private int[] acceptedItemIds;

    [SerializeField]
    private int maxSlotsAvailable = 5;

    [SerializeField]
    private float waitingTimeInSeconds = 5;

    [Header("Progressbar Settings:")]
    [SerializeField]
    private string header = "Dishwasher be washing woo woo!";

    [SerializeField]
    private string tooltip = "Busy cleaning everything...";

    [Header("References:")]
    [SerializeField]
    private ProgressBar progressBar;

    #region ### Private Variables ###

    public List<Transform> allContainedObjects = new List<Transform>();

    private bool canInteract = true;

    private float interactionTimerBase = 1;
    private float interactionTimer = 1;

    private float timer;

    #endregion

    #region ### RPC Calls ###

    [PunRPC]
    private void Stream_StoreObject(int viewID, bool isBeingPooledFrom)
    {
        WieldableCleanableObject toStore = NetworkManager.GetViewByID(viewID).GetComponent<WieldableCleanableObject>();

        toStore.Set_RigidbodyState(true);
        toStore.CanBeInteractedWith = isBeingPooledFrom;

        if (isBeingPooledFrom)
        {
            toStore.transform.localPosition = Vector3.zero;
            return;
        }

        allContainedObjects.Add(toStore.transform);
        toStore.transform.position = toPoolObject;
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
    private void Stream_StateOfObject(int objectState) 
    {
        currentState = (StateOfObject)objectState;
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
    #endregion

    private void Start()
    {
        progressBar.Set_ActionName(header);
        progressBar.Set_Tooltip(tooltip);
    }
         
    public override void DeInteract(PlayerInteractionController interactionController)
    {
        interactionController.DeinteractWithCurrentObject();
    }

    public override void Interact(PlayerInteractionController interactionController)
    {
        Debug.Log("Trying to interact...");
        if (currentState == StateOfObject.Washing || ownedByTeam != (OwnedByTeam)NetworkManager.localPlayerInformation.team)
        {
            //Display a message that it's busy washing...
            DeInteract(interactionController);
            return;
        }

        Debug.Log("Passed first hurdle...");

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
                            return;
                        }
                    }

                    //Display message for object being full...
                    return;
                }

                //Display message for wrong item...
                return;
            }

            if(allContainedObjects.Count > 0) 
            {
                StartWashing();
                return;
            }

            //Display message that storage is empty...
        }

        if(currentState == StateOfObject.Done) 
        {
            Debug.Log("Object is done cleaning...");
            if (interactionController.currentlyWielding == null)
            {
                Debug.Log("Nothing in hands, so all good");
                if (allContainedObjects.Count > 0)
                {
                    Debug.Log("Picking object...");
                    WieldableCleanableObject toStore = NetworkManager.GetViewByID(allContainedObjects[0].gameObject.GetPhotonView().ViewID).GetComponent<WieldableCleanableObject>();

                    Set_StoreObject(allContainedObjects[0].gameObject.GetPhotonView().ViewID, true);
                    Set_GrabItemFromObject(allContainedObjects[0].gameObject.GetPhotonView().ViewID);

                    toStore.Interact(interactionController);

                    toStore.transform.localPosition = Vector3.zero;

                    interactionController.currentSelected = this;

                    if (allContainedObjects.Count == 0)
                    {
                        Set_StateOfObject(StateOfObject.Inactive);
                    }
                }
            }
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
            allContainedObjects[i].GetComponent<WieldableCleanableObject>().CleanObject();
        }

        Set_ProgressbarPopup(false);
        Set_StateOfObject(StateOfObject.Done);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsReading) 
        {
            progressBar.Set_CurrentProgress(timer);
            timer = (float)stream.ReceiveNext();
        } 
        else
        if(stream.IsWriting) 
        {
            stream.SendNext(timer);
        }
    }
}
