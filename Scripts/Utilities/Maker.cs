using TMPro;

using UnityEngine;
using UnityEngine.UI;

public sealed class Maker : MonoBehaviour
{
    [SerializeField] public Button Button;
    [SerializeField] public TextMeshProUGUI PostLabel;
    [SerializeField] public TextMeshProUGUI HandleLabel;
    [SerializeField] public string Uri;

    private void Reset()
    {
        Button = GetComponent<Button>();

        foreach (var label in GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (label.name.Contains("Post"))
            {
                PostLabel = label;
                continue;
            }
            
            if (label.name.Contains("Handle"))
            {
                HandleLabel = label; 
                continue;
            }
        }
    }
}
