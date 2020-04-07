using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CountdownWindow : UIWindow
{
	public Text countdownText;

	private Animator anim;

	public override void Awake()
	{
		base.Awake();
		anim = GetComponent<Animator>();
	}

	public void ShowText(string newNumber)
	{
		countdownText.text = newNumber;
		anim.SetTrigger("Bounce");
	}
}
