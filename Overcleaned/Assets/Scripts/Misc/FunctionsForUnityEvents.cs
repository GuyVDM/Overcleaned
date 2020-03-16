using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionsForUnityEvents : MonoBehaviour
{
	public void ExitServer()
	{
		ServiceLocator.GetServiceOfType<NetworkManager>().LeaveServer();
	}

}
