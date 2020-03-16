using UnityEngine;
using Photon.Pun;

public class WieldableCleanableObject : WieldableObject
{
    [Header("Tweakable Parameters:")]
    [SerializeField]
    private float start_Penalty_Amount;

    [SerializeField]
    private int cleanedToolID = 0;

    [SerializeField]
    private bool isBreakable;

    [Header("Renderers:")]
    [SerializeField]
    private MeshRenderer uncleaned_Variant;

    [SerializeField]
    private MeshRenderer cleaned_Variant;

    [Header("Debug Settings:")]
    public bool isCleaned = false;

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
    #endregion

    #region ### Private Variables ###
    private const int dirty_LayerMask = 10;

    private int starting_ToolID;
    #endregion

    private void Awake() => starting_ToolID = toolID;

    public void CleanObject() 
    {
        toolID = cleanedToolID;
        isCleaned = true;
        Debug.LogWarning("[WieldableCleanableObject] This script still needs to assign penalty on instance, and remove when this function is called.");

        if (uncleaned_Variant && cleaned_Variant) 
        {
            uncleaned_Variant.enabled = false;
            cleaned_Variant.enabled = true;
        }
    }

    public void DirtyObject() 
    {
        toolID = starting_ToolID;
        isCleaned = false;

        if (uncleaned_Variant && cleaned_Variant) 
        {
            uncleaned_Variant.enabled = true;
            cleaned_Variant.enabled = false;

            if(isBreakable) 
            {
                BreakObject();
            }
        }
    }

    public void BreakObject() 
    {
        Debug.Log("Should break object later on.");
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
