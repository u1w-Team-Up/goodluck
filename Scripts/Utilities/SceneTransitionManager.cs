using Cysharp.Threading.Tasks;
using LitMotion;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

internal sealed class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private TransitionProgressController TransitionProgress;
    [SerializeField] private Canvas Canvas;
    [SerializeField] private Camera MyOverlayCamera;

    private void SetProgress(float v, bool force = false)
    {
        TransitionProgress.SetProgress(v, force);
    }

    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance => _instance;

    private void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
            return;
        }

        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        if (!cameraData.cameraStack.Contains(MyOverlayCamera))
        {
            cameraData.cameraStack.Add(MyOverlayCamera);
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static async UniTask LoadSceneAsync(int v)
    {
        await Instance.LoadAsync_(v);
    }

    public async UniTask LoadAsync_(int sceneNum)
    {
        var ct = this.GetCancellationTokenOnDestroy();
        var beforeScene = SceneManager.GetActiveScene();

        await LMotion.Create(0.0f, 1.0f, 1.5f).Bind(x => SetProgress(x)).AddTo(TransitionProgress);

        var op = SceneManager.LoadSceneAsync(sceneNum);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            await UniTask.Yield(ct);
        }

        void onCompleted(AsyncOperation _)
        {
            var loadedScene = SceneManager.GetSceneByBuildIndex(sceneNum);

            var lcd = GetCameraData(loadedScene);
            if (!lcd.cameraStack.Contains(MyOverlayCamera))
            {
                lcd.cameraStack.Add(MyOverlayCamera);
            }

            SetProgress(1, true);

            if (loadedScene.name == "Title")
            {
                TitleCanvas.HideTapMask = true;
            }
        }

        op.completed += onCompleted;

        op.allowSceneActivation = true;

        await UniTask.WaitUntil(() => TransitionProgress.progress == 1.0f, cancellationToken: ct);
        var handle = LMotion.Create(1.0f, 0.0f, 1.5f).Bind(x => SetProgress(x));

        await handle.ToUniTask(ct);
    }

    private UniversalAdditionalCameraData GetCameraData(Scene scene)
    {
        var mainCamera = SceneHelper.FirstComponent<Camera>(scene, x => x.CompareTag("MainCamera"));

        return mainCamera.GetUniversalAdditionalCameraData();
    }
}

public static class SceneHelper
{
    public static T FirstComponent<T>(this Scene scene, Func<T, bool> predicate = null)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.GetComponentInChildren<T>(true) is T result)
            {
                if (predicate == null || predicate(result))
                {
                    return result;
                }
            }
        }

        return default;
    }
}

