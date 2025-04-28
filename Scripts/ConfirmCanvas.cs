using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ConfirmCanvas : CanvasBase
{
    [SerializeField] private TypewriterCore typewriter;

    [SerializeField] private CanvasGroup buttonCanvasGroup; 
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI submitButtonText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    [SerializeField] private AudioSource audioSource; 
    
    [SerializeField] private Image image;
    public static ConfirmCanvas Instance { get; private set; }

    private SelectId _selectId = SelectId.None;
    
    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
        
        submitButton.onClick.AddListener(()=> _selectId = SelectId.Submit);
        cancelButton.onClick.AddListener(()=> _selectId = SelectId.Cancel);
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        var dialogData = parcel.DialogData;
        buttonCanvasGroup.Hide();
        typewriter.TextAnimator.SetText(string.Empty);
        submitButtonText.text = dialogData.submitText;
        cancelButtonText.text = dialogData.cancelText;

        image.overrideSprite = dialogData.portrait;
        image.gameObject.SetActive(dialogData.portrait != null);
        
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);
        
        typewriter.ShowText(dialogData.text);

        if (dialogData.clip)
        {
            audioSource.clip = dialogData.clip;
            audioSource.Play();
        }

        var skip = false;
        while (typewriter.isShowingText)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (!Input.anyKeyDown) continue;
            
            SEManager.Play(SEID.Tap);
            if (typewriter.isShowingText)
            {
                typewriter.SkipTypewriter();
                skip = true;
            }
        }

        if (skip)
        {
            buttonCanvasGroup.Show();
        }
        else
        {
            await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);
        }
        
        while (!ct.IsCancellationRequested)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (_selectId == SelectId.Submit)
            {
                SEManager.Play(dialogData.submit);
                parcel.Value = (int)SelectId.Submit;
                break;
            }
            
            if (_selectId == SelectId.Cancel)
            {
                SEManager.Play(SEID.Exit);
                parcel.Value = (int)SelectId.Cancel;
                break;
            }
        }

        if (dialogData.clip && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        canvasGroup
            .FadeOutAsync(cancellationToken: ct)
            .ContinueWith(()=> canvas.enabled = false)
            .Forget();
        
        return parcel;
    }

    private enum SelectId
    {
        None,
        Submit,
        Cancel,
    }
}