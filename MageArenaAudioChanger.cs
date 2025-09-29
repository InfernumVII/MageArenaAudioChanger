using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
namespace MageArenaAudioChanger;

[BepInPlugin("com.infernumvii.magearenaaudiochanger", "MageArenaAudioChanger", "0.0.1")]
public class MageArenaAudioChanger : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony("com.infernumvii.magearenaaudiochanger");
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        harmony.PatchAll();
        Logger.LogInfo("MageArenaAudioChanger loaded!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartCoroutine(ComprehensiveAudioSearch());
        }
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