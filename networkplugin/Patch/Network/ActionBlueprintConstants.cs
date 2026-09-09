namespace NetworkPlugin.Patch.Network;

public static class ActionBlueprintConstants
{
    // Action Kinds
    public const string KindDamage = "Damage";
    public const string KindBlockShield = "BlockShield";
    public const string KindHeal = "Heal";
    public const string KindApplyStatusEffect = "ApplyStatusEffect";
    public const string KindPerformAction = "PerformAction";
    public const string KindAnimation = "Animation";
    public const string KindSfx = "Sfx";
    public const string KindWait = "Wait";

    // Unit Kinds
    public const string UnitPlayer = "Player";
    public const string UnitEnemy = "Enemy";
    public const string UnitUnknown = "Unknown";

    // Common Blueprint Keys
    public const string KeyKind = "Kind";
    public const string KeyCaster = "Caster";
    public const string KeySource = "Source";
    public const string KeyTarget = "Target";
    public const string KeyTargets = "Targets";
    public const string KeyDamage = "Damage";
    public const string KeyDamageType = "DamageType";
    public const string KeyIsAccuracy = "IsAccuracy";
    public const string KeyDontBreakPerfect = "DontBreakPerfect";
    public const string KeyGunName = "GunName";
    public const string KeyGunType = "GunType";
    public const string KeyAmount = "Amount";
    public const string KeyHealType = "HealType";
    public const string KeyWaitTime = "WaitTime";
    public const string KeyEffectId = "EffectId";
    public const string KeyLevel = "Level";
    public const string KeyDuration = "Duration";
    public const string KeyCount = "Count";
    public const string KeyLimit = "Limit";
    public const string KeyStartAutoDecreasing = "StartAutoDecreasing";
    public const string KeyBlock = "Block";
    public const string KeyShield = "Shield";
    public const string KeyCast = "Cast";
    public const string KeyType = "Type";
    public const string KeyPlayerId = "PlayerId";
    public const string KeyEnemyId = "EnemyId";
    public const string KeyRootIndex = "RootIndex";
    public const string KeyId = "Id";

    // Animation Names
    public const string AnimIdle = "idle";
    public const string AnimDefend = "defend";
    public const string AnimSpell = "spell";
    public const string AnimCast = "cast";
    public const string AnimSkill = "skill";
    public const string AnimHit = "hit";

    // VFX Names
    public const string VfxCastShield = "CastShield";
    public const string VfxCastBlock = "CastBlock";
    public const string VfxCardCast = "CardCast";
    public const string VfxUnitHeal = "UnitHeal";
    public const string VfxUnitHealLarge = "UnitHealLarge";

    // SFX Names
    public const string SfxShieldCast = "ShieldCast";
    public const string SfxBuff = "Buff";
    public const string SfxDebuff = "Debuff";
    public const string SfxHeal = "Heal";
    public const string SfxHealLarge = "HealLarge";

    // Shoot Status Values (UnitView.ShootStatus)
    public const int ShootStatusIdle = 0;
    public const int ShootStatusDirect = 1;
    public const int ShootStatusComplex = 2;
}
