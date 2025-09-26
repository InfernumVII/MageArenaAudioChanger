using System;
using UnityEngine;
using HarmonyLib;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace MageArenaAudioChanger.Patches
{
    [HarmonyPatch(typeof(TutorialScript), "StartTutorial")]
    public static class TutorialScriptPatch
    {
        private static readonly string modPath = Path.GetDirectoryName(typeof(TutorialScriptPatch).Assembly.Location);
        private static readonly string tutorialClipsFolderPath = Path.Combine(modPath, $"TutorialClips");
        private static readonly string[] tutorialClipsPath = Directory.GetFiles(tutorialClipsFolderPath, "*.wav");
        private static bool isLoadingClips = false;
        private static Dictionary<string, AudioClip> clipMap;
        public static bool areClipsLoaded => clipMap != null && !isLoadingClips;
        


        
        private static IEnumerator LoadAllAudioClipsAsync(TutorialScript __instance)
        {
            if (isLoadingClips) yield break;
            
            isLoadingClips = true;
            MageArenaAudioChanger.Logger.LogInfo("Starting async audio clip loading...");
            
            string[] tutorialClipsOrder = {
                "greetings mage",
                "tutclip1",
                "lightbraziertut",
                "flagpoletut",
                "flagpolept2",
                "flagpolept3",
                "flagpolept4",
                "flagpole5",
                "flagpole6",
                "crafting1",
                "craft2",
                "fireballtut",
                "frostbolttut",
                "wormp1",
                "wormp2",
                "magicmissle",
                "finalclip",
            };
        
            clipMap = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
            var loadOperations = new List<Coroutine>();
            
            foreach (string clipName in tutorialClipsOrder)
            {
                string matchingFile = tutorialClipsPath.FirstOrDefault(file => 
                    Path.GetFileNameWithoutExtension(file).IndexOf(clipName, StringComparison.OrdinalIgnoreCase) >= 0);
                
                if (matchingFile != null)
                {
                    var operation = LoadAudioClipAsync(__instance, clipName, matchingFile);
                    loadOperations.Add(operation);
                }
                else
                {
                    MageArenaAudioChanger.Logger.LogWarning($"No file found for clip: {clipName}");
                }
            }
            foreach (var operation in loadOperations)
            {
                yield return operation;
            }
            MageArenaAudioChanger.Logger.LogInfo($"Successfully loaded {clipMap.Count} audio clips");
            isLoadingClips = false;
        }

        private static Coroutine LoadAudioClipAsync(TutorialScript __instance, string clipName, string filePath)
        {
            return __instance.StartCoroutine(LoadSingleAudioClip(clipName, filePath));
        }
        
        private static IEnumerator LoadSingleAudioClip(string clipName, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                MageArenaAudioChanger.Logger.LogError($"Empty path for clip: {clipName}");
                yield break;
            }
            
            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    MageArenaAudioChanger.Logger.LogError($"Failed to load audio for '{clipName}': {www.error}");
                    yield break;
                }
                
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    clip.name = clipName; 
                    clipMap[clipName] = clip;
                    MageArenaAudioChanger.Logger.LogInfo($"Successfully loaded: {clipName}");
                }
                else
                {
                    MageArenaAudioChanger.Logger.LogError($"Failed to create AudioClip for: {clipName}");
                }
            }
        }
        
        public static AudioClip GetAudioClip(string clipName)
        {
            if (clipMap != null && clipMap.TryGetValue(clipName, out AudioClip clip))
            {
                return clip;
            }
            return null;
        }

        private static IEnumerator waitAndReplace(TutorialScript __instance)
        {
            __instance.StartCoroutine(LoadAllAudioClipsAsync(__instance));
            yield return new WaitUntil(() => areClipsLoaded);
            TutorialGoblin tutorialGoblin = __instance.tgob;
            AudioClip[] clips = tutorialGoblin.clipagios;
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = GetAudioClip(clips[i].name);
            }
        }

        [HarmonyPrefix]
        public static bool Prefix(TutorialScript __instance)
        {
            __instance.StartCoroutine(waitAndReplace(__instance));
            return true;
        }

    }
}