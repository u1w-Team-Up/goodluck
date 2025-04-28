using System;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
public class PartInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI damage;
    
    [SerializeField] private GameObject selectedLight;
    [SerializeField] private GameObject damageLight;
    [SerializeField] private GameObject destructionLight;
    
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    
    private void Reset()
    {
        var labels = GetComponentsInChildren<TextMeshProUGUI>();
        header = labels[0]; 
        level = labels[1];
        damage = labels[2];

        var gameObjects = GetComponentsInChildren<Transform>(); 
        selectedLight = gameObjects.First(x => x.name == "selected_light").gameObject;
        
        // 位置的に取れない.
        // damageLight = gameObjects.First(x=> x.name == "Damage_light").gameObject;
        // destructionLight = gameObjects.First(x=> x.name == "destructionLight").gameObject;
    }
    
    public void SetLevel(PartData partData)
    {
        _stringBuilder.Clear();

        _stringBuilder.Append("Lv. ");
        _stringBuilder.Append(partData.ToLevelText());
        
        level.SetText(_stringBuilder);
    }
    
    public void SetHealth(int health)
    {
        _stringBuilder.Clear();

        string text = health switch
        {
            2 => "軽傷",
            1 => "重傷",
            0 => "壊滅",
            _ => " ",
        };
        
        _stringBuilder.Append(text);
        
        damage.SetText(_stringBuilder);
    }
    
    [Button]
    public void SetSelected(bool value)
    {
        selectedLight.SetActive(value);
    }

    [Button]
    public void SetDamageLight(int value, bool isFatal)
    {
        if (isFatal)
        {
            damageLight.SetActive(false);
            destructionLight.SetActive(true);
        }
        else if (value > 0)
        {
            damageLight.SetActive(true);
            destructionLight.SetActive(false);
        }
        else
        {
            damageLight.SetActive(false);
            destructionLight.SetActive(false);
        }
    }
}