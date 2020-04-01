using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class StunComponent : MonoBehaviour
{

    Collider[] collisions;
    List<int> hitPhotonIds = new List<int>();
    Vector3 offset = new Vector3(0, 1, 0);

    private const float RADIUS = 0.4f;
    private const int TARGETMASK = 1 << 9; //PlayerCollision layer

    private const float ATTACK_DURATION = 0.6f;
    private const float PUNISHMENT_DURATION = 3;

    private async void Start()
    {
        await Task.Delay(TimeSpan.FromSeconds(ATTACK_DURATION));

        Destroy(this);
    }

    private bool AlreadyDetected(int toCheck) 
    {
        if (hitPhotonIds.Count > 0)
        {
            if (hitPhotonIds.Contains(toCheck))
            {
                return true;
            }
        }

        return false;
    }

    private void FixedUpdate()
    {
        collisions = Physics.OverlapSphere(transform.position + transform.TransformDirection(offset), RADIUS, TARGETMASK);

        if (collisions.Length > 0)
        {
            for (int i = 0; i < collisions.Length; i++) 
            {
                if (collisions[i].transform.parent.gameObject.GetPhotonView().Owner != PhotonNetwork.LocalPlayer)
                {
                    if (!AlreadyDetected(collisions[i].transform.parent.gameObject.GetPhotonView().ViewID)) 
                    {
                        const string TARGET_METHOD_NAME = "Stream_StunPlayer";
                        const int FORCE = 500;

                        Player owner = collisions[i].transform.parent.gameObject.GetPhotonView().Owner;

                        hitPhotonIds.Add(collisions[i].transform.parent.gameObject.GetPhotonView().ViewID);

                        collisions[i].transform.parent.gameObject.GetPhotonView().RPC(TARGET_METHOD_NAME, owner, PUNISHMENT_DURATION, transform.forward * FORCE);

                        collisions[i].transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * FORCE, transform.position);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(offset), RADIUS);
    }
#endif
}
