using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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
    
    

}