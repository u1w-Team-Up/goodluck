using System;
using System.Collections.Generic;
using System.Linq;
using NRandom;
using NRandom.Linq;
using R3;
using UnityEngine;

[Serializable]
public class MapData
{
    public PointData[] Points { get; set; }

    private const int PointCount = RowCount * ColumnCount;
    private const int RowCount = 3;
    private const int ColumnCount = 3;

    private readonly ReactiveProperty<int> _indexRp = new(0);
    public ReadOnlyReactiveProperty<int> IndexRp => _indexRp;
    
    private readonly ReactiveProperty<int> _areaCountRp = new(1);
    public ReadOnlyReactiveProperty<int> AreaCountRp => _areaCountRp;
    
    private readonly ReactiveProperty<int> _salaryRp = new(GameData.InitialSalary);
    public ReadOnlyReactiveProperty<int> SalaryRp => _salaryRp;

    private readonly ReactiveProperty<int> _totalBountyRp = new();
    public ReadOnlyReactiveProperty<int> TotalBountyRp => _totalBountyRp;

    private readonly ReactiveProperty<int> _nextIndexRp = new(0);
    public ReadOnlyReactiveProperty<int> NextIndexRp => _nextIndexRp;

    private int Index => IndexRp.CurrentValue;
    public int AreaCount => AreaCountRp.CurrentValue;

    private const int FinalAreaCount = 4;

    public MapData()
    {
        Create();
    }
    
    public void Create()
    {
        _indexRp.Value = 0;
        var random = RandomEx.Shared;
        Points = new PointData[PointCount];

        IEnumerable<int> chances = Enumerable.Range(1, PointCount - 1 - 1);
        int[] enumerable = chances as int[] ?? chances.ToArray();
        int bonus = enumerable.RandomElement();
        int regular = enumerable.Where(x => x != bonus).RandomElement();

        for (int i = 0; i < PointCount; i++)
        {
            PointType pointType = (PointType)(random.NextInt(0, 2 + 1));

            EffectType effectType = EffectType.None;
            if (i == bonus)
            {
                effectType = EffectType.BigBonus;
            }
            else if (i == regular)
            {
                effectType = EffectType.RegularBonus;
            }
            else if (i == PointCount - 1)
            {
                effectType = EffectType.AddDoubleDamageChance;
            }
            
            Points[i] = new PointData(pointType, effectType);
        }
    }

    public bool CanMove(Direction direction)
    {
        return direction switch
        {
            Direction.Right => Index < PointCount - RowCount,
            Direction.Down => Index % RowCount != RowCount - 1,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public void IncrementAreaCount()
    {
        _areaCountRp.Value++;
    }
    
    public void UpdateSalary()
    {
        _salaryRp.Value = Mathf.FloorToInt(GameData.InitialSalary * Mathf.Pow(GameData.SalaryCommonRatio, AreaCount - 1));
        UpdateTotalBounty(Index);
    }
    
    public bool IsStoryFinalArea => AreaCount == FinalAreaCount;

    public bool IsStoryClear => IsEnd && IsStoryFinalArea;
    
    public bool IsEnd => Index == PointCount - 1;
    
    public void Move(Direction direction)
    {
        _indexRp.Value = Calculate(direction);
    }
    
    public PointData Current => Points[Index];

    public int Calculate(Direction direction)
    {
        if (direction == Direction.Right)
        {
            return _indexRp.Value + RowCount;
        }

        return IndexRp.CurrentValue + 1;
    }
    
    public void UpdateTotalBounty(int nextIndex)
    {
        _nextIndexRp.Value = nextIndex;
        
        var salary = SalaryRp.CurrentValue;
        _totalBountyRp.Value = 
            salary + Points[nextIndex].GetBounty(salary);
    }

    public string GetAreaText()
    {
        return
            AlphabetTextHelper.ToAlphabet(AreaCount)
            + "-"
            + (Index + 1);
    }
}
