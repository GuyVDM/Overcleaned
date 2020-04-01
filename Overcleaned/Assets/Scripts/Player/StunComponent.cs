using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class StunComponent : MonoBehaviour
{

    private const float ATTACK_DURATION = 0.3f;
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

    Collider[] collisions;
    List<int> hitPhotonIds = new List<int>();
    private void FixedUpdate()
    {
        const float RADIUS = 0.2f;
        const int TARGETMASK = 1 << 9; //PlayerCollision layer

        collisions = Physics.OverlapSphere(transform.position, RADIUS, TARGETMASK);

        if (collisions.Length > 0)
        {
            for (int i = 0; i < collisions.Length; i++) 
            {
                Debug.Log(collisions[i].name);

                if (collisions[i].transform.parent.gameObject.GetPhotonView().Owner != PhotonNetwork.LocalPlayer)
                {
                    if (AlreadyDetected(collisions[i].transform.parent.gameObject.GetPhotonView().ViewID)) 
                    {
                        const string TARGET_METHOD_NAME = "Stream_StunPlayer";

                        Player owner = collisions[i].transform.parent.gameObject.GetPhotonView().Owner;

                        hitPhotonIds.Add(collisions[i].transform.parent.gameObject.GetPhotonView().ViewID);

                        collisions[i].transform.parent.gameObject.GetPhotonView().RPC(TARGET_METHOD_NAME, owner, PUNISHMENT_DURATION);
                    }
                }
            }
        }
    }
}
