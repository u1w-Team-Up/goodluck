using UnityEngine;

public static class AudioHelper
{
    public const string NAMEBGM    = "MusicVolume";
    public const string NAMESE     = "SfxVolume";

    public const float MINDB = -80.0f;
    public const float MAXDB = 0.0f;

    /// <summary>
    /// デシベル変換
    /// 0, 100, 1000パーセント→MIX, 0, MAXのデシベル
    /// </summary>
    /// <param name="pa"></param>
    /// <returns></returns>
    public static float PaToDb(float pa)
    {
        pa *= 0.01f;
        pa = Mathf.Clamp(pa, 0.0001f, 10f);
        return 20f * Mathf.Log10(pa);
    }

    /// <summary>
    /// 音圧変換
    /// MIN, 0, MAXのデシベル→0, 1, 10の音圧.
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static float DbTo01(float db)
    {
        db = Mathf.Clamp(db, MINDB, MAXDB);
        return Mathf.Pow(10f, db / 20f);
    }

    /// <summary>
    /// 音圧変換
    /// MIN, 0, MAXのデシベル→0, 100, 1000のパーセント.
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static float DbToPacent(float db)
    {
        return DbTo01(db) * 100;
    }
}
