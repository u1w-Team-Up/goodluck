using UnityEngine;

internal sealed class UserPrefs
{
    private const string EndingKey = "u.ek";

    public static int ClearEnding()
    {
        return PlayerPrefs.GetInt(EndingKey, 0);
    }

    public static void SetClearEnding(int value)
    {
        PlayerPrefs.SetInt(EndingKey, value);
    }

    private const string SkipStepKey = "u.ssk";

    public static bool ShouldSkipSteps()
    {
        return PlayerPrefs.GetInt(SkipStepKey, -1) == 1;
    }

    public static void SetSkipSteps()
    {
        PlayerPrefs.SetInt(SkipStepKey, 1);
    }

    public static void AttendSteps()
    {
        PlayerPrefs.SetInt(SkipStepKey, 0);
    }

    private const string LoopCountKey = "u.lc";

    public static int GetLoopCount()
    {
        return PlayerPrefs.GetInt(LoopCountKey, 0);
    }

    public static void IncLoopCount()
    {
        int count = GetLoopCount();
        PlayerPrefs.SetInt(LoopCountKey, count + 1);
    }

    private const string PrefixViewedPageCharaKey = "u.vpc.";

    private static string GetViewedPageCharaKey(string name)
    {
        return PrefixViewedPageCharaKey + name;
    }

    internal static PageAndCharacterIndex GetViewed(string scenarioName)
    {
        string text = PlayerPrefs.GetString(GetViewedPageCharaKey(scenarioName), "0,-1");
        return PageAndCharacterIndex.Deselialize(text);
    }

    internal static void SetViewed(string scenarioName, int pageIndex, int characterIndex)
    {
        string key = GetViewedPageCharaKey(scenarioName);
        string text = PageAndCharacterIndex.Selialize(new PageAndCharacterIndex(pageIndex, characterIndex));
        PlayerPrefs.SetString(key, text);

#if UNITY_EDITOR
        Debug.Log($"[Save] key:{key}, string:{text}");
#endif
    }
}

[System.Serializable]
public readonly struct PageAndCharacterIndex
{
    public readonly int PageIndex;
    public readonly int CharacterIndex;

    public PageAndCharacterIndex(int pageIndex, int characterIndex)
    {
        PageIndex = pageIndex;
        CharacterIndex = characterIndex;
    }

    public static PageAndCharacterIndex Deselialize(string text)
    {
        string[] vs = text.Split(',');
        return new PageAndCharacterIndex(int.Parse(vs[0]), int.Parse(vs[1]));
    }

    public static string Selialize(PageAndCharacterIndex v)
    {
        return v.PageIndex + "," + v.CharacterIndex;
    }
}