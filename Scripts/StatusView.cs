using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class StatusView : MonoBehaviour
{
    [SerializeField] private PartInfo head;
    [SerializeField] private PartInfo body;
    [SerializeField] private PartInfo leftArm;
    [SerializeField] private PartInfo rightArm;
    [SerializeField] private PartInfo leftLeg;
    [SerializeField] private PartInfo rightLeg;
    
    [SerializeField] private ContentInfo intelligence;
    [SerializeField] private ContentInfo stamina; 
    [SerializeField] private ContentInfo strength;
    [SerializeField] private ContentInfo dexterity;
    [SerializeField] private ContentInfo endurance;
    [SerializeField] private ContentInfo agility;

    [SerializeField] private SliderInfo stress; 
    [SerializeField] private DeathRatioInfo deathInfo;
    
    private void Reset()
    {
        var partInfos = GetComponentsInChildren<PartInfo>();
        head = partInfos[0];
        body = partInfos[1];
        leftArm = partInfos[2];
        rightArm = partInfos[3];
        leftLeg = partInfos[4];
        rightLeg = partInfos[5];
        
        var contentInfos = GetComponentsInChildren<ContentInfo>();
        intelligence = contentInfos[0];
        stamina = contentInfos[1];
        strength = contentInfos[2];
        dexterity = contentInfos[3];
        endurance = contentInfos[4];
        agility = contentInfos[5];

        stress = GetComponentInChildren<SliderInfo>();
        deathInfo = GetComponentInChildren<DeathRatioInfo>();
    }
    
    public void SetPlayerData(PlayerData playerData)
    {
        var ct = destroyCancellationToken;

        PresentPart(head, playerData.head);
        PresentPart(body, playerData.body);
        PresentPart(leftArm, playerData.leftArm);
        PresentPart(rightArm, playerData.rightArm);
        PresentPart(leftLeg, playerData.leftLeg);
        PresentPart(rightLeg, playerData.rightLeg);
        
        PresentContent(intelligence, playerData.feeling);
        PresentContent(stamina, playerData.thrust);
        PresentContent(strength, playerData.strength);
        PresentContent(dexterity, playerData.dexterity);
        PresentContent(endurance, playerData.maneuver);
        PresentContent(agility, playerData.agility);

        return;

        void PresentPart(PartInfo partInfo, PartData data)
        {
            data.LevelRp.Subscribe(_ => partInfo.SetLevel(data)).AddTo(ct);
            data.Health.Subscribe(partInfo.SetHealth).AddTo(ct);
        }
        
        void PresentContent(ContentInfo contentInfo, ContentData data)
        {
            data.PointRp.Subscribe(_=>contentInfo.SetValue(data.GetDisplayPointValueText())).AddTo(ct);
        }
    }

    public void SetGameData(GameData gameData)
    {
        var ct = destroyCancellationToken;
        gameData.Stress.Subscribe(stress.SetValue).AddTo(ct);
        gameData.CriticalRatio.Subscribe(deathInfo.SetValue).AddTo(ct);
    }
    
    public void SetSelected(PartId partId)
    {
        head.SetSelected(partId == PartId.Head);
        body.SetSelected(partId == PartId.Body);
        leftArm.SetSelected(partId == PartId.LeftArm);
        rightArm.SetSelected(partId == PartId.RightArm);
        leftLeg.SetSelected(partId == PartId.LeftLeg);
        rightLeg.SetSelected(partId == PartId.RightLeg);
    }

    public void SetupDamageLights(PlayerData playerData)
    {
        head.SetDamageLight(playerData.head.GetDamageAmount(), playerData.head.IsLost);
        body.SetDamageLight(playerData.body.GetDamageAmount(), playerData.body.IsLost);
        leftArm.SetDamageLight(playerData.leftArm.GetDamageAmount(), playerData.leftArm.IsLost);
        rightArm.SetDamageLight(playerData.rightArm.GetDamageAmount(), playerData.rightArm.IsLost);
        leftLeg.SetDamageLight(playerData.leftLeg.GetDamageAmount(), playerData.leftLeg.IsLost);
        rightLeg.SetDamageLight(playerData.rightLeg.GetDamageAmount(), playerData.rightLeg.IsLost);
    }
}
