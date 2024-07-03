using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Core
{
    public class DiceThrower : MonoBehaviour
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private Dice dicePrototype;
        [SerializeField] private int maxSimulationFrameCount = 300;

        [Header("Spawn parameters:")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnRadius = 0.5f;
        [SerializeField] private float maxForce = 25f;
        [SerializeField] private float maxTorque = 50f;

        private readonly List<Dice> _dice = new();
        private readonly PhysicsSimulator _simulator = new();

        public void ThrowDice(int diceCount, int expectedResult)
        {
            _simulator.StopAnyPlayingSimulation();

            // Simulate throwing dice and record
            InitializeDice(diceCount);
            _simulator.RecordSimulation(_dice, maxSimulationFrameCount);

            // Rotate dice visuals, so expected result comes out on top face
            foreach (var die in _dice)
            {
                die.RotateDiceVisuals(die.GetRollResult(), expectedResult);
            }

            // Play simulation with rotated visuals for each dice
            _simulator.PlayRecordedSimulation(this.GetCancellationTokenOnDestroy());
        }

        private void InitializeDice(int diceCount)
        {
            // Instantiate dice using object pool and add initial forces
            foreach (var die in _dice)
            {
                objectPool.Release(die);
            }

            _dice.Clear();

            for (var i = 0; i < diceCount; i++)
            {
                var die = objectPool.Get(dicePrototype);
                die.SetupAndAddForces(CreateInitialDiceState());
                _dice.Add(die);
            }
        }

        private DiceState CreateInitialDiceState()
        {
            var dicePosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;

            float GetRandomAngle() => Random.Range(0, 360);
            var x = GetRandomAngle();
            var y = GetRandomAngle();
            var z = GetRandomAngle();
            var rotation = Quaternion.Euler(x, y, z);

            float GetRandomForce() => Random.Range(-maxForce, maxForce);
            x = GetRandomForce();
            y = -Mathf.Abs(GetRandomForce()); // Throw dice down, not up
            z = GetRandomForce();
            var force = new Vector3(x, y, z);

            float GetRandomTorque() => Random.Range(-maxTorque, maxTorque);
            x = GetRandomTorque();
            y = GetRandomTorque();
            z = GetRandomTorque();
            var torque = new Vector3(x, y, z);

            return new DiceState(dicePosition, rotation, force, torque);
        }

        private void OnDrawGizmos()
        {
            if (spawnPoint == null) return;
            Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
        }
    }
}