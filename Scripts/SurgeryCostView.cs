using System;
using System.Text;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(TextMeshProUGUI))]
public class SurgeryCostView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    
    private readonly StringBuilder _sb = new();

    private void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void SetValue(int cost)
    {
        _sb.Clear();
        _sb.Append("-");
        _sb.Append(cost);
        _sb.Append("$");
        
        label.SetText(_sb);
    }

    public void Present(ReadOnlyReactiveProperty<int> costRp)
    {
        costRp.Subscribe(SetValue).AddTo(destroyCancellationToken);
    }
}
