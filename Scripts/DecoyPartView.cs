using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
public class DecoyPartView : MonoBehaviour
{
    [SerializeField] private Image[] damages;

    private void Reset()
    {
        var images = GetComponentsInChildren<Image>(true).ToList();
        damages = images.Skip(1).OrderBy(x => x.name.Split("_").Last()).ToArray();
    }

    private void SetupActiveAt(int index)
    {
        for (int i = 0; i < damages.Length; i++)
        {
            damages[i].gameObject.SetActive(i == index);
        }
    }

    [Button]
    public void UpdateView(int health)
    {
        int damage = PartData.InitialHealth - health - 1;

        if (damage >= damages.Length)
        {
            damage = damages.Length - 1;
        }

        SetupActiveAt(damage);
    }
}
