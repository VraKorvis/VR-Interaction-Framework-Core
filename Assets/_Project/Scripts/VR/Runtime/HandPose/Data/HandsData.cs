using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    public enum HandsType
    {
        Left,
        Right
    }
    
    [Serializable]
    public class FingerBoneData
    {
        public Transform transform;
        public string jointNameInPose; 
    }
    
    public class FingerChain
    {
        public string fingerName; 
        [Range(0, 1)] 
        public float curlValue;
        public List<FingerBoneData> bones = new();
        public Quaternion[] baseRotations;
    }
}