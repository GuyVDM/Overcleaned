using System;
using System.Threading.Tasks;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField]
    private float destroytime = 3;

    private async void Start() 
    {
        await Task.Delay(TimeSpan.FromSeconds(destroytime));
        Destroy(gameObject);
    }
}
