using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

internal sealed class BGMManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] Sources;

    private int Current = 0;

    public int Play(int index, bool forcePlay = false)
    {
        var before = Current;
        Current = index;

        for (int i = 0; i < Sources.Length; i++)
        {
            var item = Sources[i];

            if (i == index)
            {
                if (!item.isPlaying || forcePlay)
                {
                    item.Play();
                }

                LMotion.Create(item.volume, 1, 0.6f).BindToVolume(item).AddTo(item);
            }
            else
            {
                if (item.volume != 0)
                {
                    LMotion.Create(item.volume, 0.0f, 0.6f).BindToVolume(item).AddTo(item);
                }
            }
        }

        return before;
    }

    private static BGMManager _instance;
    public static BGMManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }

    public void ForceStop()
    {
        var bgm = Sources[Current];
        bgm.volume = 0;
        bgm.Stop();
    }

    public void Stop()
    {
        var bgm = Sources[Current];
        LMotion.Create(bgm.volume, 0, 0.6f).BindToVolume(bgm).AddTo(bgm);
    }

    public void Pause()
    {
        for (int i = 0; i < Sources.Length; i++)
        {
            var item = Sources[i];
            LMotion.Create(item.volume, 0, 0.6f).BindToVolume(item).AddTo(item);
        }
    }

    public void Resume()
    {
        Play(Current);
    }
}
