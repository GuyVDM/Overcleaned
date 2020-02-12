using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour, IServiceOfType
{
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

	public void LoadScene(int buildIndex)
	{
		if (sceneLoadingRoutine != null)
		{
			StopCoroutine(sceneLoadingRoutine);
		}

		FadeOut();
		sceneLoadingRoutine = StartCoroutine(SceneLoading(buildIndex));
	}

	public void LoadScene(string buildName)
	{
		if (sceneLoadingRoutine != null)
		{
			StopCoroutine(sceneLoadingRoutine);
		}

		FadeOut();
		sceneLoadingRoutine = StartCoroutine(SceneLoading(buildName));
	}

	private IEnumerator SceneLoading(int buildIndex)
	{
		yield return new WaitUntil(() => isFading == false);

		fadeScreen = null;
		SceneManager.LoadScene(buildIndex);
	}

	private IEnumerator SceneLoading(string buildName)
	{
		yield return new WaitUntil(() => isFading == false);

		fadeScreen = null;
		SceneManager.LoadScene(buildName);
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
