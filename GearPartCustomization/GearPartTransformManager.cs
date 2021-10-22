using Gear;
using MTFO.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using UnityEngine;

namespace GearPartCustomization
{
    class GearPartTransformManager
    {
        public static void Initialize()
        {
            Log.Message($"{Plugin.NAME} version {Plugin.VERSION} by Mccad00 | Setting up custom datablocks");

            FilePath = Path.Combine(ConfigManager.CustomPath, "GearPartTransform.json");
            if (File.Exists(FilePath))
            {
                Log.Message($"Reading the custom datablock");
                FileContent = File.ReadAllText(FilePath);
                Config = JsonSerializer.Deserialize<List<GearPartTransformDatablock>>(FileContent, JsonSerializerOptions);
            }
            else
            {
                Log.Message($"First time setup | Writing the custom datablock");
                Config = GearPartTransformDatablock.NewConfig;
                FileContent = JsonSerializer.Serialize(Config, JsonSerializerOptions);
                File.WriteAllText(FilePath, FileContent);
            }

            if (Config == null)
            {
                Log.Message("Error reading custom datablock content!");
            }
            else foreach (var entry in Config) Settings.Add(entry.OfflineID, entry);
        }

        

        public static void ApplyCustomization(GearPartHolder partHolder) 
        {
            if (!TryGetConfig(partHolder, out var config)) return;

            Log.Message($"Applying customization config {config.Name}");
            Log.Verbose($"Modify {partHolder.name} - Start\n\n\n");

            foreach (var partConfig in config.Parts) ModifyPart(config, partConfig, partHolder);

            Log.Verbose($"Modify {partHolder.name} - Complete\n\n\n");
        }

        public static bool TryGetConfig(GearPartHolder partHolder, out GearPartTransformDatablock config)
        {
            config = null;
            uint gearID;
            if (CheckSumLookup.ContainsKey(partHolder.GearIDRange.m_checksum)
            && CheckSumLookup.TryGetValue(partHolder.GearIDRange.m_checksum, out gearID))
            {
                if (!Settings.TryGetValue(gearID, out config)) return false;
            }
            else
            {
                if (!TryConvertToOfflineID(partHolder.GearIDRange.PlayfabItemInstanceId, out gearID) || gearID == 0) return false;
                if (!Settings.TryGetValue(gearID, out config)) return false;
                CheckSumLookup.Add(partHolder.GearIDRange.m_checksum, config.OfflineID);
            }
            return true;
        }

        public static void ModifyPart(GearPartTransformDatablock config, PartsConfig partConfig, GearPartHolder partHolder) 
        {
            if (partConfig.PartHolderObject == null)
            {
                if (partConfig.PartType == 0)
                {
                    Log.Verbose($"Modify part fail - missing part reference\n");
                    return;
                }
                partConfig.PartHolderObject = partConfig.PartType.ToString();
            }

            Log.Verbose($"Modify part {partConfig.PartHolderObject}");

            if (!TryGetPart(partConfig.PartHolderObject, partHolder, out var part))
            {
                Log.Verbose($"Modify part fail - {partConfig.PartHolderObject} not found in {partHolder.name}\n");
                return;
            }

            part.active = partConfig.Enabled;
            ApplyPartTransform(partConfig.PartTransform, part);

            if (partConfig.Children == null || partConfig.Children.Count == 0)
            {
                Log.Verbose($"Part {partConfig.PartHolderObject} has no children\n");
                return;
            }
            foreach (var childPart in partConfig.Children) ModifyChild(config, childPart, part.transform, partHolder);
        }
        public static void ModifyChild(GearPartTransformDatablock config, ChildrenConfig childConfig, Transform parent, GearPartHolder partHolder) 
        {
            if (childConfig.ChildName == null)
            {
                Log.Verbose($"Modify child fail - missing child reference\n");
                return;
            }

            if (!TryGetChild(childConfig.ChildName, parent, partHolder, out var part))
            {
                Log.Verbose($"Modify child fail - {childConfig.ChildName} not found in {parent.gameObject.name}\n");
                return;
            }

            Log.Verbose($"Modify child {childConfig.ChildName}");

            part.active = childConfig.Enabled;
            ApplyPartTransform(childConfig.PartTransform, part);

            if (childConfig.Children == null || childConfig.Children.Count == 0)
            {
                Log.Verbose($"Child {childConfig.ChildName} has no children\n");
                return;
            }
            foreach (var childPart in childConfig.Children) ModifyChild(config, childPart, part.transform, partHolder);
        }



        public static void ApplyPartTransform(PartTransformConfig partTransform, GameObject part) 
        {
            part.transform.localPosition = partTransform.LocalPosition;
            part.transform.localEulerAngles = partTransform.Rotation;
            part.transform.localScale = partTransform.Scale;
        }
        public static bool TryGetPart(string partHolderObject, GearPartHolder partHolder, out GameObject part)
        {
            var property = partHolder.GetType().GetProperty(partHolderObject);             //Retrieve the property from the part holder
            if (property == null) { part = null; return false; }                           //If the property is null, output a null game object and return false

            part = (GameObject)property.GetValue(partHolder);                              //Output the game object of the given part
            return part != null;                                                           //else return whether or not the part is null
        }
        public static bool TryGetChild(string childObject, Transform parent, GearPartHolder partHolder, out GameObject part) 
        {
            var child = parent.FindChild(childObject);

            if (child == null)
            {
                part = null;
                return false;
            }

            part = child.gameObject;
            return true;
        }

        public static bool TryConvertToOfflineID(string? itemInstanceID, out uint offlineID)
        {
            offlineID = 0;
            if (string.IsNullOrEmpty(itemInstanceID)) return false; 
            return uint.TryParse(itemInstanceID.Substring("OfflineGear_ID_".Length), out offlineID);
        }

        public static string FilePath;
        public static string FileContent;

        public static List<GearPartTransformDatablock> Config;
        public static Dictionary<uint, GearPartTransformDatablock> Settings = new();
        public static JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, AllowTrailingCommas = true };

        public static Dictionary<uint, uint> CheckSumLookup = new();
    }
}
