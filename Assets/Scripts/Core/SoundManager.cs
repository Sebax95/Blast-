using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class SoundManager : BaseMonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private List<Sound> gameplaySounds;
    [SerializeField] private float localSoundSpatialBlend = 0.5f;

    private List<AudioSource> audioSourcePlaying = new();
    private List<(string, AudioSource)> _actualAudios;

    private ObjectPool<AudioSource> _audioPool;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;

        _audioPool = new ObjectPool<AudioSource>(CreateAudioSource, TurnOn, TurnOff, 10);

        UpdateManager.OnPause += PauseAudios;
        UpdateManager.OnUnPause += UnPauseAudios;
        _actualAudios = new();
    }

    #region Pooling

    private AudioSource CreateAudioSource()
    {
        var newGameObject = new GameObject("AudioSource");
        newGameObject.transform.parent = transform;
        var audioSource = newGameObject.AddComponent<AudioSource>();
        return audioSource;
    }

    private void TurnOn(AudioSource audioSource)
    {
        Reset(audioSource);
        audioSource.gameObject.SetActive(true);
        audioSourcePlaying.Add(audioSource);
    }

    private void Reset(AudioSource audioSource)
    {
        audioSource.clip = null;
    }

    private void TurnOff(AudioSource audioSource)
    {
        audioSource.gameObject.name = "Audiosource";
        audioSource.Stop();
        audioSource.gameObject.SetActive(false);
    }

    #endregion

    #region SoundManager

    public void SetNewVolumeSfx(float value) => SetNewVolume("Sfx", value);

    public void SetNewVolumeMusic(float value) => SetNewVolume("Music", value);

    public void FadeOutMusic(string audioSound, float targetVolume, Action callback)
    {
        var audio = SearchAudioSource(audioSound);
        AudioSource auido = null;
        DOTween.To(() => audio.volume, x => audio.volume = x, targetVolume, 4f).SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                PauseAudios();
                callback?.Invoke();
            });
    }


    private void SetNewVolume(string type, float value)
    {
        mixer.GetFloat(type, out var oldValue);
        float newValue = SetMixerValue(type, value);
    }

    public void LoadVolumeSfx(float value) => SetMixerValue("SFX", value);

    public void LoadVolumeMusic(float value) => SetMixerValue("Music", value);

    private float SetMixerValue(string type, float value)
    {
        float newValueM = -Mathf.Pow(81, 1f - value) + 1;
        mixer.SetFloat(type, newValueM);
        return newValueM;
    }

    public static void PlaySound(string soundName, bool deleteOnFinish = true)
    {
        if (Instance)
            Instance.PlaySound(soundName, true, new Vector3(), deleteOnFinish);
    }

    public static void PlaySound(string soundName, float delay, bool isGlobal = true, Vector3 position = new()) =>
        Instance.StartCoroutine(Instance.DelayedPlay(soundName, delay, isGlobal, position));

    public static void PlaySound(string soundName, float delay, bool deleteOnFinish) =>
        Instance.StartCoroutine(Instance.DelayedPlay(soundName, delay, deleteOnFinish));

    public IEnumerator DelayedPlay(string soundName, float delay, bool deleteOnFinish = true)
    {
        yield return new WaitForSecondsRealtime(delay);
        PlaySound(soundName, deleteOnFinish);
    }

    public IEnumerator DelayedPlay(string soundName, float delay, bool isGlobal = true, Vector3 position = new())
    {
        yield return new WaitForSecondsRealtime(delay);
        PlaySound(soundName, isGlobal, position);
    }

    public static void PlaySound(string soundName, Vector3 position)
    {
        if (Instance)
            Instance.PlaySound(soundName, false, position);
    }

    /// <summary>
    /// private and non-static version of PlaySound() that handles the actual logic
    /// </summary>
    private void PlaySound(string soundName, bool isGlobalSound, Vector3 position = new(), bool deleteOnFinish = true)
    {
        if (gameplaySounds.All(x => x.name != soundName))
        {
            Debug.LogWarning(
                $"SoundManger : There is no {soundName} sound loaded in the sound manager, please add one");
            return;
        }

        var audioSource = _audioPool.GetObject();
        var sound = gameplaySounds.Find(x => x.name == soundName);
        PrepareAudioSource(audioSource, sound, isGlobalSound, position);
        audioSource.Play();

        if (!sound.musicLoop && deleteOnFinish)
            StartCoroutine(PoolAudioSourceOnStop(sound, audioSource));
    }

    /// <summary>
    /// Sets the values of the sound into the AudioSource
    /// </summary>
    private void PrepareAudioSource(AudioSource audioSource, Sound sound, bool isGlobalSound, Vector3 position)
    {
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups(sound.musicLoop ? "Music" : "Sfx")[0];
        audioSource.resource = sound.clip;
        audioSource.loop = sound.musicLoop;
        audioSource.volume = sound.volume;
        audioSource.pitch = 1;
        _actualAudios.Add((sound.name, audioSource));
        audioSource.gameObject.name = sound.name;
        if (!isGlobalSound)
        {
            audioSource.spatialBlend = localSoundSpatialBlend;
            audioSource.gameObject.transform.position = position;
        }
        else
            audioSource.spatialBlend = 0;
    }


    public AudioSource SearchAudioSource(string soundName)
    {
        var tuple = _actualAudios.FirstOrDefault(x => x.Item1 == soundName);
        return tuple != default ? tuple.Item2 : null;
    }

    public static void ChangePitch(string soundName, float pitch)
    {
        var audio = Instance.SearchAudioSource(soundName);
        if(pitch < 0)
            audio.time = audio.clip.length-1;
        audio.pitch = pitch;
    }
    
    public void StopAudio(string soundName)
    {
        var audio = SearchAudioSource(soundName);
        if (audio == null)
            return;
        audio.Stop();
        _actualAudios.Remove((name, audio));
        //audioSourcePool.Add(audio);
        audioSourcePlaying.Remove(audio);
        _audioPool.ReturnObject(audio);
    }

    private IEnumerator PoolAudioSourceOnStop(Sound sound, AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        _actualAudios.Remove((sound.name, audioSource));
        //audioSourcePool.Add(audioSource);
        audioSourcePlaying.Remove(audioSource);
        _audioPool.ReturnObject(audioSource);
    }

    public void PauseAudio(AudioSource audioSource) => audioSource.Pause();

    public void PauseAudios()
    {
        foreach (var audioSource in audioSourcePlaying)
            audioSource.Pause(); //TODO: doesnt pause pooling coroutine
    }

    public void UnPauseAudios()
    {
        foreach (var audioSource in audioSourcePlaying)
            audioSource.UnPause();
    }

    #endregion
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Space]
    public bool musicLoop;

    public float volume = 1;
}