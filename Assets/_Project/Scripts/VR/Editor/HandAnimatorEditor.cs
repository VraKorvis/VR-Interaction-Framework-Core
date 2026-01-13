using System.Linq;
using Project.VR.Runtime.HandPose;
using UnityEditor;
using UnityEngine;

namespace Project.VR.Runtime.HandPose.VR.Editor
{
    [CustomEditor(typeof(HandAnimator))]
    // [CanEditMultipleObjects]
    public class HandAnimatorEditor : UnityEditor.Editor
    {
        private readonly int _buttonWidth = 100;

        private HandAnimator _animator;
        
        private SerializedProperty _resolverTypeProp;
        
        private SerializedProperty _defaultPoseProp;
        private SerializedProperty _animationPoseProp;
        private SerializedProperty _secondPoseProp;
        
        private SerializedProperty _rootBoneProp;
        private SerializedProperty _posesProp;
        private SerializedProperty _handTypeProp;
        private SerializedProperty _curlAxisProp;
        private SerializedProperty _openPose;
        private SerializedProperty _fistPose;
        private bool _showPoses;
        
        private void OnEnable()
        {
            _animator = (HandAnimator)target;
            if (_animator == null)
            {
                return;
            }
            
            _resolverTypeProp = serializedObject.FindProperty("_boneResolverType");
            
            _defaultPoseProp = serializedObject.FindProperty("<DefaultPose>k__BackingField");
            _animationPoseProp = serializedObject.FindProperty("<AnimationPose>k__BackingField");
            _secondPoseProp = serializedObject.FindProperty("<SecondButtonPose>k__BackingField");
            
            _rootBoneProp = serializedObject.FindProperty("rootBone");
            _posesProp = serializedObject.FindProperty("poses");
            _handTypeProp = serializedObject.FindProperty("handType");
            _curlAxisProp = serializedObject.FindProperty("curlAxis");
            _openPose = serializedObject.FindProperty("openPose");
            _fistPose = serializedObject.FindProperty("fistPose");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetPropertyFields();

            DrawSetup();
            DrawCurrentPoses();
            DrawPoseList();
            DrawFingerCurlSliders();
            serializedObject.ApplyModifiedProperties();

            DrawSaveButton();

            
        }

        private void SetPropertyFields()
        {
            EditorGUILayout.PropertyField(_resolverTypeProp, false);
            EditorGUILayout.PropertyField(_handTypeProp, false);
            EditorGUILayout.PropertyField(_curlAxisProp, false);
        }

        private void DrawSetup()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (_rootBoneProp.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(_rootBoneProp);
            }
            else
            {
                EditorGUILayout.HelpBox("Root bone not assigned", MessageType.Warning);
                EditorGUILayout.PropertyField(_rootBoneProp);
            }
            EditorGUILayout.Space();
        }
        
        private void DrawCurrentPoses()
        {
            GUILayout.Space(5f);
            GUILayout.Label("Active Poses", EditorStyles.boldLabel);
            DrawLine();
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            var labelToolTip = new GUIContent("Default Pose", 
                "The relaxed hand pose used as the starting point.");
            EditorGUILayout.PropertyField(_defaultPoseProp, labelToolTip);
            if (GUILayout.Button(new GUIContent("Animate", "Apply relaxed pose to the model"), GUILayout.MaxWidth(_buttonWidth)))
            {
                _animator.AnimateToDefault();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            labelToolTip = new GUIContent("Animation Pose", 
                "Target pose used for finger curling (usually a fist or grip pose).");
            EditorGUILayout.PropertyField(_animationPoseProp, labelToolTip);
            if (GUILayout.Button("Animate", GUILayout.MaxWidth(_buttonWidth)))
            {
                _animator.AnimateInstantly(_animator.AnimationPose);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            labelToolTip = new GUIContent("Second Button Pose", "An additional pose for secondary interactions.");
            EditorGUILayout.PropertyField(_secondPoseProp, labelToolTip);
            if (GUILayout.Button(new GUIContent("Animate", "Apply second pose to the model"), GUILayout.MaxWidth(_buttonWidth)))
            {
                _animator.AnimateInstantly(_animator.SecondButtonPose);
            }
            GUILayout.EndHorizontal();
        }
        
        void DrawPoseList()
        {
            DrawLine();
            
            _showPoses = EditorGUILayout.ToggleLeft("Show Poses", _showPoses);
            if (_showPoses)
            {
                EditorGUILayout.PropertyField(_posesProp, _showPoses);
            }
            
            DrawLine();
            EditorGUILayout.LabelField("Testing animate pose", EditorStyles.boldLabel);
        
            foreach (var pose in _animator.poses)
            {
                if (!pose)
                {
                    continue;
                }
        
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(pose, typeof(HandPoseSO), false);
        
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    _animator.AnimateInstantly(pose);
                }
        
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFingerCurlSliders()
        {
            GUILayout.Space(10);
            
            DrawLine();
            EditorGUILayout.LabelField("Finger Curl Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Finger curl open poses", GUILayout.Width(160));
            EditorGUILayout.PropertyField(_openPose, false);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Finger curl fist pose", GUILayout.Width(160));
            EditorGUILayout.PropertyField(_fistPose, false);
            EditorGUILayout.EndHorizontal();

            
            EditorGUI.BeginChangeCheck();

            foreach (var chain in _animator.fingerChains)
            {
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUILayout.Slider(chain.fingerName, chain.curlValue, 0f, 1f);
                
                if (!Mathf.Approximately(chain.curlValue, newValue))
                {
                    chain.curlValue = newValue;
                    if (EditorGUI.EndChangeCheck())
                    {
                        _animator.ApplyFingerCurl(chain);
                    } 
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_animator);
            }
            
            var scanContent = new GUIContent("Setup Finger Chains", 
                "Automatically finds and caches finger bones in the hierarchy based on naming conventions (Index, Middle, etc.).");
            
            if (GUILayout.Button(scanContent, GUILayout.Height(20)))
            {
                _animator.SetupFingerChains();
                _animator.SyncSlidersToCurrentPose();
                EditorUtility.SetDirty(_animator);
            }
            
        }
        
        private void DrawSaveButton()
        {
            EditorGUILayout.Space();
        
            using (new EditorGUI.DisabledScope(_rootBoneProp == null))
            {
                var saveContent = new GUIContent("Save Current Pose", 
                    "Captures the current position and rotation of all bones under Root Bone and saves them as a new HandPoseSO asset.");

                if (GUILayout.Button(saveContent, GUILayout.Height(30)))
                {
                    SavePose();
                }
            }
        }
        
        private void SavePose()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Hand Pose",
                "HandPose_new",
                "asset",
                "Save hand pose"
            );

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
        
            var pose = CreateInstance<HandPoseSO>();
        
            pose.jointsData = _animator.RootBone
                .GetComponentsInChildren<Transform>()
                .Select(j => new HandPoseSO.JointData()
                {
                    jointName = _animator.BoneResolver.GetCanonicalName(j),
                    localPosition = j.localPosition,
                    localRotation = j.localRotation
                }).ToArray();
        
            AssetDatabase.CreateAsset(pose, path);
            AssetDatabase.SaveAssets();
        }
            
        private void DrawLine()
        {
            Rect horizontalLine = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            horizontalLine.height = 1f;
            EditorGUI.DrawRect(horizontalLine, Color.black);
        }
    }
}