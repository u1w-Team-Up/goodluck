using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using NRandom;
using NRandom.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RewardCanvas : CanvasBase
{
    [SerializeField] private CanvasGroup buttonCanvasGroup; 
    [SerializeField] private Button submitButton;

    [SerializeField] private DecoyPartView decoyHead;
    [SerializeField] private DecoyPartView decoyBody;
    [SerializeField] private DecoyPartView decoyLeftArm;
    [SerializeField] private DecoyPartView decoyRightArm;
    [SerializeField] private DecoyPartView decoyLeftLeg;
    [SerializeField] private DecoyPartView decoyRightLeg;

    [SerializeField] private RectTransform[] cutInParents;

    [SerializeField] private RectTransform decoyRoot;

    [SerializeField] private DamageVoicePlayer damageVoicePlayer;

    [SerializeField] private RewardCutIn battleCutInPrefab;
    [SerializeField] private RewardCutIn normalCutInPrefab;

    [SerializeField] private ParticleSystem flashParticlePrefab;
    
    [SerializeField] private AudioSource heartBeatSlowly;
    [SerializeField] private AudioSource heartBeatHighest;

    private readonly List<RewardCutIn> _rewardCutIns = new();

    private SelectId _selectId = SelectId.None;

    private GameData _gameData;
    private PlayerData _playerData;
    private MapData _mapData;
    private SimpleBGMManager _bgmManager;

    private void Reset()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        submitButton = GetComponentsInChildren<Button>().First(x => x.name.Contains("Enter"));
        
        buttonCanvasGroup = GetComponentsInChildren<CanvasGroup>().First(x => x.name.Contains("Button"));
        
        var decoys = GetComponentsInChildren<DecoyPartView>().ToArray();
        decoyHead = decoys[0];
        decoyBody = decoys[1];
        
        // いつもの並びと逆.
        decoyRightArm = decoys[2]; 
        decoyLeftArm = decoys[3]; 
        decoyRightLeg = decoys[4];
        decoyLeftLeg = decoys[5];

        var rectTransforms = GetComponentsInChildren<RectTransform>();
        
        // 処理が重たいかもね.
        cutInParents = rectTransforms
            .Where(x => x.name.Contains("CutInParent"))
            .ToArray();

        decoyRoot = rectTransforms.First(x => x.name == "Decoy");

        damageVoicePlayer = GetComponentInChildren<DamageVoicePlayer>();
    }

    public void Inject(GameData gameData, PlayerData playerData, MapData mapData, SimpleBGMManager bgmManager)
    {
        _gameData = gameData;
        _playerData = playerData;
        _mapData = mapData;
        _bgmManager = bgmManager;
    }
    
    protected override void OnAwake()
    {
        base.OnAwake();

        submitButton.onClick.AddListener(()=> _selectId = SelectId.Submit);
    }

    public async UniTask ShowAsync(CancellationToken ct)
    {
        buttonCanvasGroup.Hide();
        RefreshDecoyPartViews();

        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);
    }

    public override async UniTask<Parcel> MainAsync(Parcel parcel, CancellationToken ct)
    {
        if (_playerData.ShouldHeartBeatHighest())
        {
            heartBeatHighest.FadeInVolume();
            heartBeatHighest.Play();
        }
        
        // BattleBGMの再生.
        _bgmManager.PlayAt(_gameData.CriticalRatio.CurrentValue > 0 ? BGMId.BattleB : BGMId.BattleA);
        
        NormalCutIn(CutInFaceId.Begin, "任務開始");
        await UniTask.WaitForSeconds(1.0f, cancellationToken: ct);
        
        PointData areaPoint = _mapData.Current;
        float damageRatio = _gameData.GetDamageRatio(areaPoint, _playerData);

        bool wasDamaged = false;
        
        float damageTryCount = damageRatio <= 0 ? 0 : GameData.DamageTryMax;

        if (areaPoint.IsAddDoubleDamageChance && damageTryCount != 0)
        {
            damageTryCount += 2;
        }
        
        IRandom random = RandomEx.Shared;

        var list = new List<PartId>();

        while (damageTryCount > 0)
        {
            damageTryCount--;
            
            float randomValue = random.NextFloat(1.0f);
            bool hit = randomValue <= damageRatio;
            
            #if UNITY_EDITOR
            Debug.Log($"{randomValue} <= {damageRatio} = {hit}");
            #endif
            
            // 発射演出.
            await SpawnActionAsync(ct);

            if (hit)
            {
                wasDamaged = true;
                list.Clear();
                _playerData.GetLiveParts(list);

                PartId hitPart = list.RandomElement();
                _playerData.Damage(_gameData, hitPart, WhenBattleCutIn, 1);
                UpdateDecoyPartViews(_playerData, hitPart);

                int currentDamagedValue = _playerData.GetCurrentDamagedValue(hitPart);
                damageVoicePlayer.Play(currentDamagedValue);
                SEManager.Play(SEID.BarrageShot);

                if (_playerData.IsAlive && !heartBeatHighest.isPlaying && _playerData.ShouldHeartBeatHighest())
                {
                    heartBeatHighest.FadeInVolume();
                    heartBeatHighest.Play();
                }
                
                await UniTask.WaitForSeconds(1.2f, cancellationToken: ct);
            }
            else
            {
                // 攻撃を躱した.
                SEManager.Play(SEID.Dodge);
                await UniTask.WaitForSeconds(1.0f, cancellationToken: ct);
            }

            if (_playerData.IsDead)
            {
                if (heartBeatHighest.isPlaying)
                {
                    heartBeatHighest.Stop();
                }
                
                break;
            }
        }

        float criticalRatio = _gameData.CriticalRatio.CurrentValue;

        if (_playerData.IsAlive && criticalRatio > 0)
        {
            float randomValue = random.NextFloat(1.0f);
            bool isCritical = randomValue <= criticalRatio;
            Debug.Log($"{randomValue} <= {damageRatio} = {isCritical}");

            if (isCritical)
            {
                wasDamaged = true;

                await SpawnActionAsync(ct);

                SEManager.Play(SEID.Critical);

                PartId partId = _playerData.GetCriticalHitPartId(random);
                _playerData.Damage(_gameData, partId, WhenBattleCutIn, PartData.InitialHealth);

                UpdateDecoyPartViews(_playerData, partId);
                damageVoicePlayer.Play(_playerData.GetCurrentDamagedValue(partId));
                await UniTask.WaitForSeconds(2.0f, cancellationToken: ct);
            }
        }

        // 行軍の待機時間.
        await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);

        if (_playerData.IsAlive)
        {
            if (wasDamaged)
            {
                NormalCutIn(CutInFaceId.Begin, "任務完了");
            }
            else
            {
                NormalCutIn(CutInFaceId.Damage1, "無傷で任務完了");
                await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);
                
                NormalCutIn(CutInFaceId.Reward, "安心してくれるかな");
                _gameData.RegainStress(random);
            }
            await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);

            await WasGrantGift();
        }
        else
        {
            NormalCutIn(CutInFaceId.Critical, "任務失敗");
            await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);
            parcel.Ending = Ending.Death;
        }

        if (heartBeatHighest.isPlaying)
        {
            heartBeatHighest.FadeOutVolume();
        }

        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);
        
        while (!ct.IsCancellationRequested)
        {
            _selectId = SelectId.None;
            
            await UniTask.Yield(ct);

            if (_selectId == SelectId.Submit)
            {
                parcel.Value = (int)SelectId.Submit;
                break;
            }
        }

        // bgmを止める.
        int bgmIndex = _bgmManager.StopCurrent();
        
        if (!_playerData.IsDead)
        {
            await CanvasFadeOut();
            return parcel;
        }

        int type = _gameData.GetRandomRebornChance(random);
        
        if (type <= 0)
        {
            await CanvasFadeOut();
            return parcel;
        }

        await CanvasFadeOut();

        await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);

        heartBeatSlowly.FadeInVolume();
        heartBeatSlowly.Play();

        await UniTask.WaitForSeconds(5.0f, cancellationToken: ct);

        if (type != 2)
        {
            heartBeatSlowly.FadeOutVolume();
            await CanvasFadeOut(speakable:false);
            return parcel;
        }

        await UniTask.WaitForSeconds(5.0f, cancellationToken: ct);

        // 復活.
        SEManager.Play(SEID.Soul);
        _playerData.Reborn(_gameData);
        heartBeatSlowly.FadeOutVolume();
        await UniTask.WaitForSeconds(2.5f, cancellationToken: ct);

        buttonCanvasGroup.Hide();
        canvas.enabled = true;
        await canvasGroup.FadeInAsync(cancellationToken: ct);

        NormalCutIn(CutInFaceId.Damage3, "息を吹き返した");
        await UniTask.WaitForSeconds(2.0f, cancellationToken: ct);

        // バトルA BGMの再生.
        _bgmManager.PlayAt(BGMId.BattleA);
        
        NormalCutIn(CutInFaceId.Reward, "……いま帰るよ");
        await UniTask.WaitForSeconds(1.0f, cancellationToken: ct);

        // 報酬を受け取る.
        await WasGrantGift();

        await buttonCanvasGroup.FadeInAsync(cancellationToken: ct);

        _selectId = SelectId.None;
        await UniTask.WaitUntil(() => _selectId == SelectId.Submit, cancellationToken: ct);

        // エンディングフラグをリセット.
        parcel.Ending = Ending.None;

        // bgmを止める.
        _bgmManager.StopCurrent();

        await CanvasFadeOut();
        return parcel;
        
        async Task CanvasFadeOut(bool speakable = true)
        {
            if (speakable)
            {
                SEID seId = _mapData.IsStoryClear ? SEID.Tap : SEID.Sally;   
                SEManager.Play(seId);
            }
            
            await canvasGroup.FadeOutAsync(cancellationToken: ct);
            canvas.enabled = false;
            
            // カットインを始末.
            foreach (RewardCutIn cutIn in _rewardCutIns)
            {
                Destroy(cutIn.gameObject);
            }
            _rewardCutIns.Clear();
        }
        
        async UniTask WasGrantGift()
        {
            int gift = _mapData.SalaryRp.CurrentValue;
            if (areaPoint.IsBigBonus)
            {
                int bonus = gift * 2;
                NormalCutIn(CutInFaceId.Reward, $"特別報奨{bonus}$が支給された");
                _gameData.AddMoney(bonus);
                SEManager.Play(SEID.Bonus);
                await UniTask.WaitForSeconds(2.0f, cancellationToken: ct);
            }
            if (areaPoint.IsRegularBonus)
            {
                int bonus = Mathf.FloorToInt(gift * 0.5f);
                NormalCutIn(CutInFaceId.Reward, $"報奨{bonus}$が支給された");
                _gameData.AddMoney(bonus);
                SEManager.Play(SEID.Bonus);
                await UniTask.WaitForSeconds(1.5f, cancellationToken: ct);
            }

            _gameData.AddMoney(gift);
            NormalCutIn(CutInFaceId.Reward, $"基本給{gift}$を受け取った！");
            SEManager.Play(SEID.Reward);
            await UniTask.WaitForSeconds(0.8f, cancellationToken: ct);
        }
    }

    private async Task SpawnActionAsync(CancellationToken ct)
    {
        SEManager.Play(SEID.SingleShot);
        SpawnFlashParticle();

        // デコイ揺れ.
        LMotion.Shake.Create(0, 15f, 0.3f).WithDelay(0.15f).BindToLocalEulerAnglesZ(decoyRoot);
        await UniTask.WaitForSeconds(0.2f, cancellationToken: ct);
    }

    private void NormalCutIn(CutInFaceId faceId, string message)
    {
        CreateCutIn(faceId, message, normalCutInPrefab);
    }

    private void WhenBattleCutIn(CutInFaceId faceId, string message)
    {
        // カットインの表示を遅らせる.
        UniTask.WaitForSeconds(0.12f, cancellationToken: destroyCancellationToken).ContinueWith(
            () => CreateCutIn(faceId, message, battleCutInPrefab))
            .Forget();
    }
    
    private void SpawnFlashParticle()
    {
        ParticleSystem flashParticle = Instantiate(flashParticlePrefab, gameObject.transform);
        flashParticle.Play();
        Destroy(flashParticle.gameObject, 1.0f);
    }
    
    private void CreateCutIn(CutInFaceId faceId, string message, RewardCutIn prefab)
    {
        int index = _rewardCutIns.Count;
        if (index >= cutInParents.Length)
        {
            index -= cutInParents.Length;
        }
        RewardCutIn cutIn = Instantiate(prefab, cutInParents[index]);
        cutIn.transform.localPosition = Vector3.zero;
        cutIn.Show(faceId, message, _playerData.head);

        _rewardCutIns.Add(cutIn);
    }

    private void RefreshDecoyPartViews()
    {
        const int initialHealth = PartData.InitialHealth;
        decoyHead.UpdateView(initialHealth);
        decoyBody.UpdateView(initialHealth);
        decoyLeftArm.UpdateView(initialHealth);
        decoyRightArm.UpdateView(initialHealth);
        decoyLeftLeg.UpdateView(initialHealth);
        decoyRightLeg.UpdateView(initialHealth);
    }
    
    private void UpdateDecoyPartViews(PlayerData playerData, PartId hitId)
    {
        switch (hitId)
        {
            case PartId.Head: decoyHead.UpdateView(playerData.GetHealth(hitId)); break;
            case PartId.Body: decoyBody.UpdateView(playerData.GetHealth(hitId)); break;
            case PartId.LeftArm: decoyLeftArm.UpdateView(playerData.GetHealth(hitId)); break;
            case PartId.RightArm: decoyRightArm.UpdateView(playerData.GetHealth(hitId)); break;
            case PartId.LeftLeg: decoyLeftLeg.UpdateView(playerData.GetHealth(hitId)); break;
            case PartId.RightLeg: decoyRightLeg.UpdateView(playerData.GetHealth(hitId)); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(hitId), hitId, null);
        }
    }

    private enum SelectId
    {
        None,
        Submit,
    }
}

public enum CutInFaceId : int
{
    Begin = 0,
    Damage1,
    Damage2,
    Damage3,
    Critical,
    Reward,
}