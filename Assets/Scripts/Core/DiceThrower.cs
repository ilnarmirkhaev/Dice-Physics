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
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnRadius;

        private readonly List<Dice> _dice = new();
        private readonly PhysicsSimulator _simulator = new();
        private readonly List<int> _diceResults = new();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ThrowDice(10, 3);
            }
        }

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
            var t = transform;
            var scale = t.localScale;
            var position = t.position;

            //Randomize X, Y, Z position in the bounding box
            var x = position.x + Random.Range(-scale.x / 2, scale.x / 2);
            var y = position.y + Random.Range(-scale.y / 2, scale.y / 2);
            var z = position.z + Random.Range(-scale.z / 2, scale.z / 2);
            var dicePosition = new Vector3(x, y, z);

            float GetRandomAngle() => Random.Range(0, 360);
            x = GetRandomAngle();
            y = GetRandomAngle();
            z = GetRandomAngle();
            var rotation = Quaternion.Euler(x, y, z);

            float GetRandomForce() => Random.Range(0, 25);
            x = GetRandomForce();
            y = GetRandomForce();
            z = GetRandomForce();
            var force = new Vector3(x, -y, z);

            float GetRandomTorque() => Random.Range(0, 50);
            x = GetRandomTorque();
            y = GetRandomTorque();
            z = GetRandomTorque();
            var torque = new Vector3(x, y, z);

            return new DiceState(dicePosition, rotation, force, torque);
        }
    }
}