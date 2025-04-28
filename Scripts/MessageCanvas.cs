using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;
using UnityEngine.UI;
public class MessageCanvas : CanvasBase
{
    [SerializeField] private TypewriterCore typewriter;
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private Image image;
    
    public static MessageCanvas Instance { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        typewriter.TextAnimator.SetText(string.Empty);

        foreach (Comment comment in parcel.CommentList.comments)
        {
            image.overrideSprite = comment.talker;
            image.enabled = image.overrideSprite != null; 

            await UniTask.Yield(ct);
            
            if (Mathf.Approximately(canvasGroup.alpha, 0))
            {
                canvas.enabled = true;
                await canvasGroup.FadeInAsync(cancellationToken: ct);
            }

            typewriter.ShowText(comment.message);
            if (comment.clip)
            {
                audioSource.clip = comment.clip;
                audioSource.volume = 1f;
                audioSource.time = 0;
                audioSource.Play();
            }

            while (ct.IsCancellationRequested == false)
            {
                await UniTask.Yield(ct);

                if (audioSource.isPlaying == false)
                {
                    // ありえないけど一応.
                    if (typewriter.isShowingText)
                    {
                        typewriter.SkipTypewriter();
                    }
                    
                    // 音声が再生中でない場合は、メッセージ待ちをスキップ.
                    break;
                }
                
                if (!Input.anyKeyDown) continue;
                
                SEManager.Play(SEID.Tap);
                if (typewriter.isShowingText)typewriter.SkipTypewriter();
                break;
            }
        }
        
        return parcel;
    }

    public async UniTask ShowAsync(CancellationToken ct)
    {
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);
    }
    
    public void Hide()
    {
        if (canvasGroup.alpha > 0)
        {
            if (audioSource.isPlaying)
            {
                LMotion.Create(audioSource.volume, 0, 0.6f).BindToVolume(audioSource);
            }
            
            canvasGroup.FadeOutAsync().ContinueWith(OnEnd).Forget();
        }
        return;

        void OnEnd()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            canvas.enabled = false;
        }
    }
}