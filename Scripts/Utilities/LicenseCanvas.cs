using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

internal sealed class LicenseCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup Canvas;

    [SerializeField] private Button BackButton;

    [SerializeField] private AnimatableWindow AnimatableWindow;

    private SelectId SelectedId = SelectId.None;

    private void Reset()
    {
        Canvas = GetComponent<CanvasGroup>();
        BackButton = GetComponentInChildren<Button>();
        AnimatableWindow = GetComponentInChildren<AnimatableWindow>();
    }

    private void Awake()
    {
        Canvas.Hide();

        BackButton.onClick.AddListener(() => OnClicked(SelectId.Back));
    }

    public async UniTask MainTaskAsync(CancellationToken ct)
    {
        AnimatableWindow.Open();

        await Canvas.FadeInAsync(cancellationToken: ct);

        do
        {
            SelectedId = SelectId.None;

            await UniTask.Yield(ct);
        }
        while (SelectedId != SelectId.Back);

        SEManager.Play(SEID.Exit);

        AnimatableWindow.Close();

        await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);

        await Canvas.FadeOutAsync(cancellationToken: ct);
    }

    private void OnClicked(SelectId id)
    {
        SelectedId = id;
    }

    private enum SelectId
    {
        None = 0,
        Back = 1,
    }
}