using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    [Serializable]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class HandPoseOnFixedGrab : BaseHandPose
    {
        [SerializeField] 
        private bool useAttachDelay;
        
        protected override void BeginNewHandPoses(HandAnimator hand)
        {
            base.BeginNewHandPoses(hand);
            MoveHandToAttachPoint(hand);
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
            if (overrideEaseTime)
            {
                return easeInTimeOverride;
            }
            
            if (interactable is XRGrabInteractable grab)
            {
                return grab.attachEaseInTime;
            }
            
            return 0f;
        }
        
    }
}
