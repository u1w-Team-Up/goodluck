using System.Text;
using TMPro;
using UnityEngine;
public class ContentInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI point;
    [SerializeField] private TextMeshProUGUI diff;
    
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    
    private void Reset()
    {
        var labels = GetComponentsInChildren<TextMeshProUGUI>();
        header = labels[0]; 
        point = labels[1];
        diff = labels[2];
    }
    
    public void SetValue(string value)
    {
        _stringBuilder.Clear();
        _stringBuilder.Append(value);
        point.SetText(_stringBuilder);
    }
}