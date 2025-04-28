using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class MapView : MonoBehaviour
{
    [SerializeField] private PointView[] pointViews;
    [SerializeField] private GameObject nextNotice;
    
    [SerializeField] private MapTitle title;
    private void Reset()
    {
        pointViews = GetComponentsInChildren<PointView>();
        title = GetComponentInChildren<MapTitle>();
    }

    public void Present(MapData mapData)
    {
        UpdatePoints(mapData);

        var ct = destroyCancellationToken;
        mapData.IndexRp.Subscribe(UpdateIndex).AddTo(ct);
        
        mapData.AreaCountRp.Subscribe(title.SetTitle).AddTo(ct);
        title.PresentSalary(mapData.TotalBountyRp);
    }

    public void UpdatePoints(MapData mapData)
    {
        for (int index = 0; index < mapData.Points.Length; index++)
        {
            PointData data = mapData.Points[index];
            PointView pointView = pointViews[index];
            pointView.Setup(data);
        }

        nextNotice.SetActive(!mapData.IsStoryFinalArea);
    }

    private void UpdateIndex(int index)
    {
        for (int i = 0; i < pointViews.Length; i++)
        {
            PointView point = pointViews[i];
            point.SetToggle(i == index);
        }
    }

    public void SetSelected(int index)
    {
        for (int i = 0; i < pointViews.Length; i++)
        {
            PointView point = pointViews[i];
            point.SetIsSelected(i == index);
        }
    }

    public void SetToggleAt(int index, bool value)
    {
        pointViews[index].SetToggle(value);
    }
}