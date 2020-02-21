using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WieldableCleanableObject : WieldableObject
{
    [Header("Tweakable Parameters:")]
    [SerializeField]
    private float start_Penalty_Amount;

    [SerializeField]
    private int cleanedToolID = 0;

    [Header("Renderers:")]
    [SerializeField]
    private MeshRenderer uncleaned_Variant;

    [SerializeField]
    private MeshRenderer cleaned_Variant;

    #region ### RPC Calls ###
    protected override void Stream_OnInteractionComplete() 
    {
        CleanObject();
    }
    #endregion

    public void CleanObject() 
    {
        toolID = cleanedToolID;
        Debug.LogWarning("[WieldableCleanableObject] This script still needs to assign penalty on instance, and remove when this function is called.");

        if (uncleaned_Variant && cleaned_Variant) 
        {
            uncleaned_Variant.enabled = false;
            cleaned_Variant.enabled = true;
        }
    }
}
