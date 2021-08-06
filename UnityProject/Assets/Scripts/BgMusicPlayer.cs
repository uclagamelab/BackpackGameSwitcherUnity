using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;

public class BgMusicPlayer : MonoBehaviour
{
    [SerializeField]
    AudioMixerGroup _bgmGroup;

    List<FileInfo> bgMusicList = new List<FileInfo>();
    int currentBgMusicIdx = 0;
    public AudioSource bgMusicSource;

    Coroutine loadingNextSongRoutine = null;

    public float targMusicVolume;

    public static BgMusicPlayer instance { get; private set; }
    bool newSongRequested;

    bool multipleBGM => bgMusicList.Count > 1;


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

        float finalTargVolume = targMusicVolume;
        if (!ProcessRunner.SwitcherAppHasFocus)
        {
            finalTargVolume = 0;
        }

        this.bgMusicSource.volume = Mathf.MoveTowards(this.bgMusicSource.volume, finalTargVolume, .35f * Time.deltaTime);
    }

    IEnumerator GetAudioClipFromDisk(FileInfo fileToUse)
    {
        WWW audRequest = new WWW(fileToUse.FullName);
        yield return audRequest;
        AudioClip newClip = audRequest.GetAudioClip();// false, false);
        OnNewMusicClipLoaded(newClip);
        loadingNextSongRoutine = null;
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
            string[] okExtensions = { "ogg", "wav" };
            FileInfo maybeFile = new FileInfo(path);
            foreach (string ext in okExtensions)
            {
                if (maybeFile.Extension.ToLower().EndsWith(ext))
                {
                    //Debug.Log("!!!!!!!!!!!!!!!" + maybeFile);
                    // fileToUse = maybeFile;
                    bgMusicList.Add(maybeFile);
                }
            }
        }



        if (bgMusicList.Count > 0)
        {
            LoadNewSong(Random.Range(0, bgMusicList.Count));
        }
    }
}
