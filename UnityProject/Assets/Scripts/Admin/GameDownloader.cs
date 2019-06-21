using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;


public class GameDownloader : MonoBehaviour
{
    string downloadedZipPath = "";
    WebClient wc = new WebClient();

    [ContextMenu("Start Download DBG")]
    public  void downloadAndUpdateWwiseBanks()
    {
        GameDownloader downloadHelper = this;


        using (downloadHelper.wc)
        {
            string bankArchiveUrl =
                //"http://142.93.93.56/svn/timeloop/ReleaseWwiseBank.zip";
                //"https://drive.google.com/uc?export=download&id=1OLk-C_iaI0ItswTyI_0yfc0ioo8BaqqE";
                "https://drive.google.com/uc?export=download&id=14q1oOoVBWz2YZ-OPROvktOdTBIVdHTB1";

            string downloadNameSuffix = XuFileUtil.GetDateSuffixForFileName();
            downloadHelper.downloadedZipPath = Application.temporaryCachePath + "/" + downloadNameSuffix + "Audio.zip";
            Debug.Log(downloadHelper.downloadedZipPath);



           
            downloadHelper.finished = false;
            downloadHelper.downloadPercentage = 0;

            downloadHelper.wc.DownloadProgressChanged += downloadHelper.wc_DownloadProgressChanged;
            downloadHelper.wc.DownloadFileCompleted += downloadHelper.Wc_DownloadDataCompleted;

            System.Uri uri = new System.Uri(bankArchiveUrl);
            Debug.Log(uri);
            downloadHelper.wc.DownloadFileAsync(uri, downloadHelper.downloadedZipPath);
        }
           
    }
    bool finished = false;
    void Wc_DownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
    {
        Debug.Log("4 reel????");
        downloadPercentage = 100;
        finished = true;
    }

    float downloadPercentage = 0;
    void OnGUI()
    {
        AlexUtil.DrawCenteredText(Vector2.zero, "Downloaded: " + downloadPercentage + "%", 18, Color.magenta, null);

        if (GUI.Button(new Rect(Screen.height - 45, 10, 70, 45), "Cancel"))
        {
            wc.CancelAsync();
            XuFileUtil.DeleteFileOrDirectory(downloadedZipPath);
        }

        if (finished)
        {
            if (GUI.Button(new Rect(Screen.height - 45, 85, 70, 45), "Load Into Project"))
            {
                LoadDownloadedBank();
            }
        }

    }

    
    void LoadDownloadedBank()
    {
        //string gameFolderName = "tada_no_rei_v2";
        string unzipDestination = SwitcherSettings.Data.GamesFolder;// System.IO.Path.Combine(SwitcherSettings.GamesFolder, gameFolderName);//Application.streamingAssetsPath;

        string originalFilePath = Application.streamingAssetsPath + "/Audio";
        bool saveExistingTemporarily = System.IO.Directory.Exists(originalFilePath);
        string saveVersionOfBank = originalFilePath + "_safe_" + XuFileUtil.GetDateSuffixForFileName();
        if (saveExistingTemporarily)
        {
            XuFileUtil.MoveFileOrDirectory(originalFilePath, saveVersionOfBank);
        }

        //[!!!] Warning will overwrite an existing file with no warning!
        ZipUtil.Unzip(downloadedZipPath, unzipDestination);

        XuFileUtil.DeleteFileOrDirectory(downloadedZipPath);
    }

    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        
        downloadPercentage = e.ProgressPercentage;
    }
}
