using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	public string windowName;
	internal bool isActive;

	public virtual void Start()
	{
		ServiceLocator.GetServiceOfType<UIManager>().AddWindowToList(this);
	}

	public virtual void ShowThisWindow()
	{
		transform.GetChild(0).gameObject.SetActive(true);
		isActive = true;
		OnWindowEnabled();
	}

	public virtual void HideThisWindow()
	{
		transform.GetChild(0).gameObject.SetActive(false);
		isActive = false;
		OnWindowDisabled();
	}

	protected virtual void OnWindowEnabled() { }
	protected virtual void OnWindowDisabled() { }
}