using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RewardCutIn : MonoBehaviour
{
    [SerializeField] private Image face;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private HumanPartView humanPartView;
    [SerializeField] private Sprite[] faceSprites;
    
    public void Show(CutInFaceId cutInFaceId, string message, PartData headData)
    {
        face.overrideSprite = faceSprites[(int)cutInFaceId];
        label.SetText(message);

        if (cutInFaceId == CutInFaceId.Critical)
        {
            // パーツ分け非表示.
            humanPartView.SetupActiveAt(-1);
        }
        else
        {
            humanPartView.UpdateView(headData);
        }
    }
}
