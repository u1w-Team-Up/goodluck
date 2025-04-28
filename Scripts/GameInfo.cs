using UnityEngine;
public class GameInfo : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public static GameInfo Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void Open()
    {
        animator.Play("FallIn");
    }
    
    public void Close()
    {
        animator.Play("FlowOut");
    }
}
