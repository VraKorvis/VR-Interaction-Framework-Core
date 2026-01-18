using System;
using System.Collections.Generic;
using _Project.VR.Runtime.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    [Serializable]
    [RequireComponent(typeof(XRHandAwareGrabInteractable))]
    public class HandPoseManager : MonoBehaviour
    {
        [SerializeField] private HandPoseSO leftHandPose;
        [SerializeField] private HandPoseSO rightHandPose;
    
        private HandAnimator _currentLeftHand = null;
        private HandAnimator _currentRightHand = null;
        
        [SerializeField] private float blendTime = 0.15f;
        [SerializeField] private protected float easeInTimeOverride;
        [SerializeField] private protected bool overrideEaseTime;
        [SerializeField] private bool hasAnimationPose;

        public bool OverrideEaseTime => overrideEaseTime;
        public float EaseInTimeOverride => easeInTimeOverride;
        
        public XRHandAwareGrabInteractable interactable;
        
        private protected List<HandAnimator> currentlyGrabbingHands = new();
        
        private IGrabModule[] _modules;
        
        private IGrabModule[] Modules 
        {
            get 
            {
                if (_modules == null || _modules.Length == 0)
                {
                    _modules = GetComponents<IGrabModule>();
                }
                return _modules;
            }
        }
      
        private protected void Awake()
        {
            interactable = GetComponent<XRHandAwareGrabInteractable>();
            _modules = GetComponents<IGrabModule>(); 
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnRelease);
        }
        
        void OnDestroy()
        {
            if (interactable)
            {
                interactable.selectEntered.RemoveListener(OnSelectEntered);
                interactable.selectExited.RemoveListener(OnRelease);
            }
        }
    
        private void OnValidate()
        {
            if (!interactable)
            {
                interactable = GetComponent<XRHandAwareGrabInteractable>()?.GetComponentInParent<XRHandAwareGrabInteractable>();
            }

            if (!interactable)
            {
                VRLogger.LogSimpleWarning($"{gameObject.name} does not have an HandAwareGrabInteractable assigned in parent");
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

            foreach (var m in Modules)
            {
                m.OnRelease(handAnimator);
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
                VRLogger.LogSetupWarning($"Pose not found for {gameObject.name}");
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
            
            foreach (var m in _modules) m.OnGrab(hand);
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