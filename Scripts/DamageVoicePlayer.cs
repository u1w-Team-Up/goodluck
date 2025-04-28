using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class DamageVoicePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    
    [SerializeField] private RandomClipStore hit;
    [SerializeField] private RandomClipStore damage;
    [SerializeField] private RandomClipStore fatal;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(int damageValue)
    {
        RandomClipStore store = damageValue switch{
            1 => hit,
            3 => fatal,
            _ => damage,
        };
        
        AudioClip clip = store.GetRandomClip();
        audioSource.PlayOneShot(clip);
    }
}
