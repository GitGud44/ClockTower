using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderDisplay : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueText;

    void OnEnable()
    {
        if (slider != null && valueText != null)
        {
            UpdateDisplay(slider.value);
            slider.onValueChanged.AddListener(UpdateDisplay);
        }
    }

    void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(UpdateDisplay);
    }

    public void UpdateDisplay(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
