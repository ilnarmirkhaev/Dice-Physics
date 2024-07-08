using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DiceRollSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Text diceCountText;
        [SerializeField] private Slider diceCountSlider;
        [SerializeField] private Text diceValueText;
        [SerializeField] private Slider diceValueSlider;
        [SerializeField] private Button launchButton;
        [SerializeField] private DiceThrower diceThrower;

        private void Start()
        {
            launchButton.onClick.AddListener(Launch);
            diceCountSlider.onValueChanged.AddListener(UpdateCountText);
            diceValueSlider.onValueChanged.AddListener(UpdateValueText);
            
            UpdateCountText(diceCountSlider.value);
            UpdateValueText(diceValueSlider.value);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }
        }

        private void Launch()
        {
            diceThrower.ThrowDice((int)diceCountSlider.value, (int)diceValueSlider.value);
        }

        private void UpdateCountText(float count) => diceCountText.text = ((int)count).ToString();

        private void UpdateValueText(float value) => diceValueText.text = ((int)value).ToString();

        private void OnDestroy()
        {
            launchButton.onClick.RemoveAllListeners();
            diceCountSlider.onValueChanged.RemoveAllListeners();
            diceValueSlider.onValueChanged.RemoveAllListeners();
        }
    }
}