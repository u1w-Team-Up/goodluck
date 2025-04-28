using System;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MoneyAmount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    private void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
    }
    
    public void SetAmount(int amount)
    {
        _stringBuilder.Clear();

        _stringBuilder.Append("残金\t");
        _stringBuilder.Append(amount);
        _stringBuilder.Append("$");
        
        label.SetText(_stringBuilder);
    }
}