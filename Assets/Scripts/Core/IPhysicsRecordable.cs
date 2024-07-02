using UnityEngine;

namespace Core
{
    public interface IPhysicsRecordable
    {
        public Transform Transform { get; }
        public Rigidbody Rigidbody { get; }
    }
}