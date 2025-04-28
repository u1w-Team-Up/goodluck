using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public sealed class PartSettingStore : MonoBehaviour
{
    [SerializeField] private List<Item> _partSettings = new();

    [Button]
    public void CreateList(string label)
    {
        PartSetting partSetting = new() { Parts = GetComponentsInChildren<Transform>(true).Select(x => new Part(x.gameObject.name, x.gameObject.activeSelf)).ToArray() };

        _partSettings.Add(new Item(label, partSetting));
    }

    public Item GetItem(string label) => _partSettings.First(x => x.Key == label);

    [Serializable]
    public class Item
    {
        public string Key;
        public PartSetting Value;

        public Item(string key, PartSetting value)
        {
            Key = key;
            Value = value;
        }
    }
}
