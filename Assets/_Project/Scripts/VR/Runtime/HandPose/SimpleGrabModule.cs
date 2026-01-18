using Project.VR.Runtime.HandPose;
using UnityEngine;

namespace _Project.VR.Runtime.HandPose
{
    /// <summary>
    /// Module for standard items like cans, bottles, or tools.
    /// Currently, handles basic pose logic via HandPoseManager.
    /// Requires XRHandAwareGrabInteractable with 'snapObjectToHand' set to false.
    /// </summary>
    [RequireComponent(typeof(HandPoseManager))]
    public class SimpleGrabModule : MonoBehaviour, IGrabModule
    {
        public void OnGrab(HandAnimator hand)
        {
            
        }

        public void OnRelease(HandAnimator hand)
        {
            
        }
    }
}