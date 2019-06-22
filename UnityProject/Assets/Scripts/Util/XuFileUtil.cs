using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Some General Purpose file handling Helpers
static class XuFileUtil
{
    public static string RunningAppDirectory =>
        #if UNITY_EDITOR
        Path.GetDirectoryName(Application.dataPath); //while in editor, use the project directory
        #else
        AppDomain.CurrentDomain.BaseDirectory;
        #endif


    public static string GetParentDirectory(string path, int nLevelsUp = 1)
    {
        string ret = path;
        for (int i = 0; i < nLevelsUp; i++)
        {
            ret = System.IO.Path.GetDirectoryName(ret);
        }
        return ret;
    }
    public static void GetAllDirectoriesRecursive(string startDirectory, List<string> pathPop, bool includeStartDir = true)
    {
        pathPop.Clear();
        if (includeStartDir)
        {
            pathPop.Add(startDirectory);
        }
        string[] allSubDirsRecursive = Directory.GetDirectories(startDirectory, "*", SearchOption.AllDirectories);
        pathPop.AddRange(allSubDirsRecursive);

    }

    public static void SelectFileInExplorer(string itemPathOrig)
    {
        //#if UNITY_EDITOR
        string itemPath = itemPathOrig;
        //Courtesy of stackoverflow 'yoyo'
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes

        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);


        //#endif
    }

    public static void OpenPathInExplorer(string itemPathOrig)
    {
        //#if UNITY_EDITOR
        string itemPath = itemPathOrig;
        //Courtesy of stackoverflow 'yoyo'
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes


        System.Diagnostics.Process.Start("explorer.exe", itemPath);


        //#endif
    }

    public static void ProcessAllFilesRecursive(IEnumerable<string> startDirectories, System.Action<string> fileAction)
    {
        ProcessAllFilesRecursive(startDirectories, "*", fileAction);
    }

    /// <summary>
    /// Not thoroughly tested, should copy more vetted version from somewhere
    /// </summary>
    /// <param name="maybeSubdirectory"></param>
    /// <param name="maybeParentDirectory"></param>
    /// <returns></returns>
    public static bool IsSubdirectory  (string maybeSubdirectory, string maybeParentDirectory)
    {

        return maybeSubdirectory.Replace('\\', '/').StartsWith(maybeParentDirectory.Replace('\\', '/'));
    }

    public static string ComputeRelativePath(string exeFullPath, string relativeToPath)//_currentGame.rootFolder.FullName
    {
        Uri exeUri = new Uri(exeFullPath);
        Uri gameDirPath = new Uri(relativeToPath);
        Uri relPathUri = gameDirPath.MakeRelativeUri(exeUri);
        string relPath = Uri.UnescapeDataString(relPathUri.ToString());//HttpUtility.HtmlDecode(relPathUri.ToString());
        int rootFolderPortion = (new DirectoryInfo(relativeToPath).Name.Length + 1);
        string finalRelPath = relPath.Substring(rootFolderPortion, relPath.Length - rootFolderPortion);
        return finalRelPath; 
    }

    public static void ProcessAllFilesRecursive(IEnumerable<string> startDirectories, string fileMatchPattern, System.Action<string> fileAction)
    {
        foreach (string startDirectory in startDirectories)
        {
            ProcessAllFilesRecursive(startDirectory, fileMatchPattern, fileAction);
        }
    }
    public static bool DeleteFileOrDirectory(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting file at path : " + path + "\n" + e);
        }


        return false;
    }

    public static bool MoveFileOrDirectory(string originalPath, string desiredPath)
    {
        try
        {
            if (File.Exists(originalPath))
            {
                File.Move(originalPath, desiredPath);
            }
            else if (Directory.Exists(originalPath))
            {
                Directory.Move(originalPath, desiredPath);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error move file at path : '" + originalPath + "' to '" + desiredPath + "'\n" + e);
        }
        return false;
    }
    public static void ProcessAllFilesRecursive(string startDirectory, System.Action<string> fileAction)
    {
        ProcessAllFilesRecursive(startDirectory, "*", fileAction);
    }
    public static void ProcessAllFilesRecursive(string startDirectory, string fileMatchPattern, System.Action<string> fileAction)
    {
        List<string> directoriesToCheck = new List<string>();
        GetAllDirectoriesRecursive(startDirectory, directoriesToCheck);

        foreach (string directoryToCheck in directoriesToCheck)
        {
            foreach (string eachFile in Directory.GetFiles(directoryToCheck, fileMatchPattern))
            {
                fileAction(eachFile);
            }
        }
    }

    public static string ReadText(string filePath)
    {
        string text = null;
        if (System.IO.File.Exists(filePath))
        {
            StreamReader sr = new StreamReader(filePath);
            text = sr.ReadToEnd();
            sr.Close();
        }
        return text;
    }

    public static void WriteText(string text, string absoluteFilePath)
    {
        FileStream fs = new FileStream(absoluteFilePath, FileMode.OpenOrCreate);
        fs.SetLength(0);
        StreamWriter sr = new StreamWriter(fs);
        
        sr.Write(text);
        sr.Close();
    }

    public static bool DeleteFile(string absoluteFilePath)
    {
        if (System.IO.File.Exists(absoluteFilePath))
        {
            System.IO.File.Delete(absoluteFilePath);
            return true;
        }
        return false;
    }


#if UNITY_EDITOR
    //TODO(ALEX) : fix this function to actually recur, and make recursion optional
    public static T LoadAssetWithNameAndExtensionOneDirectoyDeep<T>(string nameWithExtension, string startDir, bool checkSubDirectories, bool ignoreUnderScorePrefixDirectories = false) where T : UnityEngine.Object
    {
        string pat = GetAbsoluteAssetPathInDirectoryOrImmediateSubDirectories(nameWithExtension, startDir, checkSubDirectories, ignoreUnderScorePrefixDirectories);
        T ret = null;
        if (pat != null)
        {
            pat = ConvertAbsolutePathForAssetDatabase(pat);
            ret = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(pat);
        }
        return ret;
    }

    public static string GetAbsoluteAssetPathInDirectoryOrImmediateSubDirectories(string nameWithExtension, string startDir, bool checkSubDirectories, bool ignoreUnderScorePrefixDirectories = false) 
    {
        //brutality....
        List<string> pathsToCheck = new List<string>();
        pathsToCheck.Add(startDir);

        //NOTE(ALEX): Again... not truly recursive...
        if (checkSubDirectories)
        {
            pathsToCheck.AddRange(Directory.GetDirectories(startDir));
        }


        foreach (string potentialDirPath in pathsToCheck)
        {
            if (ignoreUnderScorePrefixDirectories && new DirectoryInfo(potentialDirPath).Name.StartsWith("_"))
            {
                continue;
            }

            string maybePath = potentialDirPath.Replace('\\', '/') + "/" + nameWithExtension;//takeID + ".fbx";

            if (File.Exists(maybePath))
            {
                return maybePath;
            }
        }
        return null;

        //throw new System.Exception();
        //return UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(ConvertAbsolutePathForAssetDatabase(GetAnimationPathForTake(takeID, true)));
    }

#endif

    public static string GetDateSuffixForFileName( bool includeYear = true)
    {
        System.DateTime dateTime = System.DateTime.Now;
        if (includeYear)
        {
            return dateTime.ToString("yyMMddHHmmss");
        }
        else
        {
            return dateTime.ToString("MMddHHmm");
        }
    }


    public static string ConvertAbsolutePathForAssetDatabase(string originalPath)
    {
        string forwardSlashVersionedPath = originalPath.Replace('\\', '/');
        int assetsStartIdx = forwardSlashVersionedPath.IndexOf("Assets/");
        return originalPath.Substring(assetsStartIdx);
    }

    public static string ConvertAssetDatabasePathToAbsolute(string assetPath)
    {
        return Application.dataPath.Substring(0, Application.dataPath.Length - ("/Assets").Length)
            +
            "/"
            +
            assetPath;
    }

}

