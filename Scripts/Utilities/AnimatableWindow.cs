using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class AnimatableWindow : MonoBehaviour
{
    [SerializeField] private Animator Animator;

    private void Reset()
    {
        Animator = GetComponent<Animator>();
    }

    public void Open()
    {
        Animator.Play("Open");
    }

    public void Close()
    {
        Animator.Play("Close");
    }
}
