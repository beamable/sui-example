using System.Collections.Generic;

namespace MoeBeam.Game.Scripts.Data
{
    [System.Serializable]
    public class WeaponMetaData
    {
        public int Xp;
        public int Level;
        public int CurrentDamage ;
        public float CurrentAttackSpeed;
        
        public WeaponMetaData(int xp, int level, int currentDamage, float currentAttackSpeed)
        {
            Xp = xp;
            Level = level;
            CurrentDamage = currentDamage;
            CurrentAttackSpeed = currentAttackSpeed;
        }
        
        public void Update(int xp, int level, int currentDamage, float currentAttackSpeed)
        {
            Xp = xp;
            Level = level;
            CurrentDamage = currentDamage;
            CurrentAttackSpeed = currentAttackSpeed;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {"xp", Xp.ToString()},
                {"level", Level.ToString()},
                {"currentDamage", CurrentDamage.ToString()},
                {"currentAttackSpeed", CurrentAttackSpeed.ToString()},
            };
        }
    }
}