using UnityEngine;
using Photon.Pun;
using System.Linq;

public class WieldableCleanableObject : WieldableObject
{
    public bool IsCleanedAndStored => isCleaned && isStored;

    public bool isCleaned { get; private set; }
    public bool isStored { get; private set; }

    [Header("Cleaning Wieldable Specific Parameters:")]
    public int cleaningWeight;

    [SerializeField]
    private int cleanedToolID = 0;

    [SerializeField]
    private bool isBreakable;

    [Header("References:")]
    [SerializeField]
    private Material dirty_Material;

    [SerializeField]
    private Material cleaned_Material;

    [SerializeField]
    private MeshRenderer object_Renderer;

    #region ### RPC Calls ###
    [PunRPC]
    protected override void Stream_OnInteractionComplete()
    {
        CleanObject();
    }

    [PunRPC]
    private void Stream_OnDirtyObject()
    {
        DirtyObject();
    }

    [PunRPC]
    private void Stream_BreakObjectCompletely() 
    {
        isCleaned = false;
        isStored = false;

        //--- Done in order to prevent the object from being pooled again, thus permanently applying the penalty ---//
        ObjectPool.RemoveObjectFromPool(this.gameObject);
        gameObject.SetActive(false);
    }

    [PunRPC]
    private void Stream_RigidbodyState(bool isKinematic) 
    {
        GetComponent<Rigidbody>().isKinematic = isKinematic;
    }

    public void Set_RigidbodyState(bool isKinematic) 
    {
        if(NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_RigidbodyState), RpcTarget.AllBuffered, isKinematic); 
            return;
        }

        Stream_RigidbodyState(isKinematic);
    }

    private void Set_BreakObjectCompletely()
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_BreakObjectCompletely), RpcTarget.AllBuffered);
            return;
        }

        Stream_BreakObjectCompletely();
    }
    #endregion

    #region ### Private Variables ###
    private const int dirty_LayerMask = 10;

    private int starting_ToolID;
    #endregion

    protected void Awake() 
    {
        starting_ToolID = toolID;

        if ((int)ownedByTeam == NetworkManager.localPlayerInformation.team) 
        {
            HouseManager.AddInteractableToObservedLists(this);
        }

        DirtyObject();
    }

    public void CleanObject() 
    {
        toolID = cleanedToolID;
        isCleaned = true;

        object_Renderer.material = cleaned_Material;
    }

    public void DirtyObject() 
    {
        toolID = starting_ToolID;
        isCleaned = false;

        object_Renderer.material = dirty_Material;
    }

    public void StoreObject()
    {
        isStored = true;
        HouseManager.InvokeOnObjectStatusCallback(NetworkManager.localPlayerInformation.team);
    }

    public void BreakObject() 
    {
        if (isBreakable) 
        {
            Set_BreakObjectCompletely();
        }
    }

    public override void OnEnable() 
    {
        base.OnEnable();

        isStored = false;

        DirtyObject();

        HouseManager.InvokeOnObjectStatusCallback((int)ownedByTeam);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.gameObject.layer == dirty_LayerMask) 
        {
            if(isCleaned == true) 
            {
                if(NetworkManager.IsConnectedAndInRoom) 
                {
                    photonView.RPC(nameof(Stream_OnDirtyObject), RpcTarget.AllBuffered);
                    return;
                }

                Stream_OnDirtyObject();
            }
        }
    }
}
