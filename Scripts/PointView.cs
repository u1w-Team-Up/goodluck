using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
public class PointView : MonoBehaviour
{
    [SerializeField] private MPImage area;
    [SerializeField] private Image bigBonus;
    [SerializeField] private Image regularBonus;
    [SerializeField] private Image incDamageChance;
    [SerializeField] private Toggle toggle;

    [SerializeField] private GameObject selected;
    
    [SerializeField] private Color LeftColor;
    [SerializeField] private Color MiddleColor;
    [SerializeField] private Color RightColor;
    
    public void Setup(PointData pointData)
    {
        area.color = pointData.PointType switch
        {
            PointType.Left => LeftColor,
            PointType.Middle => MiddleColor,
            PointType.Right => RightColor,
            _ => area.color
        };

        bigBonus.enabled = pointData.IsBigBonus;
        regularBonus.enabled = pointData.IsRegularBonus;

        if (incDamageChance)
        {
            incDamageChance.enabled = pointData.IsAddDoubleDamageChance;
        }
    }
    
    public void SetToggle(bool isOn)
    {
        toggle.isOn = isOn;
    }
    
    public void SetIsSelected(bool value)
    {
        selected.SetActive(value);
    }
}
