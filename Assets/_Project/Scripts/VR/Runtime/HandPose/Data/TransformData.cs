using UnityEngine;

namespace Project.VR.Runtime.HandPose
{
    /// <summary>
    /// Stores transform data without instantiating a transform to store the data in.Transforms can only exist as a gameObject in a scene.
    /// </summary>
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void SetTransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}