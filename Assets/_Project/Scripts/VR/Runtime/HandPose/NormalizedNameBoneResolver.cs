using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.VR.Runtime.HandPose;

namespace Project.VR.Runtime.HandPose
{
    public class NormalizedNameBoneResolver : BaseBoneResolver
    {
        private static readonly Dictionary<string, string> DigitToNameMap = new()
        {
            { "0", "metacarpal" },
            { "1", "proximal" },
            { "2", "intermediate" },
            { "3", "distal" },
            { "4", "tip" }
        };
        
        // LittleProximal -> Little_Proximal
        private static readonly Regex CamelCaseRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);
        // Index1 -> Index_1 | 01Index -> 01_Index
        private static readonly Regex LetterDigitRegex = new(@"([a-zA-Z])(\d)", RegexOptions.Compiled);
        private static readonly Regex DigitLetterRegex = new(@"(\d)([a-zA-Z])", RegexOptions.Compiled);

        private static readonly HashSet<string> Garbage = new(System.StringComparer.OrdinalIgnoreCase)
        { 
            "l", "r", "left", "right", "jnt", "joint", "node", "stub", "bone" 
        };

        private readonly List<string> _resultBuffer = new(8);
        
        public override string GetCanonicalName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return string.Empty;

            var name = CamelCaseRegex.Replace(rawName, "$1_$2");
            name = LetterDigitRegex.Replace(name, "$1_$2");
            name = DigitLetterRegex.Replace(name, "$1_$2");

            name = name.ToLower()
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(".", "_");

            string[] parts = name.Split('_', System.StringSplitOptions.RemoveEmptyEntries);
        
            _resultBuffer.Clear();

            foreach (var part in parts)
            {
                if (Garbage.Contains(part))
                {
                    continue;
                }

                string processedPart = part;
            
                if (char.IsDigit(part[0]))
                {
                    string digit = part.TrimStart('0');
                    if (string.IsNullOrEmpty(digit)) digit = "0"; 
        
                    if (DigitToNameMap.TryGetValue(digit, out string mapped))
                        processedPart = mapped;
                }
                _resultBuffer.Add(processedPart);
            }
            return string.Join("_", _resultBuffer);
        }

    }
}