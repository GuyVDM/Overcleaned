using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanRotation : MonoBehaviour
{
    public int speed;

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
    }
}
