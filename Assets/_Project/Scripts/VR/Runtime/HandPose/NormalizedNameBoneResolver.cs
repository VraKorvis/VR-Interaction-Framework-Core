using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Project.VR.Runtime.HandPose
{
    public class NormalizedNameBoneResolver : IBoneResolver
    {
        private static readonly Regex PrefixRegex = new(@"^(L_|R_|Left_|Right_|Left|Right)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SuffixRegex = new(@"((_?[LR])|(_?Left)|(_?Right)|(_?Jnt)|(_?Joint))+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CamelCaseRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex DigitRegex = new(@"(\d+)", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> DigitToNameMap = new()
        {
            { "0", "metacarpal" },
            { "1", "proximal" },
            { "2", "intermediate" },
            { "3", "distal" },
            { "4", "tip" }
        };

        private Dictionary<string, Transform> _canonicalCache = new();

        public void Initialize(IReadOnlyDictionary<string, Transform> boneCache)
        {
            _canonicalCache.Clear();
            foreach (var kvp in boneCache)
            {
                string canonical = GetCanonicalName(kvp.Key);
                if (!_canonicalCache.ContainsKey(canonical))
                {
                    _canonicalCache.Add(canonical, kvp.Value);
                }
            }
        }

        public Transform Resolve(string jointName)
        {
            string canonical = GetCanonicalName(jointName);
            return _canonicalCache.GetValueOrDefault(canonical);
        }

        public string GetCanonicalName(Transform bone) => GetCanonicalName(bone.name);

        public string GetCanonicalName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return string.Empty;

            string name = rawName;
            
            // LittleProximal -> Little_Proximal
            name = Regex.Replace(name, @"([a-z])([A-Z])", "$1_$2");

            // Index1 -> Index_1 | 01Index -> 01_Index
            name = Regex.Replace(name, @"([a-zA-Z])(\d)", "$1_$2");
            name = Regex.Replace(name, @"(\d)([a-zA-Z])", "$1_$2");

            name = name.ToLower().Replace(" ", "_").Replace("-", "_").Replace(".", "_");

            string[] parts = name.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> cleanParts = new List<string>();

            HashSet<string> garbage = new HashSet<string> { 
                "l", "r", "left", "right", "jnt", "joint", "node", "stub", "bone" 
            };

            foreach (var part in parts)
            {
                if (garbage.Contains(part)) continue;

                string processedPart = part;
                
                if (char.IsDigit(part[0]))
                {
                    string digit = part.TrimStart('0');
                    if (string.IsNullOrEmpty(digit)) digit = "0"; 
            
                    if (DigitToNameMap.TryGetValue(digit, out string mapped))
                        processedPart = mapped;
                }

                cleanParts.Add(processedPart);
            }

            return string.Join("_", cleanParts);
        }

        public JointTransformData MirrorJoint(Vector3 localPos, Quaternion localRot, string boneName)
        {
            Vector3 mirroredPos = new Vector3(-localPos.x, localPos.y, localPos.z);
            Vector3 euler = localRot.eulerAngles;
            Quaternion mirroredRot = Quaternion.Euler(euler.x, -euler.y, -euler.z);

            return new JointTransformData { pos = mirroredPos, rot = mirroredRot };
        }
    }
}