using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class UI_OptionsWindow : UIWindow
{
	[Serializable]
	private class GameSettings : SerializableData
	{
		public float masterVolume;
		public float sfxVolume;
		public float musicVolume;
	}

	private enum AudioSettings
	{
		Master,
		SFX,
		Music,
	}

	private GameSettings currentSettings;

	public Slider masterVolumeSlider, sfxVolumeSlider, musicVolumeSlider;

	//File Directory
	private string path;
	private string fileName;
	private string fileExtension;

	public override void Awake()
	{
		path = Application.dataPath;
		fileName = "Settings";
		fileExtension = ".data";

		base.Awake();

		AddListenerToSlider();

		if (SerializationManager.FileExists(path, fileName, fileExtension))
		{
			currentSettings = SerializationManager.LoadFile(path, fileName, fileExtension, SerializationManager.SerializationMode.Binary) as GameSettings;
			SetUIToCurrentSettings();
		}
		else
		{
			currentSettings = ReadSettingsFromUI();
		}

		Invoke(nameof(ApplySettingsToGame), 0.01f);

	}

	public void SaveSettings()
	{
		ApplySettingsToGame();
		currentSettings = ReadSettingsFromUI();
		SerializationManager.SaveFile(currentSettings, path, fileName, fileExtension, SerializationManager.SerializationMode.Binary);
	}

	private GameSettings ReadSettingsFromUI()
	{
		return new GameSettings() 
		{ 
			masterVolume = masterVolumeSlider.value, 
			sfxVolume = sfxVolumeSlider.value, 
			musicVolume = musicVolumeSlider.value 
		};
	}

	private void SetUIToCurrentSettings()
	{
		masterVolumeSlider.value = currentSettings.masterVolume;
		sfxVolumeSlider.value = currentSettings.sfxVolume;
		musicVolumeSlider.value = currentSettings.musicVolume;
	}

	private void ApplySettingsToGame()
	{
		AudioMixer audioMixer = ServiceLocator.GetServiceOfType<EffectsManager>().audioMixer;
		audioMixer.SetFloat("MasterVolume", ConvertToAudioMixerValue(masterVolumeSlider.value));
		audioMixer.SetFloat("MusicVolume", ConvertToAudioMixerValue(musicVolumeSlider.value));
		audioMixer.SetFloat("SfxVolume", ConvertToAudioMixerValue(sfxVolumeSlider.value));
	}

	#region Audio

	private void ImmediatelyApplyAudio(AudioSettings setting)
	{
		AudioMixer audioMixer = ServiceLocator.GetServiceOfType<EffectsManager>().audioMixer;

		switch (setting)
		{
			case AudioSettings.Master:
				audioMixer.SetFloat("MasterVolume", ConvertToAudioMixerValue(masterVolumeSlider.value));
				break;
			case AudioSettings.Music:
				audioMixer.SetFloat("MusicVolume", ConvertToAudioMixerValue(musicVolumeSlider.value));
				break;
			case AudioSettings.SFX:
				audioMixer.SetFloat("SfxVolume", ConvertToAudioMixerValue(sfxVolumeSlider.value));
				break;
		}

	}

	private int ConvertToAudioMixerValue(float sliderValue)
	{
		return Mathf.RoundToInt(-80 + (80 * sliderValue));
	}
	private void AddListenerToSlider()
	{
		masterVolumeSlider.onValueChanged.AddListener(delegate { ImmediatelyApplyAudio(AudioSettings.Master); });
		musicVolumeSlider.onValueChanged.AddListener(delegate { ImmediatelyApplyAudio(AudioSettings.Music); });
		sfxVolumeSlider.onValueChanged.AddListener(delegate { ImmediatelyApplyAudio(AudioSettings.SFX); });
	}

	#endregion


}
