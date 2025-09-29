using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MageArenaAudioChanger.Patches;
using UnityEngine;
using UnityEngine.Networking;
namespace MageArenaAudioChanger;

[BepInPlugin("com.infernumvii.magearenaaudiochanger", "MageArenaAudioChanger", "0.0.1")]
public class MageArenaAudioChanger : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony("com.infernumvii.magearenaaudiochanger");
    internal static new ManualLogSource Logger;
    public static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private static readonly string modPath = Path.GetDirectoryName(typeof(MageArenaAudioChanger).Assembly.Location);
    // public static Dictionary<string, string> classNameAndStartMethodName = new Dictionary<string, string>
    // {
    //     { "TutorialGoblin", "Starttheroutines" },
    //     { "ShadowWizardAI", "OnStartClient" },
    //     { "DuendeController", "Update" }, //weird perfomance
    //     { "FallenKnight", "KnightTalk" }, //weird perfomance
    //     { "FlagController", "OnStartClient" },
    //     { "MagicMirrorController", "Start" },
    //     { "MainMenuManager", "ActuallyStartGameActually" },
    //     { "PlayerRespawnManager", "startcoliroutine" },
    //     { "SoupManController", "Start" }
    // };
    public static Dictionary<string, string> classNameAndStartMethodName = new Dictionary<string, string>
    {
        { "TutorialGoblin", "Starttheroutines" },
        { "ShadowWizardAI", "Awake" },
        { "DuendeController", "SetDuendeID" }, //weird perfomance
        { "FallenKnight", "KnightTalk" }, //weird perfomance
        { "FlagController", "Awake" },
        { "MagicMirrorController", "Awake" },
        { "MainMenuManager", "Awake" },
        { "PlayerRespawnManager", "Awake" },
        { "SoupManController", "Awake" }
    };


    private void Awake()
    {
        Logger = base.Logger;
        harmony.PatchAll();

        foreach (var item in classNameAndStartMethodName)
        {
            Type targetType = AccessTools.TypeByName(item.Key);
            MethodInfo method = AccessTools.Method(targetType, item.Value);
            try
            {
                harmony
                    .CreateProcessor(method)
                    .AddPrefix(new HarmonyMethod(typeof(CustomPatch), nameof(CustomPatch.Prefix)))
                    .Patch();
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}");
                Logger.LogError($"Error for {item.Key}, {item.Value}");
            }
        }


        LoadClips("TutorialClips");
        LoadClips("AiShadowWizardClips");
        LoadClips("DuendeControllerClips");
        LoadClips("FallenKnightClips");
        LoadClips("FlagControllerClips");
        LoadClips("MagicMirrorControllerClips");
        LoadClips("MainMenuManagerClips");
        LoadClips("PlayerRespawnManagerClips");
        LoadClips("SoupManControllerClips");
        Logger.LogInfo("MageArenaAudioChanger loaded!");

    }
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.F1))
        // {
        //     StartCoroutine(ComprehensiveAudioSearch());
        // }
    }

    private void LoadAllAudioClipsAsync(string[] paths, Dictionary<string, AudioClip> clips)
    {
        Logger.LogInfo("Starting async audio clip loading...");
        foreach (string path in paths)
        {
            StartCoroutine(LoadSingleAudioClip(path, clips));
        }
    }
    
    
    
    private IEnumerator LoadSingleAudioClip(string path, Dictionary<string, AudioClip> clips)
    {
        if (string.IsNullOrEmpty(path))
        {
            Logger.LogError($"Empty path: {path}");
            yield break;
        }
    
        string url = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();
    
            if (www.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError($"Failed to load audio for '{path}': {www.error}");
                yield break;
            }
    
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                string clipName = Path.GetFileNameWithoutExtension(path);
                clips.Add(clipName, clip);
                Logger.LogInfo($"Successfully loaded: {clipName}");
            }
            else
            {
                Logger.LogError($"Failed to create AudioClip for: {path}");
            }
        }
    }
    
    private void LoadClips(string folderName)
    {
        string tutorialClipsFolderPath = Path.Combine(modPath, folderName);
        string[] tutorialClipsPath = Directory.GetFiles(tutorialClipsFolderPath, "*.wav");
        LoadAllAudioClipsAsync(tutorialClipsPath, clips);
    }



    IEnumerator ComprehensiveAudioSearch()
    {
        Logger.LogInfo("=== НАЧАЛО ПОИСКА AUDIOCLIP ===");
        yield return StartCoroutine(SearchInSceneObjects());
        Logger.LogInfo("=== ПОИСК AUDIOCLIP ЗАВЕРШЕН ===");
    }
    
    IEnumerator SearchInSceneObjects()
    {
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        int audioClipCount = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj == null) continue;
            
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;
                
                var fields = component.GetType().GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    try
                    {
                        var value = field.GetValue(component);
                        if (value is AudioClip[] clips && clips != null)
                        {
                            audioClipCount += clips.Length;
                            Logger.LogInfo($"Объект: {obj.name}, Компонент: {component.GetType().Name}, Поле: {field.Name}, Клипов: {clips.Length}");
                        }
                    }
                    catch { }
                }
            }
            
            if (audioClipCount % 100 == 0)
                yield return null;
        }
        
        Logger.LogInfo($"Всего найдено AudioClip в сцене: {audioClipCount}");
    }
}