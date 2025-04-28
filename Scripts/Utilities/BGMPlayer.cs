using LitMotion;
using UnityEngine;

public sealed class BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource[] Sources;

    private int Current = 0;

    private void Reset()
    {
        Sources = GetComponents<AudioSource>();
    }

    public int Play(int index, bool forcePlay = false)
    {
        var before = Current;
        Current = index;

        for (int i = 0; i < Sources.Length; i++)
        {
            var item = Sources[i];

            var d = 0.8f;
            var hpi = Mathf.PI / 2;

            if (i == index)
            {
                if (!item.isPlaying || forcePlay)
                {
                    item.volume = 0;
                    item.Play();
                }

                if (item.volume != 1.0f)
                {
                    _ = LMotion.Create(0.0f, hpi, d).Bind(x => item.volume = Mathf.Pow(Mathf.Sin(x), 2)).AddTo(item);
                }
            }
            else
            {
                if (item.volume != 0)
                {
                    var firstVol = item.volume;
                    _ = LMotion.Create(0.0f, hpi, d).Bind(x => item.volume = Mathf.Pow(Mathf.Cos(x), 2) * firstVol).AddTo(item);
                }
            }
        }

        return before;
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
        bgm.FadeOutVolume();
    }

    public void Pause()
    {
        for (int i = 0; i < Sources.Length; i++)
        {
            var item = Sources[i];
            item.FadeOutVolume();
        }
    }

    public void Resume()
    {
        Play(Current);
    }
}