using UnityEngine;

public class SpawnpointDisplay : MonoBehaviour
{
    [SerializeField]
    private bool shouldDisplay = true;

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (shouldDisplay)
        {
            UnityEditor.Handles.Label(transform.position, "Player Spawnpoint");
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
#endif
}
