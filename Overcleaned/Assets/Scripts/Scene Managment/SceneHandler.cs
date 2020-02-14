using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour, IServiceOfType
{
	[System.Serializable]
	public struct SceneSetting
	{
		public string sceneName;
		public Vector2 bottomLeftCameraAnchor, upperRightCameraAnchor;
	}

	[Header("Scene Settings")]
	public SceneSetting[] sceneSettings;
	public int currentSceneSetting { get; private set; }

	[Header("Fade Properties")]
	public float fadeDuration;
	public Image fadeScreen;

	private Coroutine sceneLoadingRoutine;
	private Coroutine runningFadeRoutine;
	private bool isFading;


	#region Initalize Service
	private void Awake() => OnInitialise();
	private void OnDestroy() => OnDeinitialise();
	public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
	public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
	#endregion

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		Init();
	}

	#region Scene Loading

	public void LoadScene(string buildName)
	{
		currentSceneSetting = GetSceneSettingIndex(buildName);

		if (sceneLoadingRoutine != null)
		{
			StopCoroutine(sceneLoadingRoutine);
		}

		FadeOut();
		sceneLoadingRoutine = StartCoroutine(SceneLoading(buildName));
	}

	public void LoadScene(int arrayIndex)
	{
		currentSceneSetting = arrayIndex;

		if (sceneLoadingRoutine != null)
		{
			StopCoroutine(sceneLoadingRoutine);
		}

		FadeOut();
		sceneLoadingRoutine = StartCoroutine(SceneLoading(sceneSettings[arrayIndex].sceneName));
	}

	private IEnumerator SceneLoading(string buildName)
	{
		yield return new WaitUntil(() => isFading == false);

		fadeScreen = null;
		SceneManager.LoadScene(buildName);
	}

	private int GetSceneSettingIndex(string sceneName)
	{
		for (int i = 0; i < sceneSettings.Length; i++)
		{
			if (sceneSettings[i].sceneName == sceneName)
			{
				return i;
			}
		}

		return -1;
	}

	#endregion
	
	#region Screen Fading
	public void FadeOut()
	{
		if (fadeScreen == null)
			fadeScreen = CreateFadeScreen();

		if (runningFadeRoutine != null)
		{
			StopCoroutine(runningFadeRoutine);
		}

		isFading = true;
		runningFadeRoutine = StartCoroutine(FadeOutRoutine(fadeDuration));
	}

	public void FadeIn()
	{
		if (fadeScreen == null)
			fadeScreen = CreateFadeScreen();

		if (runningFadeRoutine != null)
		{
			StopCoroutine(runningFadeRoutine);
		}

		isFading = true;
		runningFadeRoutine = StartCoroutine(FadeInRoutine(fadeDuration));
	}

	private Image CreateFadeScreen()
	{
		GameObject canvas = Resources.Load("Common Prefabs/Fading Canvas") as GameObject;
		return Instantiate(canvas, Vector3.zero, Quaternion.identity).transform.GetChild(0).GetComponent<Image>();
	}

	private void Init()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		FadeIn();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
	{
		FadeIn();
	}

	private IEnumerator FadeInRoutine(float duration)
	{
		while (fadeScreen.color.a > 0)
		{
			fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, fadeScreen.color.a - Time.deltaTime / duration);
			yield return new WaitForEndOfFrame();
		}

		fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0);
		isFading = false;
	}

	private IEnumerator FadeOutRoutine(float duration)
	{
		while (fadeScreen.color.a < 1)
		{
			fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, fadeScreen.color.a + Time.deltaTime / duration);
			yield return new WaitForEndOfFrame();
		}

		fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 1);
		isFading = false;
	}
	#endregion
}
