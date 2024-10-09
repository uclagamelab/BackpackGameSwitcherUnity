using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DiffDirMaker : MonoBehaviour
{

    [SerializeField]
    Button _createDiffButton;
    [SerializeField]
    Toggle _includeVideoToggle;

    private void Awake()
    {
        _createDiffButton.onClick.AddListener(onCreateDiffButtonClicked);
    }
    public void onCreateDiffButtonClicked()
    {
        this.buildDiffDirectory(SwitcherSettings.Data.GamesFolder, _includeVideoToggle.isOn);
    }
    void buildDiffDirectory(string origGameDir, bool includeVideos = false)
    {
        //strategy :
        //copy everything except directory containing build
        //Directory.CreateDirectory(Game);
        string suffix = System.DateTime.Now.ToString("yyyyMMddhhmmss");
        string diffRootDir = origGameDir + "_DIFF_" + suffix;//Path.Combine(SwitcherSettings.Data.GamesFolder, "___aaaDIFF");

        var srcDir = new DirectoryInfo(SwitcherSettings.Data.GamesFolder);
        //TODO: include files outside game folders?
        var copyDir = Directory.CreateDirectory(diffRootDir);
        foreach (var proj in GameCatalog.Instance.games)
        {
            var gameDir = Path.Combine(copyDir.FullName, proj.rootFolder.Name);
            Directory.CreateDirectory(gameDir);
            foreach (var file in Directory.GetFiles(proj.rootFolder.FullName))
            {
                File.Copy(file, Path.Combine(gameDir, Path.GetFileName(file)));
            }
            foreach (var directory in Directory.GetDirectories(proj.rootFolder.FullName))
            {
                bool isExeDirectory = true;

                isExeDirectory = !string.IsNullOrEmpty(proj.exePathAbsolute)
                    &&
                    XuFileUtil.IsChildPath(proj.exePathAbsolute, directory);

                bool shouldInclude = !isExeDirectory;
                shouldInclude &= !includeVideos || !directory.EndsWith("video"); //TODO: replace this hardcode

                if (!shouldInclude) continue;

                XuFileUtil.CopyDirectory(directory, Path.Combine(gameDir, Path.GetFileName(directory)), true);

            }
        }
        XuFileUtil.OpenPathInExplorer(Path.GetDirectoryName(copyDir.FullName));
    }
}
