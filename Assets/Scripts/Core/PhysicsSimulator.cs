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
        private UniTask _playSimulationTask;

        public void StopRecordedSimulation()
        {
            _cts?.Cancel();
        }

        public void RecordSimulation(IReadOnlyList<IPhysicsRecordable> recordables, int frameCount)
        {
            _simulation.Clear();
            _recordables = recordables;
            _frameCount = frameCount;

            EnablePhysics(recordables);
            SaveInitialState(recordables);
            StartRecording(recordables, frameCount);
        }

        public void PlayRecordedSimulation(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _playSimulationTask = PlaySimulation(_cts.Token);
        }

        private async UniTask PlaySimulation(CancellationToken cancellationToken)
        {
            DisablePhysics(_recordables);

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

        private void StartRecording(IReadOnlyList<IPhysicsRecordable> recordables, int frames)
        {
            Physics.simulationMode = SimulationMode.Script;

            //Begin recording position and rotation for every frame
            for (var i = 0; i < frames; i++)
            {
                //For every gameObject
                for (var j = 0; j < recordables.Count; j++)
                {
                    var recordable = recordables[j];
                    var t = recordable.Transform;
                    var position = t.position;
                    var rotation = t.rotation;

                    var frame = new SimulationFrame(position, rotation);
                    _simulation[j].Frames.Add(frame);
                }

                Physics.Simulate(Time.fixedDeltaTime);
            }

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

        public void ResetObjectsToInitialState()
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