using System.Linq;
using UnityEngine;

public sealed class PartsController : MonoBehaviour
{
    [SerializeField] private GameObject[] Parts;

    private void Reset()
    {
        Parts = gameObject.GetComponentsInChildren<Transform>(true).Select(x=>x.gameObject).ToArray();
    }

    public void Modify(PartSetting partsPriset) => Modify(partsPriset.Parts);

    private void Modify(Part[] parts)
    {
        foreach (var part in parts) 
        {
            Parts.First(x => x.name == part.Name).SetActive(part.Enable);
        }
    }
}

[System.Serializable]
public sealed class PartSetting
{
    public Part[] Parts;
}