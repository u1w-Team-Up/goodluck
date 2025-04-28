using System.Runtime.ConstrainedExecution;
using UnityEngine;

/// <summary>
/// カメラのRectが指定した解像度の比率通りに設定するやつ.
/// 参考：https://3dunity.org/game-create-lesson/clicker-game/mobile-adjustment/.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class AspectKeeper : MonoBehaviour
{
    // 更新対象とするCamera.
    [SerializeField] private Camera Camera;

    // 想定の解像度.
    [SerializeField] private Vector2 AssumedResolution;

    private float Target => AssumedResolution.x / AssumedResolution.y;

    private float Buffer = -1;

    private void Reset()
    {
        Camera = GetComponent<Camera>();
        AssumedResolution = new(1920f, 1080f);
    }

    void Update()
    {
        var current = (float)Screen.width / Screen.height; //画面のアスペクト比

        if (current == Buffer)
        {
            return;
        }

        var rate = Target / current;
        Buffer = current;

        if (rate < 1) // 横幅を調整.
        {
            var w = rate;
            var x = 0.5f - (w / 2);
            Camera.rect = new(x, 0, w, 1);
        }
        else // 縦幅を調整.
        {
            var h = (1 / rate);
            var y = 0.5f - (h / 2);
            Camera.rect = new(0, y, 1, h);
        }
    }
}