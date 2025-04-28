using System;
using System.Text;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

public class MapTitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI salary;
    private readonly StringBuilder _stringBuilder = new StringBuilder();

    private int _salaryValue;
    private int _requestValue;

    private void Reset()
    {
        var labels = GetComponents<TextMeshProUGUI>();
        label = labels[0];
        salary = labels[1];
    }

    private void Update()
    {
        UpdateSalary();
    }

    public void SetTitle(int areaCount)
    {
        _stringBuilder.Clear();
        _stringBuilder.Append("エリア");
        _stringBuilder.Append(AlphabetTextHelper.ToAlphabet(areaCount));
        label.SetText(_stringBuilder);
    }
    
    public void PresentSalary(ReadOnlyReactiveProperty<int> totalBountyRp)
    {
        _salaryValue = totalBountyRp.CurrentValue;
        UpdateSalary();

        totalBountyRp.Subscribe(RequestSalary).AddTo(destroyCancellationToken);
        return;
        void RequestSalary(int value)
        {
            _requestValue = value;
        }
    }

    public void UpdateSalary()
    {
        if (_requestValue != _salaryValue)
        {
            var current = _salaryValue;
            var diff = _requestValue - current;
            if (diff > 0)
            {
                current += 1;
            }
            else
            {
                current -= 1;
            }
            
            _stringBuilder.Clear();
            _stringBuilder.Append("$");
            _stringBuilder.Append(current.ToString("000"));
            salary.SetText(_stringBuilder);

            _salaryValue = current;
        }
    }
}