using UnityEditor;
using System.Diagnostics;
using UnityEngine;

public static class BuildManager
{
    [MenuItem("Build Tools/Build/All")]
    public static void FullBuild()
    {
        BuildClient();
        BuildLoginServer();
        BuildGameServer();
    }
    
    [MenuItem("Build Tools/Build-Run/All")]
    public static void FullBuildAndRun()
    {
        BuildLoginServerAndRun();
        BuildGameServerAndRun();
        BuildClientAndRun();
    }

    [MenuItem("Build Tools/Build/Client")]
    public static void BuildClient()
    {
        string _path_ = PlayerPrefs.GetString("clientPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Client", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("clientPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[]
        {
            "Assets/Scenes/Client.unity"
        };

        string _path = path + "/Client.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }

    [MenuItem("Build Tools/Build/Login Server")]
    public static void BuildLoginServer()
    {
        string _path_ = PlayerPrefs.GetString("loginServerPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Login Server", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("loginServerPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[] { "Assets/Scenes/LoginServer.unity" };
        string _path = path + "/LoginServer.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.EnableHeadlessMode | BuildOptions.Development;
        
        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }

    [MenuItem("Build Tools/Build/Game Server")]
    public static void BuildGameServer()
    {
        string _path_ = PlayerPrefs.GetString("gameServerPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Game Server", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("gameServerPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[] { "Assets/Scenes/GameServer.unity" };
        string _path = path + "/GameServer.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.EnableHeadlessMode | BuildOptions.Development;

        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }
    
    [MenuItem("Build Tools/Build/Client")]
    public static void BuildClientAndRun()
    {
        string _path_ = PlayerPrefs.GetString("clientPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Client", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("clientPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[]
        {
            "Assets/Scenes/Client.unity"
        };

        string _path = path + "/Client.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.AutoRunPlayer;

        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }

    [MenuItem("Build Tools/Build-Run/Login Server")]
    public static void BuildLoginServerAndRun()
    {
        string _path_ = PlayerPrefs.GetString("loginServerPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Login Server", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("loginServerPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[] { "Assets/Scenes/LoginServer.unity" };
        string _path = path + "/LoginServer.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.EnableHeadlessMode | BuildOptions.Development | BuildOptions.AutoRunPlayer;
        
        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }

    [MenuItem("Build Tools/Build-Run/Game Server")]
    public static void BuildGameServerAndRun()
    {
        string _path_ = PlayerPrefs.GetString("gameServerPath");
        string path = EditorUtility.SaveFolderPanel("Choose build location for Game Server", _path_, "");
        if (string.IsNullOrEmpty(path))
            return;
        PlayerPrefs.SetString("gameServerPath", path);
        PlayerPrefs.Save();
        string[] scenes = new string[] { "Assets/Scenes/GameServer.unity" };
        string _path = path + "/GameServer.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = _path;
        options.target = BuildTarget.StandaloneWindows;
        options.options = BuildOptions.EnableHeadlessMode | BuildOptions.Development | BuildOptions.AutoRunPlayer;

        BuildPipeline.BuildPlayer(options);

        Process p = new Process();
        p.StartInfo.FileName = path;
        p.Start();
    }

    [MenuItem("Build Tools/Build-Run/For Unity Test Client")]
    public static void BuildAndRunLoginAndGameServer()
    {
        BuildLoginServerAndRun();
        BuildGameServerAndRun();
    }
}