using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class UI_ServerBrowser : UIWindow
{
	public GameObject serverTextPrefab;
	public Transform serverTextParent;
	public Transform serverCreationWindow;
	public InputField serverCreationInputfield;

	private List<Transform> allInfoButtons = new List<Transform>();

	public void UpdateRoomInfoButtons(List<RoomInfo> infos)
	{
		for (int i = 0; i < allInfoButtons.Count; i++)
		{
			Destroy(allInfoButtons[i].gameObject);
		}

		allInfoButtons.Clear();

		for (int i = 0; i < infos.Count; i++)
		{
			if (infos[i].MaxPlayers > 0)
				ChangeInfoButton(CreateNewInfoButton(), infos[i]);
		}
	}

	public void OpenServerCreationWindow()
	{
		serverCreationWindow.gameObject.SetActive(true);
	}

	public void CloseServerCreationWindow()
	{
		serverCreationWindow.gameObject.SetActive(false);
	}

	public void CreateServer()
	{
		PhotonLobby lobby = ServiceLocator.GetServiceOfType<PhotonLobby>();
		if (lobby.HostRoom(serverCreationInputfield.text))
		{
			CloseServerCreationWindow();
		}
	}

	protected override void OnWindowEnabled()
	{
		CloseServerCreationWindow();
	}

	private void ChangeInfoButton(Transform button, RoomInfo info)
	{
		button.Find("Server Name").GetComponent<TextMeshProUGUI>().text = info.Name;
		button.Find("Player Counter").GetComponent<TextMeshProUGUI>().text = info.PlayerCount.ToString() + "/" + info.MaxPlayers.ToString();

		Button b = button.GetComponent<Button>();
		b.onClick.RemoveAllListeners();
		PhotonLobby pl = ServiceLocator.GetServiceOfType<PhotonLobby>();
		b.onClick.AddListener(delegate { pl.JoinRoom(info.Name); });
	}

	private Transform CreateNewInfoButton()
	{
		Transform infoButton = Instantiate(serverTextPrefab, Vector3.zero, Quaternion.identity).transform;
		infoButton.SetParent(serverTextParent, false);
		allInfoButtons.Add(infoButton);
		RectTransform rTransform = serverTextParent.GetComponent<RectTransform>();
		rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, 70 * allInfoButtons.Count);
		return infoButton;
	}
}
