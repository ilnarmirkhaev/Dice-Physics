using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] private InputField diceCountInput;
        [SerializeField] private InputField diceResultInput;
        [SerializeField] private Button launchButton;
        [SerializeField] private DiceThrower diceThrower;

        private void Start()
        {
            launchButton.onClick.AddListener(Launch);
        }

        private void Launch()
        {
            if (!int.TryParse(diceCountInput.text, out var diceCount) ||
                !int.TryParse(diceResultInput.text, out var expectedDiceResult))
            {
                Debug.LogError("Invalid input");
                return;
            }

            diceThrower.ThrowDice(diceCount, expectedDiceResult);
        }

        private void OnDestroy()
        {
            launchButton.onClick.RemoveListener(Launch);
        }
    }
}