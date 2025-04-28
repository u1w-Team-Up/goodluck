using UnityEngine;
using UnityEngine.UI;
public class SliderInfo : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Reset()
    {
        slider = GetComponentInChildren<Slider>();
    }
    
    public void SetValue(float value)
    {
        slider.value = value * 100;
    }
}
