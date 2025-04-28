using Cysharp.Threading.Tasks;
using LitMotion;
using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PartsController), typeof(PartSettingStore))]
public class PartsView : MonoBehaviour
{
    [SerializeField] private PartsController _partsController;
    [SerializeField] private PartSettingStore _partSettingStore;
    [SerializeField] private SpriteRenderer[] Renderers;

    private void Reset()
    {
        _partsController = GetComponent<PartsController>();
        _partSettingStore = GetComponent<PartSettingStore>();
        Renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void SetAlpha(float alpha)
    {
        foreach (var renderer in Renderers)
        {
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public async UniTask ShowAsync(string label, CancellationToken cancellationToken)
    {
        _partsController.Modify(_partSettingStore.GetItem(label).Value);

        try
        {
            await LMotion.Create(0f, 1.0f, 0.5f).Bind(x => SetAlpha(x)).ToUniTask(cancellationToken);
        }
        finally
        {
            SetAlpha(1);
        }
    }

    public async UniTask HideAsync(CancellationToken cancellationToken)
    {
        try
        {
            await LMotion.Create(1f, 0f, 1.5f).Bind(x => SetAlpha(x)).ToUniTask(cancellationToken);
        }
        finally
        {
            SetAlpha(0);
        }
    }

    [Button]
    public void Change(string label)
    {
        _partsController.Modify(_partSettingStore.GetItem(label).Value);
        SetAlpha(1);

        EditorHelper.SetDirty(this);
    }

    [Button]
    public void Hide()
    {
        _partsController.Modify(_partSettingStore.GetItem("非表示").Value);
        SetAlpha(0);

        EditorHelper.SetDirty(this);
    }
}
