using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    [Serializable]
    [RequireComponent(typeof(XRBaseInteractable))]
    public class BaseHandPose : MonoBehaviour
    {
        [SerializeField] private HandPoseSO leftHandPose;
        [SerializeField] private HandPoseSO rightHandPose;
        
        [SerializeField] 
        protected Transform leftHandAttach = null;
        [SerializeField] 
        protected Transform rightHandAttach = null;
    
        private HandAnimator _currentLeftHand = null;
        private HandAnimator _currentRightHand = null;
        
        [SerializeField] private float blendTime = 0.15f;
        [SerializeField] private protected float easeInTimeOverride;
        [SerializeField] private protected bool overrideEaseTime;
        [SerializeField] private bool hasAnimationPose;

        public XRGrabInteractable interactable;
        
        private protected List<HandAnimator> currentlyGrabbingHands = new();
      
        private protected void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
            OnValidate();
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnRelease);
        }
        
        void OnDestroy()
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnRelease);
        }
    
        private void OnValidate()
        {
            if (!interactable)
            {
                interactable = GetComponent<XRGrabInteractable>()?.GetComponentInParent<XRGrabInteractable>();
            }

            if (!interactable)
            {
                VRLogger.LogSimpleWarning($"{gameObject.name} does not have an XRGrabInteractable assigned in parent");
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var handReference = args.interactorObject.transform.GetComponentInParent<HandController>();
            if (handReference == null)
            {
                VRLogger.LogSimpleWarning($"Cant find HandReference");
                return;
            }
            
            BeginNewHandPoses(handReference.Hand);

        }
    
        private void OnRelease(SelectExitEventArgs args)
        {
            var handReference = args.interactorObject.transform.GetComponentInParent<HandController>();
            var handAnimator = handReference?.Hand;
            
            if (handAnimator == null || handReference == null || handReference.Hand == null )
            {
                return;
            }
            
            if (!currentlyGrabbingHands.Contains(handAnimator))
            {
                return;
            }
            
            ReleaseHand(handAnimator);
        }

        private void ReleaseHand(HandAnimator hand)
        {
            if (!hand)
            {
                return;
            }
            
            hand.ReturnHandToPlayer();
            hand.ReturnAnimationsToOriginal();
            hand.ReturnToDefaultPosing();
            
            currentlyGrabbingHands.Remove(hand);
        }
    
        private void TryStartPosing(SelectEnterEventArgs x)
        {
            var handReference = x.interactorObject.transform.GetComponentInParent<HandController>();
            
            if (!handReference)
            {
                 VRLogger.LogSimpleWarning($"{gameObject.name} does not have an HandReference.");
                return;
            }
            
            BeginNewHandPoses(handReference.Hand);
        }
    
        protected virtual void BeginNewHandPoses(HandAnimator hand)
        {
            if (!hand || !CheckIfPoseExistForHand(hand))
            {
                VRLogger.LogSimpleWarning($"Pose not found for {gameObject.name}");
                return;
            }

            currentlyGrabbingHands.Add(hand);
        
            if (hand.HandType == HandsType.Left)
            {
                _currentLeftHand = hand;
                SetToPose(_currentLeftHand, leftHandPose);
            }
            else
            {
                _currentRightHand = hand;
                SetToPose(_currentRightHand, rightHandPose);
            }

        }
    
        private void SetToPose(HandAnimator hand, HandPoseSO targetPose)
        {
            hand.BeginNewPoses(targetPose);
        }
    
        private bool CheckIfPoseExistForHand(HandAnimator hand)
        {
            if (leftHandPose && hand.HandType == HandsType.Left)
            {
                return true;
            }
            return rightHandPose && hand.HandType == HandsType.Right;
        }
        
    }
}