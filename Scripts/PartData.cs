using System;
using R3;

[Serializable]
public class PartData
{
    private readonly ReactiveProperty<int> _levelRp = new(0);
    public ReadOnlyReactiveProperty<int> LevelRp => _levelRp;

    private readonly ReactiveProperty<int> _healthRp = new(InitialHealth);
    public ReadOnlyReactiveProperty<int> Health => _healthRp;

    public const int InitialHealth = 3;
    public const int LimitLevel = 5;

    private int _beforeLevel = 0;

    public void IncrementLevel()
    {
        _levelRp.Value += 1;
    }

    public void Damage(int amount)
    {
        _healthRp.Value -= amount;
        if (Health.CurrentValue < 0)
        {
            _healthRp.Value = 0;
        }
    }

    public int OnLevelUpHealth()
    {
        if (Health.CurrentValue == 0)
        {
            var initialHealth = InitialHealth;
            _healthRp.Value = initialHealth;
            return initialHealth;
        }
        
        var diff = InitialHealth - Health.CurrentValue;
        _healthRp.Value += diff;
        return diff;
    }

    public bool IsLost => Health.CurrentValue == 0;
    
    public bool IsLive => !IsLost;
    
    public bool IsPinch => Health.CurrentValue == 1;
    
    public bool IsUpgradable =>
        true;
        // IsLive && !IsLevelMax;

    public string ToLevelText()
    {
        int value = LevelRp.CurrentValue;
        if (IsLost)
        {
            return "-";
        }
        
        return value switch
        {
            LimitLevel => "★",
            > LimitLevel => "★" + (1 + value - LimitLevel),
            _ => value.ToString()
        };
    }

    public int GetVisualLevel()
    {
        if (LevelRp.CurrentValue >= LimitLevel)
        {
            return LimitLevel;
        }
        return LevelRp.CurrentValue;
    }

    public int GetHealth()
    {
        return Health.CurrentValue;
    }

    public int GetDamageAmount()
    {
        return InitialHealth - Health.CurrentValue;
    }
    
    public void Deficit()
    {
        _beforeLevel = LevelRp.CurrentValue;
        _levelRp.Value = 0;
    }

    public void Reborn()
    {
        _levelRp.Value = _beforeLevel;
        _healthRp.Value = 1;
    }
}
