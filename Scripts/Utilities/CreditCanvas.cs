using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

internal sealed class CreditCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup Canvas;

    [SerializeField] private Maker[] Makers;

    [SerializeField] private Button BackButton;

    [SerializeField] private AnimatableWindow AnimatableWindow;

    private SelectId SelectedId = SelectId.None;
    private string SelectedUri = string.Empty;

    private void Reset()
    {
        Canvas = GetComponent<CanvasGroup>();
        Makers = GetComponentsInChildren<Maker>();
        BackButton = GetComponentInChildren<Button>();
        AnimatableWindow = GetComponentInChildren<AnimatableWindow>();
    }

    private void Awake()
    {
        Canvas.Hide();

        foreach (var m in Makers)
        {
            var b = m.Button;
            b.onClick.AddListener(() => OnClickedByMarkerButton(m.Uri));
        }

        BackButton.onClick.AddListener(() => OnClicked(SelectId.Back));
    }

    internal async UniTask MainTaskAsync(CancellationToken ct)
    {
        AnimatableWindow.Open();

        await Canvas.FadeInAsync(1.0f, cancellationToken: ct);

        do
        {
            SelectedId = SelectId.None;

            await UniTask.Yield(ct);

            if (SelectedId == SelectId.Marker)
            {
                // クリックSE.
                SEManager.Play(SEID.Tap);

                Application.OpenURL(SelectedUri);
            }
        }
        while (SelectedId != SelectId.Back);

        SEManager.Play(SEID.Exit);

        AnimatableWindow.Close();

        await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);

        Canvas.interactable = false;
        await Canvas.FadeOutAsync(1.0f, cancellationToken: ct);
    }

    private void OnClicked(SelectId id)
    {
        SelectedId = id;
    }

    private void OnClickedByMarkerButton(string uri)
    {
        SelectedId = SelectId.Marker;
        SelectedUri = uri;
    }

    private enum SelectId
    {
        None = 0,
        Back = 1,
        Marker = 2,
    }
}
