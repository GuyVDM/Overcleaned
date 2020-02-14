using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleChangingText : MonoBehaviour
{
	[Header("Text Changing")]
	public Text label;
	public string on, off;

	private Toggle toggle;

	private void Awake()
	{
		toggle = GetComponent<Toggle>();
		toggle.onValueChanged.AddListener(delegate { ChangeLabelText(); });
	}

	private void ChangeLabelText()
	{ 
		if (toggle.isOn)
			label.text = on;
		else
			label.text = off;
	}
}
