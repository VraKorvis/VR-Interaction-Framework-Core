using System;
using System.Collections;
using System.Collections.Generic;
using _Project.VR.Runtime.HandPose;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    //TODO need add cache for all used poses after first apply in runtime
    //TODO check Big O notation
    [Serializable]
    public class HandAnimator : MonoBehaviour
    {
        [SerializeField] 
        private BoneResolverType _boneResolverType = BoneResolverType.Exact;

        private BaseBoneResolver _boneResolver;

        public BaseBoneResolver BoneResolver => _boneResolver;

        [SerializeField] private HandSkeletonGizmos rootBone;
        public HandSkeletonGizmos RootBone => rootBone;

        [SerializeField] private HandsType handType = HandsType.Right;
        private bool isRightHand => handType == HandsType.Right;
        public HandsType HandType => handType;

        [field: SerializeField] public HandPoseSO DefaultPose { get; set; }
        
        [field: SerializeField] public HandPoseSO SecondButtonPose { get; set; }
        [field: SerializeField] public HandPoseSO AnimationPose { get; set; }

        private HandPoseSO _originalPose;

        public List<HandPoseSO> poses = new();
        public List<Transform> currentJoints = new();
        private List<Transform> _goalPoseJoints = new();

        [Tooltip("Time hand skeleton animates to next pose")]
        public float skeletonAnimationTime = .1f;

        [Tooltip("Time to move hand to the item being grabbed")]
        public float handMovingAnimationTime = .1f;

        [SerializeField] [Tooltip("Minimum angle (in degrees) to consider a bone as contributing to curl")]
        private float minEffectiveCurlAngle = 0.5f;

        private Transform _handPositionTarget;
        private bool _isTrackingPosition;

        private IEnumerator _animateToGrabPose;
        private IEnumerator _animateHandToTarget;

        private OriginTranslation _handOriginalTransform;

        private Dictionary<string, Transform> _resolvedBoneCache = new();
        private Dictionary<string, Transform> _boneCache = new();

        private class OriginTranslation
        {
            public readonly Vector3 originalPosition;
            public readonly Quaternion originalRotation;
            public readonly Transform originalParent;
            public readonly Quaternion originalParentRotation;

            public OriginTranslation(Vector3 position, Quaternion rotation, Transform parent)
            {
                originalPosition = position;
                originalRotation = rotation;
                originalParent = parent;
                originalParentRotation = parent != null ? parent.localRotation : Quaternion.identity;
            }
        }

        [SerializeField] public HandPoseSO openPose, fistPose;

        [NonSerialized] public List<FingerChain> fingerChains = new();

        [SerializeField] [Tooltip("Local axis used to calculate finger curl angle")]
        public Vector3 curlAxis = Vector3.down;

        [SerializeField] [Tooltip("Maximum curl angle per finger bone, in degrees")]
        public float maxRotationPerBone = 90f;

        private readonly string[] _fingersNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

        private readonly Dictionary<string, string[]> _fingerAliases =
            new()
            {
                { "Thumb", new[] { "Thumb" } },
                { "Index", new[] { "Index" } },
                { "Middle", new[] { "Middle" } },
                { "Ring", new[] { "Ring" } },
                { "Pinky", new[] { "Pinky", "Little" } },
            };

        public void AnimateToDefault() => AnimateInstantly(DefaultPose);

        void Awake()
        {
            InitializeAnimator();
            _handOriginalTransform =
                new OriginTranslation(transform.localPosition, transform.localRotation, transform.parent);

            _originalPose = DefaultPose;

            if (!DefaultPose && HandPosesSettings.Instance)
            {
                DefaultPose = HandPosesSettings.Instance.DefaultPose;
            }

            if (RootBone == null)
            {
                Debug.LogError($"{name}: RootBone not assigned.");
                return;
            }

            AnimateInstantly(DefaultPose);
        }

        private void OnEnable()
        {
            _openPoseLookup ??= new Dictionary<string, HandPoseSO.JointData>(64);
            _fistPoseLookup ??= new Dictionary<string, HandPoseSO.JointData>(64);
        }

        private void LateUpdate()
        {
            if (_isTrackingPosition && _handPositionTarget)
            {
                transform.SetPositionAndRotation(_handPositionTarget.position, _handPositionTarget.rotation);
            }
        }

        public void InitializeAnimator()
        {
            CacheHierarchyToMap();
            BuildResolver();
        }

        public void BeginNewPoses(HandPoseSO primaryPose)
        {
            ApplyPose(DefaultPose, _goalPoseJoints);
            TransformData[] oldPose = CopyTransformData(_goalPoseJoints);

            DefaultPose = primaryPose;

            ApplyPose(primaryPose, _goalPoseJoints);

            TransformData[] newPose = CopyTransformData(_goalPoseJoints);

            if (_animateToGrabPose != null)
            {
                StopCoroutine(_animateToGrabPose);
            }

            _animateToGrabPose = LerpPose(oldPose, newPose);
            StartCoroutine(_animateToGrabPose);
        }

        public void AnimateInstantly(HandPoseSO pose)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                BuildResolver();
            }
#endif

            if (_animateToGrabPose != null)
            {
                StopCoroutine(_animateToGrabPose);
            }

            ApplyPose(pose, _goalPoseJoints);
            AnimateInstant(_goalPoseJoints);
        }

        public void ApplyPose(HandPoseSO pose, List<Transform> jointList)
        {
            if (!pose)
            {
                 VRLogger.LogSimpleWarning("[HandAnimator] HandPoseSO not assigned to set bones.");
                return;
            }

            jointList.Clear();

            foreach (var poseJoint in pose.jointsData)
            {
                Transform match = BoneResolver.Resolve(poseJoint.jointName);

                if (match != null)
                {
                    Vector3 finalPos = poseJoint.localPosition;
                    Quaternion finalRot = poseJoint.localRotation;

                    if (isRightHand)
                    {
                        var mirroredData = BoneResolver.MirrorJoint(finalPos, finalRot);
                        finalPos = mirroredData.pos;
                        finalRot = mirroredData.rot;
                    }

                    match.localPosition = finalPos;
                    match.localRotation = finalRot;
                    jointList.Add(match);
                }
                else
                {

#if !UNITY_EDITOR
                    VRLogger.LogRuntimeError($"Critical: Bone {poseJoint.jointName} not found on prefab!");
#endif
                    VRLogger.LogSetupWarning($"Bone {poseJoint.jointName} missing. Click 'Setup Finger Chain' button in HandAnimator component.");
                    return;
                }
            }
        }

        public void MoveHandToTarget(Transform attachPoint, float attachDelay, bool useAttachDelay)
        {
            StartCoroutine(
                MoveHandToTargetIE(attachPoint, attachDelay, useAttachDelay));
        }

        public void ReturnHandToPlayer()
        {
            if (_animateToGrabPose != null)
            {
                StopCoroutine(_animateToGrabPose);
                _animateToGrabPose = null;
            }

            if (_animateHandToTarget != null)
            {
                StopCoroutine(_animateHandToTarget);
                _animateHandToTarget = null;
            }

            transform.parent = _handOriginalTransform.originalParent;
            if (transform.parent != null)
            {
                transform.parent.localRotation = _handOriginalTransform.originalParentRotation;
            }

            ResetHandTransform();
            if (_animateHandToTarget != null)
            {
                StopCoroutine(_animateHandToTarget);
            }

            transform.localPosition = _handOriginalTransform.originalPosition;
            transform.localRotation = _handOriginalTransform.originalRotation;
        }

        public void ReturnAnimationsToOriginal()
        {
            DefaultPose = _originalPose;
        }

        public void ReturnToDefaultPosing()
        {
            BeginNewPoses(DefaultPose);
        }

        private void SetJoint(ref Transform joint, Vector3 newPosition, Quaternion newRotation)
        {
            joint.localPosition = newPosition;
            joint.rotation = newRotation;
        }

        private TransformData[] CopyTransformData(List<Transform> joints)
        {
            var transforms = new TransformData[joints.Count];
            for (int i = 0; i < joints.Count; i++)
            {
                var j = joints[i];
                transforms[i].SetTransformData(j.localPosition, j.localRotation, Vector3.one);
            }

            return transforms;
        }

        private IEnumerator LerpPose(TransformData[] fromPose, TransformData[] toPose)
        {
            float timer = 0;
            while (timer <= skeletonAnimationTime + Time.deltaTime)
            {
                for (int i = 0; i < currentJoints.Count; i++)
                {
                    var joint = currentJoints[i];
                    if (!joint) continue;

                    var pos = Vector3.Lerp(fromPose[i].position, toPose[i].position, timer / skeletonAnimationTime);
                    var rot = Quaternion.Lerp(fromPose[i].rotation, toPose[i].rotation, timer / skeletonAnimationTime);
                    SetJoint(ref joint, pos, rot);
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator MoveHandToTargetIE(Transform targetAttachPoint, float attachDelay, bool useAttachDelay)
        {
            if (useAttachDelay)
            {
                yield return new WaitForSeconds(attachDelay);
            }

            transform.parent = null;

            if (_animateHandToTarget != null)
            {
                StopCoroutine(_animateHandToTarget);
            }

            _animateHandToTarget = AnimateHandToTarget(handMovingAnimationTime, targetAttachPoint);
            yield return StartCoroutine(_animateHandToTarget);

            SetHandTransformToTracking(targetAttachPoint);
        }

        private IEnumerator AnimateHandToTarget(float animationLength, Transform newTransform)
        {
            float timer = 0;
            var startPos = transform.position;
            var startRot = transform.rotation;

            while (timer < animationLength + Time.deltaTime)
            {
                var newPosition = Vector3.Lerp(startPos, newTransform.position, timer / animationLength);
                var newRotation = Quaternion.Lerp(startRot, newTransform.rotation, timer / animationLength);

                transform.SetPositionAndRotation(newPosition, newRotation);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.SetPositionAndRotation(newTransform.position, newTransform.rotation);
        }

        private void AnimateInstant(List<Transform> goalPose)
        {
            for (int i = 0; i < currentJoints.Count; i++)
            {
                var joint = currentJoints[i];
                if (!joint || i >= goalPose.Count || !goalPose[i]) continue;
                SetJoint(ref joint, goalPose[i].localPosition, goalPose[i].localRotation);
            }

#if UNITY_EDITOR
            SyncSlidersToCurrentPose();
#endif
        }

        private void SetHandTransformToTracking(Transform target)
        {
            _isTrackingPosition = true;
            _handPositionTarget = target;
        }

        private void ResetHandTransform()
        {
            _isTrackingPosition = false;
            _handPositionTarget = null;
        }

        /// <summary>
        /// Scan and cache hand bones on awake event or by button in editor mode.
        /// </summary>
        private void CacheHierarchyToMap()
        {
            _boneCache.Clear();

            if (rootBone == null)
            {
                Debug.LogError($"[HandAnimator] RootBone not assigned {gameObject.name}!");
                return;
            }

            Transform[] allChildren = rootBone.GetComponentsInChildren<Transform>(true);

            foreach (var child in allChildren)
            {
                _boneCache.TryAdd(child.name, child);
            }
        }
        
        #region Fingers Controls

        public void SetupFingerChains()
        {
            if (openPose == null || fistPose == null)
            {
                 VRLogger.LogSimpleWarning("[HandAnimator] Poses not assigned for Finger Chains.");
                return;
            }

#if UNITY_EDITOR
            InitializeAnimator();
#endif

            if (BoneResolver == null)
            {
                BuildResolver();
            }

            fingerChains.Clear();

            foreach (var fingerType in _fingersNames)
            {
                fingerChains.Add(new FingerChain { fingerName = fingerType });
            }

            foreach (var jointData in openPose.jointsData)
            {
                Transform boneTransform = BoneResolver.Resolve(jointData.jointName);
                if (boneTransform == null) continue;

                for (int i = 0; i < _fingersNames.Length; i++)
                {
                    if (IsBoneMatchFinger(jointData.jointName, _fingersNames[i]))
                    {
                        fingerChains[i].bones.Add(new FingerBoneData
                        {
                            transform = boneTransform,
                            jointNameInPose = jointData.jointName
                        });
                        break;
                    }
                }
            }
        }

        private bool IsBoneMatchFinger(string boneName, string fingerCategory)
        {
            string lowerBone = boneName.ToLowerInvariant();
            if (_fingerAliases.TryGetValue(fingerCategory, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    if (lowerBone.Contains(alias.ToLowerInvariant())) return true;
                }
            }

            return lowerBone.Contains(fingerCategory.ToLowerInvariant());
        }

        public void ApplyFingerCurl(FingerChain chain)
        {
            if (openPose == null || fistPose == null || chain.bones == null || chain.bones.Count == 0)
            {
                return;
            }

            BuildPoseLookups();

            foreach (var boneData in chain.bones)
            {
                if (boneData.transform == null) continue;

                if (_openPoseLookup.TryGetValue(boneData.jointNameInPose, out var startData) &&
                    _fistPoseLookup.TryGetValue(boneData.jointNameInPose, out var endData))
                {
                    var start = GetJointDataForHand(startData, boneData.jointNameInPose);
                    var end = GetJointDataForHand(endData, boneData.jointNameInPose);

                    boneData.transform.localPosition = Vector3.Lerp(start.pos, end.pos, chain.curlValue);
                    boneData.transform.localRotation = Quaternion.Slerp(start.rot, end.rot, chain.curlValue);
                }
            }
        }
        private (Vector3 pos, Quaternion rot) GetJointDataForHand(HandPoseSO.JointData data, string jointName)
        {
            Vector3 pos = data.localPosition;
            Quaternion rot = data.localRotation;

            if (isRightHand)
            {
                var mirrored = BoneResolver.MirrorJoint(pos, rot);
                pos = mirrored.pos;
                rot = mirrored.rot;
            }

            return (pos, rot);
        }
        

        #endregion

        #region Bone's names resolver

        private void BuildResolver()
        {
            BuildBoneResolver();
            BuildResolvedBoneCache(DefaultPose);
        }

        private void BuildResolvedBoneCache(HandPoseSO pose)
        {
            _resolvedBoneCache ??= new Dictionary<string, Transform>();
            _resolvedBoneCache.Clear();

            if (pose == null)
            {
                return;
            }

            foreach (var joint in pose.jointsData)
            {
                var resolvedTransform = _boneResolver.Resolve(joint.jointName);

                if (resolvedTransform != null)
                {
                    _resolvedBoneCache[joint.jointName] = resolvedTransform;
                }
            }
        }

        private void BuildBoneResolver()
        {
            _boneResolver = _boneResolverType switch
            {
                BoneResolverType.Exact => new ExactNameBoneResolver(),
                BoneResolverType.Normalized => new NormalizedNameBoneResolver(),
                _ => new ExactNameBoneResolver()
            };

            _boneResolver.Initialize(_boneCache);
        }

        #endregion

        #region Editor-only

        private Dictionary<string, HandPoseSO.JointData> _openPoseLookup;
        private Dictionary<string, HandPoseSO.JointData> _fistPoseLookup;

        /// <summary>
        /// Editor-only. Sync finger curl sliders from the current pose.
        /// </summary>
        public void SyncSlidersToCurrentPose()
        {
            if (openPose == null || fistPose == null)
            {
                return;
            }

            BuildPoseLookups();

            foreach (var chain in fingerChains)
            {
                if (chain.bones.Count == 0)
                    continue;

                chain.curlValue = CalculateChainCurl(
                    chain.bones,
                    _openPoseLookup,
                    _fistPoseLookup
                );
            }
        }

        /// <summary>
        /// Editor-only, small data set — no lookup caching needed
        /// </summary>
        private void BuildPoseLookups()
        {
            if (openPose == null || fistPose == null)
                return;

            if (openPose.jointsData == null || fistPose.jointsData == null)
                return;

            _openPoseLookup ??= new Dictionary<string, HandPoseSO.JointData>(64);
            _fistPoseLookup ??= new Dictionary<string, HandPoseSO.JointData>(64);

            _openPoseLookup.Clear();
            _fistPoseLookup.Clear();

            foreach (var j in openPose.jointsData)
            {
                if (!string.IsNullOrEmpty(j.jointName))
                    _openPoseLookup[j.jointName] = j;
            }

            foreach (var j in fistPose.jointsData)
            {
                if (!string.IsNullOrEmpty(j.jointName))
                    _fistPoseLookup[j.jointName] = j;
            }
        }

        private float CalculateChainCurl(
            IReadOnlyList<FingerBoneData> bones,
            Dictionary<string, HandPoseSO.JointData> openPoseMap,
            Dictionary<string, HandPoseSO.JointData> fistPoseMap)
        {
            float sum = 0f;
            int count = 0;

            foreach (var boneData in bones)
            {
                if (!openPoseMap.TryGetValue(boneData.jointNameInPose, out var openData) ||
                    !fistPoseMap.TryGetValue(boneData.jointNameInPose, out var fistData))
                    continue;

                var open = GetJointDataForHand(openData, boneData.jointNameInPose);
                var fist = GetJointDataForHand(fistData, boneData.jointNameInPose);

                float total = Quaternion.Angle(open.rot, fist.rot);
                if (total < minEffectiveCurlAngle) continue;

                float current = Quaternion.Angle(open.rot, boneData.transform.localRotation);
        
                sum += Mathf.Clamp01(current / total);
                count++;
            }

            return count > 0 ? sum / count : 0f;
        }

        #endregion
        
    }
}