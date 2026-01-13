using System;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    [Serializable]
    public class HandSkeletonGizmos : MonoBehaviour
    {
        public bool debugSpheresEnabled;
        [SerializeField] 
        private float jointSphereSize = .0045f;

        void OnDrawGizmosSelected()
        {
            if (debugSpheresEnabled) DrawJoints(transform);
        }

        public void DrawJoints(Transform joint)
        {
            if (!joint.name.EndsWith("aux"))
            {
                Gizmos.DrawWireSphere(joint.position, jointSphereSize);
            }

            for (int i = 0; i < joint.childCount; ++i)
            {
                Transform child = joint.GetChild(i);
                Gizmos.DrawLine(joint.position, child.position);
                DrawJoints(child);
            }
        }
    }
}