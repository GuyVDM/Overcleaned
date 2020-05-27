using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReferencesToDontDestroyOnLoadObjects : MonoBehaviour
{

	public void SetPlayerNickname(InputField inputField)
	{
		ServiceLocator.GetServiceOfType<NetworkManager>().SetLocalPlayerNickname(inputField.text);
	}

}
