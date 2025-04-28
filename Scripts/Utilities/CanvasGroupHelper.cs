using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public static class CanvasGroupHelper
{
    public static void Hide(this CanvasGroup canvas)
    {
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public static void Show(this CanvasGroup canvas)
    {
        canvas.alpha = 1;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    public static async UniTask FadeInAsync(this CanvasGroup canvas, float duration = 0.8f, CancellationToken cancellationToken = default)
    {
        canvas.Hide();

        canvas.interactable = true;
        canvas.blocksRaycasts = true;

        var from = canvas.alpha;
        var to = 1.0f;
        try
        {
            MotionHandle handle = LMotion.Create(from, to, duration).WithEase(Ease.InOutCirc).BindToAlpha(canvas);
            await handle.ToUniTask(cancellationToken);
        }
        finally
        {
            canvas.alpha  = to;
        }
    }

    public static async UniTask FadeOutAsync(this CanvasGroup canvas, float duration = 0.6f, CancellationToken cancellationToken = default)
    {
        var from = canvas.alpha;
        var to = 0.0f;

        var handle = LMotion.Create(from, to, duration).WithEase(Ease.InQuad).BindToAlpha(canvas);
        await handle.ToUniTask(cancellationToken);

        canvas.Hide();
    }
}

public static class AnimatorHelper
{
    public static async UniTask UntilStateAsync(this Animator animator, string name, CancellationToken ct, float span = 0.5f)
    {
        if (span == 0)
        {
            await UniTask.Yield(ct);
        }
        else
        {
            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
        }

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            await UniTask.Yield(ct);
        }
    }
}
