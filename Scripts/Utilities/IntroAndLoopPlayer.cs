using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
public sealed class IntroAndLoopMusicPlayer : MonoBehaviour, IMusicPlayer
{
    [SerializeField] private AudioSource IntroSources;
    [SerializeField] private AudioSource LoopSources;

    private void Reset()
    {
        var sources= GetComponents<AudioSource>();
        IntroSources = sources[0]; 
        LoopSources = sources[1];
        
        IntroSources.playOnAwake = false;
        LoopSources.playOnAwake = false;
    }

    public int Play()
    {
        if (!LoopSources.isPlaying)
        {
            IntroSources.Stop();
            IntroSources.time = 0;

            LoopSources.Stop();
            LoopSources.time = 0;
        }

        //イントロ部分の再生開始
        IntroSources.PlayScheduled(AudioSettings.dspTime);

        //イントロ終了後にループ部分の再生を開始
        LoopSources.PlayScheduled(AudioSettings.dspTime + ((float)IntroSources.clip.samples / (float)IntroSources.clip.frequency));
        
        LMotion.Create(IntroSources.volume, 1.0f, 0.6f).BindToVolume(IntroSources).AddTo(this);
        LMotion.Create(LoopSources.volume, 1.0f, 0.6f).BindToVolume(LoopSources).AddTo(this);

        return 0;
    }

    public void Stop()
    {
        LMotion.Create(IntroSources.volume, 0, 0.6f).BindToVolume(IntroSources).AddTo(this);
        LMotion.Create(LoopSources.volume, 0, 0.6f).BindToVolume(LoopSources).AddTo(this);
    }

    public void Resume()
    {
        LMotion.Create(IntroSources.volume, 1.0f, 0.6f).BindToVolume(IntroSources).AddTo(this);
        LMotion.Create(LoopSources.volume, 1.0f, 0.6f).BindToVolume(LoopSources).AddTo(this);
    }
}
