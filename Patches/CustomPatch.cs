using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MageArenaAudioChanger.Patches;

public static class CustomPatch
{
    public static bool Prefix(object __instance)
    {
        if (__instance is MonoBehaviour behaviour)
        {
            foreach (FieldInfo field in behaviour.GetType().GetFields())
            {
                try
                {
                    object value = field.GetValue(behaviour);
                    if (value is AudioClip[] audioClipsArray && audioClipsArray != null)
                    {
                        foreach (AudioClip singleClip in audioClipsArray)
                        {
                            ReplaceClip(singleClip);
                        }
                    }
                    else if (value is AudioClip singleClip && singleClip != null)
                    {
                        ReplaceClip(singleClip);
                    }
                }
                catch { }
            }
            MageArenaAudioChanger.Logger.LogInfo($"Hello from custom patch {behaviour.name}");
        }

        return true;
    }

    private static void ReplaceClip(AudioClip clipToReplace)
    {
        var customClip = MageArenaAudioChanger.clips.FirstOrDefault(clip => clip.Key == clipToReplace.name);
        if (customClip.Key != null)
        {
            clipToReplace = customClip.Value;
            MageArenaAudioChanger.Logger.LogInfo($"{customClip.Key} Replaced");
        }
    }
}