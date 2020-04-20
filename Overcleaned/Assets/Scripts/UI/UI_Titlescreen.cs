using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Titlescreen : UIWindow
{
	public float waitTime;

	public override void ShowThisWindow()
	{
		base.ShowThisWindow();
		StartCoroutine(TitleScreenWait());
	}

	private IEnumerator TitleScreenWait()
	{
		yield return new WaitForSeconds(waitTime);
		ServiceLocator.GetServiceOfType<UIManager>().ShowWindow("Name Input");
	}
}
