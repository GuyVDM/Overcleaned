using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopManualIndicatorHandler : MonoBehaviour
{
    private ObjectStateIndicator indicator;

    private void Start()
    {
        SetupIndicator();
    }

    protected void SetupIndicator() 
    {
        GameObject indicatorObject = GameObject.Instantiate(Resources.Load("[Indicator_Prefab]") as GameObject, Vector3.zero, Quaternion.identity);

        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localPosition = Vector3.zero;
        indicator = indicatorObject.GetComponent<ObjectStateIndicator>();
        indicator.Set_TeamOwner(2);
        indicator.Set_IndicatorState(ObjectStateIndicator.IndicatorState.IsPoop);
    }
}
