using _Project.VR.Runtime.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    /// <summary>
    /// Specialized grab module for stationary objects like steering wheels, levers, or valves.
    /// It forces the hand to move toward the object's attach points instead of snapping the object to the hand.
    /// Requires XRHandAwareGrabInteractable with 'snapObjectToHand' set to false.
    /// </summary>
    [RequireComponent(typeof(HandPoseManager))]
    public class FixedSnapModule : MonoBehaviour, IGrabModule
    {
        [SerializeField] 
        private bool useAttachDelay;
        
        [SerializeField] 
        private Transform leftHandAttach = null;
        [SerializeField] 
        private Transform rightHandAttach = null;

        private HandPoseManager _manager;

        private void Start()
        {
            _manager = GetComponent<HandPoseManager>();
        }

        public void OnGrab(HandAnimator hand)
        {
            MoveHandToAttachPoint(hand);
        }

        public void OnRelease(HandAnimator hand)
        {
            
        }
    
        private void MoveHandToAttachPoint(HandAnimator hand)
        {
            var attachPoint = hand.HandType == HandsType.Left ? leftHandAttach : rightHandAttach;
            if (attachPoint != null)
            {
                hand.MoveHandToTarget(attachPoint, GetAttachDelay(), useAttachDelay);
            }
        }
        
        private float GetAttachDelay()
        {
            if (_manager.OverrideEaseTime)
            {
                return _manager.EaseInTimeOverride;
            }
            
            if (_manager.interactable is XRGrabInteractable grab)
            {
                return grab.attachEaseInTime;
            }
            
            return 0f;
        }
    }
}