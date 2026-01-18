using Project.VR.Runtime.HandPose;

namespace _Project.VR.Runtime.HandPose
{
    public interface IGrabModule
    {
        void OnGrab(HandAnimator hand);
        void OnRelease(HandAnimator hand);
    }
}