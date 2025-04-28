using System;
using NRandom;
using R3;
using UnityEngine;

[Serializable]
public class GameData
{
    private readonly ReactiveProperty<int> _moneyRp = new(InitialMoney);
    public ReadOnlyReactiveProperty<int> Money => _moneyRp;

    private readonly ReactiveProperty<int> _dayRp = new(0);
    public ReadOnlyReactiveProperty<int> Day => _dayRp;

    private readonly ReactiveProperty<float> _stressRp = new(InitialStress);
    public ReadOnlyReactiveProperty<float> Stress => _stressRp;

    private float _diffDeathBoarder;
    private float _diffDamageBoarder;

    private readonly ReactiveProperty<float> _criticalRatioRp = new(InitialCriticalRatio);
    public ReadOnlyReactiveProperty<float> CriticalRatio => _criticalRatioRp;
    
    private readonly ReactiveProperty<int> _surgeryCostRp = new(InitialSurgeryCost);
    public ReadOnlyReactiveProperty<int> SurgeryCostRp => _surgeryCostRp;

    public int SurgeryCost => SurgeryCostRp.CurrentValue;

    private const int InitialMoney = 152;
    private const float InitialCriticalRatio = 0.0f;
    public const float ViewerCriticalRatioMin = 0.02f;
    private const float InitialStress = 0.05f;
    
    public const float FatalStressMin = 0.005f;
    public const float FatalStressMaximum = 0.009f;
    
    public const float InjuryStressMin = 0.01f;
    public const float InjuryStressMaximum = 0.03f;
    
    public const float FlawlessStressMin = 0.12f;
    public const float FlawlessStressMaximum = 0.18f;
    
    public const int InitialSurgeryCost = 50;
    public const float SurgeryCommonRatio = 1.02f;
    
    public const int InitialSalary = 50;
    public const float SalaryCommonRatio = 0.96f;

    public const float InitialTotalPoint = 6f + 4.8f;
    public const float DamageTryMax = 3;
    private const float DeathRatioDaySpan = 1.98f;

    private const int MinimumDayWhenDeathRatio = 0;

    public const float InitialDamageTotalPoint = 4.28f + 1.4f;
    private const float DamageRatioDaySpan = 1.48f;
    public const float ViewerDamageRatioMin = 0.001f;

    private const float RegainStressMin = 0.05f;
    private const float RegainStressMax = 0.18f;

    public const float RebornChanceRate = 1 / 2f;
    public const float RebornWinningRate = 1 / 6f;

    public void AddMoney(int amount)
    {
        _moneyRp.Value += amount;
    }

    public void SubMoney(int amount)
    {
        _moneyRp.Value -= amount;
    }

    public bool IsEnoughMoney(int amount)
    {
        return _moneyRp.Value >= amount;
    }
    
    public void AddStress(float amount)
    {
        _stressRp.Value += amount;

        if (_stressRp.Value > 1)
        {
            _stressRp.Value = 1;
        }
    }

    public bool IsStressLimit => _stressRp.Value >= 1.0f;

    public void NextDay()
    {
        _dayRp.Value++;
    }

    public void OnDestruction(float lostPoint)
    {
        float diff = lostPoint * 0.9f;
        _diffDeathBoarder -= diff; 
        _diffDamageBoarder -= diff;
    }

    public void OnReborn(float recoverPoint)
    {
        OnDestruction(-recoverPoint);
    }
    
    public void UpdateDeathRatio(PlayerData playerData)
    {
        if (_dayRp.Value <= MinimumDayWhenDeathRatio)
        {
            _criticalRatioRp.Value = 0;
            return;
        }
        
        float boarder = InitialTotalPoint + DeathRatioDaySpan * Day.CurrentValue + _diffDeathBoarder;
        float point = playerData.GetTotalPoint();
        float ratio = Mathf.Clamp01((boarder - point) / boarder);
        _criticalRatioRp.Value = ratio;
    }

    public float GetDamageBoarder(PointType pointType)
    {
        return InitialDamageTotalPoint + DamageRatioDaySpan * Day.CurrentValue + _diffDamageBoarder;
    }
    
    public void UpdateSurgeryCost(int areaCount, PartData data)
    {
        _surgeryCostRp.Value = Mathf.FloorToInt(InitialSurgeryCost * Mathf.Pow(SurgeryCommonRatio, areaCount));

        if (data.IsLost)
        {
            _surgeryCostRp.Value *= 2;
        }
    }
    
    public void RegainStress(IRandom random)
    {
        _stressRp.Value -= random.NextFloat(RegainStressMin, RegainStressMax);
        if (_stressRp.Value < 0)
        {
            _stressRp.Value = 0;
        }
    }
    
    public float GetDamageRatio(PointData areaPoint, PlayerData playerData)
    {
        float total = playerData.GetTotalWithPointType(areaPoint.PointType);
        float boarder = GetDamageBoarder(areaPoint.PointType);
        float damageRatio = Mathf.Clamp01((boarder - total) / boarder);

        #if UNITY_EDITOR
        Debug.Log($"{boarder} - {total} = {damageRatio:P3}");
        #endif
        return damageRatio;
    }
    
    public int GetCommentIndex(int length)
    {
        var index = Day.CurrentValue;
        while (index >= length)
        {
            index -= length;
        }
        
        return index;
    }

    public int GetRandomRebornChance(IRandom random)
    {
        if (!(random.NextFloat(0, 1) <= RebornChanceRate)) return 0;
        return random.NextFloat(0, 1) <= RebornWinningRate ? 2 : 1;
    }
}
