using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Core
{
    public class Dice : MonoBehaviour, IPhysicsRecordable
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform diceVisuals;
        [SerializeField] private List<Transform> faces;

        private Transform _transform;

        public Transform Transform => _transform;
        public Rigidbody Rigidbody => rb;

        private void Awake()
        {
            _transform = transform;
        }

        public void Setup(DiceState state)
        {
            _transform.position = state.position;
            _transform.rotation = state.rotation;
            diceVisuals.localRotation = Quaternion.identity;

            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(state.force, ForceMode.VelocityChange);
            rb.AddTorque(state.torque, ForceMode.VelocityChange);
        }

        public int GetRollResult()
        {
            if (!faces.Any()) return -1;

            var maxIndex = 0;
            for (var i = 1; i < faces.Count; i++)
            {
                if (faces[i].position.y > faces[maxIndex].position.y)
                {
                    maxIndex = i;
                }
            }

            return maxIndex + 1;
        }

        public void RotateDiceVisuals(int fromResult, int expectedResult)
        {
            if (fromResult == expectedResult) return;

            // Directions from center to current and desired top sides
            var currentTopLocal = diceVisuals.InverseTransformPoint(faces[fromResult - 1].position);
            var desiredTopLocal = diceVisuals.InverseTransformPoint(faces[expectedResult - 1].position);

            currentTopLocal.Normalize();
            desiredTopLocal.Normalize();

            Quaternion rotation = Quaternion.FromToRotation(desiredTopLocal, currentTopLocal);
            diceVisuals.localRotation = rotation * diceVisuals.localRotation;
        }
    }
}