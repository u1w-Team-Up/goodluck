using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

internal sealed class TitleBackgroundController : MonoBehaviour 
{
    [SerializeField] private Animator Animator;

    private void Reset()
    {
        Animator = GetComponent<Animator>();  
    }

    public async UniTask PlayAsync(int clearEnd, CancellationToken ct)
    {
        string stateName;
        if (clearEnd == 0)
        {
            stateName= "First";
        }
        else if (clearEnd == 1)
        {
            stateName = "Second";
        }
        else 
        {
            stateName = "Third";
        }
        Animator.Play(stateName);

        await Animator.UntilStateAsync(stateName, ct);
    }
}
