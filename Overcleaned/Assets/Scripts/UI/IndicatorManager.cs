using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{

    public static List<ObjectStateIndicator> allIndicators = new List<ObjectStateIndicator>();

    private readonly KeyCode displayIndicatorKey = KeyCode.P;

    private void Update() 
    {
        if(Input.GetKeyDown(displayIndicatorKey)) 
        {
            Set_DisplayStateIndicators(true);
        }

        if(Input.GetKeyUp(displayIndicatorKey)) 
        {
            Set_DisplayStateIndicators(false);
        }
    }

    private void Set_DisplayStateIndicators(bool shouldDisplay)
    {
        if (allIndicators.Count > 0) 
        {
            for (int i = 0; i < allIndicators.Count; i++) 
            {
                allIndicators[i].Set_PopupState(shouldDisplay);
            }
        }
    }
}
