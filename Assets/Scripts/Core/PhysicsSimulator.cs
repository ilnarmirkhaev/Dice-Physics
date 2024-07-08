using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Core
{
    public class PhysicsSimulator
    {
        private readonly List<SimulationData> _simulation = new();
        private IReadOnlyList<IPhysicsRecordable> _recordables;
        private int _frameCount;
        private CancellationTokenSource _cts;

        public void RecordSimulation(IReadOnlyList<IPhysicsRecordable> recordables, int maxFrameCount)
        {
            _simulation.Clear();
            _recordables = recordables;

            EnablePhysics(recordables);
            SaveInitialState(recordables);
            StartRecording(recordables, maxFrameCount);
        }

        public void PlayRecordedSimulation(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            PlaySimulation(_cts.Token).Forget();
        }

        public void StopAnyPlayingSimulation()
        {
            _cts?.Cancel();
        }

        private async UniTask PlaySimulation(CancellationToken cancellationToken)
        {
            // Turn off physics and reset objects to initial state of simulation
            DisablePhysics(_recordables);
            ResetObjectsToInitialState();

            // Move objects via Transforms
            for (var i = 0; i < _frameCount; i++)
            {
                for (var j = 0; j < _simulation.Count; j++)
                {
                    var recordable = _recordables[j];
                    var data = _simulation[j];
                    var frame = data.Frames[i];

                    recordable.Transform.SetPositionAndRotation(frame.position, frame.rotation);
                }

                await UniTask.WaitForFixedUpdate(cancellationToken, true);
            }
        }

        private void StartRecording(IReadOnlyList<IPhysicsRecordable> recordables, int maxFrameCount)
        {
            Physics.simulationMode = SimulationMode.Script;

            var framesPassed = 0;

            while (true)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                var allDiceStopped = true;
                framesPassed++;

                // Record every frame of the simulation for each dice
                for (var j = 0; j < recordables.Count; j++)
                {
                    var recordable = recordables[j];
                    var t = recordable.Transform;
                    var position = t.position;
                    var rotation = t.rotation;

                    if (!HasRigidbodyStopped(recordable.Rigidbody))
                    {
                        allDiceStopped = false;
                    }

                    var frame = new SimulationFrame(position, rotation);
                    _simulation[j].Frames.Add(frame);
                }

                // Stop recording when all dice stop or maxFrameCount is over the limit
                if (allDiceStopped) break;

                if (framesPassed >= maxFrameCount)
                {
                    Debug.LogError($"Max frame count of {maxFrameCount} was exceeded. Increase maxFrameCount");
                    break;
                }
            }

            _frameCount = framesPassed;
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        private void SaveInitialState(IReadOnlyList<IPhysicsRecordable> recordables)
        {
            foreach (var recordable in recordables)
            {
                var t = recordable.Transform;
                var simulationData = new SimulationData(t.position, t.rotation);
                _simulation.Add(simulationData);
            }
        }

        private void ResetObjectsToInitialState()
        {
            if (!_recordables.Any()) return;

            for (var i = 0; i < _recordables.Count; i++)
            {
                var recordable = _recordables[i];
                var data = _simulation[i];
                recordable.Transform.SetPositionAndRotation(data.StartPosition, data.StartRotation);
            }
        }

        private void EnablePhysics(IReadOnlyList<IPhysicsRecordable> recordables)
        {
            foreach (var recordable in recordables)
            {
                recordable.Rigidbody.useGravity = true;
                recordable.Rigidbody.isKinematic = false;
            }
        }

        private void DisablePhysics(IReadOnlyList<IPhysicsRecordable> recordables)
        {
            foreach (var recordable in recordables)
            {
                recordable.Rigidbody.useGravity = false;
                recordable.Rigidbody.isKinematic = true;
            }
        }
        
        private bool HasRigidbodyStopped(Rigidbody rb)
        {
            return rb.velocity == Vector3.zero && rb.angularVelocity == Vector3.zero;
        }
    }
}