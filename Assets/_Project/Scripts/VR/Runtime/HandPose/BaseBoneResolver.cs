using System.Collections.Generic;
using UnityEngine;

namespace _Project.VR.Runtime.HandPose
{
    public struct JointTransformData
    {
        public Vector3 pos;
        public Quaternion rot;
    }
    
    public abstract class BaseBoneResolver
    {
        private readonly Dictionary<string, Transform> _cache = new();

        public JointTransformData MirrorJoint(Vector3 localPos, Quaternion localRot)
        {
            return new JointTransformData
            {
                pos = new Vector3(-localPos.x, localPos.y, localPos.z),
                rot = new Quaternion(localRot.x, -localRot.y, -localRot.z, localRot.w)
            };
        }

        public virtual void Initialize(IReadOnlyDictionary<string, Transform> boneCache)
        {
            _cache.Clear();
            foreach (var kvp in boneCache)
            {
                string canonical = GetCanonicalName(kvp.Key);
                _cache.TryAdd(canonical, kvp.Value);
            }
        }

        public Transform Resolve(string jointName)
        {
            if (string.IsNullOrEmpty(jointName)) return null;
            return _cache.GetValueOrDefault(GetCanonicalName(jointName));
        }

        public abstract string GetCanonicalName(string rawName);
        public string GetCanonicalName(Transform bone) => GetCanonicalName(bone.name);
    }
}