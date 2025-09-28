using HarmonyLib;
using UnityEngine;

namespace MageArenaAudioChanger.Patches
{
    [HarmonyPatch(typeof(AudioSource), "PlayOneShot", typeof(AudioClip), typeof(float))]
    public static class AudioSourcePatch {
        [HarmonyPrefix]
        public static bool Prefix(AudioSource __instance, AudioClip clip, float volumeScale)
        {
            // MageArenaAudioChanger.Logger.LogInfo($"{clip.name} Played");
            return true;
        }
    }
    
}