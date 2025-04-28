using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Table/Scenario", fileName = "Scenario")]
public partial class Scenario : ScriptableObject
{
    [SerializeField]
    [InlineProperty]
    internal Page[] Pages;

    [NonSerialized] private int index = 0;

    internal void OnBegin()
    {
        index = 0;
    }

    internal int GetPageIndex()
    {
        return index - 1;
    }

    internal Page GetPage()
    {
        return Pages[index];
    }

    internal int NextPage()
    {
        index++;
        return index;
    }

    internal bool ShouldNext()
    {
        return index < Pages.Length;
    }
}

public interface ICommand
{
    string Parameter { get; }
    string Label { get; }
    CommandProperty CProperty { get; }
}

[System.Serializable]
public sealed class Page : ICommand
{
    [ShowInInspector]
    [HideLabel]
    [PropertyOrder(-1)]
    public string Header
    {
        get => Text;
        set => Text = value;
    }
    
    [TextArea(1,10)]
    [FoldoutGroup("Text", VisibleIf = "IsInvalidMainCommand")]
    [HideLabel]
    public string Text;

    [HideLabel]
    [FoldoutGroup("Main Command")]
    public string Command;

    [HideLabel]
    [FoldoutGroup("Main Command")]
    [InlineProperty]
    [ShowIf("IsValidMainCommand")]
    public CommandProperty Property;

    [Space]
    [FoldoutGroup("Text")]
    [InlineProperty]
    public SubCommand[] SubCommands;

    public bool IsValidMainCommand => !string.IsNullOrEmpty(Command);
    public bool IsInvalidMainCommand => !IsValidMainCommand;

    // interfaces.
    public string Label => Command;
    public string Parameter => Text;
    public CommandProperty CProperty => Property;
    
    [FoldoutGroup("Main Command")]
    [Button("CopyTextOfCommand")]
    private void CopyTextOfCommand()
    {
        Text = Command;
    }

    [FoldoutGroup("Text")]
    [Button("WritePrinter")]
    private void WritePrinter()
    {
#if UNITY_EDITOR
        MessageLabel label = GameObject.FindFirstObjectByType<MessageLabel>();
        label.SetText(Text);
#endif
    }
}


[System.Serializable]
public sealed class CommandProperty
{
    [AssetsOnly]
    public Scenario Child;
}

[System.Serializable]
public sealed class SubCommand : ICommand
{
    public string Text;

    public string Command;

    [InlineProperty]
    [HideLabel]
    public CommandProperty Property;

    public string Label => Command;

    public CommandProperty CProperty => Property;

    public string Parameter => Text;
}

