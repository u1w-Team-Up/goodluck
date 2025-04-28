using System.Linq;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DamageRatioInfo : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI label;
    
    [SerializeField] private Color[] pointColors;

    private void Reset()
    {
        image = GetComponent<Image>();
        label = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void UpdateDamageRatioAt(int nextIndex, MapData mapData, GameData gameData, PlayerData playerData)
    {
        PointData pointData = mapData.Points[nextIndex];
        float value = gameData.GetDamageRatio(pointData, playerData);
        
        image.color = pointColors[(int)pointData.PointType];
        SetValue(value);
    }
    
    private void SetValue(float value)
    {
        value = Mathf.Max(GameData.ViewerDamageRatioMin, value);
        label.SetText("被弾: "+ value.ToString("P1"));
    }
}
