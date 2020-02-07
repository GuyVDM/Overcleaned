using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ServiceOfType
{
	private Dictionary<string, UIWindow> allwindows = new Dictionary<string, UIWindow>();

	private void Start()
	{
		HideAllWindows();
	}

	public void AddWindowToList(UIWindow window)
	{
		if (!allwindows.ContainsKey(window.windowName))
			allwindows.Add(window.windowName, window);
	}

	public void ShowWindow(string windowName)
	{
		allwindows[windowName].ShowThisWindow();
	}

	public void HideWindow(string windowName)
	{
		allwindows[windowName].HideThisWindow();
	}

	public void HideAllWindows()
	{
		foreach(KeyValuePair<string, UIWindow> window in allwindows)
		{
			window.Value.HideThisWindow();
		}
	}
}
