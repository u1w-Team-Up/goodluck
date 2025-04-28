using UnityEngine;

internal sealed class NewLineNode : MonoBehaviour
{
    [SerializeField] private Animator Animator;
    [SerializeField] private RectTransform RectTransform;

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    internal bool Modify(CharaData charaInfo)
    {
        if (charaInfo.Chara is '\n'
         && charaInfo.IsWaitCr)
        {
            gameObject.SetActive(true);
            Animator.Play("Blink");
            RectTransform.anchoredPosition = charaInfo.RectPosLB;

            return true;
        }
        else
        {
            Hide();

            return false;
        }
    }
}
