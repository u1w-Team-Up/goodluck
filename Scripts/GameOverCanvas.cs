using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Core;
using TweetWithScreenShot;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR && UNITY_WEBGL
using System;
using Unityroom.Client;
#endif

public class GameOverCanvas : CanvasBase
{
    [SerializeField] private TypewriterCore typewriter;

    [SerializeField] private CanvasGroup buttonCanvasGroup; 
    [SerializeField] private Button postButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button endlessModeButton;

    [SerializeField] private DialogData dialogData;
    
    private SelectId _selectId = SelectId.None;
    
    private GameData _gameData;
    private PlayerData _playerData;
    private MapData _mapData;

    public void Inject(GameData gameData, PlayerData playerData, MapData mapData)
    {
        _gameData = gameData;
        _playerData = playerData;
        _mapData = mapData;
    }
    
    protected override void OnAwake()
    {
        base.OnAwake();

        postButton.onClick.AddListener(()=> _selectId = SelectId.Post);
        titleButton.onClick.AddListener(()=> _selectId = SelectId.Title);
        retryButton.onClick.AddListener(()=> _selectId = SelectId.Retry);
        endlessModeButton.onClick.AddListener(()=> _selectId = SelectId.EndlessMode);
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        buttonCanvasGroup.Hide();

        StringBuilder sb = new();
        string post;

        if (parcel.Ending == Ending.StressLimit)
        {
            SEManager.Play(SEID.GameOver);
            string areaText = _mapData.GetAreaText();
            sb.Append($"\u25a0任務記録\n\u3000{areaText}地点にて出撃待機中\n\n\u25a0稼働記録\n\u3000出撃準備完了\n\u3000しかしドクター不在により中止\n\n\u25a0備考\n\u3000医療記録が断絶\n\u3000照合不能な署名を確認");
            post = "あの子はちゃんと見送ってくれてたのに。\n私は、気づいてやれなかった。\n";
        }
        else if (parcel.Ending == Ending.AreaComplete)
        {
            sb.Append("\u25a0任務記録 \n\u3000全戦場に到達\n\n\u25a0稼働時間\n\u3000全戦闘行動を完遂\n\n\u25a0帰還状態\n\u3000正常動作を確認\n\u3000以降の再配置は保留中");
            post = "生き延びた。ただそれだけで、全てが報われた気がした。\n";
        }
        else // if (parcel.Ending == Ending.Death)
        {
            SEManager.Play(SEID.GameOver);
            int day = _gameData.Day.CurrentValue + 1;
            sb.Append($"\u25a0任務記録\n\u3000稼働{day}日目、戦闘中に頭部破損\n\n\u25a0廃棄区分\n\u3000損傷範囲：脳幹\n\u3000廃案の決定");
            post = $"稼働{day}日目。戦闘中の頭部破損により作戦終了。\n";
        }
        
        endlessModeButton.gameObject.SetActive(parcel.Ending == Ending.AreaComplete);
        
        typewriter.ShowText(sb.ToString());

        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

#if !UNITY_EDITOR && UNITY_WEBGL
        try
        {
            UnityroomClient client = new()
            {
                // HmacKeyにAPIキー画面から取得したHMAC認証用キーを渡します
                HmacKey = "p3ju7TPj0ao9fuSTkK9lY9G0BHn7c9uxLTuhu0dfBsM9o1Ztjv6s/1L0QRZEsak0rF0iIT4vICt4b50FVZ5nSA=="
            };
            await client.Scoreboards.SendAsync(new SendScoreRequest() { ScoreboardId = 1, Score = _gameData.Day.CurrentValue }, ct);
            await client.Scoreboards.SendAsync(new SendScoreRequest() { ScoreboardId = 2, Score = _gameData.Money.CurrentValue }, ct);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
#endif

        while (typewriter.isShowingText)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (!Input.anyKeyDown) continue;
            
            SEManager.Play(SEID.Tap);
            if (typewriter.isShowingText)typewriter.SkipTypewriter();
        }

        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);
        
        while (!ct.IsCancellationRequested)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (_selectId == SelectId.Post)
            {
                SEManager.Play(SEID.Tap);
                buttonCanvasGroup.Hide();
                await TweetManager.TweetWithScreenShot(post).ToUniTask(this);
                buttonCanvasGroup.Show();
            }
            
            if (_selectId == SelectId.Title)
            {
                SEManager.Play(SEID.Tap);

                parcel.DialogData = dialogData;
                await ConfirmCanvas.Instance.MainAsync(parcel, ct);
                if (parcel.Value == 2)
                {
                    continue;
                }
                parcel.Result = false;
                
                break;
            }
            
            if (_selectId == SelectId.Retry)
            {
                SEManager.Play(SEID.Tap);
                parcel.Result = true;
                parcel.Value = 0;
                break;
            }

            if (_selectId == SelectId.EndlessMode)
            {
                parcel.Result = true;
                parcel.Value = 1;

                // GAMEOVERレイヤ―を隠す.
                await canvasGroup.FadeOutAsync(cancellationToken: ct);
                canvas.enabled = false;

                break;
            }
        }

        return parcel;
    }

    private enum SelectId
    {
        None,
        Post,
        Title,
        Retry,
        EndlessMode,
    }
}
