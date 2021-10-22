using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using Gear;

namespace GearPartCustomization
{
    class Patch_GearPartHolder
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GearPartHolder), nameof(GearPartHolder.OnAllPartsSpawned))]
        public static void OnAllPartsSpawned(GearPartHolder __instance)
        {
            GearPartTransformManager.ApplyCustomization(__instance);
        }
    }
}