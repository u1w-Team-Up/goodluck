using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Core;
using UnityEngine;
using UnityEngine.UI;

public class PostOpeCanvas : CanvasBase
{
    [SerializeField] private TypewriterCore typewriter;

    [SerializeField] private CanvasGroup buttonCanvasGroup; 
    [SerializeField] private Button submitButton;
    [SerializeField] private AudioSource audioSource; 
    
    public static PostOpeCanvas Instance { get; private set; }

    private SelectId _selectId = SelectId.None;
    
    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
        
        submitButton.onClick.AddListener(()=> _selectId = SelectId.Submit);
    }

    public async UniTask ShowAsync(CancellationToken ct)
    {
        buttonCanvasGroup.Hide();
        typewriter.TextAnimator.SetText(string.Empty);

        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        var dialogData = parcel.DialogData;
        
        typewriter.ShowText(dialogData.text);
        
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
        }

        await canvasGroup.FadeOutAsync(cancellationToken: ct);
        canvas.enabled = false;

        return parcel;
    }

    private enum SelectId
    {
        None,
        Submit,
    }
}