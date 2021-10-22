using System;
using System.Collections.Generic;
using System.Text;
using Gear;
using Newtonsoft.Json;

namespace GearPartCustomization
{
    public class GearPartTransformDatablock
    {
        public uint OfflineID { get; set; } = 0;
        public string Name { get; set; } = "Internal Name";
        public List<PartsConfig> Parts { get; set; } = new() { new()};
        public bool InternalEnabled { get; set; } = false;
        [JsonIgnore]
        public static List<GearPartTransformDatablock> NewConfig => new() { new() { Parts = new() { new() { Children = new() { new() } } } } };
    }

    public class PartsConfig
    {
        public string PartHolderObject { get; set; } = "FrontPart";
        public eGearComponent PartType { get; set; }
        public bool Enabled { get; set; } = true;
        public PartTransformConfig PartTransform { get; set; } = new();
        public List<ChildrenConfig> Children { get; set; } = new();
    }

    public class PartTransformConfig
    {
        public Vector3Wrapper LocalPosition { get; set; } = new(0, 0, 0);
        public Vector3Wrapper Scale { get; set; } = new(1, 1, 1);
        public Vector3Wrapper Rotation { get; set; } = new(0, 0, 0);
    }

    public class ChildrenConfig
    {
        public string ChildName { get; set; } = "";
        public bool Enabled { get; set; } = false;
        public PartTransformConfig PartTransform { get; set; } = new();
        public List<ChildrenConfig> Children { get; set; } = new();
    }

    public class Vector3Wrapper
    {
        public Vector3Wrapper(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public static implicit operator UnityEngine.Vector3(Vector3Wrapper wrapper) => new UnityEngine.Vector3(wrapper.X, wrapper.Y, wrapper.Z);
    }
}
