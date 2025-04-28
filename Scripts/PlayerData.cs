using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRandom;
using NRandom.Linq;

[Serializable]
public class PlayerData
{
    public PartData head = new();
    public PartData body = new();
    public PartData leftArm = new();
    public PartData rightArm = new();
    public PartData leftLeg = new();
    public PartData rightLeg = new();
    
    // イニシャライザに渡してるのは表示用係数.
    public ContentData feeling = new(0.1f);
    public ContentData thrust = new(1000f);
    public ContentData strength = new(10f);
    public ContentData dexterity = new(10f);
    public ContentData maneuver = new(0.1f);
    public ContentData agility = new(100f);
    
    public bool IsAlive => head.IsLive;
    public bool IsDead => !IsAlive;

    public void Birth()
    {
        var random = RandomEx.Shared;
        feeling.Birth(random);
        thrust.Birth(random);
        strength.Birth(random);
        dexterity.Birth(random);
        maneuver.Birth(random);
        agility.Birth(random);
    }
    
    public void Surgery(PartId partId, StringBuilder stringBuilder)
    {
        IRandom random = RandomEx.Shared;

        switch (partId)
        {
            case PartId.Head:
                DoSurgery(head,  "頭蓋", feeling, "感度");
                break;
            case PartId.Body:
                DoSurgery(body,  "胴体", thrust, "推力");
                break;
            case PartId.LeftArm:
                DoSurgery(leftArm,  "左腕", strength, "筋力");
                break;
            case PartId.RightArm:
                DoSurgery(rightArm,  "右腕", dexterity, "器量");
                break;
            case PartId.LeftLeg:
                DoSurgery(leftLeg,  "左脚", maneuver, "機動");
                break;
            case PartId.RightLeg:
                DoSurgery(rightLeg,  "右脚", agility, "敏捷");
                break;
        }
        return;
        
        void DoSurgery(PartData part, string partName, ContentData content, string contentName)
        {
            float diff = 0f;
            switch (part.OnLevelUpHealth())
            {
                case PartData.InitialHealth:
                    part.IncrementLevel();
                    stringBuilder.AppendLine($"{partName} Lv. {part.ToLevelText()}!<waitfor=0.5>");
                    stringBuilder.AppendLine($"{partName}の移植が完了した！<waitfor=0.5>");
                    diff += content.Birth(random);
                    break;
                case > 0:
                    part.IncrementLevel();
                    stringBuilder.AppendLine($"{partName} Lv. {part.ToLevelText()}!<waitfor=0.5>");
                    stringBuilder.AppendLine($"{partName}の負傷が回復した!<waitfor=0.5>");
                    break;
                default:
                    part.IncrementLevel();
                    stringBuilder.AppendLine($"{partName} Lv. {part.ToLevelText()}!<waitfor=0.5>");
                    break;
            }

            diff += content.IncrementLevel(random);
            stringBuilder.AppendLine($"{contentName} {content.GetDisplayPointValueText()}(+{diff:F2})<waitfor=0.5>");
        }
    }

    public void GetLiveParts(List<PartId> liveParts)
    {
        if (head.IsLive) liveParts.Add(PartId.Head);
        if (body.IsLive) liveParts.Add(PartId.Body);
        if (leftArm.IsLive) liveParts.Add(PartId.LeftArm);
        if (rightArm.IsLive) liveParts.Add(PartId.RightArm);
        if (leftLeg.IsLive) liveParts.Add(PartId.LeftLeg);
        if (rightLeg.IsLive) liveParts.Add(PartId.RightLeg);
    }
    
    public void GetOperableParts(List<PartId> parts)
    {
        if (head.IsUpgradable) parts.Add(PartId.Head);
        if (body.IsUpgradable) parts.Add(PartId.Body);
        if (leftArm.IsUpgradable) parts.Add(PartId.LeftArm);
        if (rightArm.IsUpgradable) parts.Add(PartId.RightArm);
        if (leftLeg.IsUpgradable) parts.Add(PartId.LeftLeg);
        if (rightLeg.IsUpgradable) parts.Add(PartId.RightLeg);
    }

    public float GetTotalPoint()
    {
        return feeling.CalculationValue +
               thrust.CalculationValue +
               strength.CalculationValue +
               dexterity.CalculationValue +
               maneuver.CalculationValue +
               agility.CalculationValue;
    }

    public float GetTotalWithPointType(PointType pointType)
    {
        return pointType switch
        {
            PointType.Left => strength.CalculationValue + maneuver.CalculationValue,
            PointType.Middle => feeling.CalculationValue + thrust.CalculationValue,
            PointType.Right => dexterity.CalculationValue + agility.CalculationValue,
            _ => 0
        };
    }

    public void Reborn(GameData gameData)
    {
        head.Reborn();
        feeling.Reborn();
        gameData.OnReborn(feeling.CalculationValue);
    }

    public bool AnyDestruction()
    {
        return head.IsLost || body.IsLost || leftArm.IsLost || rightArm.IsLost || leftLeg.IsLost || rightLeg.IsLost;
    }
    
    public int Damage(GameData gameData, PartId hitPart, Action<CutInFaceId, string> cutInAction, int amount)
    {
        switch (hitPart)
        {
            case PartId.Head:
                return DoHit(PartId.Head, "頭蓋", feeling);
            case PartId.Body:
                return DoHit(PartId.Body, "胴体", thrust);
            case PartId.LeftArm:
                return DoHit(PartId.LeftArm, "左腕", strength);
            case PartId.RightArm:
                return DoHit(PartId.RightArm, "右腕", dexterity);
            case PartId.LeftLeg:
                return DoHit(PartId.LeftLeg, "左脚", maneuver);
            case PartId.RightLeg:
                return DoHit(PartId.RightLeg, "右脚", agility);
        }
        return 0;
        
        int DoHit(in PartId partId,in string partName,in ContentData content)
        {
            PartData part = GetPart(partId);

            part.Damage(amount);
            if (part.IsPinch)
            {
                cutInAction(CutInFaceId.Damage2, $"{partName}に深刻なダメージ");
            }
            else if (part.IsLost)
            {
                if (amount == PartData.InitialHealth)
                {
                    CutInFaceId face = partId == PartId.Head ? CutInFaceId.Critical : CutInFaceId.Damage3; 
                    cutInAction(face, $"{partName}にクリティカルヒット");
                }
                else
                {
                    cutInAction(CutInFaceId.Damage3, $"{partName}が壊滅した");
                }
                part.Deficit();
                gameData.OnDestruction(content.CalculationValue);
                content.Deficit();
            }
            else if (part.IsLive)
            {
                cutInAction(CutInFaceId.Damage1, $"{partName}を負傷した！");
            }
            else  
            {
                throw new ArgumentOutOfRangeException(nameof(part), part, "Invalid part");
            }
            
            return amount;
        }
    }

    public int GetHealth(PartId partId)
    {
        return GetPart(partId).GetHealth();
    }
    
    public int GetCurrentDamagedValue(PartId partId)
    {
        return PartData.InitialHealth - GetHealth(partId);
    }

    public PartData GetPart(PartId partId)
    {
        return partId switch
        {
            PartId.Head => head,
            PartId.Body => body,
            PartId.LeftArm => leftArm,
            PartId.RightArm => rightArm,
            PartId.LeftLeg => leftLeg,
            // PartId.RightLeg => RightLeg,
            _ => rightLeg,
        };
    }
    
    public PartId GetCriticalHitPartId(IRandom random)
    {
        var parts = new List<PartId>();
        GetLiveParts(parts);
        return parts.Count == 6 
            ? parts.Where(x => x != PartId.Head).RandomElement(random) 
            : PartId.Head;
    }
    
    public bool ShouldHeartBeatHighest()
    {
        return head.IsPinch || AnyDestruction();
    }
}
