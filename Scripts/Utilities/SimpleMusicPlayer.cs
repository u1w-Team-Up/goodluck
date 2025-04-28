using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
public class SimpleMusicPlayer : MonoBehaviour, IMusicPlayer
{
    [SerializeField] private AudioSource Source;

    private void Reset()
    {
        Source = GetComponent<AudioSource>();
    }

    public int Play()
    {
        Source.volume = 1f;
        Source.time = 0f;
        Source.Play();
        return 0;
    }

    public void Stop()
    {
        LMotion.Create(Source.volume, 0, 0.6f).BindToVolume(Source).AddTo(this);
    }
    
    public void Resume()
    {
        LMotion.Create(Source.volume, 1, 0.6f).BindToVolume(Source).AddTo(this);
    }
}
