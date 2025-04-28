using Alchemy.Inspector;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MessageLabel : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI _label;
    
#if UNITY_EDITOR 
    private void Reset()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }
#endif

    [Button]
    public void SetText(string text)
    {
        _label.text = text;
    }
}
