using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks, IServiceOfType
{
    #region ### Service Locator Snippet ###
    private void Awake() => OnInitialise();

    private void OnDestroy() => OnDeinitialise();

    public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);

    public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
    #endregion


    public static GameObject SpawnObjectBasedOnConnectionState(string resourceName, Vector3 pos, Quaternion rotation) 
    {
        if((PhotonNetwork.IsConnected && PhotonNetwork.InRoom)) 
        {
            return PhotonNetwork.InstantiateSceneObject(resourceName, pos, rotation);
        }

        return Instantiate(Resources.Load(resourceName) as GameObject, pos, rotation);
    }
}
