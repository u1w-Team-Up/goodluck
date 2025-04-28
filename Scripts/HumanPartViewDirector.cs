using System.Linq;
using UnityEngine;
public class HumanPartViewDirector : MonoBehaviour
{
    [SerializeField] private HumanPartView headView;
    [SerializeField] private HumanPartView bodyView;
    [SerializeField] private HumanPartView leftArmView;
    [SerializeField] private HumanPartView rightArmView;
    [SerializeField] private HumanPartView leftLegView;
    [SerializeField] private HumanPartView rightLegView;

    private void Reset()
    {
        var humanParts = GetComponentsInChildren<HumanPartView>().Reverse().ToArray();
        headView = humanParts.First(x => x.name.ToLower().Contains("head"));
        bodyView = humanParts.First(x => x.name.ToLower().Contains("body"));
        leftArmView = humanParts.First(x => x.name.ToLower().Contains("l_arm"));
        rightArmView = humanParts.First(x => x.name.ToLower().Contains("r_arm"));
        leftLegView = humanParts.First(x => x.name.ToLower().Contains("l_leg"));
        rightLegView = humanParts.First(x => x.name.ToLower().Contains("r_leg"));
    }

    public void UpdateView(PlayerData playerData)
    {
        headView.UpdateView(playerData.head);
        bodyView.UpdateView(playerData.body);
        leftArmView.UpdateView(playerData.leftArm);
        rightArmView.UpdateView(playerData.rightArm);
        leftLegView.UpdateView(playerData.leftLeg);
        rightLegView.UpdateView(playerData.rightLeg);
    }
}
