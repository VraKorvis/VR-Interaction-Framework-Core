using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose
{
    public class HandController : MonoBehaviour
    {
        [field: SerializeField] public HandAnimator Hand { get; private set; }
        [field: SerializeField] public HandsType handsType { get; private set; }
        [field: SerializeField] public HandController OtherHand { get; private set; }
        [field: SerializeField] public XRDirectInteractor directInteractor { get; private set; }

        private Transform _attachPoint;
        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;

        private void OnValidate()
        {
            if (!Hand)
            {
                Hand = GetComponentInChildren<HandAnimator>();
            }
            
            if (!directInteractor)
            {
                directInteractor = GetComponent<XRDirectInteractor>();
            }
        }
        
        private void Awake()
        {
            if (!Hand)
            {
                Hand = GetComponentInChildren<HandAnimator>();
            }

            if (!directInteractor)
            {
                 VRLogger.LogSimpleWarning($"{name}: no XRBaseInteractable found; posing disabled.");
            }
            
            _attachPoint = directInteractor.attachTransform;

            _originalLocalPos = _attachPoint.localPosition;
            _originalLocalRot = _attachPoint.localRotation;
        }
        
        private void OnEnable()
        {
            directInteractor.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            directInteractor.selectExited.RemoveListener(HandleSelectExited);
        }
        
        private void HandleSelectExited(SelectExitEventArgs args)
        {
            ResetAttachTransform();
        }

        private void ResetAttachTransform()
        {
            _attachPoint.localPosition = _originalLocalPos;
            _attachPoint.localRotation = _originalLocalRot;
        }
    }
}