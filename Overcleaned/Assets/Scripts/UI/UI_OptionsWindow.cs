using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class UI_OptionsWindow : UIWindow
{
	class GameSettings : SerializableData
	{
		public float masterVolume;
		public float sfxVolume;
		public float musicVolume;
	}

	private GameSettings currentSettings;

	public Slider masterVolumeSlider, sfxVolumeSlider, musicVolumeSlider;

	//File Directory
	private string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Test/Settings";
	private string fileName = "Settings";
	private string fileExtension = ".data";

	public override void Awake()
	{
		if (SerializationManager.FileExists(path, fileName, fileExtension))
		{
			currentSettings = SerializationManager.LoadFile(path, fileName, fileExtension, SerializationManager.SerializationMode.Binary) as GameSettings;
			SetUIToCurrentSettings();
		}
		else
		{
			currentSettings = ReadSettingsFromUI();
		}

		ApplySettingsToGame();

		base.Awake();
	}

	private void SaveSettings()
	{
		currentSettings = ReadSettingsFromUI();
		SerializationManager.SaveFile(currentSettings, path, fileName, fileExtension, SerializationManager.SerializationMode.Binary);
	}

	private GameSettings ReadSettingsFromUI()
	{
		return new GameSettings() { masterVolume = masterVolumeSlider.value, sfxVolume = sfxVolumeSlider.value, musicVolume = musicVolumeSlider.value };
	}

	private void SetUIToCurrentSettings()
	{
		masterVolumeSlider.value = currentSettings.masterVolume;
		sfxVolumeSlider.value = currentSettings.sfxVolume;
		musicVolumeSlider.value = currentSettings.musicVolume;
	}

	private void ApplySettingsToGame()
	{
		//Audio
		AudioMixer audioMixer = ServiceLocator.GetServiceOfType<EffectsManager>().audioMixer;
		audioMixer.SetFloat("Master", currentSettings.masterVolume);
		audioMixer.SetFloat("Sfx", currentSettings.sfxVolume);
		audioMixer.SetFloat("Music", currentSettings.musicVolume);
	}
}
