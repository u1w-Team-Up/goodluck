using System.Linq;
using TMPro;
using UnityEngine;
public class PercentInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI point;

    private void Reset()
    {
        var labels = GetComponentsInChildren<TextMeshProUGUI>();
        point = labels.Last();
    }
    
    public void SetValue(float value)
    {
        point.SetText(value.ToString("P1"));
    }
}