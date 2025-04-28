using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SortieCanvas : CanvasBase
{
    [SerializeField] private Button sortieButton;
    [SerializeField] private Button backButton;

    [SerializeField] private Toggle RightToggle;
    [SerializeField] private Toggle DownToggle;

    [SerializeField] private DialogData dialogData;

    private SelectId _selectedId = SelectId.None;
    private Direction _selectedDirection = Direction.Right;
    
    private MapData _mapData;

    public void Inject(MapData mapData)
    {
        _mapData = mapData;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        sortieButton.onClick.AddListener(() => _selectedId = SelectId.Sortie);
        backButton.onClick.AddListener(() => _selectedId = SelectId.Back);
        
        RightToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedDirection = Direction.Right; });
        DownToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedDirection = Direction.Down; });
    }
    
    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        RightToggle.interactable = _mapData.CanMove(Direction.Right);
        DownToggle.interactable = _mapData.CanMove(Direction.Down);

        if (RightToggle.interactable)
        {
            RightToggle.isOn = true;
        }
        else
        {
            DownToggle.isOn = true;
        }
        
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        while (!ct.IsCancellationRequested)
        {
            _selectedId = SelectId.None;
            await UniTask.Yield(ct);

            if (_selectedId == SelectId.Sortie)
            {
                SEManager.Play(SEID.Open);
                parcel = await ConfirmCanvas.Instance.MainAsync(new Parcel() { DialogData = dialogData }, ct);
                if (parcel.Value == 1)
                {
                    parcel.Value = (int)SelectId.Sortie;
                    parcel.Direction = _selectedDirection;
                    break;
                }
            }
            
            if (_selectedId == SelectId.Back)
            {
                SEManager.Play(SEID.Exit);
                parcel.Value = (int)SelectId.Back;
                break;
            }
        }

        await canvasGroup.FadeOutAsync(cancellationToken: ct);
        OnClose();

        return parcel;
    }

    private enum SelectId
    {
        None,
        Sortie,
        Back,
    }
}