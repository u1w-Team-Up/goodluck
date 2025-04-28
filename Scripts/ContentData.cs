using System;
using NRandom;
using R3;
using UnityEngine.Rendering.Universal;

[Serializable]
public class ContentData
{
    private readonly ReactiveProperty<float> _pointRp = new(0);
    
    public ReadOnlyReactiveProperty<float> PointRp => _pointRp;

    private readonly float _coefficient;

    private float _beforeValue = 0;

    public float CalculationValue => PointRp.CurrentValue;
    
    public float DisplayValue => PointRp.CurrentValue * _coefficient;
    
    public string GetDisplayPointValueText()  
    {
        return DisplayValue.ToString("F2");
    }
    
    public ContentData(float coefficient)
    {
        _coefficient = coefficient;
    }

    public float IncrementLevel(IRandom random)
    {
        float diff = random.NextFloat(1.0f, 2.0f);
        _pointRp.Value += diff;
        return diff * _coefficient;
    }
    
    public float Birth(IRandom shared)
    {
        _pointRp.Value = shared.NextFloat(1.0f, 2.0f);
        return DisplayValue; 
    }
    
    public void Deficit()
    {
        _beforeValue = PointRp.CurrentValue; 
        _pointRp.Value = 0;
    }
    
    public void Reborn()
    {
        _pointRp.Value = _beforeValue;
    }
}
