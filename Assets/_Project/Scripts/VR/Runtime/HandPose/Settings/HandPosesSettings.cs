using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    [CreateAssetMenu(
        fileName = "HandPosesSettings",
        menuName = "VR/Hand Poser Settings",
        order = 0
    )]
    public class HandPosesSettings : ScriptableObject
    {
        private static HandPosesSettings _instance;

        public static HandPosesSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = Resources.Load<HandPosesSettings>(nameof(HandPosesSettings));
                if (_instance != null)
                {
                    return _instance;
                }

                throw new FileNotFoundException("HandPosesSettings not found");
            }
        }
        
        public HandAnimator LeftHand;
        public HandAnimator RightHand;
        public HandPoseSO DefaultPose;
        public List<HandPoseSO> ReferencePoses;
        public bool sortReferencePoses;

        private void OnValidate()
        {
            if (sortReferencePoses)
            {
                ReferencePoses = ReferencePoses.OrderBy(x => x).ToList();
            }
        }

    }
}