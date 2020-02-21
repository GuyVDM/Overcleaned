using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInRoomElement : MonoBehaviour
{
	public bool isLocal { get; private set; }

	private Dropdown teamDropdown;
	private Text playerName, teamText, readyText;
	private Toggle readyToggle;
	private int dropdownIndex;

	public void Init(bool _isLocal)
	{
		isLocal = _isLocal;

		teamDropdown = transform.Find("Team Dropdown").GetComponent<Dropdown>();
		teamText = transform.Find("Team Text").GetComponent<Text>();
		playerName = transform.Find("Player Name").GetComponent<Text>();
		readyToggle = transform.Find("Ready Toggle").GetComponent<Toggle>();
		readyText = transform.Find("Ready Text").GetComponent<Text>();

		teamDropdown.gameObject.SetActive(isLocal);
		readyToggle.gameObject.SetActive(isLocal);
		teamText.gameObject.SetActive(!isLocal);
		readyText.gameObject.SetActive(!isLocal);
		teamText.text = teamDropdown.options[0].text;
		NetworkManager.localPlayerInformation.team = 0;
	}

	public void ChangeTeam(int _dropdownIndex)
	{
		dropdownIndex = _dropdownIndex;
		teamText.text = teamDropdown.options[_dropdownIndex].text;
	}

	public int GetDropdownIndex()
	{
		if (isLocal)
			return teamDropdown.value;
		else
			return dropdownIndex;
	}

	public void AddListenerToDropdown(UnityEngine.Events.UnityAction<int> call)
	{
		teamDropdown.onValueChanged.AddListener(call);
	}

	public void AddListenerToToggle(UnityEngine.Events.UnityAction<bool> call)
	{
		readyToggle.onValueChanged.AddListener(call);
	}

	public int GetDropdownLength()
	{
		return teamDropdown.options.Count;
	}

	public void ChangePlayerName(string newName)
	{
		playerName.text = newName;
	}

	public void SetToggle(bool toggle)
	{
		if (toggle)
			readyText.text = "Ready";
		else
			readyText.text = "Not Ready";
	}

	public bool GetToggleValue()
	{
		return readyToggle.isOn;
	}

	public bool IsReady()
	{
		if (isLocal)
			return readyToggle.isOn;
		else
			if (readyText.text == "Ready")
			return true;

		return false;
	}

}
