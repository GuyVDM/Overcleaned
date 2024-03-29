﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class EffectsManager : MonoBehaviourPun, IServiceOfType
{
    #region Structs & Sub classes & enums
    public class EffectTracker<T>
    {
        public EffectTracker(int newId)
        {
            ID = newId;
        }

        public int ID { get; protected set; }
        public T reference { get; protected set; }
        public bool referenceIsSet { get; protected set; }

        public virtual void CreateReference(T newRef)
        {
            reference = newRef;
            referenceIsSet = true;
        }

        public virtual void DeleteReference()
        {
            reference = default;
            referenceIsSet = false;
        }
    }

    public class ParticleTracker : EffectTracker<ParticleSystem>
    {
        public Coroutine deathTimer;

        public ParticleTracker(int newId) : base(newId)
        {
            ID = newId;
        }

        public IEnumerator ParticleDeathTimer()
        {
            yield return new WaitForSeconds(reference.main.duration);

            if (reference != null)
                reference.Stop();

            while(reference.particleCount > 0)
            {
                yield return null;
            }

            DeleteParticle();
        }

        public IEnumerator Stop()
        {
            if (reference == null)
                yield return null;

            reference.Stop();

            while (reference.particleCount > 0)
            {
                yield return null;
            }

            DeleteParticle();
        }

        private void DeleteParticle()
        {
            Destroy(reference.gameObject);

            DeleteReference();
        }
    }

    [System.Serializable]
    public struct PlayOnStart
    {
        public string audioName;
        public string audioMixerGroup;
        public bool loop;
        [Range(0, 1)]
        public float volume;
        public float delay;
    }

    public enum SyncMode
    {
        Singleplayer,
        Multiplayer,
    }

	#endregion

	#region Audio Variables

	[Header("Audio Properties")]
    public AudioMixer audioMixer;
    public AudioClip[] allAudioclips;
    public PlayOnStart[] playOnStartClips;
    public int startSources;

    private List<EffectTracker<AudioSource>> localAudioSources = new List<EffectTracker<AudioSource>>();
    private List<EffectTracker<AudioSource>> syncedAudioSources = new List<EffectTracker<AudioSource>>();

    #endregion

    #region Particle Variables

    [Header("Particle Properties")]
    public ParticleSystem[] allParticleSystemPrefabs;

    private readonly List<ParticleTracker> activeParticleSystems = new List<ParticleTracker>();

    #endregion

    #region Initalize Service
    private void Awake() => OnInitialise();
    private void OnDestroy() => OnDeinitialise();
    public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);
    public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
    #endregion

    private void Start()
    {
        for (int i = 0; i < startSources; i++)
        {
            CreateNewAudioSource(SyncMode.Singleplayer);
        }

        foreach (PlayOnStart playOnStart in playOnStartClips)
        {
            if (playOnStart.delay > 0)
                StartCoroutine(AudioOnStartDelay(playOnStart));
            else
                PlayAudio(playOnStart.audioName, volume: playOnStart.volume, loop: playOnStart.loop, audioMixerGroup: playOnStart.audioMixerGroup);
        }
    }

    #region Public Audio Functions

    //------------------------------------------------------ Public Audio Functions ----------------------------------------------\\

    /// <summary>
    /// Plays an Audioclip.
    /// <para>If you don't have an AudioClip to play, use the FindAudioClip function.</para>
    /// </summary>
    /// <param name="toPlay">Audio clip to play</param>
    /// <param name="volume">volume of the audioSource</param>
    /// <param name="loop">Should the audio be looping?</param>
    /// <param name="pitch">pitch of the audio</param>
    /// <param name="spatialBlend">Should the audio be 3D?</param>
    /// <param name="audioPosition">Position of audio in worldspace. Only effective if the spatialBlend parameter is not 0</param>
    /// <param name="parent">The object that the audio source will be parented to. audioPosition will be ignored if this parameter is filled.</param>
    /// <param name="fade">Should the audio fade in?</param>
    /// <param name="step">Amount that will be added to volume when fading</param>
    /// <param name="audioMixerGroup">Thhe AudioMixerGroup that will be added to the AudioSource that will play the audioclip</param>
    /// <returns>Returns AudioSource ID used to stop that specific audioSource.</returns>
    public int PlayAudio(AudioClip toPlay, float volume = 1, bool loop = false, float pitch = 1, float spatialBlend = 0, float minDistance = 1, float maxDistance = 20,  Vector3 audioPosition = default, Transform parent = null, bool fade = false, float step = 0.1f, string audioMixerGroup = "Master")
    {
        EffectTracker<AudioSource> tracker = FindAvailableSource(SyncMode.Singleplayer);
        AudioSource source = tracker.reference;

        source.clip = toPlay;
        source.volume = fade ? 0 : volume;
        source.loop = loop;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;

        if (parent != null)
        {
            source.transform.SetParent(parent);
            source.transform.position = parent.position;
        }
        else
        {
            source.transform.position = audioPosition;
        }

        if (audioMixer != null)
        {
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioMixerGroup);
            if (audioMixerGroups.Length <= 0)
            {
                audioMixerGroups = audioMixer.FindMatchingGroups("Master");
            }

            source.outputAudioMixerGroup = audioMixerGroups[0];
        }

        source.Play();

        if (fade)
            StartCoroutine(AudioFadeIn(source, volume, step));

        return tracker.ID;
    }

    /// <summary>
    /// Plays an Audioclip.
    /// </summary>
    /// <param name="audioName">Name of the AudioClip that will be played (Clip needs to be stored in the Effects Manager)</param>
    /// <param name="volume">volume of the audioSource</param>
    /// <param name="loop">Should the Audio be looping?</param>
    /// <param name="pitch">pitch of the Audio</param>
    /// <param name="spatialBlend">Should the audio be 3D?</param>
    /// <param name="audioPosition">Position of audio in worldspace. Only effective if the spatialBlend parameter is not 0</param>
    /// <param name="parent">The object that the audio source will be parented to. audioPosition will be ignored if this parameter is filled.</param>
    /// <param name="fade">Should the audio fade in?</param>
    /// <param name="step">Amount that will be added to volume when fading</param>
    /// <param name="audioMixerGroup">Thhe AudioMixerGroup that will be added to the AudioSource that will play the audioclip</param>
    /// <returns>Returns AudioSource ID used to stop that specific audioSource.</returns>
    public int PlayAudio(string audioName, float volume = 1, bool loop = false, float pitch = 1, float spatialBlend = 0, float minDistance = 1, float maxDistance = 20, Vector3 audioPosition = default, Transform parent = null, bool fade = false, float step = 0.1f, string audioMixerGroup = null)
    {
        return PlayAudio(FindAudioClip(audioName), volume, loop, pitch, spatialBlend, minDistance, maxDistance, audioPosition, parent, fade, step, audioMixerGroup);
    }

    public int PlayAudioMultiplayer(string audioName, float volume = 1, bool loop = false, float pitch = 1, float spatialBlend = 0, float minDistance = 1, float maxDistance = 20, Vector3 audioPosition = default, bool fade = false, float step = 0.1f, string audioMixerGroup = "Master")
    {
        int audioSourceID = PlayAudioFromRPC(FindAudioClip(audioName), volume: volume, loop: loop, pitch: pitch, spatialBlend: spatialBlend, minDistance: minDistance, maxDistance: maxDistance, audioPosition: audioPosition, fade: fade, step: step, audioMixerGroup: audioMixerGroup);

        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(PlayAudioRPC), RpcTarget.Others, audioName, volume, loop, pitch, spatialBlend, minDistance, maxDistance, audioPosition.x, audioPosition.y, audioPosition.z, fade, step, audioMixerGroup);

        return audioSourceID;
    }

    /// <summary>
    /// Simplified PlayAudio Function to use with UnityEvents.
    /// </summary>
    /// <param name="toPlay"></param>
    public void SimplePlayAudio(AudioClip toPlay)
    {
        PlayAudio(toPlay);
    }

    /// <summary>
    /// Finds an AudioClip stored in the Effect Manager
    /// </summary>
    /// <param name="audioName">Name of the AudioClip stored in the Effects Manager</param>
    /// <returns>Returns the AudioClip that will be played</returns>
    public AudioClip FindAudioClip(string audioName)
    {
        for (int i = 0; i < allAudioclips.Length; i++)
        {
            if (allAudioclips[i] != null && allAudioclips[i].name == audioName)
            {
                return allAudioclips[i];
            }
        }

        Debug.LogError("There is no AudioClip named " + audioName + " In the Effects Manager");
        return null;
    }

    /// <summary>
    /// Checks if the toCheck AudioClip is already being played.
    /// </summary>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    public bool AudioClipIsPlaying(AudioClip toCheck, SyncMode syncMode)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].reference.clip != null)
            {
                if (audioSources[i].reference.clip.name == toCheck.name)
                {
                    if (audioSources[i].reference.isPlaying)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the toCheck AudioClip is already being played.
    /// </summary>
    /// <param name="clipName">Name of the AudioClip that will be checked</param>
    /// <returns></returns>
    public bool AudioClipIsPlaying(string clipName, SyncMode syncMode)
    {
        AudioClip toCheck = FindAudioClip(clipName);

        return AudioClipIsPlaying(toCheck, syncMode);
    }

    /// <summary>
    /// Stops the audio that is being played on the audiosource with audioSourceID.
    /// </summary>
    /// <param name="audiosourceID">ID of the Audio Source that will be checked.</param>
    public void StopAudio(int audioSourceID, SyncMode syncMode, bool fade = false, float step = 0.1f)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].ID == audioSourceID)
            {
                if (!fade)
                    audioSources[i].reference.Stop();
                else
                    StartCoroutine(AudioFadeOut(audioSources[i].reference, step));
            }
        }
    }

    public void StopAudioMultiplayer(int audioSourceID, bool fade = false, float step = 0.1f)
    {
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(StopAudioRPC), RpcTarget.Others, audioSourceID, fade, step);

        StopAudio(audioSourceID, SyncMode.Multiplayer, fade, step);
    }

    /// <summary>
    /// Stops all instances of the toStop AudioClip.
    /// </summary>
    /// <param name="toStop">All playing instances of the audioclip will be stopped.</param>
    public void StopAllPlayingClips(AudioClip toStop)
    {
        for (int i = 0; i < localAudioSources.Count; i++)
        {
            if (localAudioSources[i].reference.clip.name == toStop.name)
            {
                localAudioSources[i].reference.Stop();
                // audioSources[i].clip = null;
            }
        }
    }

    public void StopAllPlayingClipsMultiplayer(string audioClipName)
    {
        if (PhotonNetwork.InRoom)
            photonView.RPC("StopAllPlayingClipsRPC", RpcTarget.Others, audioClipName);

        StopAllPlayingClips(audioClipName);
    }

    /// <summary>
    /// Stops all instances of the toStop AudioClip.
    /// </summary>
    /// <param name="clipName">Name of the AudioClip that will be stopped</param>
    public void StopAllPlayingClips(string clipName)
    {
        AudioClip toStop = FindAudioClip(clipName);

        StopAllPlayingClips(toStop);
    }

    /// <summary>
    /// Adjust the volume of the audio source with the specified audioSourceID.
    /// </summary>
    /// <param name="audioSourceID">ID of the audio source you want to adjust.</param>
    /// <param name="volume">volume you want to set the audiosource to.</param>
    public void AdjustVolume(int audioSourceID, float volume, SyncMode syncMode)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].ID == audioSourceID)
            {
                audioSources[i].reference.volume = volume;
            }
        }
    }

    #endregion

    #region Private Audio Functions
    //------------------------------------------------------ Private Audio Functions ----------------------------------------------\\

    /// <summary>
    /// Plays an Audioclip.
    /// <para>If you don't have an AudioClip to play, use the FindAudioClip function.</para>
    /// </summary>
    /// <param name="toPlay">Audio clip to play</param>
    /// <param name="volume">volume of the audioSource</param>
    /// <param name="loop">Should the audio be looping?</param>
    /// <param name="pitch">pitch of the audio</param>
    /// <param name="spatialBlend">Should the audio be 3D?</param>
    /// <param name="audioPosition">Position of audio in worldspace. Only effective if the spatialBlend parameter is not 0</param>
    /// <param name="parent">The object that the audio source will be parented to. audioPosition will be ignored if this parameter is filled.</param>
    /// <param name="fade">Should the audio fade in?</param>
    /// <param name="step">Amount that will be added to volume when fading</param>
    /// <param name="audioMixerGroup">Thhe AudioMixerGroup that will be added to the AudioSource that will play the audioclip</param>
    /// <returns>Returns AudioSource ID used to stop that specific audioSource.</returns>
    private int PlayAudioFromRPC(AudioClip toPlay, float volume = 1, bool loop = false, float pitch = 1, float spatialBlend = 0, float minDistance = 1, float maxDistance = 20, Vector3 audioPosition = default, Transform parent = null, bool fade = false, float step = 0.1f, string audioMixerGroup = "Master")
    {
        EffectTracker<AudioSource> tracker = FindAvailableSource(SyncMode.Multiplayer);
        AudioSource source = tracker.reference;

        source.clip = toPlay;
        source.volume = fade ? 0 : volume;
        source.loop = loop;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;

        if (parent != null)
        {
            source.transform.SetParent(parent);
            source.transform.position = parent.position;
        }
        else
        {
            source.transform.position = audioPosition;
        }

        if (audioMixer != null)
        {
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioMixerGroup);
            if (audioMixerGroups.Length <= 0)
            {
                audioMixerGroups = audioMixer.FindMatchingGroups("Master");
            }

            source.outputAudioMixerGroup = audioMixerGroups[0];
        }

        source.Play();

        if (fade)
            StartCoroutine(AudioFadeIn(source, volume, step));

        return tracker.ID;
    }

    private EffectTracker<AudioSource> FindAvailableSource(SyncMode syncMode)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        for (int s = 0; s < audioSources.Count; s++)
        {
            if (!audioSources[s].reference.isPlaying)
            {
                return audioSources[s];
            }
        }

        return CreateNewAudioSource(syncMode);
    }

    private AudioSource FindSourceByID(int id, SyncMode syncMode)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].ID == id)
            {
                return audioSources[i].reference;
            }
        }

        return null;
    }

    private EffectTracker<AudioSource> CreateNewAudioSource(SyncMode syncMode)
    {
        List<EffectTracker<AudioSource>> audioSources = GetCorrectList(syncMode);

        string sync = syncMode == SyncMode.Singleplayer ? "Singleplayer" : "Multiplayer";

        GameObject newSource = new GameObject
        {
            name = "AudioSource " + sync + " " + audioSources.Count
        };

        newSource.transform.SetParent(transform);

        AudioSource source = newSource.AddComponent<AudioSource>();
        source.playOnAwake = false;

        EffectTracker<AudioSource> toReturn = new EffectTracker<AudioSource>(audioSources.Count);
        toReturn.CreateReference(source);
        audioSources.Add(toReturn);

        return toReturn;
    }

    private IEnumerator AudioFadeIn(AudioSource source, float goal, float step)
    {
        while (source.volume < goal)
        {
            source.volume += step * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AudioFadeOut(AudioSource source, float step)
    {
        while (source.volume > 0)
        {
            source.volume -= step * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AudioOnStartDelay(PlayOnStart audio)
    {
        yield return new WaitForSeconds(audio.delay);
        PlayAudio(audio.audioName, volume: audio.volume, loop: audio.loop);
    }

    private List<EffectTracker<AudioSource>> GetCorrectList(SyncMode syncMode)
    {
        if (syncMode == SyncMode.Singleplayer)
        {
            return localAudioSources;
        }
        else if (syncMode == SyncMode.Multiplayer)
        {
            return syncedAudioSources;
        }

        return localAudioSources;
    }

	#endregion

	#region Audio RPCs

    [PunRPC]
    private void PlayAudioRPC(string audioName, float volume = 1, bool loop = false, float pitch = 1, float spatialBlend = 0, float minDistance = 1, float maxDistance = 20, float posX = 0, float posY = 0, float posZ = 0, bool fade = false, float step = 0.1f, string audioMixerGroup = null)
    {
        PlayAudioFromRPC(FindAudioClip(audioName), volume: volume, loop: loop, pitch: pitch, spatialBlend: spatialBlend, minDistance: minDistance, maxDistance: maxDistance, audioPosition: new Vector3(posX, posY, posZ), fade: fade, step: step, audioMixerGroup: audioMixerGroup);
    }

    [PunRPC]
    private void StopAudioRPC(int audioID, bool fade = false, float step = 0.1f)
    {
        StopAudio(audioID, SyncMode.Multiplayer, fade, step);
    }

    [PunRPC]
    private void StopAllPlayingClipsRPC(string audioName)
    {
        StopAllPlayingClips(audioName);
    }

    #endregion

    #region Public Particle Functions

    //------------------------------------------------------ Public Particle Functions ----------------------------------------------\\

    /// <summary>
    /// Play a particle.
    /// <para>If you don't have a particle to play, use the FindParticle function.</para>
    /// </summary>
    /// <param name="toPlay">The particlesystem that will be played</param>
    /// <param name="position">position in world space where the particle will be played at</param>
    /// <param name="rotation">rotation in world space how the particle will be played.</param>
    public int PlayParticle(ParticleSystem toPlay, Vector3 position, Quaternion rotation, Transform toFollow = null, bool posInWorldSpace = true)
    {
        ParticleTracker system = CreateNewParticleSystem(toPlay);


        if (posInWorldSpace || toFollow == null)
        {
            system.reference.transform.position = position;
        }

        system.reference.transform.rotation = rotation;
        system.reference.Play();

        if (toFollow != null)
            system.reference.transform.SetParent(toFollow);

        if (!posInWorldSpace && toFollow != null)
        {
            system.reference.transform.localPosition = position;
        }


        if (!system.reference.main.loop)
            system.deathTimer = StartCoroutine(system.ParticleDeathTimer());

        return system.ID;
    }

    /// <summary>
    /// Play a particle.
    /// <para>If you don't have a particle to play, use the FindParticle function.</para>
    /// </summary>
    /// <param name="particleName">name of the particle that will be played</param>
    /// <param name="position">position in world space where the particle will be played at</param>
    /// <param name="rotation">rotation in world space how the particle will be played.</param>
    public int PlayParticle(string particleName, Vector3 position, Quaternion rotation, Transform toFollow = null, bool posInWorldSpace = true)
    {
        return PlayParticle(FindParticlePrefab(particleName), position, rotation, toFollow, posInWorldSpace);
    }

    public int PlayParticleMultiplayer(string particleName, Vector3 position, Quaternion rotation, int objectToFollowPhotonID = -1, bool posInWorldSpace = true)
    {
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(PlayParticleRPC), RpcTarget.Others, particleName, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, objectToFollowPhotonID, posInWorldSpace);

        if (objectToFollowPhotonID > -1)
        {
            Transform toFollow = NetworkManager.GetViewByID(objectToFollowPhotonID).transform;
            return PlayParticle(FindParticlePrefab(particleName), position, rotation, toFollow, posInWorldSpace);
        }
        else
        {
           return PlayParticle(FindParticlePrefab(particleName), position, rotation);
        }
    }

    public void StopParticle(int particleID)
    {
        ParticleTracker toStop = FindActiveParticle(particleID);
        if (toStop.deathTimer != null)
            StopCoroutine(toStop.deathTimer);

        StartCoroutine(toStop.Stop());
    }

    public void StopParticleMultiplayer(int particleID)
    {
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(StopParticleRPC), RpcTarget.Others, particleID);

        StopParticle(particleID);
    }

    /// <summary>
    /// Finds a particle stored in the Effect Manager.
    /// </summary>
    /// <param name="particleName"></param>
    /// <returns></returns>
    public ParticleSystem FindParticlePrefab(string particleName)
    {
        for (int i = 0; i < allParticleSystemPrefabs.Length; i++)
        {
            if (allParticleSystemPrefabs[i] != null)
            {
                if (allParticleSystemPrefabs[i].name == particleName)
                {
                    return allParticleSystemPrefabs[i];
                }
            }
            else
            {
                Debug.LogError("There is a null particle element in the Effect Manager");
            }
        }

        Debug.LogError("There is no Particle System named " + particleName + " in the Effects Manager");
        return null;
    }

	#endregion

	#region Private Particle Functions

	//------------------------------------------------------ Private Particle Functions ----------------------------------------------\\

	private ParticleTracker CreateNewParticleSystem(ParticleSystem system)
    {
        ParticleSystem pSystem = Instantiate(system.gameObject, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleTracker toReturn = FindAvailableParticleTracker();
        toReturn.CreateReference(pSystem);

        return toReturn;
    }

    private ParticleTracker FindAvailableParticleTracker()
    {
        for (int i = 0; i < activeParticleSystems.Count; i++)
        {
            if (!activeParticleSystems[i].referenceIsSet)
            {
                return activeParticleSystems[i];
            }
        }

        ParticleTracker toReturn = new ParticleTracker(activeParticleSystems.Count);
        activeParticleSystems.Add(toReturn);

        return toReturn;
    }

    private ParticleTracker FindActiveParticle(int id)
    {
        for (int i = 0; i < activeParticleSystems.Count; i++)
        {
            if (activeParticleSystems[i].ID == id)
            {
                return activeParticleSystems[i];
            }
        }

        return null;
    }

    #endregion

    #region Particle RPCs

    [PunRPC]
    private void PlayParticleRPC(string particleName, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, int objectToFollowPhotonID, bool posInWorldSpace = true)
    {
        if (objectToFollowPhotonID > -1)
        {
            Transform toFollow = NetworkManager.GetViewByID(objectToFollowPhotonID).transform;
            PlayParticle(FindParticlePrefab(particleName), new Vector3(posX, posY, posZ), new Quaternion(rotX, rotY, rotZ, rotW), toFollow, posInWorldSpace);
        }
        else
        {
            PlayParticle(FindParticlePrefab(particleName), new Vector3(posX, posY, posZ), new Quaternion(rotX, rotY, rotZ, rotW));
        }
    }

    private void StopParticleRPC(int particleID)
    {
        StopParticle(particleID);
    }

    #endregion
}