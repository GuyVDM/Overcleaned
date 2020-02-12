using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInRoomElement : MonoBehaviour
{

	private Dropdown teamDropdown;
	private Text playerName, teamText;
	private bool isLocal;
	private int dropdownIndex;

	public void Init(bool _isLocal)
	{
		isLocal = _isLocal;

		teamDropdown = transform.Find("Team Dropdown").GetComponent<Dropdown>();
		teamText = transform.Find("Team Text").GetComponent<Text>();
		playerName = transform.Find("Player Name").GetComponent<Text>();

		teamDropdown.gameObject.SetActive(isLocal);
		teamText.gameObject.SetActive(!isLocal);
		teamText.text = teamDropdown.options[0].text;
	}

	public void ChangeTeam(int _dropdownIndex)
	{
		teamText.text = teamDropdown.options[dropdownIndex].text;
		dropdownIndex = _dropdownIndex;
	}

	public int GetDropdownIndex()
	{
		return dropdownIndex;
	}

	public int GetDropdownLength()
	{
		return teamDropdown.options.Count;
	}

	public void ChangePlayerName(string newName)
	{
		playerName.text = newName;
	}

}
