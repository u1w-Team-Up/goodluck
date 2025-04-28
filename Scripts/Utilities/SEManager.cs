using UnityEngine;

internal sealed class SEManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] AudioClips;
    [SerializeField] private AudioSource AudioSource;

    public static void Play(SEID eID)
    {
        Instance.Play_((int)eID);
    }
    
    private void Play_(int index)
    {
        AudioSource.PlayOneShot(AudioClips[index]);
    }

    private static SEManager _instance;
    public static SEManager Instance => _instance;

    private void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
public enum SEID : int
{
    Tap = 0,
    Exit,
    Open,
    Sally,
    Surgery,
    LostJingle,
    SingleShot,
    BarrageShot,
    GameOver,
    Dodge,
    Bonus,
    Reward,
    Critical,
    Soul,
}
