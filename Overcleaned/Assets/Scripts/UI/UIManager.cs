using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour, IServiceOfType
{
	public static bool UIIsOpen { get; private set; }

	public string openWindowOnStart;
	public KeyBinding[] keyBindings;
	public TextMeshProUGUI messageObject;

	private Dictionary<string, UIWindow> allwindows = new Dictionary<string, UIWindow>();
	private string activeWindowName;

	#region Initalize Service
	private void Awake() => OnInitialise();
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		HideAllWindows();

		if (openWindowOnStart != null && openWindowOnStart != "")
			ShowWindow(openWindowOnStart);
	}

	private void Update()
	{
		for (int i = 0; i < keyBindings.Length; i++)
		{
			if (Input.GetButtonDown(keyBindings[i].buttonName))
			{
				keyBindings[i].functionality.Invoke();
			}
		}
	}

	public void AddWindowToList(UIWindow window)
	{
		if (!allwindows.ContainsKey(window.windowName))
			allwindows.Add(window.windowName, window);
	}

	#region Windows

	public UIWindow ShowWindowReturn(string windowName)
	{
		if (activeWindowName != null && activeWindowName != "")
			if (!allwindows[activeWindowName].lockWindow)
				HideWindow(activeWindowName);
			else
				return allwindows[activeWindowName];

		allwindows[windowName].ShowThisWindow();
		activeWindowName = windowName;

		UIIsOpen = true;
		return allwindows[windowName];
	}

	public void ShowWindow(string windowName)
	{
		ShowWindowReturn(windowName);
	}

	public void HideWindow(string windowName)
	{
		allwindows[windowName].HideThisWindow();
		UIIsOpen = AWindowIsOpen();
	}

	public void ToggleWindow(string windowName)
	{
		if (allwindows[windowName].isActive)
			allwindows[windowName].HideThisWindow();
		else
			allwindows[windowName].ShowThisWindow();

		UIIsOpen = AWindowIsOpen();
	}

	public void HideAllWindows()
	{
		foreach(KeyValuePair<string, UIWindow> window in allwindows)
		{
			window.Value.HideThisWindow();
		}

		UIIsOpen = false;
	}

	private bool AWindowIsOpen()
	{
		bool toReturn = false;

		foreach (KeyValuePair<string, UIWindow> window in allwindows)
		{
			if (window.Value.isActive)
				toReturn = true;
		}

		return toReturn;
	}

	#endregion

	#region Messages

	public void ShowMessage(string message, float duration = 3)
	{
		ShowMessage(message, Color.red, duration);
	}

	public void ShowMessage(string message, Color color, float duration = 3)
	{
		StartCoroutine(MessageToScreen(message, color, duration));
	}

	private IEnumerator MessageToScreen(string message, Color color, float duration)
	{
		if (messageObject == null)
		{
			Debug.LogWarning("You tried to send a message on screen, but the message object is null");
			yield return null;
		}

		messageObject.text = message;
		messageObject.color = color;

		messageObject.GetComponent<Animator>().SetBool("sendMessage", true);
		yield return new WaitForSeconds(duration);
		messageObject.GetComponent<Animator>().SetBool("sendMessage", false);
	}

	#endregion
}
