using System;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    [CreateAssetMenu(menuName = "HandPoser/HandPoseScriptableObject", fileName = "NewHandPoseSO")]
        [System.Serializable]
    public class HandPoseSO : ScriptableObject, IComparable<HandPoseSO>
    {
        [System.Serializable]
        public struct JointData
        {
            public string jointName;
            public Vector3 localPosition;
            public Quaternion localRotation;
        }

        public JointData[] jointsData;

        public int CompareTo(HandPoseSO other)
        {
            return !other ? 1 : string.Compare(this.name, other.name, StringComparison.Ordinal);
        }
    }
}