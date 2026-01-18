using _Project.VR.Runtime.HandPose;
using UnityEditor;
using UnityEngine;

namespace Project.VR.Runtime.HandPose.VR.Editor
{
    [CustomEditor(typeof(HandPoseManager))]
    public class HandPoseManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HandPoseManager manager = (HandPoseManager)target;

            GUILayout.Space(10);
            GUILayout.Label("Add Specialized Modules:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
        
            if (GUILayout.Button("Add Fixed Snap (Lever/Wheel)"))
            {
                if (!manager.GetComponent<FixedSnapModule>())
                {
                    Undo.AddComponent<FixedSnapModule>(manager.gameObject);
                }
            }

            if (GUILayout.Button("Add Simple Grab Logic (cans/bottles)"))
            {
                if (!manager.GetComponent<SimpleGrabModule>())
                {
                    Undo.AddComponent<SimpleGrabModule>(manager.gameObject);
                }
            }

            EditorGUILayout.EndHorizontal();
        
            if (GUILayout.Button("Remove All Modules"))
            {
                var modules = manager.GetComponents<IGrabModule>();
                foreach (var m in modules)
                {
                    Undo.DestroyObjectImmediate((MonoBehaviour)m);
                }
            }
        }
    }
}