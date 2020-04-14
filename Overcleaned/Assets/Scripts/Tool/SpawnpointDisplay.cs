using UnityEngine;

public class SpawnpointDisplay : MonoBehaviour
{

#if UNITY_EDITOR
    public void OnDrawGizmos() => Gizmos.DrawWireCube(transform.position, Vector3.one);
#endif
}
