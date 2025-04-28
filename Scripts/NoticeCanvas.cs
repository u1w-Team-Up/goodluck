using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class NoticeCanvas : CanvasBase
{
    [SerializeField] private Button submitButton;
    [SerializeField] private AnimatableWindow animatableWindow; 
    
    private SelectId _selectId = SelectId.None;

    public static NoticeCanvas Instance;
    
    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;

        submitButton.onClick.AddListener(()=> _selectId = SelectId.Submit);
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        animatableWindow.Open();

        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        while (!ct.IsCancellationRequested)
        {
            _selectId = SelectId.None;

            await UniTask.Yield(ct);

            if (_selectId != SelectId.Submit) continue;
            
            SEManager.Play(SEID.Tap);
            break;
        }
        
        animatableWindow.Close();
        
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