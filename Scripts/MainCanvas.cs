using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : CanvasBase
{
    [SerializeField] private Button sallyButton;
    [SerializeField] private Button surgeryButton;

    private SelectId _selectedId = SelectId.None;
    
    [SerializeField] private MainComments comments;
    
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    
    private MapData _mapData;
    
    protected override void OnAwake()
    {
        base.OnAwake();
        
        sallyButton.onClick.AddListener(() => _selectedId = SelectId.Sally);
        surgeryButton.onClick.AddListener(() => _selectedId = SelectId.Surgery);
    }
    
    public void Inject(MapData mapData)
    {
        _mapData = mapData;
    }
    
    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        canvas.enabled = true;
        buttonCanvasGroup.Hide();
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        // メッセージ.
        await MessageCanvas.Instance.MainAsync(new Parcel() { CommentList = comments[0] }, ct);

        // ボタン表示.
        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);
        
        while (!ct.IsCancellationRequested)
        {
            _selectedId = SelectId.None;
            await UniTask.Yield(ct);

            if (_selectedId == SelectId.Sally)
            {
                SEManager.Play(SEID.Tap);
                parcel.Value = (int)SelectId.Sally;
                break;
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

    private enum SelectId
    {
        None,
        Sally,
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
    }
}


public abstract class CanvasBase : MonoBehaviour
{
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.Hide();
        canvas.enabled = false;
        
        OnAwake();
    }

    protected virtual void OnAwake(){}

    protected virtual void OnClose()
    {
        canvas.enabled = false;
        canvasGroup.Hide();
    }
    
    public abstract UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct);
}

public class Parcel
{
    public bool Result;
    public int Value;
    public bool ShouldTutorial = false;
    public CommentList CommentList;
    public PartId PartId { get; set; }
    public Direction Direction { get; set; }
    public DialogData DialogData { get; set; }
    public Ending Ending { get; set; }
    public bool ShouldEnding => Ending != Ending.None;
    
    public PlayerData PlayerData { get; set; }
    public GameData GameData { get; set; }
}

public enum Ending
{
    None,
    Death,
    AreaComplete,
    StressLimit,
}

[System.Serializable]
public class DialogData
{
    public Sprite portrait;
    
    [TextArea]
    public string text = "本当に手術するよ…？";
    public AudioClip clip;

    public string submitText = "おねがい";
    public string cancelText = "ちょっとまって";

    public SEID submit = SEID.Surgery;
}