using System;
public static class AlphabetTextHelper
{
    /// <summary>
    /// アルファベットを返す。
    /// 例えば、引数が1の時はA、2の時はB、27の時はAAとなる。
    /// 引数が不正な場合は<see cref="string.Empty"/>を返す。
    /// </summary>
    /// <param name="index">インデックス。1以上の値が有効</param>
    /// <returns>アルファベット</returns>
    public static string ToAlphabet(int index)
    {
        string alphabet = string.Empty;
        if (index < 1) return alphabet;

        while(index > 0)
        {
            // A-Zの変換を0-25にするため1を引く
            index--;
            // ASCIIではAは10進数で65
            alphabet = Convert.ToChar(index % 26 + 65) + alphabet; 
            index = index / 26;
        }

        return alphabet;
    }
}
