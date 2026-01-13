using System.Collections.Generic;
using Project.VR.Runtime.HandPose;
using UnityEngine;

namespace Project.VR.Runtime.HandPose.VR.Runtime.HandPose
{
    public sealed class ExactNameBoneResolver : IBoneResolver
    {
        private IReadOnlyDictionary<string, Transform> _cache;

        public void Initialize(IReadOnlyDictionary<string, Transform> boneCache)
        {
            _cache = boneCache;
        }

        public string GetCanonicalName(Transform bone)
        {
            return bone.name;
        }

        public JointTransformData MirrorJoint(Vector3 localPos, Quaternion localRot, string boneName)
        {
            Vector3 mirroredPos = new Vector3(-localPos.x, localPos.y, localPos.z);
            Vector3 euler = localRot.eulerAngles;
            Quaternion mirroredRot = Quaternion.Euler(euler.x, -euler.y, -euler.z);

            return new JointTransformData { pos = mirroredPos, rot = mirroredRot };
        }

        public Transform Resolve(string jointName)
        {
            if (string.IsNullOrEmpty(jointName))
                return null;

            _cache.TryGetValue(jointName, out var t);
            return t;
        }
    }
}