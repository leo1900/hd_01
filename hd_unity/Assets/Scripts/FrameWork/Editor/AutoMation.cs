using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DragonU3DSDK;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Automation : Editor
{
    [MenuItem("Automation/Tools/Build/AndroidBuild", false, 5)]
    public static void AndroidBuild()
    {
        Debug.Log("AndroidBuild  Start-----------------------");
        Build(BuildTarget.Android);
    }
     
    [MenuItem("Automation/Tools/Build/iOSBuild", false, 4)]
    public static void iOSBuild()
    {
        Build(BuildTarget.iOS);
        Debug.Log("iOSBuild  Start-----------------------");
    } 
    class BuildArgs
    {
        public BuildTarget target;
        public bool isDebug;
    }
    static void PreBuild(BuildTarget target, bool isDebug = false)
    {
        //检测max-ironsource
        {
            AssetDatabase.CopyAsset(
                "Assets/DragonSDK/DragonU3DSDK/SDK/Dlugins/Managed/MaxPlugin/MaxSdk/Mediation/IronSource/Editor/Dependencies.xml.back",
                "Assets/DragonSDK/DragonU3DSDK/SDK/Dlugins/Managed/MaxPlugin/MaxSdk/Mediation/IronSource/Editor/Dependencies.xml");
        }
        
        //max-ironsource和ironsource版本一致检测
        // {
        //     TextAsset version_max = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/DragonSDK/DragonU3DSDK/SDK/Dlugins/Managed/MaxPlugin/MaxSdk/Mediation/IronSource/Editor/Dependencies.xml");
        //     TextAsset version_ironsource = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/DragonSDK/DragonU3DSDK/SDK/Dlugins/Managed/IronSourcePlugin/IronSource/Editor/IronSourceSDKDependencies.xml");
        //     if (null != version_max)
        //     {
        //         if (!version_max.text.Contains(
        //                 "<androidPackage spec=\"com.applovin.mediation:ironsource-adapter:7.1.5.1.2\">") ||
        //             !version_max.text.Contains(
        //                 "<iosPod name=\"AppLovinMediationIronSourceAdapter\" version=\"7.1.5.0.0\" />"))
        //         {
        //             throw new Exception("max-ironsource is not the specified version！！！");
        //         }
        //     }
        //     
        //     if (null != version_ironsource)
        //     {
        //         if (!version_ironsource.text.Contains(
        //                 "<androidPackage spec=\"com.ironsource.sdk:mediationsdk:7.1.5.1\">") ||
        //             !version_ironsource.text.Contains("<iosPod name=\"IronSourceSDK\" version=\"7.1.5.0\">"))
        //         {
        //             throw new Exception("ironsource is not the specified version！！！");
        //         }
        //     }
        // }
        //
        // //max-ironsource和ironsource互斥
        // IronSourceConfigInfo config = PluginsInfoManager.Instance.GetPluginConfig<IronSourceConfigInfo>(Constants.IronSource);
        // if (config != null)
        // {
        //     if ((target == BuildTarget.Android && config.ExclusivehMaxAndroid) ||
        //         (target == BuildTarget.iOS && config.ExclusivehMaxiOS))
        //     {
        //         AssetDatabase.DeleteAsset("Assets/DragonSDK/DragonU3DSDK/SDK/Dlugins/Managed/MaxPlugin/MaxSdk/Mediation/IronSource/Editor/Dependencies.xml");
        //     }
        // }
    }
    static void Build(BuildTarget target, bool isDebug = false)
    {
        BuildArgs args = new BuildArgs();
        args.target = target;
        args.isDebug = isDebug;

        // PreBuild(target, isDebug);

        bool wait = false;
        var type = System.Reflection.Assembly.GetExecutingAssembly().GetType("CKEditor." + "ClientMacro");
        if (type != null)
        {
            type.InvokeMember("SetPlatform", System.Reflection.BindingFlags.InvokeMethod |
                                             System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, null);
        }
            
        EditorPrefs.SetBool("BuildIsDebug", isDebug);
        EditorPrefs.DeleteKey("AppLovinQualityServiceState");

        if (EditorApplication.isCompiling)
        {
            var str = JsonUtility.ToJson(args);
            EditorPrefs.SetString("BuildParams", str);
            wait = true;
        }

        if (!wait)
            AfterBuild(target, isDebug);
    }
    
    static void AfterBuild(BuildTarget target, bool isDebug = false)
    {
        // SDKEditor.SetFirebase();
        // PluginConfigInfo info = PluginsInfoManager.LoadPluginConfig();
        // if (info != null && info.m_Map.ContainsKey(Constants.FaceBook))
        // {
        //     FacebookConfigInfo fbInfo = info.m_Map[Constants.FaceBook] as FacebookConfigInfo;
        //     SDKEditor.SetFacebook(fbInfo.AppID);
        // }     // PluginConfigInfo info = PluginsInfoManager.LoadPluginConfig();
        // if (info != null && info.m_Map.ContainsKey(Constants.FaceBook))
        // {
        //     FacebookConfigInfo fbInfo = info.m_Map[Constants.FaceBook] as FacebookConfigInfo;
        //     SDKEditor.SetFacebook(fbInfo.AppID);
        // }
        Debug.Log("AfterBuild ----1");
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER

        //强制设置NDK版本
        //Unity修改本 key 为 AndroidNdkRootR16b，详见：https://forum.unity.com/threads/android-ndk-path-editorprefs-key-changed.639103/
        if (!EditorPrefs.HasKey("AndroidNdkRoot") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRoot")))
            EditorPrefs.SetString("AndroidNdkRoot", "/Users/dragonplus/Downloads/android-ndk-r16b");
        if (!EditorPrefs.HasKey("AndroidNdkRootR16b") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRootR16b")))
            EditorPrefs.SetString("AndroidNdkRootR16b", "/Users/dragonplus/Downloads/android-ndk-r16b");
#endif
        
#if UNITY_2019_3_OR_NEWER
        if (!EditorPrefs.HasKey("AndroidNdkRootR19") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRootR19")))
            EditorPrefs.SetString("AndroidNdkRootR19", "/Users/dragonplus/Downloads/android-ndk-r19");
#endif
        Debug.Log("AfterBuild ----2");


        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenes();

        PlayerSettings.SplashScreen.showUnityLogo = false;

        string platform = "";
        string platformFolder = "";
        if (target == BuildTarget.Android)
        {
            platform = "Android";
            Debug.Log("AfterBuild ----3");

            EditorUserBuildSettings.androidCreateSymbolsZip = false;
            if (!isDebug)
            {
                EditorUserBuildSettings.androidCreateSymbolsZip = true;

#if UNITY_2019_1_OR_NEWER

                PlayerSettings.Android.useCustomKeystore = true;
#endif

                if (ConfigurationController.Instance.AndroidKeyStoreUseConfiguration)
                {
                    PlayerSettings.Android.keystoreName = ConfigurationController.Instance.AndroidKeyStorePath;
                    PlayerSettings.Android.keystorePass = ConfigurationController.Instance.AndroidKeyStorePass;
                    PlayerSettings.Android.keyaliasName = ConfigurationController.Instance.AndroidKeyStoreAlias;
                    PlayerSettings.Android.keyaliasPass = ConfigurationController.Instance.AndroidKeyStoreAliasPass;
                }
                else
                {
#if !UNITY_2019_1_OR_NEWER
                    PlayerSettings.Android.keystoreName = "/Users/dragonplus/smartfunapp.keystore";
                    PlayerSettings.Android.keystorePass = "SmartFun123";
                    PlayerSettings.Android.keyaliasName = "SmartFun.keystore";
                    PlayerSettings.Android.keyaliasPass = "SmartFun123";
#endif
                }
            }

#if UNITY_2019_1_OR_NEWER
            DebugUtil.Log("useCustomKeystore is " + PlayerSettings.Android.useCustomKeystore);
#endif
            DebugUtil.Log("PlayerSettings.Android.keystoreName is " + PlayerSettings.Android.keystoreName);
            DebugUtil.Log("PlayerSettings.Android.keystorePass is " + PlayerSettings.Android.keystorePass);
            DebugUtil.Log("PlayerSettings.Android.keyaliasName is " + PlayerSettings.Android.keyaliasName);
            DebugUtil.Log("PlayerSettings.Android.keyaliasPass is " + PlayerSettings.Android.keyaliasPass);

#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
            if (ConfigurationController.Instance.BuildAppBundle)
                EditorUserBuildSettings.buildAppBundle = true;
            else
                EditorUserBuildSettings.buildAppBundle = false;

#endif
            Debug.Log("AfterBuild ----4");

        }
        else if (target == BuildTarget.iOS)
        {
            platform = "iOS";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        }
        platformFolder = Path.GetFullPath(Application.dataPath + "/../" + platform + "/build/");
        if (!Directory.Exists(platformFolder))
            Directory.CreateDirectory(platformFolder);

        if (target == BuildTarget.Android)
        {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
            if (ConfigurationController.Instance.BuildAppBundle)
                platformFolder = platformFolder + PlayerSettings.productName + ".aab";
            else
                platformFolder = platformFolder + PlayerSettings.productName + ".apk";
#else
            platformFolder = platformFolder + PlayerSettings.productName + ".apk";
#endif
        }

        if (target == BuildTarget.StandaloneOSX)
        {
            platformFolder = platformFolder + PlayerSettings.productName + ".app";
        }

        buildPlayerOptions.locationPathName = platformFolder;

        Debug.Log("AfterBuild ----5");

        buildPlayerOptions.target = target;
        if (isDebug)
        {
            buildPlayerOptions.options |= BuildOptions.Development;
        }
        //else
        //{
        //    buildPlayerOptions.options = BuildOptions.None;
        //}
        Debug.Log("AndroidBuild  Start2-----------------------");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        // CheckAppLovinMAXQualityService();
        EditorPrefs.DeleteKey("BuildIsDebug");
        DebugUtil.Log(summary.result.ToString());
    }
    static string[] GetScenes()
    {
        List<string> scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            scenes.Add(scene.path);
        }

        return scenes.ToArray();
    }
}
