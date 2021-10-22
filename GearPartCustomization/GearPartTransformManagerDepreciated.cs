using System;
using System.Collections.Generic;
using System.Text;
using MTFO.Managers;
using Newtonsoft.Json;
using System.IO;
using Gear;
using UnityEngine;

namespace GearPartCustomization
{
    class GearPartTransformManager
    {
        public static bool Deserialize()
        {

            Debug.Log("GearPartCustomization by mccad: Deserializing the datablock");                   //Debug logging
            var filepath = Path.Combine(ConfigManager.CustomPath, "GearPartTransform.json");            //Retrieve the datablock path
            if (!File.Exists(filepath))                                                                 //If the datablock doesnt exist
            {
                Debug.LogWarning("GearPartTransform.json not found, generating");                       //Log a warning
                Serialize(filepath);                                                                    //Generate a new datablock
            }

            var datablockContent = File.ReadAllText(filepath);                                          //Retrieve the datablock content
            Config = JsonConvert.DeserializeObject<List<GearPartTransformDatablock>>(datablockContent); //Deserialized the contents
            return true;
        }



        public static void CustomizationSetup(GearPartHolder offlineGear)
        {
            if (Config == null) { Debug.LogError("GearPartCustomization: Deserialzed contents null!"); return; }    //If the deserialized contents are null, return
            foreach(GearPartTransformDatablock partCustomization in Config)                                         //For each entry in GearPartTransformDatablock:
            {
                if (offlineGear.GearIDRange.PlayfabItemInstanceId != partCustomization.PlayfabInstanceID) continue;                                             //Skip if it's not the desired weapon
                if (!partCustomization.InternalEnabled) { Debug.LogWarning($"GearPartCustomization: {partCustomization.Name} disabled; skipping"); continue; }  //Skip if the customization for this weapon is disabled
                Debug.Log($"GearPartCustomization: Applying customization {partCustomization.Name} to offline gear ID {partCustomization.OfflineID}");

                foreach (var entry in partCustomization.Parts)                                                       //Iterate through every entry in the datablock for this weapon
                {
                    TryApplyCustomizationPart(entry, offlineGear, partCustomization);                                //Apply customization from the datablock
                }
            }
        }



        public static bool TryGetPart(string partHolderObject, GearPartHolder offlineGear, out GameObject part)
        {
            var property = offlineGear.GetType().GetProperty(partHolderObject);             //Retrieve the property from the part holder
            if (property == null) { part = null; return false; }                            //If the property is null, output a null game object and return false

            part = (GameObject)property.GetValue(offlineGear);                              //Output the game object of the given part
            return part != null;                                                            //else return whether or not the part is null
        }

        public static bool TryApplyCustomizationPart(PartsConfig entry, GearPartHolder offlinegear, GearPartTransformDatablock partCustomization)
        {
            if (entry.PartHolderObject == null)                                                                                 //If the PartHolderObject string is not set
            {
                if (entry.PartType == 0)                                                                                        //and the PartType interger is not set
                {
                    Debug.LogError($"GearPartCustomization: {partCustomization.Name} references a null part. Skipping part");   //throw an error and skip this part
                    return false;                                                                                               //return false
                }
                entry.PartHolderObject = entry.PartType.ToString();                                                             //otherwise, set the PartHolderObject string to the PartType
            }

            if (TryGetPart(entry.PartHolderObject, offlinegear, out GameObject part))                                           //If the referenced part exists in the gearPartHolder:
            {
                part.SetActive(entry.Enabled);                                                                                  //Set the part to active or inactive based on the datablock
                part.transform.localPosition = entry.PartTransform.LocalPosition;                                               //Set the position
                part.transform.localScale = entry.PartTransform.Scale;                                                          //Set the scale
                part.transform.localEulerAngles = entry.PartTransform.Rotation;                                                 //Set the rotation
                Debug.Log($"GearPartCustomization: finished modifying {entry.PartHolderObject} {part.name} in {partCustomization.Name}");
            }
            else                                                                                                                //if the referenced part is null
            {
                Debug.LogError($"GearPartCustomization: Failed to modify {entry.PartHolderObject} in {partCustomization.Name}: part is null");  //Debug logging
                return false;                                                                                                                   //return false
            }

            if (entry.Children != null)                                                                                         //if the part has children
            {
                Debug.Log($"GearPartCustomization: Modifying child objects of {entry.PartHolderObject} in {partCustomization.Name}");           //Debug logging
                foreach(ChildrenConfig child in entry.Children) TryApplyCustomizationChild(part, child, offlinegear, partCustomization);        //Modify each child in the entry
                Debug.Log($"GearPartCustomization: Finished modifying child objects of {entry.PartHolderObject} in {partCustomization.Name}");  //Debug logging
            }
            return true;
        }

        public static bool TryApplyCustomizationChild(GameObject part, ChildrenConfig entry, GearPartHolder offlinegear, GearPartTransformDatablock partCustomization)
        {
            var childObject = part.transform.FindChild(entry.ChildName);                                        //Attempt to retrieve the child's transform
            
            if (childObject == null)                                                                            //If it doesnt exist:
            {
                Debug.LogError($"GearPartCustomization: child object {entry.ChildName} is null. Skipping");     //Throw an error
                return false;                                                                                   //skip this entry
            }

            childObject.gameObject.SetActive(entry.Enabled);                                //Set the child to active or inactive based on the datablock
            childObject.localPosition = entry.PartTransform.LocalPosition;                  //Set the position
            childObject.localScale = entry.PartTransform.Scale;                             //Set the scale
            childObject.localEulerAngles = entry.PartTransform.Rotation;                    //Set the rotation

            Debug.Log($"GearPartCustomization: Modify child object {entry.ChildName} complete");
            if (entry.Children != null)                                                                                         //if the part has children
            {
                Debug.Log($"GearPartCustomization: Modifying child objects of {entry.ChildName} in {partCustomization.Name}");                                  //Debug logging
                foreach (ChildrenConfig child in entry.Children) TryApplyCustomizationChild(childObject.gameObject, child, offlinegear, partCustomization);     //Modify each child in the entry
                Debug.Log($"GearPartCustomization: Finished modifying child objects of {entry.ChildName} in {partCustomization.Name}");                         //Debug logging
            }
            return true;
        }



        public static void Serialize(string filepath)
        {
            string defaultContents = @"[
{
    ""OfflineID"": 0,
    ""Name"": ""Gear customization example"",
    ""Parts"": [
        {
            ""PartHolderObject"": ""FrontPart"",
            ""PartType"": 12,
            ""Enabled"": true,
            ""PartTransform"": {
                ""LocalPosition"": {
                    ""X"": 0.0,
                    ""Y"": 0.0,
                    ""Z"": 0.0
                },
                ""Scale"": {
                    ""X"": 1.0,
                    ""Y"": 1.0,
                    ""Z"": 1.0
                },
                ""Rotation"": {
                    ""X"": 0.0,
                    ""Y"": 0.0,
                    ""Z"": 0.0
                }
            },
            ""Children"": [
                {
                    ""ChildName"": ""Example"",
                    ""Enabled"": false,
                    ""PartTransform"": {
                        ""LocalPosition"": {
                            ""X"": 0.0,
                            ""Y"": 0.0,
                            ""Z"": 0.0
                        },
                        ""Scale"": {
                            ""X"": 1.0,
                            ""Y"": 1.0,
                            ""Z"": 1.0
                        },
                        ""Rotation"": {
                            ""X"": 0.0,
                            ""Y"": 0.0,
                            ""Z"": 0.0
                        }
                    }
                }
            ]
        }
    ],
    ""InternalEnabled"": false
}
]";     //Note: please forgive me
            File.WriteAllText(filepath, defaultContents);
            Config = JsonConvert.DeserializeObject<List<GearPartTransformDatablock>>(defaultContents);
        }

        public static List<GearPartTransformDatablock> Config { get; set; }
    }
}
