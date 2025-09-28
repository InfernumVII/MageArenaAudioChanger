using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace MageArenaAudioChanger.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), "Awake")]
    public static class MainMenuManagerPatch
    {
        private static readonly string modPath = Path.GetDirectoryName(typeof(MainMenuManagerPatch).Assembly.Location);
        public static Dictionary<string, AudioClip> tutorialClips = new Dictionary<string, AudioClip>();
        // public static List<AudioClip> tutorialClips = new List<AudioClip>();

        private static void LoadAllAudioClipsAsync<T>(T __instance, string[] paths, Dictionary<string, AudioClip> clips)
            where T : MonoBehaviour
        {
            MageArenaAudioChanger.Logger.LogInfo("Starting async audio clip loading...");
            foreach (string path in paths)
            {
                __instance.StartCoroutine(LoadSingleAudioClip(path, clips));
            }
        }



        private static IEnumerator LoadSingleAudioClip(string path, Dictionary<string, AudioClip> clips)
        {
            if (string.IsNullOrEmpty(path))
            {
                MageArenaAudioChanger.Logger.LogError($"Empty path: {path}");
                yield break;
            }

            string url = "file://" + path;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    MageArenaAudioChanger.Logger.LogError($"Failed to load audio for '{path}': {www.error}");
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    string clipName = Path.GetFileNameWithoutExtension(path);
                    clips.Add(clipName, clip);
                    MageArenaAudioChanger.Logger.LogInfo($"Successfully loaded: {clipName}");
                }
                else
                {
                    MageArenaAudioChanger.Logger.LogError($"Failed to create AudioClip for: {path}");
                }
            }
        }

        private static void LoadTutorialClips(MainMenuManager __instance)
        {
            string tutorialClipsFolderPath = Path.Combine(modPath, $"TutorialClips");
            string[] tutorialClipsPath = Directory.GetFiles(tutorialClipsFolderPath, "*.wav");
            LoadAllAudioClipsAsync(__instance, tutorialClipsPath, tutorialClips);
        }

        [HarmonyPrefix]
        public static bool Prefix(MainMenuManager __instance)
        {
            LoadTutorialClips(__instance);
            return true;
        }
        

    }
}