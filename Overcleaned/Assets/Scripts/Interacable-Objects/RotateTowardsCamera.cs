using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{

    #region ### Property ###
    private Transform m_MainCameraTransform;
    private Transform MainCameraTransform => m_MainCameraTransform ?? (m_MainCameraTransform = Camera.main.transform);
    #endregion

    private void Update()
    {
        if (MainCameraTransform)
        {
            transform.LookAt(MainCameraTransform);
        }
    }
}
