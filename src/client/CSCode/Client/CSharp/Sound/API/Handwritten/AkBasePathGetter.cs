#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public class AkBasePathGetter
{
    public static PathResolver pathReslover = null;

    public static void Initialize()
    {
        if (pathReslover == null)
            pathReslover = new PathResolver();
    }

    /// Returns the full base path
    public static string GetPlatformBasePath()
	{
        return pathReslover.GetPlatformBasePath();
	}

    public static string GetFullSoundBankPath()
    {
        // Get full path of base path
#if UNITY_ANDROID && !UNITY_EDITOR
 		string fullBasePath = pathReslover.GetBasePath();
#else
        string fullBasePath = Path.Combine(Application.streamingAssetsPath, pathReslover.GetBasePath());
#endif

#if UNITY_SWITCH
		fullBasePath = fullBasePath.Substring(1);
#endif
        FixSlashes(ref fullBasePath);
        return fullBasePath;
    }

    public static void FixSlashes(ref string path, char separatorChar, char badChar, bool addTrailingSlash)
	{
		if (string.IsNullOrEmpty(path))
			return;

		path = path.Trim().Replace(badChar, separatorChar).TrimStart('\\');

		// Append a trailing slash to play nicely with Wwise
		if (addTrailingSlash && !path.EndsWith(separatorChar.ToString()))
			path += separatorChar;
	}

	public static void FixSlashes(ref string path)
	{
#if UNITY_WSA
		char separatorChar = '\\';
#else
		char separatorChar = Path.DirectorySeparatorChar;
#endif // UNITY_WSA
		char badChar = separatorChar == '\\' ? '/' : '\\';
		FixSlashes(ref path, separatorChar, badChar, true);
	}

    public static string GetSoundbankBasePath()
    {
        return pathReslover.GetSoundbankBasePath();
    }
}

public partial class PathResolver
{
    /// <summary>
    /// User hook called to retrieve the custom platform name used to determine the base path. Do not modify platformName to use default platform names.
    /// </summary>
    /// <param name="platformName">The custom platform name.</param>
    static partial void GetCustomPlatformName(ref string platformName);

    public static string GetPlatformName()
    {
        string platformSubDir = string.Empty;
        GetCustomPlatformName(ref platformSubDir);
        if (!string.IsNullOrEmpty(platformSubDir))
            return platformSubDir;

        platformSubDir = "Undefined platform sub-folder";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
        platformSubDir = "Windows";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		platformSubDir = "Mac";
#elif UNITY_STANDALONE_LINUX
		platformSubDir = "Linux";
#elif UNITY_XBOXONE
		platformSubDir = "XBoxOne";
#elif UNITY_IOS || UNITY_TVOS
		platformSubDir = "iOS";
#elif UNITY_ANDROID
		platformSubDir = "Android";
#elif UNITY_PS4
		platformSubDir = "PS4";
#elif UNITY_WP_8_1
		platformSubDir = "WindowsPhone";
#elif UNITY_SWITCH
		platformSubDir = "Switch";
#elif UNITY_PSP2
#if AK_ARCH_VITA_SW || !AK_ARCH_VITA_HW
		platformSubDir = "VitaSW";
#else
		platformSubDir = "VitaHW";
#endif
#endif
        return platformSubDir;
    }

    /// Returns the full base path
    public virtual string GetPlatformBasePath()
    {
        // Combine base path with platform sub-folder
        string  platformBasePath = Path.Combine(GetFullSoundBankPath(), GetPlatformName());
        FixSlashes(ref platformBasePath);
        return platformBasePath;
    }

    public static string GetFullSoundBankPath()
    {
        // Get full path of base path
#if UNITY_ANDROID && !UNITY_EDITOR
 		string fullBasePath = AkInitializer.GetBasePath();
#else
        string fullBasePath = Path.Combine(Application.streamingAssetsPath, AkInitializer.GetBasePath());
#endif

#if UNITY_SWITCH
		fullBasePath = fullBasePath.Substring(1);
#endif
        FixSlashes(ref fullBasePath);
        return fullBasePath;
    }

    public static void FixSlashes(ref string path, char separatorChar, char badChar, bool addTrailingSlash)
    {
        if (string.IsNullOrEmpty(path))
            return;

        path = path.Trim().Replace(badChar, separatorChar).TrimStart('\\');

        // Append a trailing slash to play nicely with Wwise
        if (addTrailingSlash && !path.EndsWith(separatorChar.ToString()))
            path += separatorChar;
    }

    public static void FixSlashes(ref string path)
    {
#if UNITY_WSA
		char separatorChar = '\\';
#else
        char separatorChar = Path.DirectorySeparatorChar;
#endif // UNITY_WSA
        char badChar = separatorChar == '\\' ? '/' : '\\';
        FixSlashes(ref path, separatorChar, badChar, true);
    }

    public string GetSoundbankBasePath()
    {
        string basePathToSet = GetPlatformBasePath();
        bool InitBnkFound = true;
#if UNITY_EDITOR || !UNITY_ANDROID // Can't use File.Exists on Android, assume banks are there
        string InitBankPath = Path.Combine(basePathToSet, "Init.bnk");
        if (!File.Exists(InitBankPath))
        {
            InitBnkFound = false;
        }
#endif

        if (basePathToSet == string.Empty || InitBnkFound == false)
        {
            Debug.Log("WwiseUnity: Looking for SoundBanks in " + basePathToSet);

#if UNITY_EDITOR
            Debug.LogError("WwiseUnity: Could not locate the SoundBanks. Did you make sure to generate them?");
#else
			Debug.LogError("WwiseUnity: Could not locate the SoundBanks. Did you make sure to copy them to the StreamingAssets folder?");
#endif
        }

        return basePathToSet;
    }

    public virtual string GetBasePath()
    {
        return AkInitializer.GetBasePath();
    }
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.