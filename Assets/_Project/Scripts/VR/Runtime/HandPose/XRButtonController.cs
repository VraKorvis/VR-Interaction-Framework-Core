using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Events;

namespace Project.VR.Runtime.HandPose
{
    public class XRButtonController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private InputActionReference triggerAction;
        [SerializeField] private InputActionReference gripAction;
        
        public event Action OnTriggerPressed;
        public event Action OnTriggerReleased;
        public event Action<float> OnTriggerValue;

        public event Action OnGripPressed;
        public event Action OnGripReleased;
        public event Action<float> OnGripValue;

        public float GripValue { get; private set; }
        public float TriggerValue { get; private set; }
        public bool IsGripped { get; private set; }
        public bool IsTriggered { get; private set; }

        private void OnEnable()
        {
            if (triggerAction?.action != null)
            {
                triggerAction.action.Enable();
                triggerAction.action.performed += HandleTriggerPerformed;
                triggerAction.action.canceled += HandleTriggerCanceled;
            }

            if (gripAction?.action != null)
            {
                gripAction.action.Enable();
                gripAction.action.performed += HandleGripPerformed;
                gripAction.action.canceled += HandleGripCanceled;
            }
        }

        private void OnDisable()
        {
            if (triggerAction?.action != null)
            {
                triggerAction.action.performed -= HandleTriggerPerformed;
                triggerAction.action.canceled -= HandleTriggerCanceled;
            }

            if (gripAction?.action != null)
            {
                gripAction.action.performed -= HandleGripPerformed;
                gripAction.action.canceled -= HandleGripCanceled;
            }
        }

        private void Update()
        {
            if (triggerAction?.action != null)
            {
                TriggerValue = triggerAction.action.ReadValue<float>();
                OnTriggerValue?.Invoke(TriggerValue);
            }

            if (gripAction?.action != null)
            {
                GripValue = gripAction.action.ReadValue<float>();
                OnGripValue?.Invoke(GripValue);
            }
        }

        private void HandleTriggerPerformed(InputAction.CallbackContext ctx)
        {
            IsTriggered = true;
            OnTriggerPressed?.Invoke();
        }

        private void HandleTriggerCanceled(InputAction.CallbackContext ctx)
        {
            IsTriggered = false;
            OnTriggerReleased?.Invoke();
        }

        private void HandleGripPerformed(InputAction.CallbackContext ctx)
        {
            IsGripped = true;
            OnGripPressed?.Invoke();
        }

        private void HandleGripCanceled(InputAction.CallbackContext ctx)
        {
            IsGripped = false;
            OnGripReleased?.Invoke();
        }
    }
}