using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
public class HumanPartView : MonoBehaviour
{
    [SerializeField] private Image[] levels;

    [SerializeField] private int limit = 10;

    [SerializeField] private int indexDiff = 0;
    
    private void Reset()
    {
        var images = GetComponentsInChildren<Image>(true).ToList();
        levels = images.OrderBy(x => x.name.Split("_").Last()).ToArray();
        limit = levels.Length - 2;
    }

    [Button]
    public void SetupActiveAt(int index)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].gameObject.SetActive(i == index);
        }
    }
    
    public void UpdateView(PartData data)
    {
        int index = data.IsLost ? limit + 1 : data.GetVisualLevel() + indexDiff;
        SetupActiveAt(index);
    }
}