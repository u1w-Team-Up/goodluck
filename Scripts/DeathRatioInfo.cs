using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DeathRatioInfo : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Reset()
    {
        var labels = GetComponentsInChildren<TextMeshProUGUI>();
        slider = GetComponentInChildren<Slider>();
    }
    
    public void SetValue(float value)
    {
        value = Mathf.Max(GameData.ViewerCriticalRatioMin, value);
        slider.value = value * 100;
    }
}