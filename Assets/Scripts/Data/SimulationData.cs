using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public struct SimulationData
    {
        public Vector3 StartPosition { get; }
        public Quaternion StartRotation { get; }
        public List<SimulationFrame> Frames { get; }

        public SimulationData(Vector3 startPosition, Quaternion startRotation)
        {
            StartPosition = startPosition;
            StartRotation = startRotation;
            Frames = new List<SimulationFrame>();
        }
    }

    public struct SimulationFrame
    {
        public Vector3 position;
        public Quaternion rotation;

        public SimulationFrame(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}