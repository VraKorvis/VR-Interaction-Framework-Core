using Project.VR.Runtime.HandPose;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    public class HandAwareGrabInteractable : XRGrabInteractable
    {
        public Transform leftAttach;
        public Transform rightAttach;

        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            var handRef = interactor.transform.GetComponentInParent<HandController>();
            if (handRef == null)
            {
                return base.GetAttachTransform(interactor);
            }

            return handRef.Hand.HandType == HandsType.Left
                ? leftAttach
                : rightAttach;
        }
    }
}