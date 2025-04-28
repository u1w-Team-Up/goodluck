using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIUVScroller : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.1f, 0.0f);
    public Vector2 stepSize = new Vector2(0.01f, 0.01f);

    private RawImage rawImage;
    private Rect uvRect;
    private Vector2 scrollPosition;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        uvRect = rawImage.uvRect;
        scrollPosition = new Vector2(uvRect.x, uvRect.y);
    }

    void Update()
    {
        // 累積的にスクロール位置を更新
        scrollPosition.x = (scrollPosition.x + scrollSpeed.x * Time.deltaTime) % 1f;
        scrollPosition.y = (scrollPosition.y + scrollSpeed.y * Time.deltaTime) % 1f;

        // ステップにスナップ
        uvRect.x = Mathf.Floor(scrollPosition.x / stepSize.x) * stepSize.x;
        uvRect.y = Mathf.Floor(scrollPosition.y / stepSize.y) * stepSize.y;

        rawImage.uvRect = uvRect;
    }
}
