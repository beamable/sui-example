namespace MoeBeam.Game.Scripts.Data
{
    public class GameData
    {
        public enum AttackType {Swing, Thrust, Shoot}
        public enum CoinType {Beam, Star, Gold}
        
        public const string WeaponContentId = "items.weapon";
        public const string PlayerTag = "Player";
        
        //Events
        public const string OnEnemyKillRewardEvent = "EnemyReward";
        public const string OnEnemyDiedEvent = "EnemyDied";
        public const string OnBossActivateEvent = "BossActivate";
        public const string OnBossDiedEvent = "BossDied";
        public const string OnPlayerDiedEvent = "PlayerDied";
        public const string OnPlayerDeathSequenceDoneEvent = "PlayerDeathSequenceDone";
        public const string OnPlayerInjuredEvent = "PlayerInjured";
        public const string OnDemoLoadingScreenFinishedEvent = "DemoLoadingScreenFinished";
        public const string OnSceneLoadedEvent = "SceneLoaded";
        public const string OnMeleeLeveledUpEvent = "MeleeLeveledUp";
        public const string OnRangedLeveledUpEvent = "RangedLeveledUp";
        public const string OnWeaponGainedXpEvent = "WeaponGainedXp";
        public const string OnTenEnemiesKilledEvent = "TenEnemiesKilled";
        public const string OnCoinCollectedEvent = "CoinCollected";
        
        //Weapon Refs 
        public const string DamageKey = "Damage";
        public const string AttackSpeedKey = "AttackSpeed";
        public const string AttackTypeKey = "AttackType";

        public static AttackType ToAttackType(int type)
        {
            switch (type)
            {
                case 0:
                    return AttackType.Swing;
                case 1:
                    return AttackType.Thrust;
                case 2:
                    return AttackType.Shoot;
                default:
                    return AttackType.Swing;
            }
        }
    }
}