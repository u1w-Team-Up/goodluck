using Cysharp.Threading.Tasks;
using TMPro;

using UnityEngine;
using UnityEngine.UI;


internal sealed class TextLoader : MonoBehaviour 
{
    [SerializeField] private TextAsset TextAsset;

    [SerializeField] private TextMeshProUGUI Label;

    [SerializeField] private ScrollRect Scroll;

    private void Reset()
    {
        Label = GetComponentInChildren<TextMeshProUGUI>();
        Scroll = GetComponentInChildren<ScrollRect>();
    }

    private async void Start()
    {
        Scroll.verticalNormalizedPosition = 1;
        Label.SetText(TextAsset.text);
        Label.ForceMeshUpdate();

        await UniTask.Yield();

        Scroll.verticalNormalizedPosition = 1;
    }
}
