using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	public string windowName;

	public virtual void Start()
	{
		ServiceLocator.GetServiceOfType<UIManager>().AddWindowToList(this);
	}

	public virtual void ShowThisWindow()
	{
		transform.GetChild(0).gameObject.SetActive(true);
	}

	public virtual void HideThisWindow()
	{
		transform.GetChild(0).gameObject.SetActive(false);
	}
}
