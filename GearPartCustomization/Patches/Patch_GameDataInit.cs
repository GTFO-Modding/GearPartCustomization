using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using Gear;
using GameData;

namespace GearPartCustomization
{
    class Patch_GameDataInit
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
        public static void Initialize()
        {
            GearPartTransformManager.Initialize();
        }
    }
}