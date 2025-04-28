using System;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TrueShadow), typeof(TextMeshProUGUI))]
public class TextProvider : MonoBehaviour
{
    [Header("References Text")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TrueShadow trueShadow;

    private bool _isDirty;
    
    public TextMeshProUGUI Label => label;

    private void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
        trueShadow = GetComponent<TrueShadow>();
    }

    protected void Awake()
    {
        Canvas.willRenderCanvases += OnWillRenderCanvases;
    }

    protected void OnDestroy()
    {
        Canvas.willRenderCanvases -= OnWillRenderCanvases;
    }

    public void SetText(string value)
    {
        label.text = value;
        _isDirty = true;
    }

    private void OnWillRenderCanvases()
    {
        if (_isDirty)
        {
            _isDirty = false;
            trueShadow.CopyToTMPSubMeshes();
        }
    }
}
