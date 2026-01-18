using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    public class XRHandAwareGrabInteractable : XRGrabInteractable
    {
        public Transform leftHandAttach;
        public Transform rightHandAttach;

        [Header("Snap Settings")]
        [Tooltip(
            "If enabled, the object snaps to the hand (e.g., a can). If disabled, the hand moves to the object (e.g., a steering wheel).")]
        public bool snapObjectToHand = true;

        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            if (!snapObjectToHand)
            {
                return base.GetAttachTransform(interactor);
            }

            var handRef = interactor.transform.GetComponentInParent<HandController>();
            if (handRef == null)
            {
                return base.GetAttachTransform(interactor);
            }

            return handRef.Hand.HandType == HandsType.Left
                ? leftHandAttach
                : rightHandAttach;
        }
    }
}