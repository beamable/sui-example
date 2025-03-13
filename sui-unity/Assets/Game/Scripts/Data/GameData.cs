namespace MoeBeam.Game.Scripts.Data
{
    public class GameData
    {
        public enum AttackType {Swing, Thrust, Shoot}
        
        public const string WeaponContentId = "items.weapon";
        public const string PlayerTag = "Player";
        public const string OnEnemyKillReward = "EnemyReward";
        public const string EnemyDiedEvent = "EnemyDied";
        
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