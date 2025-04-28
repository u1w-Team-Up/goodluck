using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using NRandom;
using UnityEngine;
using UnityEngine.UI;
public class SurgeryCanvas : CanvasBase
{
    [SerializeField] private Toggle headToggle;
    [SerializeField] private Toggle bodyToggle;
    [SerializeField] private Toggle leftArmToggle;
    [SerializeField] private Toggle rightArmToggle;
    [SerializeField] private Toggle leftLegToggle;
    [SerializeField] private Toggle rightLegToggle;

    [SerializeField] private HumanPartViewDirector partViewDirector;
        
    [SerializeField] private Button surgeryButton;
    [SerializeField] private Button backButton;

    [SerializeField] private AudioSource voiceSource;
    
    [SerializeField] private StatusView statusView;
    [SerializeField] private SurgeryCostView surgeryCostView;
    
    [SerializeField] private DialogData dialogData;
    [SerializeField] private RandomClipStore voiceStore;
    private SelectId _selectedId = SelectId.None;
    private PartId _selectedPartId = PartId.Head;
    
    private GameData _gameData;
    private PlayerData _playerData;
    private MapData _mapData;
    
    public Action OnSurgery;

    private void Reset()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        var toggles = GetComponentsInChildren<Toggle>().Reverse().ToArray();
        headToggle = toggles[0];
        bodyToggle = toggles[1];
        leftArmToggle = toggles[2];
        rightArmToggle = toggles[3];
        leftLegToggle = toggles[4];
        rightLegToggle = toggles[5];

        partViewDirector = GetComponent<HumanPartViewDirector>();
        
        var buttons = GetComponentsInChildren<Button>();
        surgeryButton = buttons.First(x => x.name.Contains("Ope"));
        backButton = buttons.First(x => x.name.Contains("Return"));
        
        statusView = GetComponentInChildren<StatusView>();

        voiceSource = GetComponent<AudioSource>();
        surgeryCostView = GetComponentInChildren<SurgeryCostView>();
    }

    public void Inject(GameData gameData, PlayerData playerData, MapData mapData)
    {
        _gameData = gameData;
        _playerData = playerData;
        _mapData = mapData;
        
        surgeryCostView.Present(gameData.SurgeryCostRp);
    }
    
    protected override void OnAwake()
    {
        base.OnAwake();
        
        surgeryButton.onClick.AddListener(() => _selectedId = SelectId.Surgery);
        backButton.onClick.AddListener(() => _selectedId = SelectId.Back);
        
        headToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.Head; });
        bodyToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.Body; });
        leftArmToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.LeftArm; });
        rightArmToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.RightArm; });
        leftLegToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.LeftLeg; });
        rightLegToggle.onValueChanged.AddListener(isOn => { if (isOn) _selectedPartId = PartId.RightLeg; });
        
        statusView.SetSelected(_selectedPartId);
    }
    
    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        UpdateToggleInteractable(_playerData);
        partViewDirector.UpdateView(_playerData);
        
        SetupAndUpdateCost();

        // ダメージ部位を点灯.
        statusView.SetupDamageLights(_playerData);
        
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        while (!ct.IsCancellationRequested)
        {
            _selectedId = SelectId.None;

            var before = _selectedPartId;

            await UniTask.Yield(ct);

            if (before != _selectedPartId)
            {
                SEManager.Play(SEID.Tap);
                statusView.SetSelected(_selectedPartId);
                SetupAndUpdateCost();
            }

            if (_selectedId == SelectId.Surgery)
            {
                SEManager.Play(SEID.Open);
                parcel = await ConfirmCanvas.Instance.MainAsync(new Parcel {DialogData = dialogData}, ct);
                if (parcel.Value == 1)
                {
                    parcel.Value = (int)SelectId.Surgery;
                    parcel.PartId = _selectedPartId;

                    // 手術結果レイヤーで手術画面を隠す.
                    await PostOpeCanvas.Instance.ShowAsync(ct);

                    var cost = _gameData.SurgeryCost;
                    _gameData.SubMoney(cost);
                    surgeryButton.interactable = _gameData.IsEnoughMoney(cost);
                    var stringBuilder = new StringBuilder();
                    var partId = parcel.PartId;
                    var random = RandomEx.Shared;
                    
                    // 回復前にストレス値を計算する.
                    var health = _playerData.GetHealth(partId);
                    
                    float stress = health switch
                    {
                        2 => random.NextFloat(GameData.InjuryStressMin, GameData.InjuryStressMaximum), 
                        PartData.InitialHealth => random.NextFloat(GameData.FlawlessStressMin, GameData.FlawlessStressMaximum),
                        _ => random.NextFloat(GameData.FatalStressMin, GameData.FatalStressMaximum), 
                    };

                    _gameData.AddStress(stress);
                    
                    // 手術の実行. 内部で回復.
                    _playerData.Surgery(partId, stringBuilder);
                    partViewDirector.UpdateView(_playerData);

                    // 手術の結果の後にストレス値のログを足す.
                    stringBuilder.Append($"現在のストレス {_gameData.Stress.CurrentValue:P1} +({stress:P1})");
                    
                    // 死亡率の更新.
                    _gameData.UpdateDeathRatio(_playerData);

                    // 0.5秒後に音声を再生する.
                    voiceSource.clip = voiceStore.GetRandomClip();
                    voiceSource.time = 0;
                    voiceSource.PlayScheduled(0.5f);
                    
                    // 回復した部位を消灯.
                    statusView.SetupDamageLights(_playerData);

                    parcel.DialogData = new DialogData() { text = stringBuilder.ToString(), submit = SEID.Tap };
                    await PostOpeCanvas.Instance.MainAsync(parcel, ct);

                    if (_gameData.IsStressLimit)
                    {
                        parcel.Ending = Ending.StressLimit;
                        break;
                    }
                    
                    UpdateToggleInteractable(_playerData);
                    OnSurgery?.Invoke();
                    
                    continue;
                }
            }
            
            if (_selectedId == SelectId.Back)
            {
                SEManager.Play(SEID.Exit);
                break;
            }
        }

        await canvasGroup.FadeOutAsync(cancellationToken: ct);
        canvas.enabled = false;
        OnClose();

        return parcel;
    }
    private void SetupAndUpdateCost()
    {
        _gameData.UpdateSurgeryCost(_mapData.AreaCount, _playerData.GetPart(_selectedPartId));
        surgeryButton.interactable = _gameData.IsEnoughMoney(_gameData.SurgeryCost);
    }

    private enum SelectId
    {
        None,
        Surgery,
        Back,
    }
    
    private void UpdateToggleInteractable(PlayerData playerData)
    {
        var parts = new List<PartId>();
        playerData.GetOperableParts(parts);
        
        headToggle.interactable = IsInteractable(PartId.Head);
        bodyToggle.interactable = IsInteractable(PartId.Body);
        leftArmToggle.interactable = IsInteractable(PartId.LeftArm);
        rightArmToggle.interactable = IsInteractable(PartId.RightArm);
        leftLegToggle.interactable = IsInteractable(PartId.LeftLeg);
        rightLegToggle.interactable = IsInteractable(PartId.RightLeg);

        PartId first = parts.FirstOrDefault();

        if (parts.Count != 0 && !IsInteractable(_selectedPartId))
        {
            _selectedPartId = first;
            
            statusView.SetSelected(_selectedPartId);
            
            headToggle.isOn = first == PartId.Head;
            bodyToggle.isOn = first == PartId.Body;
            leftArmToggle.isOn = first == PartId.LeftArm;
            rightArmToggle.isOn = first == PartId.RightArm;
            leftLegToggle.isOn = first == PartId.LeftLeg;
            rightLegToggle.isOn = first == PartId.RightLeg;
        }

        return;
        bool IsInteractable (PartId partId) => parts.Contains(partId);
    }
}