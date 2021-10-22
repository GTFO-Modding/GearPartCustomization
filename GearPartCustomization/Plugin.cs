using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using MyLog = GearPartCustomization.Log;

namespace GearPartCustomization
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            VerboseLogging = base.Config.Bind("Logging","Verbose",false);

            MyLog.s_Source = base.Log;
            m_Harmony.PatchAll(typeof(Patch_GameDataInit));
            m_Harmony.PatchAll(typeof(Patch_GearPartHolder));
        }

        public readonly static Harmony m_Harmony = new(GUID);
        public static ConfigEntry<bool> VerboseLogging;

        public const string
            GUID = "com.Mccad.GearPartCustomization",
            NAME = "Mccad.GearPartCustomization",
            VERSION = "1.3.0";
    }
}
