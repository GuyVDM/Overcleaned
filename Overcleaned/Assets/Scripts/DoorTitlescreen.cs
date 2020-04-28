using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTitlescreen : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem smokeEffect;

    public void EnableParticles() 
    {
        if (smokeEffect)
        {
            smokeEffect.Play();
            return;
        }

        Debug.LogWarning("[Titlescreen] No particle effect found for the titlescreen door.");
    }
}
