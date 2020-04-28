using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TitlescreenImpulseObject : MonoBehaviour
{
    private static List<TitlescreenImpulseObject> impulseObjects = new List<TitlescreenImpulseObject>();

    // Start is called before the first frame update
    private void Start()
    {
        impulseObjects.Add(this);
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public Rigidbody GetBody() 
    {
        return GetComponent<Rigidbody>();
    }

    private static void BounceObjects() 
    {
        const float FORCE = 4;

        for (int i = 0; i < impulseObjects.Count; i++) 
        {
            Rigidbody current = impulseObjects[i].GetBody();
            current.isKinematic = true;
            current.AddForce(impulseObjects[i].transform.up * FORCE, ForceMode.Impulse);
        }
    }
}
