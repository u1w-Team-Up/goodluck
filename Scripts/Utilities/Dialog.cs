using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class Dialog : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TalkerLabel;

    [SerializeField]
    private TextProvider BodyLabel;

    [SerializeField]
    private RectTransform Locator;

    // [SerializeField] private Button DialogButton;
    // [SerializeField] private Button HideButton;

    [SerializeField]
    private CanvasGroup Canvas;

    [SerializeField]
    private GraphicRaycaster GraphicRaycaster;

    [SerializeField]
    private Animator EndNodeAnim;

    [SerializeField]
    private NewLineNode NewLineNode;

    [SerializeField]
    private Color ViewedBodyColor = Color.gray;

    [SerializeField]
    private InputActionReference _hold;

    [SerializeField]
    private InputActionReference _enter;

    [SerializeField]
    private InputActionReference _close;

    private InputAction HoldAction;
    private InputAction EnterAction;
    private InputAction CloseAction;

    internal bool ShouldPause { get; private set; }

    private bool IsViewed;

    private ButtonId SelectedId = ButtonId.None;

    private readonly List<RaycastResult> _raycastResults = new();

    private readonly float WaitCharacterNormal = 0.05f;
    private readonly float WaitCharacterViewed = 0.02f;
    private readonly float WaitLineFeedNormal = 0.0f;
    private readonly float WaitLineFeedViewed = 0.5f;

    private readonly float WaitCommanded = 0f;

    private readonly float WaitPageSkip = 0.4f;

    private string LoadKey;
    private PageAndCharacterIndex Load;

    private int CurrentPageIndex;
    private int ViewedCharacterIndex;

    public bool IsShown => Mathf.Approximately(Canvas.alpha, 1);

    private void Awake()
    {
        EnterAction = _enter.action;
        EnterAction.Enable();

        CloseAction = _close.action;
        CloseAction.Enable();

        HoldAction = _hold.action;
        HoldAction.Enable();

        Canvas.Hide();

        OnBegin();
    }

    internal void OnBegin()
    {
        ShouldPause = false;
    }

    internal async UniTask FadeInAsync(CancellationToken ct)
    {
        await Canvas.FadeInAsync(cancellationToken: ct);
    }

    internal async UniTask FadeOutAsync(CancellationToken ct)
    {
        await Canvas.FadeOutAsync(cancellationToken: ct);
    }

    private async UniTask HideAsync(CancellationToken ct)
    {
        SEManager.Play(SEID.Exit);

        _ = LMotion.Create(0, -1000f, 0.6f).WithEase(Ease.InCubic).BindToAnchoredPositionY(Locator);

        await Canvas.FadeOutAsync(0.6f, ct);
    }

    private async UniTask ShowAsync(CancellationToken ct)
    {
        SEManager.Play(SEID.Open);

        _ = LMotion.Create(-1000f, 0, 0.6f).WithEase(Ease.OutCubic).BindToAnchoredPositionY(Locator);

        await Canvas.FadeInAsync(0.7f, ct);
    }

    internal async UniTask PageAsync(Page page, Scenario scenario,
        Func<ICommand, CancellationToken, UniTask<bool>> excuseFunc, CancellationToken ct)
    {
        ViewedCharacterIndex = 0;

        BodyLabel.SetText(string.Empty);

        EndNodeAnim.gameObject.SetActive(false);
        NewLineNode.Hide();

        if (Canvas.alpha == 0)
        {
            await Canvas.FadeInAsync(cancellationToken: ct);
        }

        CommandProperty p = page.Property;

        CurrentPageIndex = scenario.GetPageIndex();

        LoadViewed(scenario);
        IsViewed = GetIsViewed();
        if (IsViewed)
        {
            BodyLabel.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(ViewedBodyColor)}>");
        }

        float delta = 0.0f;
        float border = 0.0f;
        bool showed = false;
        bool waitCr = false;
        string text = page.Text;
        var script = new Script(text, page.SubCommands);
        char lastChara = char.MinValue;

        // ちょっと待つ.
        await UniTask.WaitForSeconds(0.12f);

        while (!ct.IsCancellationRequested)
        {
            SelectedId = ButtonId.None;

            await UniTask.Yield(ct);

            if (ct.IsCancellationRequested)
            {
                break;
            }

            if (ShouldPause)
            {
                continue;
            }

            if ((CloseAction.WasCompletedThisFrame() || EnterAction.WasCompletedThisFrame()) && Canvas.alpha is 0)
            {
                await ShowAsync(ct);
            }
            else if (CloseAction.WasCompletedThisFrame() && Mathf.Approximately(Canvas.alpha, 1))
            {
                await HideAsync(ct);
                continue;
            }

            if (!Mathf.Approximately(Canvas.alpha, 1))
            {
                continue;
            }

            async UniTask nextChara()
            {
                CharaData charaData = await NextChara(excuseFunc, script, ct);
                delta = 0.0f;
                border = charaData.Wait;
                lastChara = charaData.Chara;

                waitCr = NewLineNode.Modify(charaData);
            }

            if (script.IsEndOver)
            {
                if (!showed)
                {
                    NewLineNode.Hide();

                    // ちょっと待つ.
                    await UniTask.WaitForSeconds(0.12f, cancellationToken: ct);

                    EndNodeAnim.gameObject.SetActive(true);
                    EndNodeAnim.Play("Blink");

                    showed = true;
                    continue;
                }
            }
            else if (!waitCr)
            {
                if (IsViewed
                    || lastChara is not '\n')
                {
                    delta += Time.deltaTime;
                }

                if (delta >= border)
                {
                    await nextChara();

                    if (script.IsEndOver)
                    {
                        continue;
                    }
                }
            }

            if (EnterAction.WasCompletedThisFrame() || HoldAction.GetTimeoutCompletionPercentage() >= 1f)
            {
                if (showed)
                {
                    await UniTask.Yield(ct);
                    break;
                }

                // 行末表示.
                // 改行待ちなら改行だけする.
                if (waitCr)
                {
                    await nextChara();
                    continue;
                }

                while (script.IsEndOver == false && ct.IsCancellationRequested == false)
                {
                    await nextChara();

                    if (IsViewed == false && waitCr)
                    {
                        break;
                    }
                }

                if (HoldAction.GetTimeoutCompletionPercentage() >= 1f)
                {
                    await UniTask.WaitForSeconds(WaitPageSkip, cancellationToken: ct);
                }

                continue;
            }

            if (!IsViewed)
            {
                continue;
            }

            if (HoldAction.WasPerformedThisFrame() || HoldAction.GetTimeoutCompletionPercentage() >= 1f) // 一括表示.
            {
                SEManager.Play(SEID.Tap);

                bool skip = false;

                while (HoldAction.IsPressed() && !ct.IsCancellationRequested)
                {
                    if (ShouldPause)
                    {
                        await UniTask.Yield(ct);
                        continue;
                    }

                    if (script.IsEndOver)
                    {
                        skip = true;
                        await UniTask.WaitForSeconds(WaitPageSkip, cancellationToken: ct);
                    }
                    else if (IsViewed == false && waitCr)
                    {
                        await UniTask.Yield(ct);
                    }
                    else // if (!script.IsEndOver && IsViewed && !waitCr)
                    {
                        if (IsViewed == false && waitCr == false)
                        {
                            float d = Mathf.Max(0, border - delta);
                            await UniTask.WaitForSeconds(d, cancellationToken: ct);
                        }

                        await nextChara();
                    }
                }

                if (skip)
                {
                    break;
                }

                await UniTask.Yield(ct);
            }
        }

        AutoSaveViewed();
    }

    private bool GetIsViewed()
    {
        return Load.PageIndex >= CurrentPageIndex && Load.CharacterIndex >= ViewedCharacterIndex;
    }

    private bool GetEndedViewed()
    {
        return Load.PageIndex <= CurrentPageIndex && Load.CharacterIndex <= ViewedCharacterIndex;
    }

    private void LoadViewed(Scenario scenario)
    {
        LoadKey = scenario.name;
        Load = UserPrefs.GetViewed(LoadKey);
    }

    internal void AutoSaveViewed()
    {
        if (IsViewed == false)
        {
            UserPrefs.SetViewed(LoadKey, CurrentPageIndex, ViewedCharacterIndex);
        }
    }

    private async UniTask<CharaData> NextChara(Func<ICommand, CancellationToken, UniTask<bool>> excuseFunc,
        Script script, CancellationToken ct)
    {
        char chara = default;
        float wait;
        Vector2 backPosBR = Vector2.zero;
        bool isWaitCr = false;

        if (script.TryDequeueCommand(out ICommand command))
        {
            await excuseFunc(command, ct);
            wait = WaitCommanded;
        }
        else
        {
            char character = script.Dequeue();
            BodyLabel.SetText(BodyLabel.Label.text + character);

            TMP_TextInfo textInfo = BodyLabel.Label.GetTextInfo(BodyLabel.Label.text);

            // 一個前.
            int back = textInfo.characterCount - 1 - 1;
            TMP_CharacterInfo backrInfo = textInfo.characterInfo.ElementAtOrDefault(back);
            char beforeChar = backrInfo.character;

            int index = textInfo.characterCount - 1;
            ViewedCharacterIndex = index;

            if (IsViewed && GetEndedViewed())
            {
                BodyLabel.SetText(BodyLabel.Label.text + "</color>");

                if (!script.IsEndOver)
                {
                    IsViewed = false;
                }
            }

            if (beforeChar.Equals(character) && character is '\n')
            {
                return await NextChara(excuseFunc, script, ct);
            }

            if (character.Equals('\n') && !char.IsControl(beforeChar))
            {
                wait = IsViewed ? WaitLineFeedViewed : WaitLineFeedNormal;

                backPosBR = backrInfo.bottomRight;
                isWaitCr = !IsViewed;
            }
            else
            {
                wait = IsViewed ? WaitCharacterViewed : WaitCharacterNormal;
            }

            chara = character;
        }

        return new CharaData(chara, wait, backPosBR, isWaitCr);
    }

    internal void HideMessage()
    {
        Canvas.Hide();
    }

    internal void Pause()
    {
        ShouldPause = true;
    }

    internal void Resume()
    {
        ShouldPause = false;
    }

    private void OnEnterEvent()
    {
        SelectedId = ButtonId.Dialog;
    }

    private void OnCanvelEvent()
    {
        SelectedId = ButtonId.Hide;
    }

    private enum ButtonId
    {
        None,
        Dialog,
        Hide
    }

    private class Script
    {
        private string Text;
        private int Index;

        private readonly ICommand[] SubCommands;
        private int CommandIndex;

        internal Script(string text, ICommand[] subCommands)
        {
            Text = text.Replace("\r\n", "\n");
            SubCommands = subCommands;

            Index = 0;
            CommandIndex = 0;
        }

        internal bool IsEndOver => Text.Length == Index;

        internal bool TryDequeueCommand(out ICommand command)
        {
            command = null;

            char head = Text[Index];

            if (head.Equals('{'))
            {
                int tail = Text.IndexOf('}', Index);

                if (tail == -1)
                {
                    Debug.LogError("[書式エラー] { に対応した } が見つかりません");
                }
                else
                {
                    Text = Text.Remove(Index, (1 + tail) - Index);

                    command = SubCommands[CommandIndex];
                    CommandIndex++;

                    return true;
                }
            }

            return false;
        }

        internal char Dequeue()
        {
            char result = Text[Index];
            Index++;

            return result;
        }

        internal void Insert(string text = "</color>")
        {
            Text = Text.Insert(0, text);
        }

        internal int GetIndex()
        {
            return Index - 1;
        }
    }

    public bool AnyActionThisFrame()
    {
        return EnterAction.WasCompletedThisFrame() || CloseAction.WasCompletedThisFrame();
    }
}

public readonly struct CharaData
{
    internal CharaData(char chara, float wait, in Vector2 position, bool isWaitCr)
    {
        Chara = chara;
        Wait = wait;
        RectPosLB = position;
        IsWaitCr = isWaitCr;
    }

    internal readonly char Chara { get; }
    internal readonly float Wait { get; }

    internal readonly Vector2 RectPosLB { get; }
    internal readonly bool IsWaitCr { get; }
}

public enum Talker
{
    None = 0,
    アリサ = 1,
    エル = 2
}