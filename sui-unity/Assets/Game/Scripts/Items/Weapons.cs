using Beamable.Common.Content;
using Beamable.Common.Inventory;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class WeaponsRef : ItemRef<Weapons> { }

[ContentType("Weapons")]
[System.Serializable]
public class Weapons : ItemContent
{
    [Header("Weapon Info")]
    public string WeaponName;
    [TextArea]public string WeaponDescription;
    public AssetReferenceSprite BulletIcon;
    
    [Header("Attributes")]
    public int Damage;
    public float AttackSpeed;
    
    [Header("Attack Type")]
    public GameData.AttackType AttackType;
}

[System.Serializable]
public class WeaponInstance
{
    public Sprite Icon;
    public Sprite BulletIcon;
    public long InstanceId;
    public string ContentId;
    public string DisplayName;
    public string Description;
    public GameData.AttackType AttackType;
    public WeaponMetaData MetaData;
    public bool IsOwned => InstanceId > 0;
    
    public WeaponInstance(Sprite icon, Sprite bulletIcon, long instanceId, string contentId, string displayName, string description,
        GameData.AttackType type, WeaponMetaData metaData)
    {
        Icon = icon;
        BulletIcon = bulletIcon;
        InstanceId = instanceId;
        ContentId = contentId;
        DisplayName = displayName;
        Description = description;
        AttackType = type;
        MetaData = metaData;
    }
    
    public void UpdateMetaData(WeaponMetaData metaData)
    {
        MetaData = metaData;
    }
}

