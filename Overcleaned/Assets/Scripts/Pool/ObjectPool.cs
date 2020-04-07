using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[Serializable]
public struct ObjectPoolData
{
    public string poolID;
    public List<GameObject> pooledObjects;

    public ObjectPoolData(string poolID, List<GameObject> pooledObjects)
    {
        this.poolID = poolID;
        this.pooledObjects = pooledObjects;
    }
}

public class ObjectPool : MonoBehaviourPunCallbacks, IServiceOfType
{
    private static List<ObjectPoolData> poolData;

    private static event Action<bool, int> OnSetObjectState;
    private static event Action<int, Vector3, Vector3> OnSetObjectTransform;
    private static event Action<int> OnAddObjectToPool;

    [Header("Pool References:")]
    [SerializeField]
    private List<ObjectPoolData> pool = new List<ObjectPoolData>();

    #region ### ServiceLocator Snipper ###
    private void Awake()
    {
        OnInitialise();
        poolData = pool;

        OnSetObjectState += Set_GameObjectEnableState;
        OnSetObjectTransform += Set_ObjectTransform;
        OnAddObjectToPool += Set_ObjectBackToPool;

    }

    private void OnDestroy()
    {
        OnDeinitialise();

        OnSetObjectState -= Set_GameObjectEnableState;
        OnSetObjectTransform -= Set_ObjectTransform;
        OnAddObjectToPool -= Set_ObjectBackToPool;
    }

    public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);

    public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);

    #endregion

    #region ### RPC Calls ###
    private void Set_GameObjectEnableState(bool enableState, int viewID)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_GameObjectEnableState), RpcTarget.AllBuffered, enableState, viewID);
            return;
        }

        Stream_GameObjectEnableState(enableState, viewID);
    }

    [PunRPC]
    private void Stream_GameObjectEnableState(bool enableState, int viewID)
    {
        GameObject newObject = NetworkManager.GetViewByID(viewID).gameObject;

        if (newObject != null)
        {
            newObject.SetActive(enableState);
        }
    }

    private void Set_ObjectToAddToPool(int viewID)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_ObjectToAddToPool), RpcTarget.AllBuffered, viewID);
            return;
        }
    }

    [PunRPC]
    private void Stream_ObjectToAddToPool(int viewID, string poolID)
    {
        ObjectPoolData poolData = pool.Where(o => o.poolID == poolID).First();
        GameObject newObject = NetworkManager.GetViewByID(viewID).gameObject;

        poolData.pooledObjects.Add(newObject);
    }

    private void Set_ObjectTransform(int viewID, Vector3 pos, Vector3 rot)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_ObjectTransform), RpcTarget.AllBuffered, viewID, pos, rot);
            return;
        }

        Stream_ObjectTransform(viewID, pos, rot);
    }

    [PunRPC]
    private void Stream_ObjectTransform(int viewID, Vector3 pos, Vector3 rot)
    {
        GameObject pooledObjectByViewID = NetworkManager.GetViewByID(viewID).gameObject;

        pooledObjectByViewID.transform.position = pos;
        pooledObjectByViewID.transform.eulerAngles = rot;
    }
    #endregion

    public static void Set_ObjectFromPool(string objectIndentifier, Vector3 pos, Vector3 orientation)
    {
        ObjectPoolData data = poolData.Where(o => o.poolID == objectIndentifier).First();
        GameObject objectToReturn = null;

        if (data.pooledObjects.Count > 0)
        {
            for (int i = 0; i < data.pooledObjects.Count; i++)
            {
                if (data.pooledObjects[i].gameObject.activeSelf == false)
                {
                    objectToReturn = data.pooledObjects[i];
                    break;
                }
            }
        }

        if (objectToReturn == null)
        {
            objectToReturn = PhotonNetwork.InstantiateSceneObject(data.pooledObjects.First().name, pos, Quaternion.Euler(orientation));
            int newObjectViewID = objectToReturn.GetPhotonView().ViewID;

            OnAddObjectToPool.Invoke(newObjectViewID);
            OnSetObjectState.Invoke(true, newObjectViewID);
            OnSetObjectTransform.Invoke(newObjectViewID, pos, orientation);
            return;
        }

        OnAddObjectToPool.Invoke(objectToReturn.GetPhotonView().ViewID);
        OnSetObjectState.Invoke(true, objectToReturn.GetPhotonView().ViewID);
        OnSetObjectTransform.Invoke(objectToReturn.GetPhotonView().ViewID, pos, orientation);
    }

    public static void RemoveObjectFromPool(GameObject objectToCompare)
    {
        foreach (ObjectPoolData data in poolData)
        {
            for (int i = 0; i < data.pooledObjects.Count; i++)
            {
                if (GameObject.ReferenceEquals(objectToCompare, data.pooledObjects[i]))
                {
                    data.pooledObjects.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public static bool HasPooledObjectAvailable(string identifier)
    {
        ObjectPoolData data = poolData.Where(o => o.poolID == identifier).First();

        if (data.pooledObjects.Count > 0)
        {
            for (int i = 0; i < data.pooledObjects.Count; i++)
            {
                if (data.pooledObjects[i].gameObject.activeSelf == false)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void Set_ObjectBackToPool(int objectViewID) => OnSetObjectState(false, objectViewID);
}