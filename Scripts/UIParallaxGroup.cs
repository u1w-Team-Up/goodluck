using UnityEngine;
using System.Collections.Generic;

public class UIParallaxGroup : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxUI
    {
        public RectTransform targetUI;

        [Header("Parallax Strength")]
        public float parallaxStrengthX = 10f;
        public float parallaxStrengthY = 10f;

        [Header("Max Offset")]
        public float maxOffsetX = 50f;
        public float maxOffsetY = 50f;

        [HideInInspector]
        public Vector2 initialPosition;
    }

    public List<ParallaxUI> uiElements = new List<ParallaxUI>();

    void Start()
    {
        foreach (var ui in uiElements)
        {
            if (ui.targetUI != null)
                ui.initialPosition = ui.targetUI.anchoredPosition;
        }
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 normalized = (mousePos - screenCenter) / screenCenter;

        foreach (var ui in uiElements)
        {
            if (ui.targetUI != null)
            {
                // 軸ごとにオフセットを計算
                float offsetX = Mathf.Clamp(normalized.x * ui.parallaxStrengthX, -ui.maxOffsetX, ui.maxOffsetX);
                float offsetY = Mathf.Clamp(normalized.y * ui.parallaxStrengthY, -ui.maxOffsetY, ui.maxOffsetY);

                Vector2 offset = new Vector2(offsetX, offsetY);
                ui.targetUI.anchoredPosition = ui.initialPosition + offset;
            }
        }
    }
}
