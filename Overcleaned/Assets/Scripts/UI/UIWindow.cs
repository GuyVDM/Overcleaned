using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	public string windowName;

	private void Start()
	{
		ServiceLocator.GetServiceOfType<UIManager>().AddWindowToList(this);
	}

	public virtual void ShowThisWindow()
	{
		gameObject.SetActive(true);
	}

	public virtual void HideThisWindow()
	{
		gameObject.SetActive(false);
	}
}
