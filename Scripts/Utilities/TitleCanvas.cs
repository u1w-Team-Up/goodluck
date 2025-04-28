using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class TitleCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup Canvas;
    [SerializeField] private CanvasGroup TapMaskCanvas;

    [SerializeField] private Button TapButton;

    [SerializeField] private Button StartButton;
    [SerializeField] private Button CreditButton;
    [SerializeField] private Button LicenseButton;

    [SerializeField] private CreditCanvas CreditCanvas;
    [SerializeField] private LicenseCanvas LicenseCanvas;

    [SerializeField] private SimpleMusicPlayer bgmPlayer;
    
    [SerializeField] private IntroCanvas introCanvas;

    private ButtonId SelectedId = ButtonId.None;

    public static bool HideTapMask = false;

    private void Awake()
    {
        StartButton.onClick.AddListener(() => SelectedId = ButtonId.Start);
        CreditButton.onClick.AddListener(() => SelectedId = ButtonId.Credit);
        LicenseButton.onClick.AddListener(() => SelectedId = ButtonId.License);

        Canvas.interactable = false;

        var ct = this.GetCancellationTokenOnDestroy();
        MainAsync(ct).Forget();
    }

    private async UniTask MainAsync(CancellationToken ct)
    {
        await UniTask.Yield(ct);

        if (HideTapMask == false)
        {
            TapMaskCanvas.Show();

            await TapButton.OnClickAsync(cancellationToken: ct);
            SEManager.Play(SEID.Tap);

            PlayBGM();

            await TapMaskCanvas.FadeOutAsync(1.0f, cancellationToken: ct);

            HideTapMask = true;
        }
        else
        {
            TapMaskCanvas.Hide();
            PlayBGM();
        }

        Canvas.interactable = true;

        while (!ct.IsCancellationRequested)
        {
            SelectedId = ButtonId.None;
            await UniTask.Yield(ct);

            if (SelectedId == ButtonId.Start)
            {
                SEManager.Play(SEID.Tap);
                UserPrefs.AttendSteps();
                await LoadSceneAsync(1, ct);
            }
            else if (SelectedId == ButtonId.Credit)
            {
                SEManager.Play(SEID.Open);
                await CreditAsync(ct);
            }
            else if (SelectedId == ButtonId.License)
            {
                SEManager.Play(SEID.Open);
                await LicenseAsync(ct);
            }
        }
    }

    private void PlayBGM()
    {
        bgmPlayer.Play();
    }

    private async UniTask LoadSceneAsync(int num, CancellationToken ct)
    {
        if (num == 1)
        {
            await introCanvas.ShowAsync(ct);
            await introCanvas.MainAsync(new Parcel(), ct);
        }
        
        bgmPlayer.Stop();
        await SceneTransitionManager.LoadSceneAsync(num);
    }

    private async UniTask CreditAsync(CancellationToken ct)
    {
        await CreditCanvas.MainTaskAsync(ct);
    }

    private async UniTask LicenseAsync(CancellationToken ct)
    {
        await LicenseCanvas.MainTaskAsync(ct);
    }

    private enum ButtonId
    {
        None,
        Start,
        Credit,
        License,
    }
}
