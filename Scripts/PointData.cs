using System;
using UnityEngine;

[Serializable]
public class PointData
{
    public readonly PointType PointType;
    private readonly EffectType _effectType;
    
    public PointData(PointType pointType, EffectType effectType)
    {
        PointType = pointType;
        _effectType = effectType;
    }
    
    // public bool IsBonus => _bonusType != BonusType.None;
    public bool IsRegularBonus => _effectType == EffectType.RegularBonus;
    public bool IsBigBonus => _effectType == EffectType.BigBonus;
    public bool IsAddDoubleDamageChance => _effectType == EffectType.AddDoubleDamageChance;

    public int GetBounty(int salary)
    {
        return _effectType switch
        {
            EffectType.RegularBonus => Mathf.FloorToInt(salary * 0.5f),
            EffectType.BigBonus => salary * 2,
            EffectType.Penalty => - Mathf.FloorToInt(salary * 0.3f),
            _ => 0
        };
    }
}

public enum EffectType
{
    None,
    RegularBonus,
    BigBonus,
    AddDoubleDamageChance,
    Penalty,
}