using System.Collections.Generic;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    public struct JointTransformData
    {
        public Vector3 pos;
        public Quaternion rot;
    }
    
    public interface IBoneResolver
    {
        /// <summary>
        /// Initialize resolver with hierarchy cache
        /// </summary>
        void Initialize(IReadOnlyDictionary<string, Transform> boneCache);

        /// <summary>
        /// Resolve pose joint name to actual Transform
        /// </summary>
        Transform Resolve(string jointName);

        public string GetCanonicalName(Transform bone);


       JointTransformData MirrorJoint(Vector3 finalPos, Quaternion finalRot, string poseJointJointName);
    }
}