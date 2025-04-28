using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public static class RenderTextureCameraUtil
{
    public static void Expose(Camera camera, RenderTexture texture, UnityEngine.Rendering.Volume volume)
    {
        volume.gameObject.SetActive(true);
        camera.gameObject.SetActive(true);
        camera.targetTexture = texture;
        camera.forceIntoRenderTexture = true;
    }

    public static void Conceal(Camera camera, UnityEngine.Rendering.Volume volume)
    {
        camera.forceIntoRenderTexture = false;
        camera.gameObject.SetActive(false);
        volume.gameObject.SetActive(false);
    }

    public static async UniTask TakeFrame(Camera camera, RenderTexture texture, UnityEngine.Rendering.Volume volume, CancellationToken ct)
    {
        Expose(camera, texture, volume);    
        await UniTask.Yield(ct);
        Conceal(camera, volume);    
    }
}