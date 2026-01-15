using _Project.VR.Runtime.HandPose;

namespace Project.VR.Runtime.HandPose
{
    public sealed class ExactNameBoneResolver : BaseBoneResolver
    {
        public override string GetCanonicalName(string rawName) => rawName;
    }
}