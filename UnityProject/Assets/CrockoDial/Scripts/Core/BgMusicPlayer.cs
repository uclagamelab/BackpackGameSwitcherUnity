using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class BgMusicPlayer : MonoBehaviour
{
    [SerializeField] AudioMixer _mixer;
    [SerializeField] AudioMixerGroup _bgmGroup;
    public AudioMixerGroup bgmGroup => _bgmGroup;

    List<FileInfo> bgMusicList = new List<FileInfo>();
    int currentBgMusicIdx = 0;
    [SerializeField] AudioSource bgMusicSource;

    Coroutine loadingNextSongRoutine = null;

    [Range(0,1)]
    public float targMusicVolume;
    float _bgmVolumeSmooth = 0;

    float _smoothMasterVolume = 1;

    public static BgMusicPlayer instance { get; private set; }
    bool newSongRequested;

    bool multipleBGM => bgMusicList.Count > 1;

    public enum ForceMode { WantOn, WantOff };
    public delegate void VolumeOverrider(ref ForceMode mode, ref int priority);
    List<VolumeOverrider> _bgmVolumeOverriders = new();
    List<VolumeOverrider> _masterVolumeOverriders = new();
    public void AddBGMVolumeOverrider(VolumeOverrider overrider)
    {
        _bgmVolumeOverriders.Add(overrider);
    }

    public void AddMasterVolumeOverrider(VolumeOverrider overrider)
    {
        _masterVolumeOverriders.Add(overrider);
    }

    void Awake()
    {
        instance = this;
        bgMusicSource.volume = 0;
        bgMusicSource.outputAudioMixerGroup = _bgmGroup;
        LoadCustomMusic();
        bgMusicSource.loop = !multipleBGM;

    }


    // Update is called once per frame
    void Update()
    {
        targMusicVolume = 1;
        bool alreadyInProcessOfLoadingNextSong = loadingNextSongRoutine != null;

        bool closeToEndOfSong =
            bgMusicSource.clip != null
            &&
            !bgMusicSource.isPlaying
            &&
            targMusicVolume == 1
            &&
            bgMusicSource.time != 0;//.time == bgMusicSource.clip.length;

        bool shouldLoadNextSong = multipleBGM && closeToEndOfSong && targMusicVolume == 1;// && bgmMaxVolume != 0;

        shouldLoadNextSong |= newSongRequested;
        newSongRequested = false;

        shouldLoadNextSong &= !alreadyInProcessOfLoadingNextSong;
        shouldLoadNextSong &= bgMusicList.Count > 0;

        if (shouldLoadNextSong)
        {

            if (bgMusicSource.clip != null)
            {
                bgMusicSource.clip.UnloadAudioData();
                DestroyImmediate(bgMusicSource.clip);
            }

            bgMusicSource.clip = null;

            currentBgMusicIdx = (currentBgMusicIdx + 1) % bgMusicList.Count;
            LoadNewSong(currentBgMusicIdx);
            //Debug.Log(currentBgMusicIdx + " / " + (bgMusicList.Count - 1));
        }

        bool shouldPlay = bgMusicSource.volume != 0 && bgMusicSource.clip != null;
        shouldPlay &= SwitcherSettings.Data._BGMusicVolume > 0;

        if (bgMusicSource.isPlaying != shouldPlay)
        {
            if (shouldPlay)
            {
                bgMusicSource.Play();
            }
            else
            {
                bgMusicSource.Pause();
            }
        }

        bool speedUpLerp = false;
        float finalTargBGMVolume = targMusicVolume;

        {
            int bestPriority = int.MinValue;
            foreach (var overrider in _bgmVolumeOverriders)
            {
                int priority = int.MinValue;
                ForceMode mode = ForceMode.WantOn;
                overrider.Invoke(ref mode, ref priority);
                if (priority > bestPriority)
                {
                    speedUpLerp = true;
                    bestPriority = priority;
                    finalTargBGMVolume = mode == ForceMode.WantOff ? 0 : 1;
                }
            }
        }

            
        float masterTargVolume = 1;
        {
            int bestPriority = int.MinValue;
            foreach (var overrider in _masterVolumeOverriders)
            {
                int priority = int.MinValue;
                ForceMode mode = ForceMode.WantOn;
                overrider.Invoke(ref mode, ref priority);
                if (priority > bestPriority)
                {
                    speedUpLerp = true;
                    bestPriority = priority;
                    masterTargVolume = mode == ForceMode.WantOff ? 0 : 1;
                }
            }
        }
        masterTargVolume = ProcessRunner.SwitcherAppHasFocus ? 1f : 0;
        _smoothMasterVolume = Mathf.MoveTowards(_smoothMasterVolume, masterTargVolume, Time.deltaTime * .3f);
        _mixer.SetFloat("master_volume", Mathf.Lerp(-80, 0, EasingFunctions.Calc(_smoothMasterVolume, EasingFunctions.QuadEaseOut)));

        _bgmVolumeSmooth = Mathf.MoveTowards(_bgmVolumeSmooth, finalTargBGMVolume, speedUpLerp ? 5 * Time.deltaTime : .35f * Time.deltaTime);
        this.bgMusicSource.volume = _bgmVolumeSmooth * SwitcherSettings.Data._BGMusicVolume;

    }


    IEnumerator GetAudioClipFromDisk(FileInfo fileToUse)
    {
        using (UnityWebRequest audRequest = UnityWebRequestMultimedia.GetAudioClip("file:///"+fileToUse.FullName, determineFromFileName(fileToUse.Name)))
        {

            yield return audRequest.SendWebRequest();
            AudioClip newClip = DownloadHandlerAudioClip.GetContent(audRequest);
            OnNewMusicClipLoaded(newClip);
            loadingNextSongRoutine = null;
        }
    }

    static AudioType determineFromFileName(string fileName)
    {
        if (fileName.EndsWith("ogg", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            return AudioType.OGGVORBIS;
        }
        else if (fileName.EndsWith("mp3", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            return AudioType.MPEG;
        }
        else if (fileName.EndsWith("wav", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            return AudioType.WAV;
        }
        else if (fileName.EndsWith("aif", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            return AudioType.AIFF;
        }
        else if (fileName.EndsWith("aiff", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            return AudioType.AIFF;
        }
        return AudioType.UNKNOWN;
    }

    public void requestNewSong()
    {
        newSongRequested = true;
    }

    void LoadNewSong(int bgmIdx)
    {
        currentBgMusicIdx = bgmIdx;
        FileInfo fileToUse = bgMusicList[currentBgMusicIdx];
        loadingNextSongRoutine = StartCoroutine(GetAudioClipFromDisk(fileToUse));
    }

    void OnNewMusicClipLoaded(AudioClip newClip)
    {
        this.bgMusicSource.clip = newClip;
        if (ProcessRunner.SwitcherAppHasFocus)
        {
            bgMusicSource.Play();
        }
    }

    void LoadCustomMusic()
    {
        bgMusicList.Clear();
        string musicLocation = SwitcherSettings.Data.BGMusicFolder;//Application.streamingAssetsPath + "/BGMusic";

        if (!Directory.Exists(musicLocation))
        {
            Debug.LogError("BGMusic warning: " + musicLocation + " does not exist");
            return;
        }

        foreach (string path in Directory.GetFiles(musicLocation))
        {
            //string[] okExtensions = { "ogg", "wav", "mp3" };
            FileInfo maybeFile = new FileInfo(path);
            var fileType = determineFromFileName(maybeFile.Name);
            if (fileType != AudioType.UNKNOWN)
            {
                bgMusicList.Add(maybeFile);
            }
            
        }



        if (bgMusicList.Count > 0)
        {
            LoadNewSong(Random.Range(0, bgMusicList.Count));
        }
    }
}
