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
        [HarmonyPrefix]
        public static bool Prefix(TutorialScript __instance)
        {
            for (int i = 0; i < __instance.tgob.clipagios.Length; i++)
            {
                var newClip = MainMenuManagerPatch.tutorialClips.FirstOrDefault(clip => clip.Key == __instance.tgob.clipagios[i].name);
                __instance.tgob.clipagios[i] = newClip.Value;
                MageArenaAudioChanger.Logger.LogInfo($"{newClip.Key} Replaced");
            }
            return true;
        }

    }
}