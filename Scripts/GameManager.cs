using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using NRandom;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SingleCommentCanvas clearEndingCanvas;
    [SerializeField] private SingleCommentCanvas stressLimitEndingCanvas;
    [SerializeField] private SimpleBGMManager bgmManager;
    
    [SerializeField] private Button helpButton;
    
    private HomeCanvas _homeCanvas;
    private SurgeryCanvas _surgeryCanvas;
    private GameOverCanvas _gameOverCanvas;
    private RewardCanvas _rewardCanvas;
    
    private MapView _map;

    private readonly GameData _gameData = new();
    private readonly PlayerData _playerData = new();
    private readonly MapData _mapData = new();
    
    private void Awake()
    {
        _playerData.Birth();
        
        // 初回は0.0%でスタート.
        // _gameData.UpdateDeathRatio(_playerData);

        _homeCanvas = FindAnyObjectByType<HomeCanvas>();
        _homeCanvas.Inject(_mapData, _gameData);
        _surgeryCanvas = FindAnyObjectByType<SurgeryCanvas>();
        _surgeryCanvas.Inject(_gameData, _playerData, _mapData);
        _surgeryCanvas.OnSurgery += () => _homeCanvas.UpdateHumanPartView(_playerData);
        _gameOverCanvas = FindAnyObjectByType<GameOverCanvas>();
        _gameOverCanvas.Inject(_gameData, _playerData, _mapData);
        _rewardCanvas = FindAnyObjectByType<RewardCanvas>();
        _rewardCanvas.Inject(_gameData, _playerData, _mapData, bgmManager);

        var ct = destroyCancellationToken;
        MoneyAmount moneyAmount = FindAnyObjectByType<MoneyAmount>();
        _gameData.Money.Subscribe(moneyAmount.SetAmount).AddTo(ct);

        _map = FindAnyObjectByType<MapView>();
        _map.Present(_mapData);


        StatusView statusViews = FindAnyObjectByType<StatusView>();
        statusViews.SetPlayerData(_playerData);
        statusViews.SetGameData(_gameData);

        helpButton.onClick.AddListener(() =>
        {
            NoticeCanvas.Instance.MainAsync(new Parcel(), ct).Forget();
        });

        foreach (DamageRatioInfo x in FindObjectsByType<DamageRatioInfo>(FindObjectsSortMode.None))
        {
            _mapData.NextIndexRp.Subscribe(v => x.UpdateDamageRatioAt(v, _mapData, _gameData, _playerData)).AddTo(destroyCancellationToken);
            _surgeryCanvas.OnSurgery += () => x.UpdateDamageRatioAt(_mapData.NextIndexRp.CurrentValue, _mapData, _gameData, _playerData); 
        }

        foreach (AudioChorusRate rate in FindObjectsByType<AudioChorusRate>(FindObjectsSortMode.None))
        {
            rate.SetupRate(_playerData.head);
            _surgeryCanvas.OnSurgery += () => rate.SetupRate(_playerData.head); 
        }
    }
    
    private void Start()
    {
        MainAsync(destroyCancellationToken).Forget();
    }

    private async UniTask MainAsync(CancellationToken ct)
    {
        var parcel = new Parcel() {ShouldTutorial = true};

        PlayMenuBgm();

        while (!ct.IsCancellationRequested)
        {
            _homeCanvas.UpdateHumanPartView(_playerData);
            parcel = await _homeCanvas.MainAsync(parcel, ct);

            if (parcel.Value == 1)
            {
                // Main BGMの停止.
                bgmManager.StopCurrent();
                GameInfo.Instance.Close();

                // 少し待つ.
                await UniTask.WaitForSeconds(1.2f, cancellationToken: ct);

                _mapData.Move(parcel.Direction);

                await _rewardCanvas.ShowAsync(ct);
                await _rewardCanvas.MainAsync(parcel, ct);

                if (!parcel.ShouldEnding)
                {
                    GameInfo.Instance.Open();
                }
            }
            else if (parcel.Value == 2)
            {
                parcel = await _surgeryCanvas.MainAsync(parcel, ct);
                if (parcel.ShouldEnding == false)
                {
                    continue;
                }
            }

            if (!parcel.ShouldEnding)
            {
                _gameData.NextDay();
                _gameData.UpdateDeathRatio(_playerData);
                
                if (_mapData.IsStoryClear)
                {
                    parcel.Ending = Ending.AreaComplete;
                }
                else if (_mapData.IsEnd)
                {
                    DoMapCreate();
                    PlayMenuBgm();
                }
                else
                {
                    PlayMenuBgm();
                }
            }

            if (parcel.ShouldEnding)
            {
                switch (parcel.Ending)
                {
                    case Ending.Death:
                        await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);
                        break;
                    case Ending.AreaComplete:
                        {
                            SingleCommentCanvas canvas = clearEndingCanvas;
                            
                            await UniTask.WaitForSeconds(1.0f, cancellationToken: ct);
                            
                            await canvas.ShowAsync(ct);
                            bgmManager.PlayAt(BGMId.Clear);
                            await UniTask.WaitForSeconds(1.5f, cancellationToken: ct);
                            
                            await canvas.MainAsync(parcel, ct);
                            await canvas.HideAsync(ct);
                            break;
                        }
                    case Ending.StressLimit:
                        {
                            SingleCommentCanvas canvas = stressLimitEndingCanvas;

                            await canvas.ShowAsync(ct);
                            await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);
                            await canvas.MainAsync(parcel, ct);
                            await canvas.HideAsync(ct);
                            break;
                        }
                }

                parcel = await _gameOverCanvas.MainAsync(parcel, ct);

                if (parcel.Result)
                {
                    if (parcel.Value == 1)
                    {
                        DoMapCreate();
                        PlayMenuBgm();

                        // エンディングフラグのリセット.
                        parcel.Ending = Ending.None;
                        continue;
                    }
                    
                    await SceneTransitionManager.LoadSceneAsync(1);
                    return;
                }
                
                await SceneTransitionManager.LoadSceneAsync(0);
                return;
            }
        }

        
        void DoMapCreate()
        {
            _mapData.Create();
            _mapData.IncrementAreaCount();
            _mapData.UpdateSalary();
            _map.UpdatePoints(_mapData);
            _gameData.RegainStress(RandomEx.Shared);
        }
    }
    private void PlayMenuBgm()
    {
        // BGMを消音.
        bgmManager.StopCurrent();

        // BGMの変更.
        var id = _mapData.AreaCount switch
        {
            1 => BGMId.MenuA,
            2 => BGMId.MenuB,
            3 => BGMId.MenuC,
            _ => BGMId.MenuD
        };
        bgmManager.PlayAt(id);
    }
}

public enum Direction
{
    Down,
    Right,
}

public enum PartId
{
    Head,
    Body,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
}

public enum ContentId
{
    Intelligence,
    Stamina,
    Strength,
    Dexterity,
    Endurance,
    Agility,
}