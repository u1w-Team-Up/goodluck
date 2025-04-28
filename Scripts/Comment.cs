using UnityEngine;

[System.Serializable]
public class Comment
{
    public Sprite talker;
    [TextArea]
    public string message;
    public AudioClip clip;
}
