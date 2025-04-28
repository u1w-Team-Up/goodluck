using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Label;

    private void Reset()
    {
        Label = GetComponent<TextMeshProUGUI>();
    }
    
    private void Awake()
    {
        var version = Application.version;

        var isFormat = Label.text.Contains("{0}");
        var result = isFormat ? string.Format(Label.text, version) : version;

        Label.SetText(result);
    }
}
