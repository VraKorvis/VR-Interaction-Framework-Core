using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;

namespace Project.VR.Runtime.HandPose.VR.Editor
{
    [CustomEditor(typeof(XRHandAwareGrabInteractable))]
    [CanEditMultipleObjects]
    public class XRHandAwareGrabInteractableEditor : XRGrabInteractableEditor
    {
        private SerializedProperty m_SnapObjectToHand;
        private SerializedProperty m_LeftHandAttach;
        private SerializedProperty m_RightHandAttach;

        protected override void OnEnable()
        {
            base.OnEnable();
        
            m_SnapObjectToHand = serializedObject.FindProperty("snapObjectToHand");
            m_LeftHandAttach = serializedObject.FindProperty("leftHandAttach");
            m_RightHandAttach = serializedObject.FindProperty("rightHandAttach");
        }

        protected override void DrawProperties()
        {
            serializedObject.Update();
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hand Pose Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SnapObjectToHand);

            if (m_SnapObjectToHand.boolValue)
            {
                EditorGUILayout.PropertyField(m_LeftHandAttach);
                EditorGUILayout.PropertyField(m_RightHandAttach);
            }
        
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            
            base.DrawProperties();
        }
    }
}