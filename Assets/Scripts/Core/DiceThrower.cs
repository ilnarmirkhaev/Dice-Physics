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
        [SerializeField] private int simulationFrames = 400;
        [SerializeField] private Dice dicePrototype;
        
        [Header("Spawn parameters:")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnRadius = 0.5f;
        [SerializeField] private float maxForce = 25f;
        [SerializeField] private float maxTorque = 50f;

        private readonly List<Dice> _dice = new();
        private readonly PhysicsSimulator _simulator = new();
        private readonly List<int> _diceResults = new();

        public void ThrowDice(int diceCount, int expectedResult)
        {
            _diceResults.Clear();
            _simulator.StopRecordedSimulation();

            GenerateDice(diceCount);
            _simulator.RecordSimulation(_dice, simulationFrames);

            foreach (var die in _dice)
            {
                _diceResults.Add(die.GetRollResult());
            }

            _simulator.ResetObjectsToInitialState();

            for (var i = 0; i < _dice.Count; i++)
            {
                var die = _dice[i];
                die.RotateDiceVisuals(_diceResults[i], expectedResult);
            }

            _simulator.PlayRecordedSimulation(this.GetCancellationTokenOnDestroy());
        }

        private void GenerateDice(int diceCount)
        {
            foreach (var die in _dice)
            {
                objectPool.Release(die);
            }

            _dice.Clear();

            for (var i = 0; i < diceCount; i++)
            {
                var die = objectPool.Get(dicePrototype);
                die.Setup(CreateInitialDiceState());
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

            float GetRandomForce() => Random.Range(0, maxForce);
            x = GetRandomForce();
            y = GetRandomForce();
            z = GetRandomForce();
            var force = new Vector3(x, -y, z);

            float GetRandomTorque() => Random.Range(0, maxTorque);
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