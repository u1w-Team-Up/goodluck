using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class HomeCanvas : CanvasBase
{
    [SerializeField] private Button surgeryButton;

    [SerializeField] private Button sortieButton;

    [SerializeField] private Toggle rightToggle;
    [SerializeField] private Toggle downToggle;

    [SerializeField] private DeathRatioInfo deathRatio;
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    
    [SerializeField] private MapView mapView;
    [SerializeField] private HumanPartViewDirector partViewDirector;
    
    private SelectId _selectedId = SelectId.None;
    private Direction _selectedDirection = Direction.Right;
    
    [SerializeField] private MainComments comments;
    [SerializeField] private DialogData dialogData;

    private MapData _mapData;
    private GameData _gameData;
    
    protected override void OnAwake()
    {
        base.OnAwake();
        
        surgeryButton.onClick.AddListener(() => _selectedId = SelectId.Surgery);
        sortieButton.onClick.AddListener(() => _selectedId = SelectId.Sortie);
        
        rightToggle.onValueChanged.AddListener(isOn => { if (isOn) OnSelectedDirection(Direction.Right); });
        downToggle.onValueChanged.AddListener(isOn => { if (isOn) OnSelectedDirection(Direction.Down); });
    }
    
    public void Inject(MapData mapData, GameData gameData)
    {
        _mapData = mapData;
        _gameData = gameData;
        
        gameData.CriticalRatio.Subscribe(deathRatio.SetValue).AddTo(destroyCancellationToken);
    }
    
    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        // トグルボタン初期化.
        rightToggle.interactable = _mapData.CanMove(Direction.Right);
        downToggle.interactable = _mapData.CanMove(Direction.Down);
        
        if (!rightToggle.interactable)
        {
            downToggle.isOn = true;
            OnSelectedDirection(Direction.Down);
        }
        else if (!downToggle.interactable)
        {
            rightToggle.isOn = true;
            OnSelectedDirection(Direction.Right);
        }
        else
        {
            OnSelectedDirection(_selectedDirection);
        }
        
        buttonCanvasGroup.Hide();
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        // メッセージ.
        parcel.CommentList = comments[_gameData.GetCommentIndex(comments.GetLength())];
        await MessageCanvas.Instance.MainAsync(parcel, ct);

        // ボタン表示.
        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);

        if (parcel.ShouldTutorial)
        {
            parcel.ShouldTutorial = false;
            await NoticeCanvas.Instance.MainAsync(parcel, ct);
        }

        GameInfo.Instance.Open();

        while (!ct.IsCancellationRequested)
        {
            _selectedId = SelectId.None;

            var before = _selectedDirection;
            await UniTask.Yield(ct);

            if (before != _selectedDirection)
            {
                SEManager.Play(SEID.Tap);
            }
            
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

            if (_selectedId == SelectId.Surgery)
            {
                SEManager.Play(SEID.Tap);
                parcel.Value = (int)SelectId.Surgery;
                break;
            }
        }

        MessageCanvas.Instance.Hide();

        await canvasGroup.FadeOutAsync(cancellationToken: ct);
        OnClose();

        return parcel;
    }
    
    private void OnSelectedDirection(Direction direction)
    {
        _selectedDirection = direction;
        var nextIndex = _mapData.Calculate(direction);
        _mapData.UpdateTotalBounty(nextIndex);
        mapView.SetSelected(nextIndex);
        
    }
    
    public void UpdateHumanPartView(PlayerData data)
    {
        partViewDirector.UpdateView(data);
    }

    private enum SelectId
    {
        None,
        Sortie,
        Surgery,
    }

    [System.Serializable]
    public class MainComments
    {
        public CommentList[] commentList;
        
        public CommentList this[int index]
        {
            get => commentList[index];
            set => commentList[index] = value;
        }
        
        public int GetLength()
        {
            return commentList.Length;
        }
    }
}
