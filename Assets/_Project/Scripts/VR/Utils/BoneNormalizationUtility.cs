using UnityEngine;
using UnityEditor;

namespace Project.VR.Runtime.HandPose.EditorUtils
{
    public static class BoneNormalizationUtility
    {
        private static readonly NormalizedNameBoneResolver Resolver = new();

        [MenuItem("Tools/VR/Normalize Selected Hand Pose")]
        public static void NormalizeSelectedPose()
        {
            var selected = Selection.activeObject as HandPoseSO;
            if (selected == null)
            {
                Debug.LogError("Select HandPoseSO in inspector!");
                return;
            }

            NormalizePose(selected);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/VR/Normalize All Poses in Project")]
        public static void NormalizeAllPoses()
        {
            string[] guids = AssetDatabase.FindAssets("t:HandPoseSO");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HandPoseSO pose = AssetDatabase.LoadAssetAtPath<HandPoseSO>(path);
                
                if (pose != null && NormalizePose(pose))
                {
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Baking] Normalizing completed. Normalized assets: {count}");
        }

        public static bool NormalizePose(HandPoseSO pose)
        {
            if (pose == null || pose.jointsData == null) return false;

            bool isDirty = false;
            Undo.RecordObject(pose, "Normalize Bone Names");

            for (int i = 0; i < pose.jointsData.Length; i++)
            {
                string oldName = pose.jointsData[i].jointName;
                string newName = Resolver.GetCanonicalName(oldName);

                if (oldName != newName)
                {
                    pose.jointsData[i].jointName = newName;
                    isDirty = true;
                }
            }

            if (isDirty)
            {
                EditorUtility.SetDirty(pose);
                Debug.Log($"[Baking] Asset '{pose.name}' normalized.");
            }

            return isDirty;
        }
    }
}