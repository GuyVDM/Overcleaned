using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{

    #region ### Property ###
    private Transform m_MainCameraTransform;
    private Transform MainCameraTransform => m_MainCameraTransform ?? (m_MainCameraTransform = ServiceLocator.GetServiceOfType<PlayerManager>().player_CameraController.transform);
    #endregion

    private bool canUpdate = false;

    private void Start() 
    {
        canUpdate = true;    
    }

    private void Update()
    {
        if (canUpdate) 
        {
            if (MainCameraTransform) 
            {
                transform.LookAt(MainCameraTransform);
            }
        }
    }
}
