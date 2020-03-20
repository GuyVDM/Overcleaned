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

	[Header("Sub windows - Server Creation")]
	public Transform serverCreationWindow;
	public InputField servernameInputfield;
	public InputField newServerPassworldInputfield;

	[Header("Sub Windows - Server Joining")]
	public Transform serverPasswordWindow;
	public InputField passwordInputfield;

	private List<Transform> allInfoButtons = new List<Transform>();
	private string chosenRoom;

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

	public void OpenSubWindow(Transform window)
	{
		window.gameObject.SetActive(true);
	}

	public void CloseSubWindow(Transform window)
	{
		window.gameObject.SetActive(false);
	}

	public void CreateServer()
	{
		PhotonLobby lobby = ServiceLocator.GetServiceOfType<PhotonLobby>();
		if (lobby.HostRoom(servernameInputfield.text, newServerPassworldInputfield.text))
		{
			CloseSubWindow(serverCreationWindow);
		}
	}

	protected override void OnWindowEnabled()
	{
		CloseSubWindow(serverCreationWindow);
		CloseSubWindow(serverPasswordWindow);
	}

	private void ChangeInfoButton(Transform button, RoomInfo info)
	{
		button.Find("Server Name").GetComponent<TextMeshProUGUI>().text = info.Name;
		button.Find("Player Counter").GetComponent<TextMeshProUGUI>().text = info.PlayerCount.ToString() + "/" + info.MaxPlayers.ToString();

		Button b = button.GetComponent<Button>();
		b.onClick.RemoveAllListeners();

		b.onClick.AddListener(delegate { CheckForPassword(info.Name); });
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

	private void CheckForPassword(string serverName)
	{
		chosenRoom = serverName;
		PhotonLobby pl = ServiceLocator.GetServiceOfType<PhotonLobby>();
		if (pl.ServerHasPassword(chosenRoom))
		{
			OpenSubWindow(serverPasswordWindow);
		}
		else
		{
			pl.JoinRoom(chosenRoom);
		}
	}

	public void TryToJoinRoom()
	{
		PhotonLobby pl = ServiceLocator.GetServiceOfType<PhotonLobby>();
		if (pl.PasswordMatches(chosenRoom, passwordInputfield.text))
		{
			pl.JoinRoom(chosenRoom);
		}
		else
		{
			ServiceLocator.GetServiceOfType<UIManager>().ShowMessage("The password does not match");
		}
	}
}
