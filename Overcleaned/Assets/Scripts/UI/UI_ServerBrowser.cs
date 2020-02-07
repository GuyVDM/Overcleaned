using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class UI_ServerBrowser : UIWindow
{
	public GameObject serverTextPrefab;
	public Transform serverTextParent;

	private List<Transform> allInfoButtons = new List<Transform>();

	public void UpdateRoomInfoButtons(List<RoomInfo> infos)
	{
		for (int i = 0; i < infos.Count; i++)
		{
			if (i < allInfoButtons.Count)
			{
				ChangeInfoButton(allInfoButtons[i], infos[i]);
			}
			else
			{
				ChangeInfoButton(CreateNewInfoButton(), infos[i]);
			}
		}
	}

	private void ChangeInfoButton(Transform button, RoomInfo info)
	{
		button.Find("Server Name").GetComponent<Text>().text = info.Name;
		button.Find("Player Counter").GetComponent<Text>().text = info.PlayerCount.ToString() + "/" + info.MaxPlayers.ToString();
		button.GetComponent<Button>().onClick.AddListener(delegate { OpenRoomInformationWindow(info); });
	}

	private Transform CreateNewInfoButton()
	{
		return Instantiate(serverTextPrefab, Vector3.zero, Quaternion.identity).transform;
	}

	private void OpenRoomInformationWindow(RoomInfo info)
	{
		//check if has password.
	}

}
