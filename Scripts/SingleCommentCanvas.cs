using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;
public class SingleCommentCanvas : CanvasBase
{
    [SerializeField] private TypewriterCore typewriter;

    [SerializeField] private CanvasGroup buttonCanvasGroup; 
    [SerializeField] private Button submitButton;
    [SerializeField] private AudioSource audioSource; 

    [SerializeField] private AnimatableWindow animatableWindow; 
    
    private SelectId _selectId = SelectId.None;
    
    [SerializeField] private CommentList commentList;
    
    protected override void OnAwake()
    {
        base.OnAwake();

        submitButton.onClick.AddListener(()=> _selectId = SelectId.Submit);
    }

    public async UniTask ShowAsync(CancellationToken ct)
    {
        buttonCanvasGroup.Hide();
        typewriter.TextAnimator.SetText(string.Empty);
        
        animatableWindow.Open();

        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);
    }

    public async UniTask HideAsync(CancellationToken ct)
    {
        animatableWindow.Close();
        
        await canvasGroup.FadeOutAsync(cancellationToken: ct);
        canvas.enabled = false;
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        var comment = commentList.comments.First();
        typewriter.ShowText(comment.message);
        
        audioSource.clip = comment.clip;
        audioSource.time = 0;
        audioSource.volume = 1f;
        audioSource.Play();

        while (typewriter.isShowingText)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (!Input.anyKeyDown) continue;
            
            SEManager.Play(SEID.Tap);
            
            if (typewriter.isShowingText)
            {
                typewriter.SkipTypewriter();
            }
        }
        
        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);
        
        while (!ct.IsCancellationRequested)
        {
            _selectId = SelectId.None;

            await UniTask.Yield(ct);

            if (_selectId != SelectId.Submit) continue;
            
            SEManager.Play(SEID.Tap);
            break;
        }

        if (audioSource.isPlaying)
        {
            LMotion.Create(audioSource.volume, 0, 0.6f).BindToVolume(audioSource);
        }

        return parcel;
    }

    private enum SelectId
    {
        None,
        Submit,
    }
}