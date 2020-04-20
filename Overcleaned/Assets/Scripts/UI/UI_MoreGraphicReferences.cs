using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UI_MoreGraphicReferences : MonoBehaviour
{
	public enum UIType
	{
		Toggle,

	}

	public UIType uiType;
	public Image[] imagesToToggle;

	private void Awake()
	{
		AssignToUIElement();
	}

	private void AssignToUIElement()
	{
		switch (uiType)
		{
			case UIType.Toggle:
				Toggle t = GetComponent<Toggle>();
				t.onValueChanged.AddListener(delegate { ValueChanged(t.isOn); });
				break;
		}
	}

	private void ValueChanged(bool isOn)
	{
		ToggleImages(isOn);
	}

	private void ToggleImages(bool isOn)
	{
		for (int i = 0; i < imagesToToggle.Length; i++)
		{
			imagesToToggle[i].enabled = isOn;
		}
	}

}
