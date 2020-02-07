using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	public string windowName;

	private void Awake()
	{
		//add this window to the ui manager.
	}

	public virtual void ShowThisWindow()
	{

	}

	public virtual void HideThisWindow()
	{

	}
}
