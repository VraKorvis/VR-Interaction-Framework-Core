using _Project.VR.Runtime.HandPose;
using NUnit.Framework;
using UnityEngine;

namespace Project.VR.Runtime.HandPose.VR.Editor
{
    //TODO run tests
    public class BoneResolverTests
    {
        private BaseBoneResolver _resolver;

        [SetUp]
        public void Setup()
        {
            _resolver = new NormalizedNameBoneResolver();
        }

        [Test]
        [TestCase("L_IndexProximal", "index_proximal")]
        [TestCase("R_Thumb_01", "thumb_01")]
        [TestCase("Hand_Left_MiddleDistal", "middle_distal")]
        [TestCase("thumb_03_r", "thumb_03")]
        [TestCase("IndexMetacarpal_L", "index_metacarpal")]
        public void TestCanonicalNormalization(string input, string expected)
        {
            string result = _resolver.GetCanonicalName(input);
            Assert.AreEqual(expected, result, $"Failed to normalize {input}");
        }
        
        [Test]
        public void TestMirroring_WithBoneContext()
        {
            Vector3 pos = new Vector3(0.1f, 0.2f, 0.3f);
            Quaternion rot = Quaternion.Euler(10, 20, 30);

            var mirroredIndex = _resolver.MirrorJoint(pos, rot);
    
            Assert.AreEqual(-0.1f, mirroredIndex.pos.x, 0.001f, "X position must be inverted");
            Assert.Less(mirroredIndex.rot.y, 0, "Y rotation should be inverted for standard bone");
            
            var mirroredThumb = _resolver.MirrorJoint(pos, rot);
    
            Assert.AreEqual(-0.1f, mirroredThumb.pos.x, 0.001f, "X position must still be inverted for thumb");
        }
    }
}